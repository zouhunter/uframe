/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源管理器接口                                                                  *
*//************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Events;

namespace UFrame.AssetBundles
{
    public interface IBundleLoader : IDisposable
    {
        bool Actived { get; }
        void SetInitCallBack(Action<bool> callback);
        void LoadBundleAsync(string assetBundleName, Action<AssetBundle,string> onAssetLoad, Action<float> onProgress = null,bool autoUnload = true);
        void UnloadAssetBundle(string assetBundleName,bool clearInstance = true);
        void UpdateDownLand();
    }
}