using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using UFrame.NodeGraph;
using UFrame.BridgeUI;

namespace UFrame.BridgeUI.Editors
{
    [CustomNodeViewAttribute(typeof(Graph.BridgeConnection))]
    public class BridgeConnectionView:ConnectionView
    {
        private Graph.BridgeConnection connecton;
        public static Graph.BridgeConnection copyed;

        private static GUIContent _endContent;
        private static GUIContent endContent
        {
            get
            {
                if (_endContent == null)
                {
#if UNITY_5_6
                    _endContent = EditorGUIUtility.IconContent("varpin tooltip@2x");
#elif UNITY_5_3_4
                    _endContent = EditorGUIUtility.IconContent("ChannelStripAttenuationMarker3");
#else
                    _endContent = EditorGUIUtility.IconContent("WarningMessageOverlay");
#endif
                }
                return _endContent;
            }
        }

        protected string Label
        {
            get
            {
                if (connecton == null && target != null)
                    connecton = target as Graph.BridgeConnection;
                var str = connecton != null ? BridgeEditorUtility.ShowModelToString(connecton.show) : "";
                return str;
            }
        }
        public override Color LineColor
        {
            get { return connecton == null ? Color.blue:(connecton.blocking?Color.red:Color.yellow); }
        }
        public override void OnDrawLabel(Vector2 centerPos, string label)
        {
            base.OnDrawLabel(centerPos, Label);
        }
        public override void OnConnectionGUI(Vector2 startV3, Vector2 endV3, Vector2 startTan, Vector2 endTan)
        {
            base.OnConnectionGUI(startV3, endV3, startTan, endTan);
            //var quater = Quaternion.Euler(endTan);
            //quater.SetLookRotation(startTan);
            //Handles.Label(endV3 + contentPos, endContent);//
        }
        public override void OnContextMenuGUI(GenericMenu menu, ConnectionGUI connectionGUI)
        {
            base.OnContextMenuGUI(menu, connectionGUI);
            menu.AddItem(
                      new GUIContent("Copy"),
                      false,
                      () =>
                      {
                          copyed = connecton;
                      }
                  );
            menu.AddItem(
                    new GUIContent("Paste"),
                    false,
                    () =>
                    {
                        if (copyed != null)
                        {
                            connecton.show = copyed.show;
                        }
                    }
                );
        }
    }

}
