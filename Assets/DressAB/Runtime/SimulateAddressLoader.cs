﻿//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace UFrame.DressAB
{
    public class SimulateAddressLoader
    {
        private AddressDefineObject m_defObj;
        private BindingFlags m_callMethodFlags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod;
        public SimulateAddressLoader()
        {
            m_defObj = AddressDefineObjectSetting.Instance.activeAddressDefineObject;
        }

        public bool ExistsAddress(string address)
        {
            if (m_defObj == null)
                return false;
            return m_defObj.addressList.Find(x => x.address == address) != null;
        }

        private string LocatAddressInfo(string address, ushort flags)
        {
            return LocatFile(address, null, flags);
        }

        private string LocatFile(string address, string assetName, ushort flags)
        {
            if (m_defObj == null)
                return null;

            var addressInfos = m_defObj.addressList.FindAll(x => x.address == address);
            if (flags != 0)
            {
                addressInfos = addressInfos.FindAll(x => (x.flags & flags) != 0);
            }
            if (!string.IsNullOrEmpty(assetName))
            {
                foreach (var info in addressInfos)
                {
                    var path = AssetDatabase.GUIDToAssetPath(info.guid);
                    if (System.IO.Directory.Exists(path))
                    {
                        var files = System.IO.Directory.GetFiles(path, "*");
                        foreach (var file in files)
                        {
                            if (file.EndsWith(".meta"))
                                continue;

                            var fileName = System.IO.Path.GetFileName(file);
                            var fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(file);
                            if (fileName == assetName || assetName == fileNameNoExt)
                            {
                                return System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file);
                            }
                        }
                    }
                }
            }
            if (addressInfos.Count > 0)
            {
                return AssetDatabase.GUIDToAssetPath(addressInfos[0].guid);
            }
            return null;
        }
        private List<AddressInfo> LocatAddressInfos(string address, ushort flags)
        {
            if (m_defObj == null)
                return null;
            var addressInfos = m_defObj.addressList.FindAll(x => x.address == address);
            if (flags != 0)
            {
                addressInfos = addressInfos.FindAll(x => (x.flags & flags) != 0);
            }
            return addressInfos;
        }

        public AsyncSceneOperation LoadSceneAsync(string address, ushort flags, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            var assetPath = LocatAddressInfo(address, flags);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                var operation = new AsyncSceneOperation(asset.name, loadSceneMode);
                if (System.Array.FindIndex(EditorBuildSettings.scenes, x => x.path == assetPath) < 0)
                {
                    var scene = new EditorBuildSettingsScene();
                    scene.enabled = true;
                    scene.path = assetPath;
                    var sceneList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                    sceneList.Add(scene);
                    EditorBuildSettings.scenes = sceneList.ToArray();
                }
                typeof(AsyncSceneOperation).InvokeMember("LoadScene", m_callMethodFlags, null, operation, new object[] { });
                return operation;
            }
            return null;
        }

        internal AsyncAssetsOperation<T> LoadAssetsAsync<T>(string address, ushort flags) where T : UnityEngine.Object
        {
            var addressInfos = LocatAddressInfos(address, flags);
            if (addressInfos != null)
            {
                List<T> objs = new List<T>();
                foreach (var item in addressInfos)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(item.guid);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                        if (asset)
                        {
                            if(!(asset is GameObject))
                            {
                                asset = Object.Instantiate(asset);
                            }
                            objs.Add(asset);
                        }
                    }
                }

                var operation = new AsyncAssetsOperation<T>();
                typeof(AsyncAssetsOperation<T>).InvokeMember("SetAsset", m_callMethodFlags, null, operation, new object[] { objs.ToArray() });
                return operation;
            }
            return null;
        }

        internal AsyncAssetOperation<T> LoadAssetAsync<T>(string address, string assetName, ushort flags) where T : UnityEngine.Object
        {
            var assetPath = LocatFile(address, assetName, flags);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset)
                {
                    if (!(asset is GameObject))
                    {
                        asset = Object.Instantiate(asset);
                    }
                    var operation = new AsyncAssetOperation<T>();
                    typeof(AsyncAssetOperation<T>).InvokeMember("SetAsset", m_callMethodFlags, null, operation, new object[] { asset });
                    return operation;
                }
            }
            UnityEngine.Debug.LogError($"not find addressInfo:{address} asssetName:{assetName}");
            return null;
        }
    }
}
#endif

