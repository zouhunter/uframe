using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
namespace UFrame.HiveBundle
{
    /// <summary>
    /// 文件路径资源包加载器
    /// </summary>
    [Preserve]
    public class FileBundleLoader : IAssetLoader,System.IDisposable
    {
        private string _folder;
        public bool Alive { get; private set; }
        private HashSet<string> _rootLoadBundles;//手动加载的资源包,只能手动卸载
        private Dictionary<string, AsyncBundleOperation> _requestsMap;//加载请求
        private Dictionary<string, List<string>> _subBundleMap;//依赖包
        private Dictionary<string, List<string>> _bundleRefMap;//被引用关系
        private Dictionary<string,string> _localFiles;
        private HashSet<string> _memOnlyBundles;
        private long _memOnlyStartTime;
        private HashSet<string> _builtInArts;

        public FileBundleLoader(string rootFolder)
        {
            this._folder = rootFolder;
            _requestsMap = new Dictionary<string, AsyncBundleOperation>();
            _bundleRefMap = new Dictionary<string, List<string>>();
            _subBundleMap = new Dictionary<string, List<string>>();
            _rootLoadBundles = new HashSet<string>();
            _memOnlyBundles = new HashSet<string>();
            _builtInArts = new HashSet<string>();
            if (!System.IO.Directory.Exists(_folder))
                System.IO.Directory.CreateDirectory(_folder);
            Alive = true;
        }

        public void Dispose()
        {
            var operations = new List<AsyncBundleOperation>(_requestsMap.Values);
            foreach (var op in operations)
            {
                op.Recover();
            }
            _requestsMap.Clear();
        }

        /// <summary>
        /// 设置内置包
        /// </summary>
        /// <param name="builtIns"></param>
        public void SetBuiltInArts(HashSet<string> builtIns)
        {
            _builtInArts = builtIns;
        }

        /// <summary>
        /// 注册依赖关系
        /// </summary>
        /// <param name="rootBundle"></param>
        /// <param name="subBundles"></param>
        public void RegistSubBundle(string rootBundle, params string[] subBundles)
        {
            if(!_subBundleMap.TryGetValue(rootBundle, out var bundles))
            {
                bundles = new List<string>();
                _subBundleMap[rootBundle] = bundles;
            }
            foreach (var subBundle in subBundles)
            {
                if(!bundles.Contains(subBundle))
                    bundles.Add(subBundle);
            }
            foreach (var item in subBundles)
            {
                if(!_bundleRefMap.TryGetValue(item, out var list))
                {
                    list = new List<string>();
                    _bundleRefMap[item] = list;
                }
                if(!list.Contains(item))
                    list.Add(rootBundle);
            }
        }
        public void RegistMemOnlyBundle(string bundle)
        {
            _memOnlyBundles.Add(bundle);
        }

