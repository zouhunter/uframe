#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
namespace UFrame.HiveBundle
{
    /// <summary>
    /// editor模块环境资源加载器
    /// </summary>
    public class SimulateBundleLoader : IAssetLoader
    {
        private Dictionary<string, Dictionary<string, string>> _bundleAssetPathMap = new Dictionary<string, Dictionary<string, string>>();
        private List<DelyAction> _delyActions = new List<DelyAction>();
        private List<string> preloadBundles = new List<string>();
        private bool isPlaying;
        private HashSet<string> memoryOnlyBundles = new HashSet<string>();
        public static string simulateUrl = $"http://192.168.109.249/Mate/Editor";
        public static bool simulateShader;
        private Dictionary<string, bool> loadedBundles = new Dictionary<string, bool>();
        public class DelyAction
        {
            public int frame;
            public System.Action action;
            public DelyAction(System.Action action)
            {
                this.frame = Random.Range(30, 60);
                this.action = action;
            }
        }

        public SimulateBundleLoader(bool fullPath = false,bool preloadAll = false)
        {
            foreach (var group in AssetBundleSetting.Instance.groups)
            {
                var buildInfo = group.CreateBuildInfo(fullPath);
                foreach (var pair in buildInfo)
                {
                    var bundle = pair.Key;
                    if (preloadAll) preloadBundles.Add(bundle);

                    foreach (var file in pair.Value)
                    {
                        var assetName = System.IO.Path.GetFileNameWithoutExtension(file);
                        if (!_bundleAssetPathMap.TryGetValue(bundle, out var assetMap))
                            assetMap = _bundleAssetPathMap[bundle] = new Dictionary<string, string>();
                        assetMap[assetName] = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file).Replace('\\', '/');
                    }
                }
            }
            var innerAbs = AssetDatabase.GetAllAssetBundleNames();
            foreach (var bundleName in innerAbs)
            {
                var assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                foreach (var item in assets)
                {
                    if (!_bundleAssetPathMap.TryGetValue(bundleName, out var assetMap))
                        assetMap = _bundleAssetPathMap[bundleName] = new Dictionary<string, string>();
                    assetMap[System.IO.Path.GetFileNameWithoutExtension(item)] = item;
                }
            }
            EditorApplication.update += EditorUpdate;
            isPlaying = Application.isPlaying;
        }

