/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 保存区域                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.NodeGraph
{
    public class SaveScope : GUI.Scope
    {
        private NodeGUI node;

        public SaveScope(NodeGUI node)
        {
            this.node = node;
        }

        protected override void CloseScope()
        {
            if (node != null)
            {
                node.ResetErrorStatus();
            }
            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_SAVE));
        }
    }
}
