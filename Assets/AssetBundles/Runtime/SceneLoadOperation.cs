/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 场景包处理器                                                                    *
*//************************************************************************************/

using UnityEngine;
using System;

namespace UFrame.AssetBundles
{
    public class SceneLoadOperation
    {
        public string assetBundleName { get; set; }
        public string sceneName { get; set; }
        public bool addictive { get; set; }
        public bool autoUnload { get; set; }
        public AsyncOperation asyncOperation { get; set; }
        public Action<bool> onBundleComplete { get; set; }
        public AssetBundle assetBundle { get; set; }

        public Action<SceneLoadOperation> onRelease { get; set; }
        public Action<float> onBundleProgress { get; internal set; }
        public Action<AsyncOperation> onSceneProgress { get; internal set; }

        public void Clean()
        {
            assetBundleName = null;
            sceneName = null;
            addictive = false;
            autoUnload = false;
            asyncOperation = null;
            onBundleComplete = null;
            assetBundle = null;
            onRelease = null;
            onBundleProgress = null;
            onSceneProgress = null;
        }

        public void OnBundleProgressCallBack(float progress)
        {
            if(onBundleProgress != null)
            {
                onBundleProgress.Invoke(progress);
            }
        }

        public void OnBundleLoadCallBack(AssetBundle bundle,string err)
        {
            assetBundle = bundle;

            if (onBundleComplete != null)
            {
                onBundleComplete.Invoke(bundle != null);
            }

            if (onRelease != null)
            {
                onRelease.Invoke(this);
            }
        }
    }
}
