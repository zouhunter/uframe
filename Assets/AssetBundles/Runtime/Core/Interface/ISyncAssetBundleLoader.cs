/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本:                                                                              *
*  功能:                                                                              *
*   - 同步加载器接口                                                                  *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.AssetBundles
{
    public interface ISyncAssetBundleLoader: IBundleLoader
    {
        T[] LoadAssets<T>(string bundleName,bool autoUnload = true) where T : UnityEngine.Object;
        T LoadAsset<T>(string bundleName, string assetName,bool autoUnload = true) where T : UnityEngine.Object;
        AssetBundle LoadBundle(string bundleName);
        AsyncOperation LoadLevel(string bundleName, string assetName, UnityEngine.SceneManagement.LoadSceneMode loadModle,bool autoUnload=true);
        void LoadAssetAsync<T>(string bundleName, string assetName, UnityAction<T> OnAssetLoad, bool autoUnload = true) where T : UnityEngine.Object;
    }
}