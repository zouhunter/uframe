/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 渲染控制器                                                                      *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UFrame;

namespace UFrame.Rendering
{
    public class SimpleRenderingController : IRenderingController
    {
        /// <summary>
        /// 透明处理字典
        /// </summary>
        private Dictionary<int, RendererOperationController> transparentDic;
        private Material transprentMat;

        public bool Regsited { get; protected set; }

        public void Init()
        {
            transparentDic = new Dictionary<int, RendererOperationController>();
            transprentMat = CreateTransparentMaterial();
            Regsited = true;
        }

        public void Release()
        {
            using (var enumerator = transparentDic.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Recovery();
                }
            }
            transparentDic.Clear();
            transparentDic = null;
            Regsited = false;
        }

        #region 透明处理
        public void SetTransparentRatio(float ratio)
        {
            transprentMat.color = new Color(1, 1, 1, ratio);
        }
        public void TransparentTargets(params Renderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                var hashCode = renderer.GetHashCode();
                RendererOperationController operation;
                if (!transparentDic.TryGetValue(hashCode, out operation))
                {
                    operation = new RendererOperationController(renderer);
                    transparentDic.Add(hashCode, operation);
                }
                operation.WorpRenderers(transprentMat);
            }
        }
        public void RecoverTransparentedTargets(params Renderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                var hashCode = renderer.GetHashCode();
                RendererOperationController operation;

                if (transparentDic.TryGetValue(hashCode, out operation))
                {
                    operation.Recovery();
                }
            }
        }
        private Material CreateTransparentMaterial()
        {
            var material = new Material(Shader.Find("Standard"));
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            return material;
        }

        #endregion

    }
}