/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接绘制                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.NodeGraph
{
    /*
	 * ScriptableObject helper object to let ConnectionGUI edit from Inspector
	 */
    public class ConnectionGUIInspectorHelper : ScriptableObject
    {
        public ConnectionGUI connectionGUI;

        public void UpdateInspector(ConnectionGUI con)
        {
            this.connectionGUI = con;
        }
    }
}
