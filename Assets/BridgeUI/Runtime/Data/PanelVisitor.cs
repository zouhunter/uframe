/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 面板访问器                                                                      *
*//************************************************************************************/

using System;

namespace UFrame.BridgeUI
{
    public class PanelVisitor : BridgeUI.IPanelVisitor
    {
        public object Data { get; private set; }

        public Action<IUIPanel, object> onCallBack { get; set; }

        public Action<IUIPanel> onCreate { get; set; }

        public Action<IUIPanel> onClose { get; set; }
        public Action<IUIPanel> onHide { get; set; }
        public Action<IUIPanel> onShow { get; set; }

        public IUIHandle uiHandle { get; private set; }

        public PanelVisitor(object data = null)
        {
            this.Data = data;
        }

        public void Send(object data)
        {
            this.Data = data;
            if (this.uiHandle != null)
            {
                this.uiHandle.Send(data);
            }
        }

        private void OnCallBack(IUIPanel panel, object target)
        {
            if (this.onCallBack != null)
            {
                onCallBack(panel, target);
            }
        }

        private void OnClose(IUIPanel panel)
        {
            if (this.onClose != null)
            {
                onClose(panel);
            }
        }

        private void OnToggleHide(IUIPanel panel, bool hide)
        {
            if (hide)
            {
                onHide?.Invoke(panel);
            }
            else
            {
                onShow?.Invoke(panel);
            }
        }

        private void OnCreate(IUIPanel panel)
        {
            if (this.onCreate != null)
            {
                onCreate(panel);
            }
        }

        public void Binding(IUIHandle uiHandle)
        {
            if (this.uiHandle != null)
                Recover();

            if (uiHandle != null)
            {
                this.uiHandle = uiHandle;
                uiHandle.RegistCallBack(OnCallBack);
                uiHandle.RegistClose(OnClose);
                uiHandle.RegistCreate(OnCreate);
                uiHandle.RegistToggleHide(OnToggleHide);
                uiHandle.RegistOnRecover(Recover);
            }
        }

        public void Recover()
        {
            if (uiHandle != null)
            {
                uiHandle.RemoveCallBack(OnCallBack);
                uiHandle.RemoveClose(OnClose);
                uiHandle.RemoveCreate(OnCreate);
                uiHandle.RemoveOnRecover(Recover);
                uiHandle = null;
            }

        }
    }
}
