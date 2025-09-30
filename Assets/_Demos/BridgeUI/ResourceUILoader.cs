//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-08-15
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;

namespace UFrame {
    //[CreateAssetMenu]
    public class ResourceUILoader : UFrame.BridgeUI.UILoader
    {
        public override void InitEnviroment()
        {
            
        }

        public override void LoadAssetAsync(string assetBundleName, string assetName, Action<GameObject> onLoad)
        {
            var prefab = Resources.Load<GameObject>(assetBundleName);
            onLoad?.Invoke(prefab);
        }

        public override void Release(string assetName)
        {
            
        }
    }
}

