using UFrame.NodeGraph.DataModel;
using UFrame.NodeGraph;

namespace UFrame.BridgeUI.Graph
{
    [CustomNodeAttribute("b.RealPanel", 1, "BridgeUI")]
    public class PanelNode : PanelNodeBase
    {
        public override void Initialize(NodeData data)
        {
            base.Initialize(data);

            if (data.InputPoints == null || data.InputPoints.Count == 0)
            {
                data.AddInputPoint("→", "bridge", 20);
            }
            else
            {
                data.InputPoints[0].Label = "→";
            }
            
            if (nodedescribe != null)
            {
                if (nodedescribe.Count > data.OutputPoints.Count)
                {
                    for (int i = 0; i < nodedescribe.Count; i++)
                    {
                        if (data.OutputPoints.Count <= i)
                        {
                            var nodeName = string.Format("{0}({1})", nodedescribe[i], i.ToString());
                            data.AddOutputPoint(nodeName, "bridge", 20);
                        }
                    }
                }
                else if (nodedescribe.Count < data.OutputPoints.Count)
                {
                    var more = data.OutputPoints.Count - nodedescribe.Count;
                    for (int i = 0; i < more; i++)
                    {
                        data.OutputPoints.RemoveAt(data.OutputPoints.Count - 1);
                    }
                }
                else
                {
                    for (int i = 0; i < nodedescribe.Count; i++)
                    {
                        var nodeName = string.Format("{0}({1})", nodedescribe[i], i.ToString());
                        data.OutputPoints[i].Label = nodeName;
                    }
                }
            }
        }

    }
}