        private void EditorUpdate()
        {
            if (isPlaying != Application.isPlaying)
            {
                Debug.Log("ignore editor update,is playing:" + Application.isPlaying);
                EditorApplication.update -= EditorUpdate;
                return;
            }

            for (int i = 0; i < _delyActions.Count; i++)
            {
                var tuple = _delyActions[i];
                tuple.frame -= 1;
                if (tuple.frame-- <= 0)
                {
                    tuple.action?.Invoke();
                    _delyActions.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="loadSceneMode"></param>
        /// <returns></returns>
        public AsyncSceneOperation LoadSceneAsync(string assetBundleName, string assetName, LoadSceneMode loadSceneMode)
        {
            var assetPath = LocatAssetPath(assetBundleName, assetName);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                var operation = new AsyncSceneOperation(asset.name, loadSceneMode);
                var sceneIndex = System.Array.FindIndex(EditorBuildSettings.scenes, x => x.path == assetPath);
                if (sceneIndex < 0)
                {
                    var scene = new EditorBuildSettingsScene();
                    scene.enabled = false;
                    scene.path = assetPath;
                    var sceneList = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                    sceneList.Add(scene);
                    EditorBuildSettings.scenes = sceneList.ToArray();
                }
                _delyActions.Add(new DelyAction(() =>
                {
                    loadedBundles[assetBundleName] = true;
                    operation.LoadScene();
                }));
                return operation;
            }
            return null;
        }
        /// <summary>
        /// 加载资源组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public AsyncAssetsOperation<T> LoadAssetsAsync<T>(string assetBundleName) where T : UnityEngine.Object
        {
            var operation = new AsyncAssetsOperation<T>();
            List<T> objs = new List<T>();
            var assetPaths = LocatAssetsPath(assetBundleName);
            if (assetPaths != null && assetPaths.Count > 0)
            {
                foreach (var assetPath in assetPaths)
                {
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                        if (asset && asset is T)
                        {
                            objs.Add(asset);
                        }
                    }
                }
                if (!preloadBundles.Contains(assetBundleName))
                    _delyActions.Add(new DelyAction(() =>
                    {
                        operation.SetAsset(objs.ToArray());
                        loadedBundles[assetBundleName] = true;
                    }));
                else
                {
                    operation.SetAsset(objs.ToArray());
                    loadedBundles[assetBundleName] = true;
                }
            }
            else
            {
                var bundleOp = LoadBundleFromNetwork(assetBundleName);
                bundleOp.RegistComplete(() =>
                {
                    T[] assets = null;
                    if (bundleOp.assetBundle)
                    {
                        assets = bundleOp.assetBundle.LoadAllAssets<T>();
                        loadedBundles[assetBundleName] = true;
                    }
                    else
                    {
                        Debug.LogError("faild load bundle:" + assetBundleName);
                    }
                    operation.SetAsset(assets);
                });
            }
            return operation;
        }
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AsyncAssetOperation<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            var operation = new AsyncAssetOperation<T>();
            var assetPath = LocatAssetPath(assetBundleName, assetName);
            if (!string.IsNullOrEmpty(assetPath) && System.IO.File.Exists(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset)
                {
                    if (!preloadBundles.Contains(assetBundleName))
                    {
                        _delyActions.Add(new DelyAction(() =>
                        {
                            loadedBundles[assetBundleName] = true;
                            operation.SetAsset(asset);
                        }));
                    }
                    else
                    {
                        loadedBundles[assetBundleName] = true;
                        operation.SetAsset(asset);
                    }
                    return operation;
                }
            }
            else
            {
                var bundleOp = LoadBundleFromNetwork(assetBundleName);
                bundleOp.RegistComplete(() =>
                {
                    T asset = null;
                    if (bundleOp.assetBundle)
                    {
                        asset = bundleOp.assetBundle.LoadAsset<T>(assetName);
                        loadedBundles[assetBundleName] = true;
                    }
                    else
                    {
                        Debug.LogError("faild load bundle:" + assetBundleName);
                    }
                    operation.SetAsset(asset);
                });
            }
            return operation;
        }

