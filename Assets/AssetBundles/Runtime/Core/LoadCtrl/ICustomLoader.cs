/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源包模拟加载器                                                                *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UFrame.AssetBundles
{
    public interface ICustomLoader
    {
        T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object;
        T[] LoadAssets<T>(string bundleName, params string[] assetNames) where T : UnityEngine.Object;
        T[] LoadAssets<T>(string bundleName) where T : UnityEngine.Object;
        void LoadSceneAsync(string bundleName, string sceneName, bool isAddictive, System.Action<AsyncOperation> onProgressChanged);
        AsyncOperation LoadLevel(string bundleName, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadModle);
        void LoadAssetAsync<T>(string bundleName, string assetName, System.Action<T> onAssetLoad, System.Action<float> onProgress) where T : UnityEngine.Object;
        void UpdateDownLand();
        void Dispose();
    }
}