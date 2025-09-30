/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 使用接口                                                                        *
*//************************************************************************************/
using System.Collections.Generic;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    public interface IUIFacade
    {
        public HashSet<IUIPanel> OpendPanels { get; }
        HashSet<string> LoadingPanels { get; }
        void Open(string panelName, object data = null);
        void Open(IUIPanel parentPanel, string panelName, object data = null);
        void Open(string panelName, IPanelVisitor uiData);
        void Open(IUIPanel parentPanel, string panelName, IPanelVisitor uiData);
        bool IsPanelOpen(string panelName,bool includeHide = false);
        bool IsPanelOpen<T>(string panelName, out T[] panels, bool includeHide = false) where T:IUIPanel;
        void Hide(string panelName);
        void UnHide(string panelName);
        bool Exist(string panelName);
        void Send(string panelName, object data);
        void Hide(IPanelGroup parentGroup, string panelName);
        void Close(string panelName);
        void Close(IPanelGroup parentGroup, string panelName);
        void RegistCreate(UnityAction<IUIPanel> onCreate);
        void RemoveCreate(UnityAction<IUIPanel> onCreate);
        void RegistClose(UnityAction<IUIPanel> onClose);
        void RemoveClose(UnityAction<IUIPanel> onClose);
        void RegistHide(UnityAction<IUIPanel, bool> onHide);
        void RemoveHide(UnityAction<IUIPanel, bool> onHide);
        void RegistGroup(IPanelGroup group);
        void RemoveGroup(IPanelGroup group);
    }
}