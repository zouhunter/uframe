/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 图数据实体                                                                      *
*//************************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.NodeGraph.DataModel
{
    public class NodeGraphObj : ScriptableObject
    {
        public const int ABG_FILE_VERSION = 2;

        [SerializeField]
        protected List<NodeData> m_allNodes = new List<NodeData>();
        [SerializeField]
        protected List<ConnectionData> m_allConnections = new List<ConnectionData>();
        [SerializeField]
        protected string m_lastModified;
        [SerializeField]
        protected string m_graphDescription;
        [SerializeField]
        protected string m_controllerType;

        public ref string ControllerType => ref m_controllerType;
        public ref string Descrption => ref m_graphDescription;
        public List<NodeData> Nodes => m_allNodes;
        public List<ConnectionData> Connections => m_allConnections;

        public static bool log = false;

        protected void OnEnable()
        {
            Initialize();
            Validate();
        }

        protected string GetFileTimeUtcString()
        {
            return DateTime.Now.ToFileTimeUtc().ToString();
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(m_lastModified))
            {
                m_lastModified = GetFileTimeUtcString();
                m_allNodes = new List<NodeData>();
                m_allConnections = new List<ConnectionData>();
                m_graphDescription = String.Empty;
            }
        }


        public List<NodeData> CollectAllLeafNodes()
        {

            var nodesWithChild = new List<NodeData>();
            foreach (var c in m_allConnections)
            {
                NodeData n = m_allNodes.Find(v => v.Id == c.FromNodeId);
                if (n != null)
                {
                    nodesWithChild.Add(n);
                }
            }
            return m_allNodes.Except(nodesWithChild).ToList();
        }

        public List<NodeData> CollectAllRootNodes()
        {
            var nodesNoRoot = new List<NodeData>();
            foreach (var c in Nodes)
            {
                if (c.InputPoints.Count == 0)
                    nodesNoRoot.Add(c);
            }
            return nodesNoRoot;
        }


        public DateTime CreateLastModified()
        {
            long utcFileTime = long.Parse(m_lastModified);
            DateTime d = DateTime.FromFileTimeUtc(utcFileTime);
            return d;
        }

        public void ApplyGraph(List<NodeData> nodes, List<ConnectionData> connections)
        {
            if (!Enumerable.SequenceEqual(nodes.OrderBy(v => v.Id), m_allNodes.OrderBy(v => v.Id)) ||
                !Enumerable.SequenceEqual(connections.OrderBy(v => v.Id), m_allConnections.OrderBy(v => v.Id)))
            {
                if(log) Debug.Log("[ApplyGraph] SaveData updated.");
                m_lastModified = GetFileTimeUtcString();
                m_allNodes = nodes;
                m_allConnections = connections;
            }
            else
            {
                if (log) Debug.Log("[ApplyGraph] SaveData update skipped. graph is equivarent.");
            }
        }

        public bool Validate()
        {
            var changed = false;

            if (m_allNodes != null)
            {
                List<NodeData> removingNodes = new List<NodeData>();
                foreach (var n in m_allNodes)
                {
                    if (!n.Validate())
                    {
                        removingNodes.Add(n);
                        changed = true;
                    }
                }
                m_allNodes.RemoveAll(n => removingNodes.Contains(n));
            }

            if (m_allConnections != null)
            {
                List<ConnectionData> removingConnections = new List<ConnectionData>();
                foreach (var c in m_allConnections)
                {
                    if (!c.Validate(m_allNodes, m_allConnections))
                    {
                        removingConnections.Add(c);
                        changed = true;
                    }
                }
                m_allConnections.RemoveAll(c => removingConnections.Contains(c));
            }

            if (changed)
            {
                m_lastModified = GetFileTimeUtcString();
            }

            return !changed;
        }
    }
}