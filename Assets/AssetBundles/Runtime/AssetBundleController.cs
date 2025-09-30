/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 资源包加载管理器                                                                *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UFrame.Pool;

namespace UFrame.AssetBundles
{
    public class AssetBundleController : IAssetBundleController
    {
        private IBundleLoader m_activeLoader;
        private string m_url;
        private ObjectPool<AssetLoadOperation> m_assetLoadPool;
        private ObjectPool<SceneLoadOperation> m_sceneLoadPool;

        private Queue<AssetLoadOperation> m_loadAssetQueue;
        private Queue<SceneLoadOperation> m_loadSceneQueue;

        protected List<SceneLoadOperation> m_loadingScenes;
        protected bool m_inited;
        private ICustomLoader m_customLoader;
        public bool Custom => m_customLoader != null;

        public bool Regsited { get; private set; }

        public float Interval => 0;
        public bool Inited => m_inited;
        public bool Runing => true;

        public int Priority => 0;

        public bool Alive => Runing;

        public void Initialize()
        {
            m_assetLoadPool = new ObjectPool<AssetLoadOperation>(0, () => new AssetLoadOperation());
            m_loadAssetQueue = new Queue<AssetLoadOperation>();
            m_sceneLoadPool = new ObjectPool<SceneLoadOperation>(0, () => new SceneLoadOperation());
            m_loadSceneQueue = new Queue<SceneLoadOperation>();
            m_loadingScenes = new List<SceneLoadOperation>();
            Regsited = true;
        }

        public void Recover()
        {
            m_loadAssetQueue.Clear();
            CrearCurrentLoader();
            Regsited = false;
            Resources.UnloadUnusedAssets();
            AssetBundle.UnloadAllAssetBundles(true);
        }

        protected void CrearCurrentLoader()
        {
            if (m_activeLoader != null)
            {
                m_activeLoader.Dispose();
                m_activeLoader = null;
            }
        }

        public void SetCustomLoader(ICustomLoader customLoader)
        {
            m_customLoader = customLoader;
        }

        public UrlAssetBundleLoadCtrl StartStreamingAsset(string menu, Action<bool> onInit = null)
        {
            var url = GetStreamingAssetPathUrl();
            url += menu;
            return StartUpWeb(url, menu, onInit);
        }

        public LocalAssetLoader StartUpLocaly(string folder, string menu, Action<bool> onInit = null)
        {
            CrearCurrentLoader();
            var loader = new LocalAssetLoader(folder, menu);
            loader.SetInitCallBack(onInit);
            loader.Initialize();
            m_activeLoader = loader;
            return loader;
        }

        public UrlAssetBundleLoadCtrl StartUpWeb(string url, string menu, Action<bool> onInit = null)
        {
            CrearCurrentLoader();
            var loader = new UrlAssetBundleLoadCtrl(url, menu);
            loader.SetInitCallBack(onInit);
            loader.Initialize();
            m_activeLoader = loader;
            return loader;
        }

        public void SetInited()
        {
            m_inited = true;
        }

        public void OnLateUpdate()
        {
            if (m_activeLoader != null)
            {
                m_activeLoader.UpdateDownLand();

                if (m_loadingScenes.Count > 0)
                {
                    for (int i = 0; i < m_loadingScenes.Count; i++)
                    {
                        var option = m_loadingScenes[i];

                        if (option.onSceneProgress != null)
                            option.onSceneProgress.Invoke(option.asyncOperation);

                        if (option.asyncOperation == null || option.asyncOperation.isDone)
                        {
                            m_loadingScenes.RemoveAt(i);
                            i--;
                            OnReleaseSceneLoad(option);
                        }

                    }
                }
            }

            if (m_activeLoader != null && m_activeLoader.Actived)
            {
                if (m_loadAssetQueue.Count > 0)
                {
                    var operation = m_loadAssetQueue.Dequeue();
                    m_activeLoader.LoadBundleAsync(operation.assetBundleName, operation.OnBundleLoadCallBack, operation.OnProgressCallBack, operation.autoUnload);
                }
                else if (m_loadSceneQueue.Count > 0)
                {
                    var operation = m_loadSceneQueue.Dequeue();
                    m_activeLoader.LoadBundleAsync(operation.assetBundleName, operation.OnBundleLoadCallBack, operation.OnBundleProgressCallBack, operation.autoUnload);
                }
            }
            if (Custom)
            {
                m_customLoader.UpdateDownLand();
            }
        }

        #region 加载指定类型的资源
        /// <summary>
        /// 从url异步加载一个资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="onAssetLoad"></param>
        public void LoadAssetAsync<T>(string assetBundleName, string assetName, Action<T> onAssetLoad, Action<float> onProgress = null, bool autoUnload = true) where T : UnityEngine.Object
        {
            if (Custom)
            {
                m_customLoader.LoadAssetAsync(assetBundleName, assetName, onAssetLoad, onProgress);
                return;
            }

            if (Regsited)
            {
                var option = m_assetLoadPool.GetObject();
                option.Clean();
                option.assetBundleName = assetBundleName;
                option.assetName = assetName;
                option.onLoadAsset = onAssetLoad;
                option.assetType = typeof(T);
                option.autoUnload = autoUnload;
                option.onRelease = OnReleaseAssetLoadOperation;
                option.onProgress = onProgress;
                m_loadAssetQueue.Enqueue(option);
            }
        }

        #endregion 加载指定类型的资源

