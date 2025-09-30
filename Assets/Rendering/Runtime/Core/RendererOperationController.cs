/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 材质切换器                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Rendering
{
    public class RendererOperationController
    {
        private bool worped;
        private Renderer m_renderer;
        private Material[] m_originalMats;
        private Material[] m_worpedMats;

        public RendererOperationController(Renderer renderder)
        {
            m_renderer = renderder;
            m_originalMats = m_renderer.materials;
            worped = false;
        }

        public void WorpRenderers(Material worpTempateMat)
        {
            if(!worped)
            {
                worped = true;
                m_renderer.materials = WorpMaterials(worpTempateMat,m_originalMats);
            }
            else
            {
                for (int i = 0; i < m_worpedMats.Length; i++)
                {
                    var color = m_worpedMats[i].color;
                    color.a = worpTempateMat.color.a;
                    m_worpedMats[i].color = color;
                }
            }
           
        }

        public void Recovery()
        {
            if (worped)
            {
                worped = false;
                m_renderer.materials = m_originalMats;
            }
        }

        private Material[] WorpMaterials(Material worpTemplateMat,params Material[] mats)
        {
            if(m_worpedMats == null)
            {
                m_worpedMats = new Material[mats.Length];
                for (int i = 0; i < m_worpedMats.Length; i++)
                {
                    m_worpedMats[i] = CopyMaterialProperty(worpTemplateMat, mats[i]);
                }
            }
            return m_worpedMats;
        }

        private Material CopyMaterialProperty(Material worpTemplateMat, Material mat)
        {
            var newMat = new Material(worpTemplateMat);
            var oldTex = mat.GetTexture("_MainTex");
            var oldColor = mat.GetColor("_Color");
            oldColor.a = worpTemplateMat.color.a;
            newMat.SetTexture("_MainTex", oldTex);
            newMat.SetColor("_MainColor", oldColor);
            return newMat;
        }
    }
}