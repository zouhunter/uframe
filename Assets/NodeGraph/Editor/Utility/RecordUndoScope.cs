/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 回退注册器                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.NodeGraph
{
    public class RecordUndoScope : GUI.Scope
    {
        private string message;
        private NodeGUI node;
        private bool saveOnScopeEnd;

        public RecordUndoScope(string message)
        {
            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
        }

        public RecordUndoScope(string message, bool saveOnScopeEnd)
        {
            this.saveOnScopeEnd = saveOnScopeEnd;
            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
        }

        public RecordUndoScope(string message, NodeGUI node)
        {
            this.node = node;
            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
        }

        public RecordUndoScope(string message, NodeGUI node, bool saveOnScopeEnd)
        {
            this.node = node;
            this.saveOnScopeEnd = saveOnScopeEnd;
            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_RECORDUNDO, message));
        }

        protected override void CloseScope()
        {
            if (node != null)
            {
                //node.UpdateNodeRect();
                node.ResetErrorStatus();
                NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_NODE_UPDATED, node));
            }
            if (saveOnScopeEnd)
            {
                //				NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_SAVE));
            }
        }
    }
}
