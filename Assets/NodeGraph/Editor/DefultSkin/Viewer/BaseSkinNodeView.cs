/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 默认皮肤                                                                        *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;

namespace UFrame.NodeGraph.DefultSkin
{
    public abstract class BaseSkinNodeView : NodeView
    {
        protected int lastSyle;
        protected GUISkin m_skin;
        protected GUIStyle m_activeStyle;
        protected GUIStyle m_inactiveStyle;

        public virtual int Style => 0;
        public virtual float CustomNodeHeight => 0;
        protected GUISkin skin
        {
            get
            {
                if(m_skin == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("75ce4a2b9ce8e45f9bcb12d38ed95952");
                    m_skin = AssetDatabase.LoadAssetAtPath<GUISkin>(path);
                    Debug.Assert(m_skin != null, "the guid of the skin is changed!");
                }
                return m_skin;
            }
        }
        public override GUIStyle ActiveStyle
        {
            get
            {
                if (Style != lastSyle)
                {
                    ResetStyle();
                }
                if (m_activeStyle == null)
                {
                    m_activeStyle = new GUIStyle(skin.FindStyle(string.Format("node {0} on", Style)));
                }
                return m_activeStyle;
            }
        }
        public override GUIStyle InactiveStyle
        {
            get
            {
                if (Style != lastSyle)
                    ResetStyle();

                if (m_inactiveStyle == null)
                    m_inactiveStyle = new GUIStyle(skin.FindStyle(string.Format("node {0}", Style)));

                return m_inactiveStyle;
            }
        }

        protected void ResetStyle()
        {
            m_activeStyle = null;
            m_inactiveStyle = null;
            lastSyle = Style;
        }
    }
}