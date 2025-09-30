/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 资源包加载器模板                                                                *
*//************************************************************************************/

using System;
using UnityEngine;

namespace UFrame.BridgeUI
{
    public abstract class UILoader : ScriptableObject
    {
        public abstract void InitEnviroment();
        public abstract void LoadAssetAsync(string assetBundleName, string assetName, Action<GameObject> onLoad);
        public abstract void Release(string assetName);
    }
}