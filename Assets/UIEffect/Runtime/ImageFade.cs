/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 图片顶点渐变                                                                    *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace UFrame.UIEffect
{
    [AddComponentMenu("UFrame/UIEffect/ImageFade")]
    public class ImageFade : BaseMeshEffect
    {
        [SerializeField]
        public Color m_leftTopColor = Color.white;
        [SerializeField]
        public Color m_rightTopColor = Color.white;
        [SerializeField]
        public Color m_leftDownColor = Color.white;
        [SerializeField]
        public Color m_rightDownColor = Color.white;

        protected Color32[] m_colorArray;

        public override void ModifyMesh(VertexHelper toFill)
        {
            if (m_colorArray == null || m_colorArray.Length != 4)
                m_colorArray = new Color32[4];
            m_colorArray[0] = m_leftDownColor;
            m_colorArray[1] = m_leftTopColor;
            m_colorArray[2] = m_rightTopColor;
            m_colorArray[3] = m_rightDownColor;

            UIVertex vertex = new UIVertex();
            for (int i = 0; i < toFill.currentVertCount; i++)
            {
                toFill.PopulateUIVertex(ref vertex, i);

                vertex.color = m_colorArray[i];
                toFill.SetUIVertex(vertex, i);
            }

        }
    }
}