        #region 加载场景资源
        /// <summary>
        /// 加载场景资源包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="onProgress"></param>
        public void LoadSceneBundleAsync(string assetBundleName, Action<bool> onComplete, Action<float> onProgress, bool autoUnload = true)
        {
            if (Regsited)
            {
                var option = m_sceneLoadPool.GetObject();
                option.Clean();
                option.assetBundleName = assetBundleName;
                option.onBundleComplete = onComplete;
                option.onBundleProgress = onProgress;
                option.autoUnload = autoUnload;
                option.onRelease = OnReleaseSceneLoad;
                m_loadSceneQueue.Enqueue(option);
            }
        }

        public void LoadSceneAsync(string assetBundleName, string sceneName, bool addictive, Action<float> onBundleProgress = null, Action<AsyncOperation> onSceneProgress = null, bool autoUnload = true)
        {
            if (onBundleProgress != null)
                onBundleProgress.Invoke(1);

            if (Custom)
            {
                m_customLoader.LoadSceneAsync(assetBundleName, sceneName, addictive, onSceneProgress);
                return;
            }

            if (Regsited)
            {
                SceneLoadOperation option = m_sceneLoadPool.GetObject();
                option.Clean();
                option.assetBundleName = assetBundleName;
                option.sceneName = sceneName;
                option.addictive = addictive;
                option.onBundleProgress = onBundleProgress;
                option.onSceneProgress = onSceneProgress;
                option.autoUnload = autoUnload;
                option.assetBundle = null;
                option.onRelease = OnStartLoadScene;
                m_loadSceneQueue.Enqueue(option);
            }
        }

        private void OnStartLoadScene(SceneLoadOperation operation)
        {
            if (operation.assetBundle)
            {
                //Debug.Log("success load scene bundle:" + operation.assetBundleName);
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(operation.sceneName, operation.addictive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                asyncOperation.allowSceneActivation = false;
                operation.asyncOperation = asyncOperation;
                operation.onRelease = null;
                operation.onBundleProgress = null;
                m_loadingScenes.Add(operation);
            }
            else
            {
                if (operation.onSceneProgress != null)
                    operation.onSceneProgress.Invoke(null);
                Debug.Log("failed load scene bundle:" + operation.assetBundleName);
            }

        }
        #endregion

        private void OnReleaseAssetLoadOperation(AssetLoadOperation operation)
        {
            m_assetLoadPool.Store(operation);
            if (operation.autoUnload)
                m_activeLoader.UnloadAssetBundle(operation.assetBundleName, false);
        }

        private void OnReleaseSceneLoad(SceneLoadOperation operation)
        {
            m_sceneLoadPool.Store(operation);
            if (operation.autoUnload)
                m_activeLoader.UnloadAssetBundle(operation.assetBundleName, false);
        }

        public void LoadBundleAsync(string path, Action<AssetBundle, string> callBack, Action<float> progressCallBack = null, bool autoUnload = true)
        {
            m_activeLoader.LoadBundleAsync(path, (bundle, error) =>
            {
                if (callBack != null)
                {
                    callBack.Invoke(bundle, error);
                }
            }, progressCallBack, autoUnload);
        }

        public T LoadAssetSync<T>(string assetBundleName, string assetName, bool autoUnload = true) where T : UnityEngine.Object
        {
            if (Custom)
            {
                return m_customLoader.LoadAsset<T>(assetBundleName, assetName);
            }

            if (!(m_activeLoader is ISyncAssetBundleLoader))
            {
                Debug.LogError("not ISyncLoader!");
                return default(T);
            }

            return (m_activeLoader as ISyncAssetBundleLoader).LoadAsset<T>(assetBundleName, assetName, autoUnload);
        }

        public bool LoadSceneSync(string assetBundleName, string sceneName, bool addictive, Action<AsyncOperation> onSceneProgress = null)
        {
            if (!(m_activeLoader is ISyncAssetBundleLoader))
            {
                Debug.LogError("not ISyncLoader!");
                return false;
            }

            if (Custom)
            {
                m_customLoader.LoadSceneAsync(assetBundleName, sceneName, addictive, onSceneProgress);
                return true;
            }

            var operation = (m_activeLoader as ISyncAssetBundleLoader).LoadLevel(assetBundleName, sceneName, addictive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            if (operation != null)
            {
                SceneLoadOperation loadOperation = m_sceneLoadPool.GetObject();
                loadOperation.Clean();
                loadOperation.sceneName = sceneName;
                loadOperation.addictive = addictive;
                loadOperation.onRelease = OnReleaseSceneLoad;
                loadOperation.onSceneProgress = onSceneProgress;
                loadOperation.asyncOperation = operation;
                m_loadingScenes.Add(loadOperation);
                return false;
            }
            return true;
        }

        public bool LoadBundleSync(string assetBundleName, Action<AssetBundle> onLoad, bool autoUnload = true)
        {
            if (!(m_activeLoader is ISyncAssetBundleLoader))
            {
                Debug.LogError("not ISyncLoader!");
                return false;
            }
            var bundle = (m_activeLoader as ISyncAssetBundleLoader).LoadBundle(assetBundleName);
            if (onLoad != null)
                onLoad.Invoke(bundle);
            if (autoUnload)
                m_activeLoader.UnloadAssetBundle(assetBundleName, false);
            return true;
        }

        public void UnloadAssetBundle(string assetBundleName, bool clearInstance = true)
        {
            if (m_activeLoader != null)
            {
                m_activeLoader.UnloadAssetBundle(assetBundleName, clearInstance);
            }
        }

        protected virtual string GetStreamingAssetPathUrl()
        {
            var url = Application.streamingAssetsPath;
            if (Application.platform != RuntimePlatform.Android &&
                Application.platform != RuntimePlatform.WebGLPlayer)
            {
                url = "file:///" + url;
            }
            return url;
        }

    }
}