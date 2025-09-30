using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;
using UFrame.NodeGraph;
namespace UFrame.BridgeUI.Graph
{
    [CustomNodeAttribute("c.RemotePanel", 2, "BridgeUI")]
    public class RemoteNode : Node
    {
        public override void Initialize(NodeData data)
        {
            base.Initialize(data);
            if (data.InputPoints == null || data.InputPoints.Count == 0)
            {
                data.AddInputPoint("→", "bridge", 100);
            }
        }
    }
}