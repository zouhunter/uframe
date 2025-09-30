/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 控制器                                                                          *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UFrame.NodeGraph.DataModel;

namespace UFrame.NodeGraph
{
    public abstract class NodeGraphController<T>: NodeGraphController where T : NodeGraphObj
    {
        public override NodeGraphObj CreateNodeGraphObject()
        {
            NodeGraphObj graph = ScriptableObject.CreateInstance<T>();
            graph.ControllerType = this.GetType().FullName;
            ProjectWindowUtil.CreateAsset(graph, string.Format("new {0}.asset",graph.GetType().Name));
            return graph;
        }
    }

    [System.Serializable]
    public abstract class NodeGraphController
    {
        protected List<NodeException> m_nodeExceptions = new List<NodeException>();
        protected NodeGraphObj m_targetGraph;
        public abstract string Group { get; }

        public bool IsAnyIssueFound
        {
            get
            {
                return m_nodeExceptions.Count > 0;
            }
        }

        public List<NodeException> Issues
        {
            get
            {
                return m_nodeExceptions;
            }
        }

        public NodeGraphObj TargetGraph
        {
            get
            {
                return m_targetGraph;
            }
            set
            {
                m_targetGraph = value;
            }
        }

        public void Perform()
        {
            LogUtility.Logger.Log(LogType.Log, "---Setup BEGIN---");

            foreach (var e in m_nodeExceptions)
            {
                var errorNode = m_targetGraph.Nodes.Find(n => n.Id == e.Id);
                // errorNode may not be found if user delete it on graph
                if (errorNode != null)
                {
                    LogUtility.Logger.LogFormat(LogType.Log, "[Perform] {0} is marked to revisit due to last error", errorNode.Name);
                    errorNode.NeedsRevisit = true;
                }
            }

            m_nodeExceptions.Clear();
            JudgeNodeExceptions(m_targetGraph, m_nodeExceptions);
            LogUtility.Logger.Log(LogType.Log, "---Setup END---");
        }
        public virtual void Build()
        {
            if(m_nodeExceptions == null || m_nodeExceptions.Count == 0)
            {
                BuildFromGraph(m_targetGraph);
            }
            else
            {
                Debug.LogError("have exception in build!");
            }
        }
        protected virtual void JudgeNodeExceptions(NodeGraphObj m_targetGraph, List<NodeException> m_nodeExceptions) { }
        public virtual void BuildFromGraph(NodeGraphObj m_targetGraph) { }
        public virtual void OnDragUpdated() { }
        public virtual List<KeyValuePair<string, Node>> OnDragAccept(UnityEngine.Object[] objectReferences) { return null; }
        public virtual void Validate(NodeGUI node) {
           
        }

        public virtual string GetConnectType(ConnectionPointData output, ConnectionPointData input)
        {
            if(output.Type == input.Type) {
                return output.Type;
            }
            return null;
        }

        public virtual void DrawNodeGUI(NodeGUI nodeGUI)
        {

        }
        public abstract NodeGraphObj CreateNodeGraphObject();

        protected static bool IsMainAsset(ScriptableObject obj, out ScriptableObject mainAsset)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            mainAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            return mainAsset == obj;
        }

        public virtual void SaveGraph(List<NodeData> nodes, List<ConnectionData> connections,bool resetAll = false)
        {
            UnityEngine.Assertions.Assert.IsNotNull(this);
            TargetGraph.ApplyGraph(nodes, connections);
            NodeGraphObj obj = TargetGraph;
            var all = new List<ScriptableObject>();
            all.AddRange(Array.ConvertAll<NodeData, Node>(nodes.ToArray(), x => x.Object));
            all.AddRange(Array.ConvertAll<ConnectionData, Connection>(connections.ToArray(), x => x.Object));
            ScriptableObject mainAsset;
            if (!IsMainAsset(obj, out mainAsset))
            {
                Undo.RecordObject(obj, "none");
                all.Add(obj);
                ScriptableObjUtility.SetSubAssets(all.ToArray(), mainAsset, resetAll, HideFlags.HideInHierarchy);
                UnityEditor.EditorUtility.SetDirty(mainAsset);
            }
            else
            {
                ScriptableObjUtility.SetSubAssets(all.ToArray(), obj, resetAll, HideFlags.HideInHierarchy);
                UnityEditor.EditorUtility.SetDirty(obj);
            }
            AssetDatabase.Refresh();
        }
    }
}
