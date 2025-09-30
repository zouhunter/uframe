using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;
using UFrame.NodeGraph;
namespace UFrame.BridgeUI.Graph
{
    [CustomNodeAttribute("a.AnyPanel", 0, "BridgeUI")]
    public class AnyNode : Node
    {
        public override void Initialize(NodeData data)
        {
            base.Initialize(data);
            if (data.OutputPoints == null || data.OutputPoints.Count == 0)
            {
                data.AddOutputPoint("→", "bridge", 100);
            }
        }
    }
}