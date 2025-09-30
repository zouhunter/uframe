using System;
using UnityEngine;
using UFrame.BridgeUI;

namespace UFrame.BridgeUI
{
    public struct UICreateHandle
    {
        public delegate void CreateCallBack(GameObject go, UIInfoBase uiNode, Bridge bridge, Transform parent, IUIPanel parentPanel,IUIPanel panel);
        public CreateCallBack onCreate { get; set; }
        public UIInfoBase uiNode { get; set; }
        public Bridge bridge { get; set; }
        public Transform parent { get; set; }
        public IUIPanel parentPanel { get; set; }
        public IUIPanel panel { get; set; }

        public void OnCreate(GameObject go)
        {
            if (onCreate != null && go != null)
            {
                onCreate(go, uiNode, bridge, parent, parentPanel, panel);
            }
        }
    }
}
