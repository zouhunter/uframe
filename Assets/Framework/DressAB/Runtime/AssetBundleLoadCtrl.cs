//*************************************************************************************
//* 作    者： 
//* 创建时间： 2023-05-11 18:25:59
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace UFrame.DressAB
{
    public class AssetBundleLoadCtrl
    {
        public DownloadFileEvent downloadFileFunc { get; private set; }
        public DownloadTextEvent downloadTxtFunc { get; private set; }
        public BundleReleaseEvent bundleReleaseFunc { get; set; }
        public string cacheFolder { get; private set; }
        public bool simulateInEditor { get; set; }
        public ref string assetBundleExt => ref m_bundleExt;
        public bool syncLoad = false;
        public bool logActive = true;
        private CatlogCheckUpdater m_updateChecker;
        private CatlogParser m_catlogParser;
        private string m_bundleExt = "bundle";
        private string m_remoteUrl { get; set; }
        private Dictionary<string, AssetBundle> m_loadedAssetBundles;
        private HashSet<string> m_downloadingBundles;
        private Dictionary<string, List<Action<string, AssetBundle>>> m_loadBundleCallBacks;
        private Dictionary<string, List<BundleItem>> m_bundleRefMap;//ref by
        private Dictionary<string, AsyncBundleOperation> m_asyncBundleOperationMap;
#if UNITY_EDITOR
        public SimulateAddressLoader simulateLoader = new SimulateAddressLoader();
#endif
        private HashSet<BundleItem> m_deeploading = new HashSet<BundleItem>();

        public AssetBundleLoadCtrl(DownloadFileEvent downloadFileFunc, DownloadTextEvent downloadTextFunc, string cacheFolder)
        {
            this.downloadFileFunc = downloadFileFunc;
            this.downloadTxtFunc = downloadTextFunc;
            this.cacheFolder = cacheFolder;
            this.m_updateChecker = new CatlogCheckUpdater(downloadFileFunc, downloadTxtFunc, cacheFolder);
            this.m_loadedAssetBundles = new Dictionary<string, AssetBundle>();
            this.m_loadBundleCallBacks = new Dictionary<string, List<Action<string, AssetBundle>>>();
            this.m_bundleRefMap = new Dictionary<string, List<BundleItem>>();
            this.m_downloadingBundles = new HashSet<string>();
            this.m_asyncBundleOperationMap = new Dictionary<string, AsyncBundleOperation>();
        }

        public AsyncCatlogOperation LoadCatlogAsync(string url)
        {
            this.m_remoteUrl = url;
            var operation = new AsyncCatlogOperation();

#if UNITY_EDITOR
            if (simulateInEditor)
            {
                operation.SetCatlogPath(url);
                return operation;
            }
#endif
            operation.RegistComplete(OnCatlogLoadFinish);
            m_updateChecker.StartCheckUpdate(url, operation);
            return operation;
        }

        private void OnCatlogLoadFinish(AsyncCatlogOperation operation)
        {
            m_catlogParser = new CatlogParser();
            m_catlogParser.LoadFromFile(operation.catlogPath);
        }

        private void OnLoadSubAssetBundle(string bundleName, AssetBundle assetBundle)
        {
            if (m_bundleRefMap.TryGetValue(bundleName, out var parentList))
            {
                for (int i = 0; i < parentList.Count; i++)
                {
                    var parentBundle = parentList[i];
                    if (m_loadedAssetBundles.TryGetValue(parentBundle.bundleName, out var bundle) && bundle)
                        continue;
                    RestartBundleLoad(parentBundle);
                }
            }
        }

        private bool DoLoadAssetBundle(BundleItem bundleItem, Action<float> onProgress, Action<string, AssetBundle> onLoadFinish)
        {
            var bundleName = bundleItem.bundleName;
            if (m_loadedAssetBundles.TryGetValue(bundleName, out var assetBundle) && assetBundle)
            {
                onLoadFinish?.Invoke(bundleName, assetBundle);
                return true;
            }
            else
            {
                var fullName = $"{bundleName}.{m_bundleExt}";
                var splitIndex = bundleName.LastIndexOf("_");
                var shortName = bundleName.Substring(0, splitIndex);
                var fileHash = bundleName.Substring(splitIndex + 1);
                var localFile = $"{cacheFolder}/{shortName}";
                if (m_downloadingBundles.Contains(localFile) || m_downloadingBundles.Contains(bundleName))
                    return false;

                RegistLoadBundleCallBack(bundleName, onLoadFinish);
                bool subFinished = true;
                if (bundleItem.refs != null && bundleItem.refs.Length > 0)
                {
                    foreach (var refItem in bundleItem.refs)
                    {
                        if (!m_bundleRefMap.TryGetValue(refItem.bundleName, out var parentList))
                            parentList = m_bundleRefMap[refItem.bundleName] = new List<BundleItem>();
                        if (!parentList.Contains(bundleItem))
                            parentList.Add(bundleItem);

                        if (!m_loadedAssetBundles.TryGetValue(refItem.bundleName, out var bundle))
                        {
                            m_deeploading.Clear();
                            var boolIsLoopUse = CheckIsParentBundle(bundleItem, refItem, m_deeploading);
                            if (!boolIsLoopUse)
                                subFinished &= DoLoadAssetBundle(refItem, null, OnLoadSubAssetBundle);
                            else
                                DebugInfo($"loop use cuse:{refItem.bundleName},{bundleItem.bundleName}");
                        }
                    }
                }
                if (subFinished)
                {
                    var fileInfo = new System.IO.FileInfo(localFile);
                    if (fileInfo.Exists && HashMd5File(localFile, fileHash.Length) == fileHash)
                    {
                        OnLoadBundleFromFile(localFile, bundleItem);
                    }
                    else
                    {
                        if (fileInfo.Exists)
                            System.IO.File.Delete(localFile);
                        m_downloadingBundles.Add(localFile);
                        var remoteFile = $"{m_remoteUrl}/{fullName}";
                        downloadFileFunc?.Invoke(remoteFile, localFile, OnLoadBundleFromFile, onProgress, bundleItem);
                    }
                }
                else
                {
                    var fileInfo = new System.IO.FileInfo(localFile);
                    if (!fileInfo.Exists && !m_downloadingBundles.Contains(localFile))
                    {
                        m_downloadingBundles.Add(localFile);
                        var remoteFile = $"{m_remoteUrl}/{fullName}";
                        downloadFileFunc?.Invoke(remoteFile, localFile, OnDownLoadAssetBundleToFile, onProgress, bundleItem);
                    }
                }
                return false;
            }
        }

        public static string HashMd5File(string filePath, int length)
        {
            using (var md = MD5.Create())
            {
                using (var mem = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    byte[] hash = md.ComputeHash(mem);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < length / 2; i++)
                        sb.Append(hash[i].ToString("x2"));
                    return sb.ToString();
                }
            }
        }

        private bool CheckIsParentBundle(BundleItem me, BundleItem other, HashSet<BundleItem> deepLoading)
        {
            if (deepLoading.Contains(other))
                return false;

            if (other.refs == null)
                return false;

            deepLoading.Add(other);

            foreach (var child in other.refs)
            {
                if (child == me)
                    return true;

                if (CheckIsParentBundle(me, child, deepLoading))
                    return true;
            }
            return false;
        }

        private void RegistLoadBundleCallBack(string bundleName, Action<string, AssetBundle> callBack)
        {
            if (m_loadBundleCallBacks.TryGetValue(bundleName, out var callbacks))
            {
                if (!callbacks.Contains(callBack))
                    callbacks.Add(callBack);
            }
            else
            {
                m_loadBundleCallBacks[bundleName] = new List<Action<string, AssetBundle>>() { callBack };
            }
        }

        private void DoUnloadAssetBundle(BundleItem bundleItem, bool unloadAll)
        {
            if (m_loadedAssetBundles.TryGetValue(bundleItem.bundleName, out var bundle))
            {
                if (bundleReleaseFunc != null && !bundleReleaseFunc.Invoke(bundleItem, bundle))
                {
                    DebugInfo("ignore bundle unload:" + bundleItem.bundleName);
                    return;
                }
                m_asyncBundleOperationMap.Remove(bundleItem.bundleName);
                DebugInfo("unload bundle:" + bundleItem.bundleName);
                bundle?.Unload(unloadAll);
                m_loadedAssetBundles.Remove(bundleItem.bundleName);
            }

            if (bundleItem.refs != null && bundleItem.refs.Length > 0)
            {
                foreach (var refBundle in bundleItem.refs)
                {
                    if (this.m_bundleRefMap.TryGetValue(refBundle.bundleName, out var parentBundles))
                    {
                        parentBundles.Remove(bundleItem);
                        if (parentBundles.Count == 0)
                        {
                            if (!m_asyncBundleOperationMap.TryGetValue(refBundle.bundleName, out var subBundleHandle) || !subBundleHandle.InUse)
                            {
                                DoUnloadAssetBundle(refBundle, unloadAll);
                            }
                        }
                    }
                }
            }
        }

        private void OnDownLoadAssetBundleToFile(string filePath, object content)
        {
            if (System.IO.File.Exists(filePath))
            {
                m_downloadingBundles.Remove(filePath);
                RestartBundleLoad((BundleItem)content);
            }
        }

        private void RestartBundleLoad(BundleItem bundleItem)
        {
            if (m_loadBundleCallBacks.TryGetValue(bundleItem.bundleName, out var callbacks))
            {
                for (int cid = 0; cid < callbacks.Count; cid++)
                {
                    var callback = callbacks[cid];
                    DoLoadAssetBundle(bundleItem, null, callback);
                }
            }
        }

        private void OnLoadBundleFromFile(string filePath, object content)
        {
            var bundleItem = (BundleItem)content;
            m_downloadingBundles.Remove(filePath);
            var bundleName = bundleItem.bundleName;
            if (!m_downloadingBundles.Contains(bundleName) && (!m_loadedAssetBundles.TryGetValue(bundleName, out var ab) || !ab))
            {
                m_downloadingBundles.Add(bundleName);
                DebugInfo("load bundle from file:" + bundleName);
                AsyncFileBundleOperation asyncFileBundle = new AsyncFileBundleOperation(filePath, bundleItem, syncLoad);
                asyncFileBundle.RegistABLoadComplate(OnLoadBundleFinish);
                asyncFileBundle.StartRequest();
            }
        }

        private void OnLoadBundleFinish(BundleItem bundleItem, string filePath, AssetBundle assetBundle)
        {
            Debug.Log("bundle finish:" + bundleItem.bundleName);
            var bundleName = bundleItem.bundleName;
            if (assetBundle == null)
            {
                m_downloadingBundles.Remove(filePath);
                m_downloadingBundles.Remove(bundleName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                RestartBundleLoad(bundleItem);
                return;
            }
            m_downloadingBundles.Remove(bundleName);
            m_loadedAssetBundles[bundleName] = assetBundle;
            if (m_loadBundleCallBacks.TryGetValue(bundleName, out var callbacks))
            {
                for (int i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    try
                    {
                        callback?.Invoke(bundleName, assetBundle);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                m_loadBundleCallBacks.Remove(bundleName);
            }
        }

        public bool ExistAddress(string address)
        {
#if UNITY_EDITOR
            if (simulateInEditor)
            {
                return simulateLoader.ExistsAddress(address);
            }
#endif

            return m_catlogParser?.ExistAddress(address) ?? false;
        }

        public bool TryGetAddressGroup(string address, out string addressGroup, out string assetName)
        {
            addressGroup = null;
            assetName = null;
            if (ExistAddress(address))
            {
                addressGroup = address;
                return true;
            }
            var addressTemp = address;
            while (!string.IsNullOrEmpty(addressTemp))
            {
                var index = addressTemp.LastIndexOf('/');
                if (index <= 0)
                    break;

                var group = address.Substring(0, index);
                if (ExistAddress(group))
                {
                    addressGroup = group;
                    assetName = address.Substring(index + 1);
                    break;
                }
                else
                {
                    addressTemp = group;
                }
            }
            return !string.IsNullOrEmpty(addressGroup);
        }

        public AsyncPreloadOperation StartPreload(ushort flags)
        {
            Debug.Log("StartPreload:" + flags);
#if UNITY_EDITOR
            if (simulateInEditor)
            {
                return new AsyncPreloadOperation(0);
            }
#endif
            HashSet<BundleItem> preloadBundles = new HashSet<BundleItem>();
            var preloads = m_catlogParser?.GetAddressByFlags(flags);
            for (int i = 0; i < preloads.Count; i++)
            {
                var abItem = preloads[i];
                while (abItem != null && abItem.bundleItem != null)
                {
                    preloadBundles.Add(abItem.bundleItem);
                    abItem = abItem.next;
                }
            }
            var operation = new AsyncPreloadOperation(preloadBundles.Count);
            if (preloadBundles.Count > 0)
            {
                m_deeploading.Clear();
                foreach (var abItem in preloadBundles)
                {
                    PreloadAssetBundle(abItem, operation.SetAssetBundle, m_deeploading);
                }
            }
            return operation;
        }


        public AsyncPreloadOperation StartPreload(params string[] address)
        {
#if UNITY_EDITOR
            if (simulateInEditor)
            {
                return new AsyncPreloadOperation(0);
            }
#endif
            HashSet<BundleItem> preloadBundles = new HashSet<BundleItem>();
            for (int i = 0; i < address.Length; i++)
            {
                var abItem = m_catlogParser?.FindAddressItem(address[i]);
                while (abItem != null && abItem.bundleItem != null)
                {
                    preloadBundles.Add(abItem.bundleItem);
                    abItem = abItem.next;
                }
            }
            var operation = new AsyncPreloadOperation(preloadBundles.Count);
            if (preloadBundles.Count > 0)
            {
                m_deeploading.Clear();
                foreach (var abItem in preloadBundles)
                {
                    PreloadAssetBundle(abItem, operation.SetAssetBundle, m_deeploading);
                }
            }
            return operation;

        }

        public AsyncBundleOperation LoadAssetBundleAsync(string address, ushort flags)
        {
            var abItem = m_catlogParser?.FindABItem(address, flags);
            if (abItem != null && abItem.bundleItem != null)
            {
                var bundleItem = abItem.bundleItem;
                if (!m_asyncBundleOperationMap.TryGetValue(bundleItem.bundleName, out var operation))
                {
                    operation = new AsyncBundleOperation(bundleItem);
                    operation.RegistUnloadBundle(DoUnloadAssetBundle);
                    DoLoadAssetBundle(abItem.bundleItem, operation.SetProgress, operation.SetAssetBundle);
                    m_asyncBundleOperationMap[bundleItem.bundleName] = operation;
                    return operation;
                }
                return operation;
            }
            else
            {
                DebugInfo($"address: {address}, not exist in catlog! abItem != null:{abItem != null}");
                return null;
            }
        }

        public AsyncAssetOperation<T> LoadAssetAsync<T>(string address, ushort flags = 0) where T : UnityEngine.Object
        {
            var assetName = address;
            var index = address.LastIndexOf('/');
            if (index >= 0)
                assetName = address.Substring(index + 1);
            return LoadAssetAsync<T>(address, assetName, flags);
        }

        public AsyncAssetOperation<T> LoadAssetAsync<T>(string address, string assetname, ushort flags = 0) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (simulateInEditor)
            {
                return simulateLoader.LoadAssetAsync<T>(address, assetname, flags);
            }
#endif
            var operation = LoadAssetBundleAsync(address, flags);
            if (operation != null)
            {
                var assetOperation = new AsyncAssetOperation<T>(assetname);
                operation.RegistComplete(assetOperation.SetAssetBundle);
                return assetOperation;
            }
            return null;
        }

        public AsyncAssetsOperation<T> LoadAssetsAsync<T>(string address, ushort flags = 0) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (simulateInEditor)
            {
                return simulateLoader.LoadAssetsAsync<T>(address, flags);
            }
#endif
            var operation = LoadAssetBundleAsync(address, flags);
            if (operation != null)
            {
                var assetOperation = new AsyncAssetsOperation<T>();
                operation.RegistComplete(assetOperation.SetAssetBundle);
                return assetOperation;
            }
            return null;
        }

        public AsyncSceneOperation LoadSceneAsync(string address, string sceneName = null, ushort flags = 0, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
#if UNITY_EDITOR
            if (simulateInEditor)
            {
                return simulateLoader.LoadSceneAsync(address, flags, loadSceneMode);
            }
#endif
            var operation = LoadAssetBundleAsync(address, flags);
            if (operation != null)
            {
                var assetOperation = new AsyncSceneOperation(sceneName, loadSceneMode);
                operation.RegistComplete(assetOperation.SetAssetBundle);
                operation.RegistProgress(assetOperation.SetProgress);
                return assetOperation;
            }
            return null;
        }

        private void PreloadAssetBundle(BundleItem bundleItem, System.Action<string, object> onLoadBundle, HashSet<BundleItem> deepLoading)
        {
            if (deepLoading.Contains(bundleItem))
                return;
            Debug.Log("preload assetbundle:" + bundleItem.bundleName);
            deepLoading.Add(bundleItem);

            if (bundleItem.refs != null && bundleItem.refs.Length > 0)
            {
                foreach (var refItem in bundleItem.refs)
                {
                    PreloadAssetBundle(refItem, onLoadBundle, deepLoading);
                }
            }
            var fullName = $"{bundleItem.bundleName}.{m_bundleExt}";
            var bundleName = bundleItem.bundleName;
            var splitIndex = bundleName.LastIndexOf("_");
            var shortName = bundleName.Substring(0, splitIndex);
            var localFile = $"{cacheFolder}/{shortName}";
            var fileInfo = new System.IO.FileInfo(localFile);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                var remoteFile = $"{m_remoteUrl}/{fullName}";
                Debug.Log("PreloadAssetBundle downloading:" + bundleItem.bundleName);
                downloadFileFunc?.Invoke(remoteFile, localFile, onLoadBundle, null, null);
            }
            else
            {
                Debug.Log("PreloadAssetBundle finished:" + bundleItem.bundleName);
                onLoadBundle?.Invoke(localFile, null);
            }
        }

        private void DebugInfo(string message)
        {
            if (!logActive)
                return;
            UnityEngine.Debug.Log("[AssetBundleLoadCtrl]" + message);
        }
    }
}