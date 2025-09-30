/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接绘制                                                                        *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;

namespace UFrame.NodeGraph
{
    public class ConnectionView
    {
        public DataModel.Connection target;
        protected Editor targetDrawer;

        public virtual int LineWidth { get { return 3; } }
        public virtual Color LineColor { get { return Color.white; } }

        public virtual void OnDrawLabel(Vector2 centerPos, string label)
        {
            if(!string.IsNullOrEmpty(label)){
                GUIStyle labelStyle = EditorStyles.miniTextField;// new GUIStyle("WhiteMiniLabel");
                labelStyle.alignment = TextAnchor.MiddleLeft;
                var labelWidth = labelStyle.CalcSize(new GUIContent(label));
                var labelPointV3 = new Vector2(centerPos.x - (labelWidth.x / 2), centerPos.y - 5);
                Handles.Label(labelPointV3, label, labelStyle);
            }
        }

        public virtual void OnConnectionGUI(Vector2 startV3, Vector2 endV3, Vector2 startTan, Vector2 endTan) { }
 
        public virtual void OnInspectorGUI()
        {
            if (target == null) return;

            if (targetDrawer == null)
                targetDrawer = Editor.CreateEditor(target);

            targetDrawer.DrawHeader();
            targetDrawer.OnInspectorGUI();
        }

        public virtual void OnContextMenuGUI(GenericMenu menu, ConnectionGUI connectionGUI) { }
    }

}