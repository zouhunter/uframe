using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;

namespace UFrame.BridgeUI.Graph
{
    public class UIGraph: NodeGraphObj
    {
        public LoadType loadType = LoadType.DirectLink;
        public List<BundleUIInfo> b_nodes = new List<BundleUIInfo>();
        public List<PrefabUIInfo> p_nodes = new List<PrefabUIInfo>();
        public List<ResourceUIInfo> r_nodes = new List<ResourceUIInfo>();
        public List<BridgeInfo> bridges = new List<BridgeInfo>();
    }

}