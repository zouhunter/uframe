/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 空控制模板                                                                      *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.NodeGraph
{
    public class EmptyController : NodeGraphController<NodeGraph.DataModel.NodeGraphObj>
    {
        public override string Group { get { return "Empty"; } }

        protected override void JudgeNodeExceptions(DataModel. NodeGraphObj m_targetGraph, List<NodeException> m_nodeExceptions)
        {
            Debug.Log("To Judge All Nodes, Is There Have Some Exceptions?");
        }
        public override void BuildFromGraph(DataModel.NodeGraphObj m_targetGraph)
        {
            Debug.Log("On Build Button Clicked!");
        }

        public override List<KeyValuePair<string, DataModel.Node>> OnDragAccept(UnityEngine.Object[] objectReferences)
        {
            Debug.Log("You Can Quick Generate Node From DragAndDrop!");
            return new List<KeyValuePair<string, DataModel.Node>>();
        }

        public override void Validate(NodeGUI node)
        {
            Debug.Log("When One Node Is  Updated, Judge Validate!");
        }
    }

}