/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 节点gui                                                                         *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.NodeGraph {
	/*
	 * ScriptableObject helper object to let NodeGUI edit from Inspector
	 */
    public class NodeGUIInspectorHelper : ScriptableObject {
	
        public NodeGUI Node { get { return _node; } }
        public List<string> Errors { get { return _errors; } }

        private NodeGUI _node;

        private List<string> _errors = new List<string>();

        public void UpdateNodeGUI(NodeGUI node)
        {
            this._node = node;
        }

		public void UpdateErrors (List<string> errorsSource) {
			this._errors = errorsSource;
		}
	}
}