/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 资源加载控制器                                                                  *
*//************************************************************************************/

using System;
using UFrame;

using UnityEngine;

namespace UFrame.AssetBundles
{
    public interface IAssetBundleController : ILateUpdate
    {
        T LoadAssetSync<T>(string assetBundleName, string assetName,bool autoUnload = true) where T : UnityEngine.Object;
        void LoadAssetAsync<T>(string assetBundleName, string assetName, Action<T> onAssetLoad, Action<float> onProgress = null,bool autoUnload=true) where T : UnityEngine.Object;
        bool LoadBundleSync(string assetBundleName,Action<AssetBundle> onLoad,bool autoUnload = true);
        void LoadSceneBundleAsync(string assetBundleName, Action<bool> onComplete, Action<float> onProgress = null, bool autoUnload = true);
        void LoadSceneAsync(string assetBundleName, string sceneName,bool addictive, Action<float> onBundleProgress = null, Action<AsyncOperation> onSceneProgress = null, bool autoUnload = true);
        bool LoadSceneSync(string assetBundleName, string sceneName,bool addictive, Action<AsyncOperation> onSceneProgress = null);
        void LoadBundleAsync(string assetBundleName, Action<AssetBundle, string> callBack, Action<float> progressCallBack = null, bool autoUnload = true);
        void UnloadAssetBundle(string assetBundleName,bool clearInstance = true);
    }
}