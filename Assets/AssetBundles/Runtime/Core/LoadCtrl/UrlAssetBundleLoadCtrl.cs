/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源包网络加载器                                                                *
*//************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace UFrame.AssetBundles
{
    public class UrlAssetBundleLoadCtrl : IBundleLoader
    {
        public string MenuName { get; set; }
        public UrlAssetBundleLoadCtrl(string urlPath, string menuName)
        {
            if (!urlPath.EndsWith("/"))
            {
                urlPath += "/";
            }
            BaseDownloadingURL = urlPath;
            this.MenuName = menuName;
        }
        public enum LogMode { All, JustErrors };
        public static LogMode m_LogMode = LogMode.All;
        private string m_BaseDownloadingURL = "";
        private string[] m_ActiveVariants = { };
        private string m_BundleAddress;
        private string m_CacheDir;
        private int m_AppendHashCode;
        private AssetBundleManifest m_AssetBundleManifest;
        private Dictionary<string, AssetBundleContent> m_LoadedAssetBundles = new Dictionary<string, AssetBundleContent>();
        private Dictionary<string, UnityWebRequest> m_DownloadingWWWs = new Dictionary<string, UnityWebRequest>();
        private Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();
        private List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();
        private Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        private Dictionary<string, float> m_DownLandProgress = new Dictionary<string, float>();
        protected Action<bool> m_onInitCallback;
        protected HashSet<string> m_delyRemove = new HashSet<string>();
        protected HashSet<string> m_delyUnload = new HashSet<string>();

        public bool Actived
        {
            get
            {
                return m_AssetBundleManifest != null;
            }
        }

        /// <summary>
        /// 设置初始化回调
        /// </summary>
        /// <param name="callback"></param>
        public void SetInitCallBack(Action<bool> callback)
        {
            m_onInitCallback = callback;
        }

        /// <summary>
        /// 获取资源加载进度
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public float GetBundleLoadProgress(string bundleName)
        {
            if (!m_Dependencies.TryGetValue(bundleName, out var dependencies) || dependencies == null || dependencies.Length <= 0)
            {
                if (m_DownLandProgress.ContainsKey(bundleName))
                {
                    return m_DownLandProgress[bundleName];
                }
            }
            else
            {
                int loadedCount = 0;
                foreach (var dependency in dependencies)
                {
                    if (!m_DownloadingErrors.TryGetValue(bundleName, out var error) && !m_LoadedAssetBundles.ContainsKey(dependency))
                        continue;
                    loadedCount++;
                }
                return loadedCount / (float)dependencies.Length;
            }
            return 0;
        }

        // The base downloading url which is used to generate the full downloading url with the assetBundle names.
        public string BaseDownloadingURL
        {
            get { return m_BaseDownloadingURL; }
            set { m_BaseDownloadingURL = value; }
        }

        // Variants which is used to define the active variants.
        public string[] ActiveVariants
        {
            get { return m_ActiveVariants; }
            set { m_ActiveVariants = value; }
        }

        public string BundleAddress
        {
            get { return m_BundleAddress; }
            set { m_BundleAddress = value; }
        }


        public int AppendHashLength
        {
            get { return m_AppendHashCode; }
            set { m_AppendHashCode = value; }
        }

        public string CacheDir
        {
            get { return m_CacheDir; }
            set { m_CacheDir = value; }
        }

        public static void SetFullLog(bool fullLog)
        {
            m_LogMode = fullLog ? LogMode.All : LogMode.JustErrors;
        }

        private void Log(string tag,string text = null)
        {
            if (m_LogMode == LogMode.All)
            {
                var logStr = "[AssetBundleLoader] " + tag;
                if (!string.IsNullOrEmpty(text))
                    logStr += text;
                Debug.Log(logStr);
            }
        }

        #region public Functions

        // Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
        public AssetBundleContent GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            //downland error
            if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
            {
                Log("empty content,downland error:" + error);
                return null;
            }

            AssetBundleContent bundleContent = null;
            if (!m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundleContent) || bundleContent == null)
            {
                return null;
            }

            // No dependencies are recorded, only the bundle itself is required.
            if (!m_Dependencies.TryGetValue(assetBundleName, out var dependencies))
                return bundleContent;

            int loadedCount = 0;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
                    return bundleContent;
                loadedCount++;
                // Wait all the dependent assetBundles being loaded.
                if (!m_LoadedAssetBundles.TryGetValue(dependency, out var dependentBundle) || dependentBundle == null)
                    return null;
            }
            return bundleContent;
        }

        public void Initialize()
        {
            if (m_AssetBundleManifest != null)
            {
                Debug.LogError("m_AssetBundleManifest != null");
                return;
            }
            LoadAssetBundle(MenuName, true);
            var operation = new AssetBundleLoadOperation(MenuName);
            operation.onRequested = OnMenuBundleLoad;
            m_InProgressOperations.Add(operation);
        }

        private void OnMenuBundleLoad(AssetBundle menuBundle, string error)
        {
            if (menuBundle != null)
            {
                this.m_AssetBundleManifest = menuBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                UnityEngine.Object.DontDestroyOnLoad(m_AssetBundleManifest);
            }

            var loadOk = menuBundle != null && m_AssetBundleManifest != null;

            menuBundle.Unload(false);

            if (m_onInitCallback != null)
            {
                m_onInitCallback.Invoke(loadOk);
            }
            else
            {
                Debug.LogError("OnMenuBundleLoad: failed!");
            }

        }

        public void UnloadAssetBundle(string assetBundleName, bool clearInstance = true)
        {
            UnloadAssetBundleInternal(assetBundleName, clearInstance);
            UnloadDependencies(assetBundleName, clearInstance);
        }

        #endregion

        // Load AssetBundle and its dependencies.
        void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
        {
            Log("Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);

            if (!isLoadingAssetBundleManifest)
            {
                if (m_AssetBundleManifest == null)
                {
                    Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleLoader.Initialize()");
                    return;
                }
            }
            bool isAlreadyProcessed = false;
           
            if(!isLoadingAssetBundleManifest)
            {
                var ab = LoadAssetBundleFromFile(assetBundleName);
                if (ab != null)
                {
                    isAlreadyProcessed = true;
                    m_LoadedAssetBundles.Add(assetBundleName, new AssetBundleContent(ab));
                }
            }

            if (!isAlreadyProcessed)
            {
                // Check if the assetBundle has already been processed.
                isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);
            }

            // Load dependencies.
            if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
                LoadDependencies(assetBundleName);
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_ActiveVariants, curSplit[1]);

                // If there is no active variant found. We still want to use the first 
                if (found == -1)
                    found = int.MaxValue - 1;

                if (found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFit == int.MaxValue - 1)
            {
                Log("Ambigious asset bundle variant chosen because there was no matching active variant: ", bundlesWithVariant[bestFitIndex]);
            }

            if (bestFitIndex != -1)
            {
                return bundlesWithVariant[bestFitIndex];
            }
            else
            {
                return assetBundleName;
            }
        }

        // Where we actuall call WWW to download the assetBundle.
        protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            // Already loaded.
            AssetBundleContent bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null)
            {
                bundle.m_ReferencedCount++;
                return true;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
            if (m_DownloadingWWWs.ContainsKey(assetBundleName))
                return true;

            string url;
            UnityWebRequest download;

            // For manifest AssetBundle, always download it as we don't have hash for it.
            if (isLoadingAssetBundleManifest)
            {
                url = m_BaseDownloadingURL + assetBundleName;
                download = UnityWebRequest.Get(url);
            }
            else
            {
                var bundleFileName = GetBundleFileName(assetBundleName);
                if (!string.IsNullOrEmpty(m_BundleAddress))
                    url = $"{m_BaseDownloadingURL}/{bundleFileName}.{m_BundleAddress}";
                else
                    url = $"{m_BaseDownloadingURL}/{bundleFileName}";

                if (!string.IsNullOrEmpty(m_CacheDir) && Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    var handler = new DownloadHandlerFile($"{m_CacheDir}/{bundleFileName}");
                    handler.removeFileOnAbort = true;
                    download = UnityWebRequest.Get(url);
                    download.downloadHandler = handler;
                }
                else
                {
                    download = UnityWebRequestAssetBundle.GetAssetBundle(url);
                }
            }

            m_DownloadingWWWs.Add(assetBundleName, download);
            Log("LoadAssetBundle:", url);
            var operation = download.SendWebRequest();
            if (operation.isDone)
            {
                Log("LoadAssetBundle isDone OnSendOnce!", url);
            }
            return false;
        }

        protected string GetBundleFileName(string assetBundleName)
        {
            var hashCode = m_AssetBundleManifest.GetAssetBundleHash(assetBundleName);
            string hashStr = null;
            if (m_AppendHashCode > 0)
            {
                hashStr = hashCode.ToString();
                hashStr = hashStr.Substring(hashStr.Length - m_AppendHashCode, m_AppendHashCode);
                assetBundleName = $"{assetBundleName}_{hashStr}";
            }
            else
                assetBundleName = $"{assetBundleName}";
            return assetBundleName;

        }

        protected virtual AssetBundle LoadAssetBundleFromFile(string assetBundleName)
        {
            var bundleFileName = GetBundleFileName(assetBundleName);
            string cacheFilePath = $"{m_CacheDir}/{bundleFileName}";
            if(System.IO.File.Exists(cacheFilePath))
            {
                return AssetBundle.LoadFromFile(cacheFilePath);
            }
            return null;
        }

        // Where we get all the dependencies and load them all.
        protected void LoadDependencies(string assetBundleName)
        {
            if (m_AssetBundleManifest == null)
            {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleLoader.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
                return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++)
                LoadAssetBundleInternal(dependencies[i], false);
        }


        protected void UnloadDependencies(string assetBundleName, bool clearInstance)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency, clearInstance);
            }

            m_Dependencies.Remove(assetBundleName);
        }

        protected void UnloadAssetBundleInternal(string assetBundleName, bool clearInstance)
        {
            if (m_DownloadingWWWs.TryGetValue(assetBundleName, out var request))
            {
                for (int i = 0; i < m_InProgressOperations.Count;)
                {
                    if (m_InProgressOperations[i].bundleName == assetBundleName)
                    {
                        m_InProgressOperations.RemoveAt(i);
                        Log("success unload AssetBundle from m_InProgressOperations:" + assetBundleName);
                    }
                    else
                    {
                        i++;
                    }
                }
                request.Abort();
                request.Dispose();
                m_DownloadingWWWs.Remove(assetBundleName);
                Log("success unload AssetBundle from m_DownloadingWWWs:" + assetBundleName);
            }

            m_DownLandProgress.Remove(assetBundleName);
            m_DownloadingErrors.Remove(assetBundleName);

            string error;
            AssetBundleContent bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if (bundle == null)
                return;

            if (--bundle.m_ReferencedCount <= 0)
            {
                if (bundle.m_AssetBundle)
                {
                    bundle.m_AssetBundle.Unload(false);
                    Log(assetBundleName + " has been unloaded successfully");
                }
                else
                {
                    Log("bundle already empty:" + assetBundleName);
                }
                bundle.m_AssetBundle = null;
                m_LoadedAssetBundles.Remove(assetBundleName);
            }
        }

        public void UpdateDownLand()
        {
            // Collect all the finished WWWs.
            m_delyRemove.Clear();
            m_delyUnload.Clear();
            foreach (var keyValue in m_DownloadingWWWs)
            {
                UnityWebRequest download = keyValue.Value;

                // If downloading fails.
                if (download.error != null)
                {
                    m_DownloadingErrors/*.Add(*/[keyValue.Key] = string.Format("Failed downloading bundle {0} from {1}: {2}", keyValue.Key, download.url, download.error);
                    m_delyRemove.Add(keyValue.Key);
                    continue;
                }

                m_DownLandProgress[keyValue.Key] = download.isDone ? 1 : download.downloadProgress;

                // If downloading succeeds.
                if (download.isDone)
                {
                    AssetBundle bundle = null;
                    if (download.downloadHandler is DownloadHandlerAssetBundle)
                    {
                        bundle = (download.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                    }
                    else if (download.downloadHandler is DownloadHandlerFile)
                    {
                        bundle = LoadAssetBundleFromFile(keyValue.Key);
                    }
                    else
                    {
                        byte[] downlandDatas = download.downloadHandler.data;
                        bundle = AssetBundle.LoadFromMemory(downlandDatas);
                    }

                    if (bundle == null)
                    {
                        Debug.LogError("bundle == null");
                        m_DownloadingErrors/*.Add(*/[keyValue.Key] = string.Format("{0} is not a valid asset bundle.", keyValue.Key);
                        m_delyRemove.Add(keyValue.Key);
                        continue;
                    }

                    //Debug.Log("Downloading " + keyValue.Key + " is done at frame " + Time.frameCount);
                    m_LoadedAssetBundles.Add(keyValue.Key, new AssetBundleContent(bundle));
                    m_delyRemove.Add(keyValue.Key);
                }
            }

            // Remove the finished WWWs.
            foreach (var key in m_delyRemove)
            {
                UnityWebRequest download = m_DownloadingWWWs[key];
                m_DownloadingWWWs.Remove(key);
                download.Dispose();
            }

            // Update all in progress operations
            for (int i = 0; i < m_InProgressOperations.Count && i >= 0;)
            {
                var operation = m_InProgressOperations[i];
                if (!operation.Update(this))
                {
                    m_InProgressOperations.RemoveAt(i);
                    if (operation.autoUnload)
                        m_delyUnload.Add(operation.bundleName);
                }
                else
                {
                    i++;
                }
            }

            //auto unload
            foreach (var key in m_delyUnload)
            {
                UnloadAssetBundle(key, false);
            }
        }

        // Load asset from the given assetBundle.
        public void LoadBundleAsync(string assetBundleName, Action<AssetBundle, string> loadCallBack, Action<float> progressCallBack = null, bool autoUnload = true)
        {
            Log("Loading " + assetBundleName);
            if (m_AssetBundleManifest == null)
            {
                Debug.LogError("m_AssetBundleManifest == null ,please init before load!");
                return;
            }
            assetBundleName = RemapVariantName(assetBundleName);
            LoadAssetBundle(assetBundleName);
            AssetBundleLoadOperation operation = new AssetBundleLoadOperation(assetBundleName);
            operation.onRequested = loadCallBack;
            operation.onProgress = progressCallBack;
            operation.autoUnload = autoUnload;
            m_InProgressOperations.Add(operation);
        }

        public void Dispose()
        {
            if (m_AssetBundleManifest)
            {
                m_AssetBundleManifest = null;
            }
            //释放所有加载的资源 
            foreach (var bundlePair in m_LoadedAssetBundles)
            {
                if (bundlePair.Value.m_AssetBundle)
                {
                    bundlePair.Value.m_AssetBundle.Unload(true);
                }
            }
            m_LoadedAssetBundles.Clear();
            foreach (var downloadingPair in m_DownloadingWWWs)
            {
                downloadingPair.Value.Dispose();
            }
            m_DownloadingWWWs.Clear();
            m_DownloadingErrors.Clear();
            m_InProgressOperations.Clear();
            m_Dependencies.Clear();
            m_DownLandProgress.Clear();
            Resources.UnloadUnusedAssets();
            Log("UrlAssetBundleLoadCtrl release!");
        }
    }

}