        /// <summary>
        /// 定位资源路径
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private string LocatAssetPath(string assetBundleName, string assetName)
        {
            loadedBundles[assetBundleName] = false;
            if (_bundleAssetPathMap.TryGetValue(assetBundleName, out var assetMap))
            {
                if (assetMap.TryGetValue(assetName, out var asset))
                    return asset;
                if (assetName.Contains('.'))
                    assetName = assetName.Substring(0, assetName.IndexOf("."));
                if (assetMap.TryGetValue(assetName, out asset))
                    return asset;
            }
            UnityEngine.Debug.LogWarning($"not find bundleName:{assetBundleName} asssetName:{assetName}");
            return null;
        }
        /// <summary>
        /// 定位资源组
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        private List<string> LocatAssetsPath(string assetBundleName)
        {
            loadedBundles[assetBundleName] = false;
            var assets = new List<string>();
            if (_bundleAssetPathMap.TryGetValue(assetBundleName, out var assetMap))
            {
                assets = assetMap.Values.ToList();
            }
            if (assets.Count <= 0)
                Debug.LogWarning($"not find bundleName:{assetBundleName}");
            return assets;
        }
        /// <summary>
        /// 注册依赖包
        /// </summary>
        /// <param name="rootBundle"></param>
        /// <param name="subBundles"></param>
        public void RegistSubBundle(string rootBundle, params string[] subBundles)
        {
            Debug.LogFormat("simulate RegistSubBundle:{0} {1}", rootBundle, subBundles.Length);
            foreach (var subBundle in subBundles)
            {
                preloadBundles.Add(subBundle);
            }
        }
        /// <summary>
        /// 注册直接占用内存的包
        /// </summary>
        /// <param name="memBundle"></param>
        public void RegistMemOnlyBundle(string memBundle)
        {
            Debug.LogFormat("simulate RegistMemOnlyBundle:{0}", memBundle);
            memoryOnlyBundles.Add(memBundle);
        }
        /// <summary>
        /// 卸渣资源包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="unloadInstance"></param>
        public void UnloadAssetBundle(string assetBundleName, bool unloadInstance)
        {
            Debug.LogFormat("simulate UnloadAssetBundle:{0} {1}", assetBundleName, unloadInstance);
            var bundle = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.name == assetBundleName).FirstOrDefault();
            bundle?.Unload(unloadInstance);
            loadedBundles.Remove(assetBundleName);
        }

        /// <summary>
        /// 从打包机加载资源包
        /// </summary>
        /// <returns></returns>
        public AsyncBundleOperation LoadBundleFromNetwork(string assetBundleName)
        {
            var bundleOp = new AsyncBundleOperation(assetBundleName);
            var bundle = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.name == assetBundleName).FirstOrDefault();
            if (bundle)
            {
                bundleOp.SetAssetBundle(assetBundleName, bundle);
                return bundleOp;
            }
            var url = $"{simulateUrl}/{assetBundleName}";
            var local = Application.persistentDataPath + "/packages/" + assetBundleName;
            if (System.IO.File.Exists(local))
            {
                if (preloadBundles.Contains(assetBundleName))
                {
                    bundle = AssetBundle.LoadFromFile(local);
                    loadedBundles[assetBundleName] = true;
                    bundleOp.SetAssetBundle(assetBundleName, bundle);
                }
                else
                {
                    _delyActions.Add(new DelyAction(() =>
                    {
                        var bundle = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.name == assetBundleName).FirstOrDefault();
                        if (bundle == null)
                            bundle = AssetBundle.LoadFromFile(local);
                        loadedBundles[assetBundleName] = true;
                        bundleOp.SetAssetBundle(assetBundleName, bundle);
                    }));
                }
                return bundleOp;
            }
            var req = UnityWebRequest.Get(url);
            req.SendWebRequest().completed += (o) =>
            {
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var bytes = req.downloadHandler.data;
                    System.IO.File.WriteAllBytes(local, bytes);
                    var allBundle = AssetBundle.GetAllLoadedAssetBundles();
                    var bundle = allBundle.Where(x => x.name == assetBundleName).FirstOrDefault();
                    if (!bundle)
                    {
                        bundle = AssetBundle.LoadFromFile(local);
                    }
                    loadedBundles[assetBundleName] = true;
                    bundleOp.SetAssetBundle(assetBundleName, bundle);
                }
                else
                {
                    Debug.LogError(url + ":" + req.result);
                    loadedBundles.Remove(assetBundleName);
                    bundleOp.SetAssetBundle(assetBundleName, null);
                }
            };
            return bundleOp;
        }
        /// <summary>
        /// 预加载资源包
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public AsyncBundleOperation PreloadAssetBundle(string assetbundleName)
        {
            preloadBundles.Add(assetbundleName);
            loadedBundles[assetbundleName] = false;
            Debug.LogFormat("simulate load assetbundle:{0}", assetbundleName);
            var operation = new AsyncBundleOperation(assetbundleName);
            _delyActions.Add(new DelyAction(() =>
            {
                loadedBundles[assetbundleName] = true;
                operation.SetAssetBundle(assetbundleName, null);
            }));
            return operation;
        }
        /// <summary>
        /// 加载资源 （同步）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string assetBundleName, string assetName) where T : Object
        {
            PreloadAssetBundle(assetBundleName);
            var operation = LoadAssetAsync<T>(assetBundleName, assetName);
            if (operation.isDone)
                return operation.asset;
            Debug.LogError("simulate load asset not exits:" + assetName);
            return null;
        }
        /// <summary>
        /// 加载资源组 （同步）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public T[] LoadAssets<T>(string assetBundleName) where T : Object
        {
            PreloadAssetBundle(assetBundleName);
            var operation = LoadAssetsAsync<T>(assetBundleName);
            if (operation.isDone)
                return operation.assets;
            Debug.LogError("simulate load bundle not exits:" + assetBundleName);
            return null;
        }

        public void SetMemOnlyBundleStartTime(long time)
        {
            Debug.LogFormat("set mem only start time:{0}", time);
        }

        /// <summary>
        /// 判断资源包是否已经加载
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="includeLoading"></param>
        /// <returns></returns>
        public bool CheckBundleLoaded(string bundle, bool includeLoading)
        {
            if (loadedBundles.TryGetValue(bundle, out var loaded))
            {
                if (includeLoading)
                    return true;
                return loaded;
            }
            return false;
        }
    }
}
#endif

