/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 资源包处理器                                                                    *
*//************************************************************************************/

using UnityEngine;
using System;

namespace UFrame.AssetBundles
{
    public class AssetLoadOperation
    {
        public string assetBundleName { get; set; }
        public string assetName { get; set; }
        public Type assetType { get; internal set; }
        public MulticastDelegate onLoadAsset { get; set; }
        public Action<float> onProgress { get; set; }
        public bool autoUnload { get; set; }

        public Action<AssetLoadOperation> onRelease { get; set; }

        public void Clean()
        {
            assetBundleName = null;
            assetName = null;
            assetType = null;
            onLoadAsset = null;
            onProgress = null;
            autoUnload = false;
            onRelease = null;
        }

        public void OnBundleLoadCallBack(AssetBundle bundle,string err)
        {
            if(!string.IsNullOrEmpty(err))
            {
                Debug.LogError(err);
                return;
            }

            if (bundle != null)
            {
                if (onLoadAsset != null)
                {
                    var asset = bundle.LoadAsset(assetName, assetType);
                    onLoadAsset.DynamicInvoke(asset);
                }
            }

            if (onRelease != null)
            {
                onRelease.Invoke(this);
            }
        }
        public void OnProgressCallBack(float progress)
        {
            if(onProgress != null)
            {
                onProgress.Invoke(progress);
            }
        }

    }

}
