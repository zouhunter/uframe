/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面组接口                                                                      *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UFrame.BridgeUI;

namespace UFrame.BridgeUI
{
    public interface IPanelGroup
    {
        Transform Trans { get; }
        Dictionary<string, UIInfoBase> Nodes { get; }
        List<IUIPanel> RetrivePanels(string panelName,bool includeHide=true);
        UIBindingController BindingCtrl { get; }
        Bridge OpenPanel(IUIPanel parentPanel, string panelName, int index);
        Bridge FindOrCreateBridge(IUIPanel parentPanel, string panelName, int index);
        bool OpenPanelWithBridge(Bridge bridge);
        void ClosePanel(string panelName);
        void HidePanel(string panelName);
        void UnHidePanel(string panelName);
        bool IsPanelOpen(string panelName,bool includeHide = false);
        bool IsPanelOpen<T>(string panelName, out T[] panels, bool includeHide = false) where T : IUIPanel;
        void CansaleInstencePanel(string panelName);
        void Send(string panelName, object data);
    }
}