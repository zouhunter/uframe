/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源包容器                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.AssetBundles
{
    public class AssetBundleContent
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public AssetBundleContent(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }
}