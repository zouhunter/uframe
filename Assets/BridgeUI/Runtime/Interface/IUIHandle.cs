/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面访问接口                                                                    *
*//************************************************************************************/

using UnityEngine.Events;
using UFrame.BridgeUI;

namespace UFrame.BridgeUI
{
    public interface IPanelVisitor
    {
        object Data { get; }
        IUIHandle uiHandle { get; }
        void Binding(IUIHandle uiHandle);
        void Recover();
    }

    public interface IUIHandle
    {
        string PanelName { get; }
        IUIPanel[] GetActivePanels();
        void Send(object data);
        void RegistCallBack(UnityAction<IUIPanel, object> onCallBack);
        void RemoveCallBack(UnityAction<IUIPanel, object> onCallBack);
        void RegistCreate(UnityAction<IUIPanel> onCreate);
        void RemoveCreate(UnityAction<IUIPanel> onCreate);
        void RegistClose(UnityAction<IUIPanel> onClose);
        void RemoveClose(UnityAction<IUIPanel> onClose);
        void RegistToggleHide(UnityAction<IUIPanel, bool> onToggleHide);
        void RemoveToggleHide(UnityAction<IUIPanel, bool> onToggleHide);
        void RegistOnRecover(UnityAction onRecover);
        void RemoveOnRecover(UnityAction onRecover);
        void Dispose();
    }

    public interface IUIHandleInternal : IUIHandle
    {
        void Reset(string panelName,UnityAction<UIHandle> onRelease);
        void RegistBridge(Bridge bridgeObj);
        void UnRegistBridge(Bridge obj);
    }
   
}