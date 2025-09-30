using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.BgTransparent
{
    public class RendererOperation
    {
        private Dictionary<Renderer, Material[]> m_OriginalMats;
        private MaterialWorp matWorp;

        public RendererOperation(Material worpMat, Renderer[] renderders, string colorName, string texName, string worpColorName, string worpTexName)
        {
            matWorp = new MaterialWorp(worpMat, colorName, texName,worpColorName, worpTexName);
            m_OriginalMats = new Dictionary<Renderer, Material[]>();
            foreach (var renderer in renderders)
            {
                m_OriginalMats.Add(renderer, renderer.materials);
            }
        }

        public void WorpRenderers()
        {
            foreach (var item in m_OriginalMats)
            {
                item.Key.materials = matWorp.Worp(item.Key.materials);
            }
        }

        public void Recovery()
        {
            foreach (var item in m_OriginalMats)
            {
                item.Key.materials = m_OriginalMats[item.Key];
            }
        }
    }
}