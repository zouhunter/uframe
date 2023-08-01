//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-11 18:25:59
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace UFrame.DressAB
{

    public interface IAssetBundleLoadCtrl
    {
        AsyncPreloadOperation StartPreload(ushort flags);
        AsyncPreloadOperation StartPreload(params string[] address);
        bool ExistAddress(string address);
        bool TryGetAddressGroup(string address, out string addressGroup, out string assetName);
        AsyncBundleOperation LoadAssetBundleAsync(string address, ushort flags);
        AsyncAssetOperation<T> LoadAssetAsync<T>(string address, ushort flags = 0) where T : UnityEngine.Object;
        AsyncAssetOperation<T> LoadAssetAsync<T>(string address, string assetname, ushort flags = 0) where T : UnityEngine.Object;
        AsyncAssetsOperation<T> LoadAssetsAsync<T>(string address, ushort flags = 0) where T : UnityEngine.Object;
        AsyncSceneOperation LoadSceneAsync(string address, string sceneName = null, ushort flags = 0, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single);
        void PreloadAssetBundle(BundleItem bundleItem, System.Action<string, object> onLoadBundle, HashSet<BundleItem> deepLoading);
    }
}