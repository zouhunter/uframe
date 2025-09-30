/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 面板接口                                                                    *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UFrame.BridgeUI;

namespace UFrame.BridgeUI
{
    public interface IUIPanel : IUIOpenClose
    {
        GameObject Target { get; }
        string Name { get; }
        int InstenceID { get; }
        IPanelGroup Group { get; set; }
        IUIPanel Parent { get; set; }
        Transform Content { get; }
        Transform GetContent(int index);
        Transform Root { get; }
        List<IUIPanel> ChildPanels { get; }
        event System.Action<IUIPanel> onClose;
        event System.Action<IUIPanel> onShow;
        event System.Action<IUIPanel> onHide;
        UIType UType { get; set; }
        bool IsShowing { get; }
        bool IsAlive { get; }
        void Binding(GameObject target);
        void Close();
        void Hide();
        void UnHide();
        void SetParent(Transform Trans, Dictionary<int, Transform> transDic, Dictionary<int, IUIPanel> transRefDic);
        void CallBack(object data);
        void RecordChild(IUIPanel childPanel);
        void HandleData(Bridge bridge);
    }


}