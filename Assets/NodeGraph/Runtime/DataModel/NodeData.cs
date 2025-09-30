/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 节点数据                                                                        *
*//************************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace UFrame.NodeGraph.DataModel
{
    [Serializable]
    public class NodeData
    {
        [SerializeField]
        protected string m_name;
        [SerializeField]
        protected string m_id;
        [SerializeField]
        protected float m_x;
        [SerializeField]
        protected float m_y;
        [SerializeField]
        protected Node m_node;
        [SerializeField]
        protected List<ConnectionPointData> m_inputPoints;
        [SerializeField]
        protected List<ConnectionPointData> m_outputPoints;
        protected bool m_nodeNeedsRevisit;

        public ref bool NeedsRevisit => ref m_nodeNeedsRevisit;
        public ref string Name => ref m_name;
        public string Id => m_id;
        public ref Node Object => ref m_node;
        public ref float X => ref m_x;
        public ref  float Y => ref m_y;

        public List<ConnectionPointData> InputPoints => m_inputPoints;
        public List<ConnectionPointData> OutputPoints => m_outputPoints;
        
        public NodeData(string name, Node node, float x, float y)
        {
            m_id = Guid.NewGuid().ToString();
            m_name = name;
            m_x = x;
            m_y = y;
            m_node = node;
            m_nodeNeedsRevisit = false;

            m_inputPoints = new List<ConnectionPointData>();
            m_outputPoints = new List<ConnectionPointData>();
        }

        public NodeData(NodeData node, bool keepId = false)
        {
            m_name = node.m_name;
            m_x = node.m_x;
            m_y = node.m_y;

            m_inputPoints = new List<ConnectionPointData>();
            m_outputPoints = new List<ConnectionPointData>();

            if (keepId)
            {
                m_id = node.m_id;
                node.InputPoints.ForEach(p => m_inputPoints.Add(new ConnectionPointData(p)));
                node.OutputPoints.ForEach(p => m_outputPoints.Add(new ConnectionPointData(p)));
            }
            else
            {
                m_id = Guid.NewGuid().ToString();
            }
            if (node.Object == null)
            {
                m_node = ScriptObjectCatchUtil.Revert(node.Id) as Node;
            }
            else
            {
                m_node = UnityEngine.Object.Instantiate((UnityEngine.Object)node.Object) as Node;
                m_node.name = node.Object.name;
            }
        }
        
        public NodeData Duplicate(bool keepId = false)
        {
            return new NodeData(this, keepId);
        }

        public ConnectionPointData AddInputPoint(string label, string type, int max = 1)
        {
            var p = new ConnectionPointData(label, type, max, this, true);
            m_inputPoints.Add(p);
            return p;
        }

        public ConnectionPointData AddOutputPoint(string label, string type, int max = 1)
        {
            var p = new ConnectionPointData(label, type, max, this, false);
            m_outputPoints.Add(p);
            return p;
        }

        public ConnectionPointData FindInputPoint(string id)
        {
            return m_inputPoints.Find(p => p.Id == id);
        }

        public ConnectionPointData FindOutputPoint(string id)
        {
            return m_outputPoints.Find(p => p.Id == id);
        }

        public ConnectionPointData FindConnectionPoint(string id)
        {
            var v = FindInputPoint(id);
            if (v != null)
            {
                return v;
            }
            return FindOutputPoint(id);
        }

        public bool Validate()
        {
            if (m_node == null)
            {
                m_node = ScriptObjectCatchUtil.Revert(Id) as Node;
            }
            return Object != null;
        }

        public bool CompareIgnoreGUIChanges(NodeData rhs)
        {
            if (m_node == null && rhs.m_node != null ||
                m_node != null && rhs.m_node == null)
            {
                LogUtility.Logger.LogFormat(LogType.Log, "{0} and {1} was different: {2}", Name, rhs.Name, "Node Type");
                return false;
            }

            if (m_inputPoints.Count != rhs.m_inputPoints.Count)
            {
                LogUtility.Logger.LogFormat(LogType.Log, "{0} and {1} was different: {2}", Name, rhs.Name, "Input Count");
                return false;
            }

            if (m_outputPoints.Count != rhs.m_outputPoints.Count)
            {
                LogUtility.Logger.LogFormat(LogType.Log, "{0} and {1} was different: {2}", Name, rhs.Name, "Output Count");
                return false;
            }

            foreach (var pin in m_inputPoints)
            {
                if (rhs.m_inputPoints.Find(x => pin.Id == x.Id) == null)
                {
                    LogUtility.Logger.LogFormat(LogType.Log, "{0} and {1} was different: {2}", Name, rhs.Name, "Input point not found");
                    return false;
                }
            }

            foreach (var pout in m_outputPoints)
            {
                if (rhs.m_outputPoints.Find(x => pout.Id == x.Id) == null)
                {
                    LogUtility.Logger.LogFormat(LogType.Log, "{0} and {1} was different: {2}", Name, rhs.Name, "Output point not found");
                    return false;
                }
            }


            return true;
        }

    }
}
