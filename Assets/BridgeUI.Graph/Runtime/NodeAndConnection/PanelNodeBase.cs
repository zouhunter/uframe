using UnityEngine;
using UFrame.NodeGraph.DataModel;
using UFrame.BridgeUI;
using System.Collections.Generic;
//using UFrame.BridgeUI.Editors;

namespace UFrame.BridgeUI.Graph
{
    public abstract class PanelNodeBase : Node, IPanelInfoHolder
    {
        public int selected;
        public int instenceID;
        public string assetName;
        public int style = 1;
        public List<string> nodedescribe = new List<string>();
        //public GenCodeRule rule;
        //public List<ComponentItem> components = new List<ComponentItem>();
        public NodeInfo nodeInfo = new NodeInfo();
        public NodeInfo Info
        {
            get
            {
                return nodeInfo;
            }
        }
    }
}