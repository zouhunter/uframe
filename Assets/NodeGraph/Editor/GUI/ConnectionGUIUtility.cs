/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接绘制工具类                                                                  *
*//************************************************************************************/

using UnityEngine;
using System;

namespace UFrame.NodeGraph
{
    public class ConnectionGUIUtility
    {

        public static Action<ConnectionEvent> ConnectionEventHandler
        {
            get
            {
                return ConnectionGUISingleton.s.emitAction;
            }
            set
            {
                ConnectionGUISingleton.s.emitAction = value;
            }
        }

        public static Material HandleMaterial
        {
            get
            {
                if (ConnectionGUISingleton.s.handleMat == null)
                {
                    Shader shader = Shader.Find("Hidden/NodeGraph.DataModel/LineDraw");
                    Material m = new Material(shader);
                    m.hideFlags = HideFlags.HideAndDontSave;
                    ConnectionGUISingleton.s.handleMat = m;
                }
                return ConnectionGUISingleton.s.handleMat;
            }
        }

        private class ConnectionGUISingleton
        {
            public Action<ConnectionEvent> emitAction;
            public Material handleMat;

            private static ConnectionGUISingleton s_singleton;

            public static ConnectionGUISingleton s
            {
                get
                {
                    if (s_singleton == null)
                    {
                        s_singleton = new ConnectionGUISingleton();
                    }

                    return s_singleton;
                }
            }
        }
    }
}
