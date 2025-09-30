using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UFrame.HiveBundle
{
    public class AssetBundleCtrl
    {
        private IAssetLoader _loader;
        public IAssetLoader loader => _loader;
        public AssetBundleCtrl(IAssetLoader loader)
        {
            this._loader = loader;
        }
        public AssetBundleCtrl(bool fullPath = false)
        {
#if UNITY_EDITOR
            if (Application.isEditor && !UnityEditor.EditorPrefs.GetBool("!HiveBundleManager.simulate"))
            {
                _loader = new SimulateBundleLoader(fullPath);
            }
#endif
            if (_loader == null)
            {
                _loader = new FileBundleLoader($"{Application.persistentDataPath}/packages");
            }
        }

        /// <summary>
        /// 设置内置包列表
        /// </summary>
        public virtual void SetBuiltInArts(HashSet<string> builtIns)
        {
            if (_loader is FileBundleLoader fileBundleLoader)
            {
                fileBundleLoader?.SetBuiltInArts(builtIns);
            }
        }

        /// <summary>
        /// 不加载packages目录,加载资源时，不使用packages目录
        /// </summary>
        /// <param name="enable"></param>
        public virtual void SetLocalFiles(Dictionary<string, string> localFiles)
        {
            if (_loader is FileBundleLoader fileBundleLoader)
            {
                fileBundleLoader?.SetLocalFiles(localFiles);
            }
        }

        /// <summary>
        /// 实例化源包中的所有资源
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public virtual async Task InstantiateAll(string assetbundleName)
        {
            Debug.LogFormat("实例化{0} 所有资源开始时间:{1}", assetbundleName, Time.realtimeSinceStartup);
            var objs = await _loader.LoadAssetsAsync<UnityEngine.Object>(assetbundleName);
            if (objs != null && objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i])
                    {
                        UnityEngine.Object.Instantiate(objs[i]);
                    }
                }
            }
            Debug.LogFormat("实例化{0} 所有资源结束时间:{1}", assetbundleName, Time.realtimeSinceStartup);
        }
        /// <summary>
        /// 资源实例化 (Runtime Don`t Use it!!!)
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public virtual async Task Instantiate<T>(string assetbundleName, string assetName) where T : UnityEngine.Object
        {
            var asset = await _loader.LoadAssetAsync<T>(assetbundleName, assetName);
            if (asset)
            {
                UnityEngine.Object.Instantiate(asset);
            }
        }

        /// <summary>
        /// 资源实例化
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public virtual AsyncAssetOperation<GameObject> InstantiateAsync(string assetbundleName, string assetName)
        {
            var instanceOperation = new AsyncAssetOperation<GameObject>();
            var operation = _loader.LoadAssetAsync<GameObject>(assetbundleName, assetName);
            operation.RegistComplete(x =>
            {
                if (operation.asset)
                    instanceOperation.SetAsset(UnityEngine.Object.Instantiate(operation.asset));
                else
                    instanceOperation.SetAsset(null);
            });
            return instanceOperation;
        }

        /// <summary>
        /// 预加载资源包
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public virtual AsyncBundleOperation PreloadAssetBundle(string assetbundleName)
        {
            return _loader.PreloadAssetBundle(assetbundleName);
        }
        /// <summary>
        /// 加载一组资源包
        /// </summary>
        /// <param name="strings"></param>
        public virtual GroupLoadOperation PreloadAssetBundles(string[] assetbundleNames)
        {
            var operation = new GroupLoadOperation(assetbundleNames.Length);
            foreach (var assetBundleName in assetbundleNames)
            {
                var abOp = _loader.PreloadAssetBundle(assetBundleName);
                abOp?.RegistComplete(operation.CompleteOne);
            }
            return operation;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public virtual AsyncAssetOperation<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            return _loader.LoadAssetAsync<T>(bundleName, assetName);
        }

        /// <summary>
        /// 同步加载资源，需要确认资源包已经加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public virtual T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            return _loader.LoadAsset<T>(bundleName, assetName);
        }

        /// <summary>
        /// 卸载资源包
        /// </summary>
        /// <param name="item"></param>
        public virtual void Unload(string assetBundleName, bool unloadInstance = true)
        {
            Debug.Log($"AssetBundleManager.Unload:{assetBundleName} unloadInstance:{unloadInstance}");
            _loader.UnloadAssetBundle(assetBundleName, unloadInstance);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual AsyncSceneOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return _loader.LoadSceneAsync(sceneName.ToLower(), sceneName, mode);
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        public virtual AsyncAssetsOperation<T> LoadAssetsAsync<T>(string assetbundleName) where T : UnityEngine.Object
        {
            return _loader.LoadAssetsAsync<T>(assetbundleName);
        }


        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        public virtual T[] LoadAssets<T>(string assetbundleName) where T : UnityEngine.Object
        {
            return _loader.LoadAssets<T>(assetbundleName);
        }


        /// <summary>
        /// 注册依赖包
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public virtual void RegistSubBundle(string rootBundle, params string[] subBundles)
        {
            _loader.RegistSubBundle(rootBundle, subBundles);
        }

        /// <summary>
        /// 直接读取内存包
        /// </summary>
        /// <param name="bundle"></param>
        public virtual void RegistMemOnlyBundle(string bundle)
        {
            _loader.RegistMemOnlyBundle(bundle);
        }

        /// <summary>
        /// 设置package时间
        /// </summary>
        /// <param name="timeTick"></param>
        public virtual void SetMemOnlyBundleStartTime(long timeTick)
        {
            _loader.SetMemOnlyBundleStartTime(timeTick);
        }

        /// <summary>
        /// 判断是否加载过资源包
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public virtual bool LoadedBundle(string bundle, bool includeLoading = true)
        {
            return _loader.CheckBundleLoaded(bundle, includeLoading);
        }
    }
}
