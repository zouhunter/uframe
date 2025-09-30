using System.Collections.Generic;
/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 本地资源包加载器                                                                *
*//************************************************************************************/
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace UFrame.AssetBundles
{
    public class LocalAssetLoader : IBundleLoader, ISyncAssetBundleLoader
    {
        protected Dictionary<string, System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>> m_requests = new Dictionary<string, System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>>();
        protected Dictionary<string, AssetBundle> m_loadedBundle = new Dictionary<string, AssetBundle>();
        protected Dictionary<string, List<string>> m_bundleRef = new Dictionary<string, List<string>>();
        protected List<System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>> m_waitingSubs = new List<System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>>();
        protected HashSet<string> m_rootBundleNames = new HashSet<string>();
        protected Dictionary<string,float> m_delyUnloadBundles = new Dictionary<string,float>();
        protected string m_folder;
        protected string m_menu;
        protected System.Action<bool> m_onInitCallback;
        public bool Actived { get; protected set; }
        protected AssetBundleManifest m_menifest;

        public LocalAssetLoader(string folder, string menuName)
        {
            m_folder = folder;
            m_menu = menuName;
        }

        public void Initialize()
        {
            if (Actived)
                return;

            if (System.IO.Directory.Exists(m_folder))
            {
                Actived = true;
            }
            else
            {
                Debug.LogError("folder not exists:" + m_folder);
                Actived = false;
            }

            if (!string.IsNullOrEmpty(m_menu))
            {
                var menuBundle = AssetBundle.LoadFromFile(m_folder + "/" + m_menu);
                if (menuBundle)
                {
                    this.m_menifest = menuBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    UnityEngine.Object.DontDestroyOnLoad(m_menifest);
                    menuBundle.Unload(false);
                }
                else
                {
                    Debug.LogError("failed load manifest!");
                    Actived = false;
                }
            }
            else
            {
                Actived = true;
            }

            if (m_onInitCallback != null)
            {
                m_onInitCallback.Invoke(Actived);
            }
        }

        public void SetInitCallBack(System.Action<bool> callback)
        {
            m_onInitCallback = callback;
        }

        protected string GetBundlePath(string bundleName)
        {
            return m_folder + "/" + bundleName;
        }

        public void UpdateDownLand()
        {
            if (m_requests.Count > 0)
            {
                List<string> m_finishTemplate = null;
                foreach (var item in m_requests)
                {
                    if (item.Value.Item1.isDone)
                    {
                        var assetbundle = item.Value.Item1.assetBundle;
                        if (assetbundle != null)
                        {
                            m_loadedBundle[assetbundle.name] = assetbundle;
                            //Debug.LogError("bundle loaded:" + assetbundle);
                        }
                        item.Value.Item2?.Invoke(assetbundle);
                        if (m_finishTemplate == null)
                            m_finishTemplate = new List<string>();
                        m_finishTemplate.Add(item.Key);
                    }
                }
                if (m_finishTemplate?.Count > 0)
                {
                    foreach (var bundleName in m_finishTemplate)
                    {
                        m_requests.Remove(bundleName);
                    }
                }
            }

            if (m_waitingSubs.Count > 0)
            {
                List<System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>> finished = null;
                for (int i = 0; i < m_waitingSubs.Count; i++)
                {
                    var waitingSub = m_waitingSubs[i];
                    var bundle = waitingSub.Item1?.assetBundle;
                    if (!bundle)
                    {
                        if (finished == null)
                            finished = new List<System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>>();
                        finished.Add(waitingSub);
                        Debug.LogError("waiting bundle lost!");
                        continue;
                    }
                    int percent = LoadSubBundles(bundle.name);
                    if (percent == 100)
                    {
                        waitingSub.Item3.Invoke(1);
                        waitingSub.Item2?.Invoke(bundle);
                        if (finished == null)
                            finished = new List<System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>>();
                        finished.Add(waitingSub);
                    }
                    else if (waitingSub.Item3 != null)
                    {
                        waitingSub.Item3.Invoke(percent / 100f);
                    }
                }

                if (finished != null)
                {
                    for (int i = 0; i < finished.Count; i++)
                    {
                        m_waitingSubs.Remove(finished[i]);
                    }
                }
            }

            if(m_delyUnloadBundles.Count > 0)
            {
                foreach (var pair in m_delyUnloadBundles)
                {
                    if (pair.Value < Time.time)
                    {
                        //Debug.LogErrorFormat("{0}/{1}", pair.Value, Time.time);
                        UnloadAssetBundle(pair.Key, false);
                        m_delyUnloadBundles.Remove(pair.Key);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 异步加载Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="OnAssetLoad"></param>
        public void LoadAssetAsync<T>(string bundleName, string assetName, UnityAction<T> OnAssetLoad, bool autoUnload = true) where T : UnityEngine.Object
        {
            m_delyUnloadBundles.Remove(bundleName);
            LoadBundleAsync(bundleName, (bundle, err) =>
             {
                 if (bundle)
                 {
                     T asset = bundle.LoadAsset<T>(assetName);
                     if (OnAssetLoad != null)
                         OnAssetLoad(asset);
                 }
                 else
                 {
                     if (OnAssetLoad != null)
                         OnAssetLoad(null);
                 }
             }, null, autoUnload);
        }

        /// <summary>
        /// 同步加载Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string bundleName, string assetName, bool autoUnload = true) where T : UnityEngine.Object
        {
            m_delyUnloadBundles.Remove(bundleName);
            AssetBundle bundle = LoadBundle(bundleName);
            if (bundle == null)
                return null;
            T asset = bundle.LoadAsset<T>(assetName);
            if(autoUnload)
            {
                RecordDelyReleaseBundle(bundleName);
            }
            return asset;
        }

        protected void RecrodBundleRef(string parentBundle, string subBundle)
        {
            //Debug.LogErrorFormat("record ref {0}->{1}:", parentBundle, subBundle);

            if (!m_bundleRef.TryGetValue(subBundle, out var parentLists))
            {
                parentLists = new List<string>();
                m_bundleRef[subBundle] = parentLists;
            }
            parentLists.Add(parentBundle);
            m_rootBundleNames.Add(parentBundle);
        }

        protected void RemoveBundleRef(string parentBundle, bool clearInstance)
        {
            if (!m_rootBundleNames.Contains(parentBundle))
                return;
            m_rootBundleNames.Remove(parentBundle);
            List<string> m_releaseTemplate = null;
            foreach (var pair in m_bundleRef)
            {
                if (pair.Value.Contains(parentBundle))
                {
                    pair.Value.Remove(parentBundle);
                }
                if (pair.Value.Count == 0)
                {
                    if (m_releaseTemplate == null)
                        m_releaseTemplate = new List<string>();
                    m_releaseTemplate.Add(pair.Key);
                }
            }

            if (m_releaseTemplate?.Count > 0)
            {
                for (int i = 0; i < m_releaseTemplate.Count; i++)
                {
                    var bundleName = m_releaseTemplate[i];
                    m_bundleRef.Remove(bundleName);
                    UnloadAssetBundle(bundleName, clearInstance);
                }
            }
        }

        public AssetBundle LoadBundle(string bundleName)
        {
            m_delyUnloadBundles.Remove(bundleName);

            if (m_menifest)
            {
                var dependencies = m_menifest.GetAllDependencies(bundleName);
                if (dependencies != null && dependencies.Length > 0)
                {
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        var subBundleName = dependencies[i];
                        var subBundle = LoadBundle(subBundleName);
                        if (subBundle != null)
                        {
                            RecrodBundleRef(bundleName, subBundleName);
                        }
                    }
                }
            }
            if (m_loadedBundle.TryGetValue(bundleName, out var bundle) && bundle != null)
                return bundle;
            var path = GetBundlePath(bundleName);
            var bundleValue = AssetBundle.LoadFromFile(path, 0);
            m_loadedBundle[bundleName] = bundleValue;
            return bundleValue;
        }

        public AsyncOperation LoadLevel(string bundleName, string assetName, UnityEngine.SceneManagement.LoadSceneMode loadModle, bool autoUnload = true)
        {
            m_delyUnloadBundles.Remove(bundleName);
            AssetBundle bundle = LoadBundle(bundleName);
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(assetName, loadModle);
            if (autoUnload)
            {
                RecordDelyReleaseBundle(bundleName);
            }
            return operation;
        }

        /// <summary>
        /// 加载依赖包
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected int LoadSubBundles(string bundleName)
        {
            int subBundleLoaded = 100;
            if (m_menifest)
            {
                var dependencies = m_menifest.GetAllDependencies(bundleName);
                if (dependencies?.Length == 0)
                    return subBundleLoaded;

                var percent = Mathf.FloorToInt(100f / dependencies.Length);
                for (int i = 0; i < dependencies?.Length; i++)
                {
                    var subBundleName = dependencies[i];
                    if (!m_loadedBundle.ContainsKey(subBundleName))
                    {
                        if (!m_requests.TryGetValue(subBundleName, out var tuple))
                        {
                            var path = GetBundlePath(subBundleName);
                            tuple = new System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>(AssetBundle.LoadFromFileAsync(path, 0), new UnityEvent<AssetBundle>(), new UnityEvent<float>());
                            m_requests.Add(subBundleName, tuple);
                            RecrodBundleRef(bundleName, subBundleName);
                        }
                        subBundleLoaded -= percent;
                    }
                }
            }
            return subBundleLoaded;
        }

        public void LoadBundleAsync(string bundleName, System.Action<AssetBundle, string> onLoadCallBack, System.Action<float> progressCallBack = null, bool autoUnload = true)
        {
            m_delyUnloadBundles.Remove(bundleName);
            //Debug.LogError("LoadBundleAsync:" + bundleName);
            var subBundleLoaded = LoadSubBundles(bundleName) == 100;
            if (subBundleLoaded && m_loadedBundle.ContainsKey(bundleName))
            {
                AssetBundle bundle = m_loadedBundle[bundleName];
                if (progressCallBack != null)
                    progressCallBack.Invoke(1);

                if (onLoadCallBack != null)
                    onLoadCallBack(bundle, null);

                if (autoUnload)
                    RecordDelyReleaseBundle(bundleName);
            }
            else
            {
                if (!m_requests.TryGetValue(bundleName, out var tuple))
                {
                    var path = GetBundlePath(bundleName);
                    tuple = new System.Tuple<AssetBundleCreateRequest, UnityEvent<AssetBundle>, UnityEvent<float>>(AssetBundle.LoadFromFileAsync(path, 0), new UnityEvent<AssetBundle>(), new UnityEvent<float>());
                    m_requests.Add(bundleName, tuple);
                }

                tuple.Item2.AddListener((x) =>
                {
                    //Debug.LogError("try load finish:" + bundleName);
                    subBundleLoaded = LoadSubBundles(bundleName) == 100;
                    if (subBundleLoaded)
                    {
                        if (progressCallBack != null)
                            progressCallBack.Invoke(1);

                        if (onLoadCallBack != null)
                            onLoadCallBack(x, null);

                        if (autoUnload)
                            RecordDelyReleaseBundle(bundleName);
                    }
                    else
                    {
                        if (progressCallBack != null)
                            progressCallBack.Invoke(0);

                        if (!m_waitingSubs.Contains(tuple))
                            m_waitingSubs.Add(tuple);
                    }
                });

                tuple.Item3.AddListener((progress) =>
                {
                    if (progressCallBack != null)
                        progressCallBack.Invoke(progress);
                    //Debug.LogError("loading progress:" + progress);
                });
            }
        }

        public void UnloadAssetBundle(string bundleName, bool clearInstance = true)
        {
            //Debug.LogError("try unload:" + bundleName);
            bool bundleUnloaded = false;
            if (m_loadedBundle.TryGetValue(bundleName, out var bundle))
            {
                bundle.Unload(clearInstance);
                m_loadedBundle.Remove(bundleName);
                //Debug.LogError("unload bundle:" + bundleName);
                bundleUnloaded = true;
            }

            if (m_requests.TryGetValue(bundleName, out var tuple))
            {
                if (tuple.Item1 != null && tuple.Item1.isDone && tuple.Item1.assetBundle != null)
                {
                    tuple.Item1.assetBundle.Unload(clearInstance);
                    //Debug.LogError("unload bundle on requesting:" + tuple.Item1.assetBundle.name);
                }
                m_requests.Remove(bundleName);
                bundleUnloaded = true;
            }

            if(bundleUnloaded)
            {
                RemoveBundleRef(bundleName, clearInstance);
            }
        }

        public T[] LoadAssets<T>(string bundleName, bool autoUnload = true) where T : Object
        {
            m_delyUnloadBundles.Remove(bundleName);

            var bundle = LoadBundle(bundleName);
            if (bundle != null)
            {
                var assets = bundle.LoadAllAssets<T>();
                if (autoUnload)
                {
                    RecordDelyReleaseBundle(bundleName);
                }
                return assets;
            }
            return null;
        }

        protected void RecordDelyReleaseBundle(string bundleName)
        {
            m_delyUnloadBundles[bundleName] = Time.time + 1;
        }

        public void Dispose()
        {
            foreach (var pair in m_loadedBundle)
            {
                if (pair.Value)
                {
                    //Debug.LogError("unload:" + pair.Key);
                    pair.Value.Unload(true);
                }
            }
            foreach (var pair in m_requests)
            {
                if (pair.Value.Item1 != null && pair.Value.Item1.isDone)
                {
                    //Debug.LogError("unload:" + pair.Key);
                    pair.Value.Item1.assetBundle?.Unload(false);
                }
            }
            m_requests.Clear();
            m_loadedBundle.Clear();
            m_menifest = null;
            m_waitingSubs.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}