        /// <summary>
        /// 外部加载资源包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public AsyncBundleOperation PreloadAssetBundle(string assetBundleName)
        {
            if (!Alive)
                return null;
            Debug.LogFormat("预加载资源包:{0}",assetBundleName);
            _rootLoadBundles.Add(assetBundleName);
            return LoadAssetBundleInternalAsync(assetBundleName);
        }

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        private AsyncBundleOperation LoadAssetBundleInternalAsync(string assetBundleName)
        {
            if (!Alive)
                return null;
            if (_requestsMap.TryGetValue(assetBundleName, out var bundleOperation) && 
                !bundleOperation.Recovered &&
                string.IsNullOrEmpty(bundleOperation.error))
            {
                return bundleOperation;
            }

            var filePath = $"{_folder}/{assetBundleName}";
            var localFile = false;
            string fileMd5 = null;
            if(_localFiles != null && _localFiles.TryGetValue(assetBundleName,out fileMd5))
                localFile = true;

            bundleOperation = new AsyncStreamingBundleOperation(_folder, assetBundleName) {
                memOnly = _memOnlyBundles.Contains(assetBundleName),//只从内存加载，不从文件加载
                startTime = _memOnlyStartTime,//最新资源开始时间
                existsInStream = localFile || _builtInArts.Contains(assetBundleName),//streamingAsset内包含文件
                bundleMd5 = fileMd5,//文件md5
            };
            _requestsMap[assetBundleName] = bundleOperation;
            //加载依赖包
            if (_subBundleMap.TryGetValue(assetBundleName,out var subBundleNames) && subBundleNames.Count > 0)
            {
                GroupLoadOperation preloadOperation = new GroupLoadOperation(subBundleNames.Count);
                foreach (var subBundleName in subBundleNames)
                {
                    var subLoadOperation = LoadAssetBundleInternalAsync(subBundleName);
                    subLoadOperation?.RegistComplete(x => {
                        preloadOperation.CompleteOne(subBundleName);
                    });
                }
                preloadOperation.RegistComplete(bundleOperation.StartRequest);
            }
            else
            {
                bundleOperation.StartRequest();
            }
            return bundleOperation;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AsyncAssetOperation<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : Object
        {
            Debug.LogFormat("[FileBundleLoader] 加载资源:{0},{1}", assetBundleName, assetName);
            var asyncAssetOp = new AsyncAssetOperation<T>(assetName);
            if (!Alive)
            {
                asyncAssetOp.SetAsset(null);
                return asyncAssetOp;
            }
            _rootLoadBundles.Add(assetBundleName);
            var bundleOperation = LoadAssetBundleInternalAsync(assetBundleName);
            bundleOperation?.RegistComplete(x => asyncAssetOp.SetAssetBundle(bundleOperation));
            return asyncAssetOp;
        }

        /// <summary>
        /// 加载资源组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AsyncAssetsOperation<T> LoadAssetsAsync<T>(string assetBundleName) where T : Object
        {
            Debug.LogFormat("[FileBundleLoader] 加载资源组:{0}", assetBundleName);
            var asyncAssetOp = new AsyncAssetsOperation<T>();
            if (!Alive)
            {
                asyncAssetOp.SetAsset(null);
                return asyncAssetOp;
            }
            _rootLoadBundles.Add(assetBundleName);
            var bundleOperation = LoadAssetBundleInternalAsync(assetBundleName);
            bundleOperation.RegistComplete(x => asyncAssetOp.SetAssetBundle(bundleOperation));
            return asyncAssetOp;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="sceneName"></param>
        /// <param name="loadSceneMode"></param>
        /// <returns></returns>
        public AsyncSceneOperation LoadSceneAsync(string assetBundleName, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.LogFormat("[FileBundleLoader] 加载场景:{0},{1}", assetBundleName,sceneName);
            var asyncAssetOp = new AsyncSceneOperation(sceneName, loadSceneMode);
            if (!Alive)
            {
                return asyncAssetOp;
            }
            _rootLoadBundles.Add(assetBundleName);
            var bundleOperation = LoadAssetBundleInternalAsync(assetBundleName);
            asyncAssetOp.SetAssetBundle(bundleOperation);
            bundleOperation.RegistComplete(x => asyncAssetOp.LoadScene());
            return asyncAssetOp;
        }

        /// <summary>
        /// 手动卸载资源包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="unloadInstance"></param>
        public void UnloadAssetBundle(string assetBundleName,bool unloadInstance = true)
        {
            if (!Alive)
                return;

            //检查是否有被引用
            if (_bundleRefMap.TryGetValue(assetBundleName, out var rootBundles))
            {
                foreach (var parentBundleName in rootBundles)
                {
                    if(_requestsMap.ContainsKey(parentBundleName))
                    {
                        Debug.Log($"failed unload {assetBundleName},reference by:{parentBundleName}");
                        return;
                    }
                }
            }

            //卸载资源包
            if (_requestsMap.TryGetValue(assetBundleName,out var bundleOperation))
            {
                bundleOperation?.ReleaseAssetBundle(unloadInstance);
                _requestsMap.Remove(assetBundleName);
            }

            //尝试卸载依赖包
            if (_subBundleMap.TryGetValue(assetBundleName, out var subBundles))
            {
                foreach (var subBundleName in subBundles)
                {
                    if(!_rootLoadBundles.Contains(subBundleName))
                    {
                        UnloadAssetBundle(subBundleName, unloadInstance);
                    }
                }
            }
            Debug.LogFormat("[FileBundleLoader] UnloadAssetBundle:{0}", assetBundleName);
        }
        /// <summary>
        /// 同步加载资源 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string assetBundleName, string assetName) where T : Object
        {
            Debug.LogFormat("[FileBundleLoader] LoadAsset:{0},{1}", assetBundleName, assetName);

            if (_requestsMap.TryGetValue(assetBundleName,out var operation) && operation.isDone && operation.assetBundle)
            {
                return operation.assetBundle.LoadAsset<T>(assetName);
            }
            return null;
        }
        /// <summary>
        /// 同步加载资源 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T[] LoadAssets<T>(string assetBundleName) where T : Object
        {
            Debug.LogFormat("[FileBundleLoader] LoadAssets:{0}", assetBundleName);

            if (_requestsMap.TryGetValue(assetBundleName, out var operation) && operation.isDone && operation.assetBundle)
            {
                return operation.assetBundle.LoadAllAssets<T>();
            }
            return null;
        }

        /// <summary>
        /// 设置package是否启用
        /// </summary>
        /// <param name="packageEnable"></param>
        public void SetLocalFiles(Dictionary<string,string> localFiles)
        {
            _localFiles = localFiles;
        }

        /// <summary>
        /// 设置初始化时间搓
        /// </summary>
        /// <param name="time"></param>
        public void SetMemOnlyBundleStartTime(long time)
        {
            _memOnlyStartTime = time;
        }

        /// <summary>
        /// 判断资源包是否已经加载
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="includeLoading"></param>
        /// <returns></returns>
        public bool CheckBundleLoaded(string bundle, bool includeLoading)
        {
            if(_requestsMap.TryGetValue(bundle,out var bundleRequest))
            {
                if (bundleRequest.Recovered)
                    return false;
                if (includeLoading)
                    return true;
                return bundleRequest.assetBundle;
            }
            return false;
        }
    }

    /// <summary>
    /// 资源加载接口
    /// </summary>
    public interface IAssetLoader
    {
        //预制加载
        AsyncBundleOperation PreloadAssetBundle(string assetbundleName);
        //注册依赖包
        void RegistSubBundle(string rootBundle, params string[] subBundles);
        //异步场景加载
        AsyncSceneOperation LoadSceneAsync(string assetBundleName, string assetName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode);
        //异步资源组加载
        AsyncAssetsOperation<T> LoadAssetsAsync<T>(string assetBundleName) where T : UnityEngine.Object;
        //异步资源加载
        AsyncAssetOperation<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object;
        //同步资源加载
        T LoadAsset<T>(string assetBundleName, string assetName) where T : UnityEngine.Object;
        //同步资源组加载
        T[] LoadAssets<T>(string assetBundleName) where T : UnityEngine.Object;
        //卸载资源包
        void UnloadAssetBundle(string assetBundleName, bool unloadInstance = true);
        //直接内存中加载
        void RegistMemOnlyBundle(string bundle);
        //设置文件时间
        void SetMemOnlyBundleStartTime(long time);
        //判断是否加载过资源包
        bool CheckBundleLoaded(string bundle, bool includeLoading);
    }
}

