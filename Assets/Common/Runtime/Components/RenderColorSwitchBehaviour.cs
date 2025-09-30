/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 颜色组切换                                                                      *
*//************************************************************************************/

using System.Collections.Generic;

using UnityEngine;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/RenderColorSwitchBehaviour")]
    public class RenderColorSwitchBehaviour : MonoBehaviour
    {
        [System.Serializable]
        public class MeshColorGroup
        {
            public MeshRenderer meshRender;
            public ColorGroup[] colorGroup;
        }

        [System.Serializable]
        public class ColorGroup
        {
            public int id;
            public Color color;
            public ColorGroup()
            {
                color = Color.white;
            }
        }

        [SerializeField]
        public List<MeshRenderer> m_meshRenders;
        [SerializeField]
        protected List<ColorGroup> m_colorGroup;
        protected List<Material> m_currentMats = new List<Material>();

        public void OnRenderSwtich(int id)
        {
            for (int k = 0; k < m_colorGroup.Count; k++)
            {
                var colorGroup = m_colorGroup[k];
                if (colorGroup.id == id)
                {
                    m_currentMats.Clear();
                    foreach (var meshRender in m_meshRenders)
                    {
                        meshRender.GetMaterials(m_currentMats);
                        for (int i = 0; i < m_currentMats.Count; i++)
                        {
                            if (m_currentMats[i].HasProperty("_Color"))
                            {
                                m_currentMats[i].SetColor("_Color", colorGroup.color);
                            }
                        }
                    }
                }
            }
        }
    }
}