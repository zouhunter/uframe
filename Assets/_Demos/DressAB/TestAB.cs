//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-22
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UFrame.AssetBundles;
using System;

namespace UFrame.Tests
{

    public class TestAB : MonoBehaviour
    {
        UrlAssetBundleLoadCtrl loader;
        private void Start()
        {
            var url = $"file://{Application.dataPath.Replace("Assets","AssetBundles")}/Output";
             loader = new UrlAssetBundleLoadCtrl(url, "StandaloneWindows.txt");
            loader.SetInitCallBack(OnInitFinish);
            loader.ActiveVariants = new string[] { "h"};
            loader.AppendHashLength = 32;
            loader.BundleAddress = "bundle";
            loader.CacheDir = Application.persistentDataPath;
            loader.Initialize();
        }

        private void OnInitFinish(bool ok)
        {
            Debug.Log("init finish");
            loader.LoadBundleAsync("testbundle", (bundle, error) =>
            {
                Debug.LogError("loadbundle:" + bundle);
            });
        }

        private void Update()
        {
            loader.UpdateDownLand();
        }
    }
}

