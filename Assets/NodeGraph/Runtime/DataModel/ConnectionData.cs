/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接数据                                                                        *
*//************************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace UFrame.NodeGraph.DataModel {

	[Serializable]
	public class ConnectionData
    {
        [SerializeField] protected string m_id;
		[SerializeField] protected string m_fromNodeId;
		[SerializeField] protected string m_fromNodeConnectionPointId;
		[SerializeField] protected string m_toNodeId;
		[SerializeField] protected string m_toNodeConnectionPoiontId;
		[SerializeField] protected string m_type;
        [SerializeField] protected Connection m_connection;

        #region Propertys
        public string Id => m_id;
        public ref string ConnectionType => ref m_type;
        public string FromNodeId => m_fromNodeId;
        public ref Connection Object => ref m_connection;
        public string FromNodeConnectionPointId => m_fromNodeConnectionPointId;
		public string ToNodeId => m_toNodeId;
		public string ToNodeConnectionPointId => m_toNodeConnectionPoiontId;
        #endregion

        public ConnectionData(string type, Connection connection, ConnectionPointData output, ConnectionPointData input) {
            m_id = Guid.NewGuid().ToString();
            m_type = type;
            m_fromNodeId = output.ParentId;
			m_fromNodeConnectionPointId = output.Id;
			m_toNodeId = input.ParentId;
			m_toNodeConnectionPoiontId = input.Id;
            m_connection = connection;
            m_connection.name = connection.name;
        }

        public bool Validate()
        {
            if (m_connection == null){
                m_connection = ScriptObjectCatchUtil.Revert(Id) as Connection;
            }
            return m_connection != null;
        }


		public bool Validate (List<NodeData> allNodes, List<ConnectionData> allConnections) {

			var fromNode = allNodes.Find(n => n.Id == this.FromNodeId);
			var toNode   = allNodes.Find(n => n.Id == this.ToNodeId);

			if(fromNode == null) {
				return false;
			}

			if(toNode == null) {
				return false;
			}

			var outputPoint = fromNode.FindOutputPoint(this.FromNodeConnectionPointId);
			var inputPoint  = toNode.FindInputPoint(this.ToNodeConnectionPointId);

			if(null == outputPoint) {
				return false;
			}

			if(null == inputPoint) {
				return false;
			}

			if( outputPoint.Type != m_type ) {
				m_type = outputPoint.Type;
			}

			return true;
		}
   
	}
}
