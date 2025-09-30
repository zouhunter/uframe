/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 异常收集                                                                        *
*//************************************************************************************/

using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;

namespace UFrame.NodeGraph
{
    public class ExecuteGraphResult
    {
        private NodeGraphObj graph;
        private List<NodeException> issues;

        public ExecuteGraphResult(NodeGraphObj g, List<NodeException> issues)
        {
            this.graph = g;
            this.issues = issues;
        }

        /// <summary>
        /// Gets a value indicating whether last graph execution has any issue found.
        /// </summary>
        /// <value><c>true</c> if this instance is any issue found; otherwise, <c>false</c>.</value>
		public bool IsAnyIssueFound
        {
            get
            {
                return issues.Count > 0;
            }
        }

        /// <summary>
        /// Gets the executed graph associated with this result.
        /// </summary>
        /// <value>The graph.</value>
		public NodeGraphObj Graph
        {
            get
            {
                return graph;
            }
        }

        /// <summary>
        /// Gets the graph asset path.
        /// </summary>
        /// <value>The graph asset path.</value>
		public string GraphAssetPath
        {
            get
            {
                return AssetDatabase.GetAssetPath(graph);
            }
        }

        /// <summary>
        /// Gets the list of issues found during last execution.
        /// </summary>
		public IEnumerable<NodeException> Issues
        {
            get
            {
                return issues.AsEnumerable();
            }
        }
    }

}
