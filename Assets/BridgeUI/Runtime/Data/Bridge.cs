/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - UI连接器                                                                        *
*//************************************************************************************/

using UnityEngine.Events;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public class Bridge
    {
        #region 实例使用
        public BridgeInfo Info { get; set; }
        public UnityAction<Queue<object>> onGet { get; set; }
        public Queue<object> dataQueue = new Queue<object>();
        public event UnityAction<Bridge> onRelease;
        public event UnityAction<IUIPanel, object> onCallBack;
        public event UnityAction<IUIPanel> onCreate;
        public event UnityAction<IUIPanel,bool> onToggleHide;
        public IUIPanel InPanel { get; private set; }
        public IUIPanel OutPanel { get; private set; }
        private UnityAction<Bridge> onReleaseFromPool { get; set; }
        public Bridge(UnityAction<Bridge> onReleaseFromPool)
        {
            this.onReleaseFromPool = onReleaseFromPool;
        }

        public void ResetInfo(BridgeInfo info)
        {
            this.Info = new BridgeInfo(info.inNode, info.outNode, info.showModel, info.index);
            this.onCreate = null;
            this.onGet = null;
            this.onCallBack = null;
            this.dataQueue.Clear();
            this.InPanel = null;
            this.OutPanel = null;
            this.onRelease = null;
            this.onToggleHide = null;
        }

        public void SetInPanel(IUIPanel parentPanel)
        {
            this.InPanel = parentPanel;
            if (InPanel != null)
            {
                Info = new BridgeInfo(parentPanel.Name, Info.outNode, Info.showModel, Info.index);
            }
            else
            {
                Info = new BridgeInfo("", Info.outNode, Info.showModel, Info.index);
            }
        }

        public void Send(object data)
        {
            dataQueue.Enqueue(data);
            onGet?.Invoke(dataQueue);
        }

        public void CallBack(IUIPanel panel, object data)
        {
            onCallBack?.Invoke(panel, data);
        }

        public void Release()
        {
            if(onRelease != null)
            {
                onRelease?.Invoke(this);
                onRelease = null;
                onReleaseFromPool?.Invoke(this);
            }
        }

        public void OnToggleHidePanel(bool hide)
        {
            onToggleHide?.Invoke(OutPanel, hide);
        }


        public void OnCreatePanel(IUIPanel panel)
        {
            OutPanel = panel;
            onCreate?.Invoke(panel);
        }
        #endregion
    }
}
