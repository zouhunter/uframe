using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.BgTransparent
{
    /// <summary>
    /// 将贴图及颜色转换到另一个球
    /// </summary>
    public class MaterialWorp
    {
        private Dictionary<Material, Material> m_Materials = new Dictionary<Material, Material>();
        public Material worpMaterial { get; private set; }
        protected string m_colorName;
        protected string m_texName;
        protected string m_worpColorName;
        protected string m_worpTexName;

        public MaterialWorp(Material worpMaterial,string colorName,string texName,string worpColorName,string worpTexName)
        {
            this.worpMaterial = worpMaterial;
            this.m_colorName = colorName;
            this.m_texName = texName;
            this.m_worpColorName = worpColorName;
            this.m_worpTexName = worpTexName;
        }

        public Material Worp(Material mat)
        {
            if (!m_Materials.ContainsKey(mat))
            {
                Record(mat);
            }

            return m_Materials[mat];
        }
        public Material[] Worp(params Material[] mats)
        {
            var newMats = new Material[mats.Length];
            for (int i = 0; i < newMats.Length; i++)
            {
                if (!m_Materials.ContainsKey(mats[i]))
                {
                    Record(mats[i]);
                }
                newMats[i] = m_Materials[mats[i]];
            }
            return newMats;
        }

        private void Record(Material mat)
        {
            if (!m_Materials.ContainsKey(mat))
            {
                var newMat = new Material(worpMaterial);
                if(!string.IsNullOrEmpty(m_texName) &&!string.IsNullOrEmpty(m_worpTexName))
                {
                    var oldTex = mat.GetTexture(m_texName);
                    newMat.SetTexture(m_worpTexName, oldTex);
                }

                if(!string.IsNullOrEmpty(m_colorName) && !string.IsNullOrEmpty(m_worpColorName))
                {
                    var oldColor = mat.GetColor(m_colorName);
                    oldColor.a = worpMaterial.GetColor(m_worpColorName).a;
                    newMat.SetColor(m_worpColorName, oldColor);
                }

                m_Materials.Add(mat, newMat);
            }
        }
    }
}