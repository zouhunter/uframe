//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks.Internal;
using UnityEngine.Scripting;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;
using System.Security.Cryptography;

namespace UFrame.HiveBundle
{
    /// <summary>
    /// 异步操作句柄
    /// </summary>
    [Preserve]
    public class AsyncOperation : YieldInstruction, IEnumerator
    {
        public bool isDone { get; protected set; }
        public string error { get; protected set; }
        public object Current => null;
        public virtual float progress { get; protected set; }
        public bool Recovered => _recovered;

        protected List<Action<AsyncOperation>> _completed;
        protected List<Action> _completed0;
        private event Action<float> _onProgressEvent;
        protected bool _recovered;

        protected void SetFinish()
        {
            if (_recovered)
                return;

            if (!isDone)
            {
                isDone = true;
                SetProgress(1);
                OnFinish();
            }
            else
            {
                Debug.Log("operation already finished!");
            }
        }
        protected void SetError(string error)
        {
            this.error = error;
        }
        /// <summary>
        /// 统一的注册完成事件接口
        /// </summary>
        /// <param name="callback"></param>
        public void RegistComplete(Action<AsyncOperation> callback)
        {
            if (_recovered)
                return;

            if (isDone)
            {
                try
                {
                    callback?.Invoke(this);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (_completed == null)
                    _completed = new List<Action<AsyncOperation>>(1);
                this._completed.Add(callback);
            }
        }
        /// <summary>
        /// 无参数的注册完成事件接口
        /// </summary>
        /// <param name="callback"></param>
        public void RegistComplete(Action callback)
        {
            if (_recovered)
                return;

            if (isDone)
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (_completed0 == null)
                    _completed0 = new List<Action>(1);
                this._completed0.Add(callback);
            }
        }

        public void RemoveComplete(Action callback)
        {
            if (_completed0 != null)
                _completed0.Remove(callback);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Recover()
        {
            _recovered = true;
        }

        /// <summary>
        /// 异步完成回调
        /// </summary>
        protected virtual void OnFinish()
        {
            if (_recovered)
            {
                return;
            }

            if (_completed != null)
            {
                foreach (var callback in _completed)
                {
                    try
                    {
                        callback?.Invoke(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                _completed.Clear();
            }

            if (_completed0 != null)
            {
                foreach (var callback in _completed0)
                {
                    try
                    {
                        callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                _completed0.Clear();
            }
        }

        public virtual bool MoveNext()
        {
            return !isDone && !_recovered;
        }

        public virtual void Reset()
        {
            isDone = false;
            error = null;
            _recovered = false;
            _completed = null;
            _completed0 = null;
            _onProgressEvent = null;
            Debug.LogError("Async operation Reset!");
        }

        /// <summary>
        /// 进度变更
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void SetProgress(float progress)
        {
            this.progress = progress;
            _onProgressEvent?.Invoke(progress);
        }

        /// <summary>
        /// 注册进度变更事件
        /// </summary>
        /// <param name="onProgress"></param>
        public void RegistProgress(Action<float> onProgress)
        {
            if (isDone)
            {
                try
                {
                    onProgress?.Invoke(1);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                _onProgressEvent += onProgress;
            }
        }
    }

    /// <summary>
    /// 资源包异步句柄
    /// </summary>
    [Preserve]
    public class AsyncBundleOperation : AsyncOperation
    {
        public string bundleName { get; private set; }
        public AssetBundle assetBundle;
        private List<object> m_locks = new List<object>();
        public bool InUse => m_locks.Count > 0;
        private bool unloadInstance;

        public AsyncBundleOperation(string abItem)
        {
            this.bundleName = abItem;
        }
        public override void Recover()
        {
            ReleaseAssetBundle(false);
            base.Recover();
        }

        public override void Reset()
        {
            base.Reset();
            assetBundle = null;
            unloadInstance = false;
            m_locks.Clear();
        }

        public virtual void StartRequest()
        {

        }

        /// <summary>
        /// 从内存中加载资源包
        /// </summary>
        protected bool LoadBundleFromMem()
        {
            var iter = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var item in iter)
            {
                if (item.name == bundleName)
                {
                    SetAssetBundle(bundleName, item);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 释放资源包
        /// </summary>
        /// <param name="unloadAll"></param>
        public virtual void ReleaseAssetBundle(bool unloadAll)
        {
            if (_recovered)
                return;
            Debug.LogFormat("unload bundle:{0} {1}", bundleName, unloadAll);
            if (assetBundle)
            {
                assetBundle.Unload(unloadAll);
                assetBundle = null;
            }
            unloadInstance = unloadAll;
            _recovered = true;
        }

        /// <summary>
        /// 添加对象加锁定，防止卸载
        /// </summary>
        /// <param name="lockObj"></param>
        public void UnLock(object lockObj)
        {
            m_locks.Remove(lockObj);
            if (m_locks.Count <= 0)
                ReleaseAssetBundle(true);
        }

        /// <summary>
        /// 移除对象锁定
        /// </summary>
        /// <param name="lockObj"></param>
        public void Lock(object lockObj)
        {
            m_locks.Add(lockObj);
        }

        /// <summary>
        /// 设置资源包
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetBundle"></param>
        public virtual void SetAssetBundle(string assetBundleName, AssetBundle assetBundle)
        {
            if (_recovered)
            {
                ReleaseAssetBundle(unloadInstance);
                Debug.Log("released assetbundle:" + assetBundleName);
                return;
            }

            if (assetBundleName == bundleName)
            {
                this.assetBundle = assetBundle;
                Debug.LogFormat("资源包加载完成：{0} valid:{1}", assetBundleName, (assetBundle != null));
                if (!assetBundle)
                {
                    error = "assetbundle null";
                }
                SetFinish();
            }
        }

        #region UniTask
        /// <summary>
        /// 获取UniTask异步句柄
        /// </summary>
        /// <returns></returns>
        [Preserve]
        public AsyncOperationAwaiter GetAwaiter()
        {
            return new AsyncOperationAwaiter(this);
        }
        /// <summary>
        /// UniTask异步句柄
        /// </summary>
        [Preserve]
        public struct AsyncOperationAwaiter : ICriticalNotifyCompletion
        {
            private AsyncBundleOperation _asyncOperation;
            private Action<AsyncBundleOperation> _continuationAction;

            public AsyncOperationAwaiter(AsyncBundleOperation asyncOperation)
            {
                this._asyncOperation = asyncOperation;
                this._continuationAction = null;
            }

            public bool IsCompleted => _asyncOperation == null || _asyncOperation.isDone;
            /// <summary>
            /// 资源包获取
            /// </summary>
            /// <returns></returns>
            [Preserve]
            public AssetBundle GetResult()
            {
                AssetBundle bundle = _asyncOperation?.assetBundle;
                if (_continuationAction != null)
                {
                    _asyncOperation?.RemoveComplete(OnFinish);
                    _continuationAction = null;
                    _asyncOperation = null;
                }
                else
                {
                    _asyncOperation = null;
                }
                return bundle;
            }
            private void OnFinish()
            {
                _continuationAction?.Invoke(_asyncOperation);
            }
            /// <summary>
            /// 注册完成事件
            /// </summary>
            /// <param name="continuation"></param>
            [Preserve]
            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }
            /// <summary>
            /// 注册完成事件 [__DynamicallyInvokable]
            /// </summary>
            /// <param name="continuation"></param>
            [Preserve]
            public void UnsafeOnCompleted(Action continuation)
            {
                _continuationAction = PooledDelegate<AsyncBundleOperation>.Create(continuation);
                _asyncOperation?.RegistComplete(OnFinish);
            }
        }
        #endregion UniTask
    }

    /// <summary>
    /// 异步加载一组资源包句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Preserve]
    public class AsyncAssetsOperation<T> : AsyncOperation where T : UnityEngine.Object
    {
        public T[] assets;
        private AsyncBundleOperation m_bundleOperation;
        protected List<Action<T[]>> assetCompleted;

        /// <summary>
        /// 设置资源包句柄
        /// </summary>
        /// <param name="bundleOperation"></param>
        public void SetAssetBundle(AsyncBundleOperation bundleOperation)
        {
            if (_recovered)
                return;

            m_bundleOperation = bundleOperation;
            if (m_bundleOperation != null && m_bundleOperation.assetBundle)
            {
                var assetNames = m_bundleOperation.assetBundle.GetAllAssetNames();
                LoadAllAssets(assetNames);
            }
            else{
                LoadAllAssets(null);
            }
        }

        /// <summary>
        /// 设置资源组
        /// </summary>
        /// <param name="assets"></param>
        public void SetAsset(T[] assets)
        {
            if (_recovered)
                return;

            if (assets != null)
            {
                m_bundleOperation?.Lock(this);
                this.assets = assets;
            }
            SetFinish();
        }


        public override void Reset()
        {
            base.Reset();
            assets = null;
            assetCompleted = null;
            m_bundleOperation = null;
        }

        /// <summary>
        /// 释放资源包锁定
        /// </summary>
        public override void Recover()
        {
            base.Recover();
            m_bundleOperation?.UnLock(this);
        }

        /// <summary>
        /// 注册资源组加载完成事件
        /// </summary>
        /// <param name="callback"></param>
        public void RegistComplete(System.Action<T[]> callback)
        {
            if (_recovered)
                return;

            if (isDone)
            {
                try
                {
                    callback?.Invoke(assets);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (assetCompleted == null)
                    assetCompleted = new List<Action<T[]>>(1);
                assetCompleted.Add(callback);
            }

        }

        /// <summary>
        /// 回调资源组加载完成事件
        /// </summary>
        protected override void OnFinish()
        {
            if (_recovered)
                return;

            base.OnFinish();
            if (assetCompleted != null)
            {
                foreach (var callback in assetCompleted)
                {
                    try
                    {
                        callback?.Invoke(assets);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                assetCompleted.Clear();
            }
        }

        /// <summary>
        /// 异步加载资源组
        /// </summary>
        /// <param name="assetNames"></param>
        [Preserve]
        private async void LoadAllAssets(string[] assetNames)
        {
            if (_recovered)
                return;

            if(m_bundleOperation != null && m_bundleOperation.assetBundle)
            {
                var tasks = new Task<T>[assetNames.Length];
                for (int i = 0; i < assetNames.Length; i++)
                {
                    tasks[i] = LoadAssetTask(m_bundleOperation.assetBundle, assetNames[i]);
                }
                //等待组件初始化
                T[] assets = await Task.WhenAll(tasks);
                SetAsset(assets);
            }
            else
            {
                SetAsset(new T[0]);
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        [Preserve]
        private async Task<T> LoadAssetTask(AssetBundle bundle, string assetName)
        {
            //Debug.Log("读取" + assetName + "资源开始时间:" + Time.realtimeSinceStartup);
            AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
            await request;
            //Debug.Log("读取" + assetName + "资源结束时间:" + Time.realtimeSinceStartup);
            if (request == null)
                return default;
            return request.asset as T;
        }

        #region UniTask
        /// <summary>
        /// 获取UniTask异步句柄
        /// </summary>
        /// <returns></returns>
        [Preserve]
        public AsyncOperationAwaiter GetAwaiter()
        {
            return new AsyncOperationAwaiter(this);
        }
        /// <summary>
        /// UniTask异步句柄
        /// </summary>
        [Preserve]
        public struct AsyncOperationAwaiter : ICriticalNotifyCompletion
        {
            AsyncAssetsOperation<T> asyncOperation;
            Action<AsyncAssetsOperation<T>> continuationAction;

            public AsyncOperationAwaiter(AsyncAssetsOperation<T> asyncOperation)
            {
                this.asyncOperation = asyncOperation;
                this.continuationAction = null;
            }

            public bool IsCompleted => asyncOperation == null || asyncOperation.isDone;
            /// <summary>
            /// 获取资源组结果
            /// </summary>
            /// <returns></returns>
            public T[] GetResult()
            {
                var assets = asyncOperation?.assets;
                if (continuationAction != null)
                {
                    asyncOperation?.RegistComplete(OnFinish);
                    continuationAction = null;
                    asyncOperation = null;
                }
                else
                {
                    asyncOperation = null;
                }
                return assets;
            }

            private void OnFinish()
            {
                continuationAction?.Invoke(asyncOperation);
            }
            /// <summary>
            /// 注册完成事件
            /// </summary>
            /// <param name="continuation"></param>
            [Preserve]
            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }
            /// <summary>
            /// 注册完成事件 ICriticalNotifyCompletion
            /// </summary>
            /// <param name="continuation"></param>
            [Preserve]
            public void UnsafeOnCompleted(Action continuation)
            {
                continuationAction = PooledDelegate<AsyncAssetsOperation<T>>.Create(continuation);
                asyncOperation?.RegistComplete(OnFinish);
            }
        }
        #endregion UniTask
    }
    /// <summary>
    /// 异步加载资源句柄
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Preserve]
    public class AsyncAssetOperation<T> : AsyncOperation where T : UnityEngine.Object
    {
        public T asset;
        private AsyncBundleOperation m_bundleOperation;
        private string m_assetName;
        protected List<Action<T>> assetCompleted;

        public AsyncAssetOperation(string assetName = null)
        {
            this.m_assetName = assetName;
        }

        /// <summary>
        /// 设置资源包句柄
        /// </summary>
        /// <param name="bundleOperation"></param>
        public void SetAssetBundle(AsyncBundleOperation bundleOperation)
        {
            if (_recovered)
                return;

            m_bundleOperation = bundleOperation;
            if (m_bundleOperation != null && m_bundleOperation.assetBundle)
            {
                T asset = null;
                if (!string.IsNullOrEmpty(m_assetName))
                {
                    try
                    {
                        asset = m_bundleOperation.assetBundle.LoadAsset<T>(m_assetName);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                if (!asset)
                {
                    try
                    {
                        var allAssets = m_bundleOperation.assetBundle.LoadAllAssets<T>();
                        asset = Array.Find(allAssets, x => x.name.ToLower().Contains(m_assetName.ToLower()));
                        if (!asset && allAssets.Length > 0)
                        {
                            asset = allAssets[0];
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                }
                SetAsset(asset);
            }
            else
            {
                SetAsset(null);
            }
        }

        /// <summary>
        /// 直接设置资源
        /// </summary>
        /// <param name="asset"></param>
        public void SetAsset(T asset)
        {
            if (_recovered)
                return;

            if (asset)
            {
                m_bundleOperation?.Lock(this);
                this.asset = asset;
            }
            SetFinish();
        }

        public override void Reset()
        {
            base.Reset();
            asset = null;
            assetCompleted = null;
        }

        /// <summary>
        /// 释放资源包锁定
        /// </summary>
        public override void Recover()
        {
            base.Recover();
            m_bundleOperation?.UnLock(this);
        }

        /// <summary>
        /// 注册完成事件
        /// </summary>
        /// <param name="callback"></param>
        public void RegistComplete(System.Action<T> callback)
        {
            if (_recovered)
                return;

            if (!isDone)
            {
                if (assetCompleted == null)
                    assetCompleted = new List<Action<T>>(1);
                assetCompleted.Add(callback);
            }
            else
            {
                try
                {
                    callback?.Invoke(asset);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// 资源加载完成回调
        /// </summary>
        protected override void OnFinish()
        {
            if (_recovered)
                return;

            base.OnFinish();

            if (assetCompleted != null)
            {
                foreach (var callback in assetCompleted)
                {
                    try
                    {
                        callback?.Invoke(asset);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                assetCompleted.Clear();
            }
        }
        #region UniTask
        [Preserve]
        public AsyncOperationAwaiter GetAwaiter()
        {
            return new AsyncOperationAwaiter(this);
        }

        /// <summary>
        /// UniTask异步句柄
        /// </summary>
        [Preserve]
        public struct AsyncOperationAwaiter : ICriticalNotifyCompletion
        {
            private AsyncAssetOperation<T> asyncOperation;
            private Action<AsyncAssetOperation<T>> continuationAction;

            public AsyncOperationAwaiter(AsyncAssetOperation<T> asyncOperation)
            {
                this.asyncOperation = asyncOperation;
                this.continuationAction = null;
            }

            private void OnFinish()
            {
                continuationAction?.Invoke(asyncOperation);
            }

            public bool IsCompleted => asyncOperation == null || asyncOperation.isDone;

            /// <summary>
            /// 获取资源
            /// </summary>
            /// <returns></returns>
            [Preserve]
            public T GetResult()
            {
                T asset = asyncOperation?.asset;
                if (continuationAction != null)
                {
                    asyncOperation?.RemoveComplete(OnFinish);
                    continuationAction = null;
                    asyncOperation = null;
                }
                else
                {
                    asyncOperation = null;
                }
                return asset;
            }
            [Preserve]
            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }
            [Preserve]
            public void UnsafeOnCompleted(Action continuation)
            {
                continuationAction = PooledDelegate<AsyncAssetOperation<T>>.Create(continuation);
                asyncOperation.RegistComplete(OnFinish);
            }
        }
        #endregion UniTask
    }

    /// <summary>
    /// 异步场景加载句柄
    /// </summary>
    [Preserve]
    public class AsyncSceneOperation : AsyncOperation
    {
        public UnityEngine.AsyncOperation sceneLoadOperation;
        public bool success => sceneLoadOperation != null;
        private AsyncBundleOperation m_bundleOperation;
        private string m_sceneName;
        private LoadSceneMode m_loadSceneMode;
        private event Action<UnityEngine.AsyncOperation> sceneLoadEvent;
        private bool _allowSceneActivation = true;
        public bool allowSceneActivation
        {
            get
            {
                return _allowSceneActivation;
            }
            set
            {
                _allowSceneActivation = value;
                if (sceneLoadOperation != null)
                    sceneLoadOperation.allowSceneActivation = value;
            }
        }
        public override float progress { get => GetProgress(); protected set => base.progress = value; }

        private float GetProgress()
        {
            if (_recovered)
                return 0;

            if (sceneLoadOperation != null)
                return sceneLoadOperation.progress * 0.5f + 0.5f;
            if (m_bundleOperation != null)
                return m_bundleOperation.progress * 0.5f;
            return 0;
        }

        public AsyncSceneOperation(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            this.m_sceneName = sceneName;
            this.m_loadSceneMode = loadSceneMode;
        }
        /// <summary>
        /// 设置资源包句柄
        /// </summary>
        /// <param name="bundleOperation"></param>
        public void SetAssetBundle(AsyncBundleOperation bundleOperation)
        {
            if (_recovered)
                return;

            m_bundleOperation = bundleOperation;
        }
        /// <summary>
        /// 注册场景回调
        /// </summary>
        /// <param name="callback"></param>
        public void RegistSceneLoadComplete(Action<UnityEngine.AsyncOperation> callback)
        {
            if (_recovered)
                return;

            if (sceneLoadOperation != null && sceneLoadOperation.isDone)
            {
                try
                {
                    callback?.Invoke(sceneLoadOperation);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                this.sceneLoadEvent += callback;
            }
        }
        /// <summary>
        /// 加载场景逻辑
        /// </summary>
        public void LoadScene()
        {
            if (_recovered)
                return;

            if (m_bundleOperation != null && m_bundleOperation.assetBundle != null)
            {
                if (string.IsNullOrEmpty(m_sceneName))
                {
                    var allScenePaths = m_bundleOperation.assetBundle.GetAllScenePaths();
                    if (allScenePaths != null && allScenePaths.Length > 0)
                    {
                        m_sceneName = allScenePaths[0];
                    }
                }
            }
            if (string.IsNullOrEmpty(m_sceneName))
            {
                Debug.LogError("empty sceneName");
                return;
            }

            sceneLoadOperation = null;

#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.GetBool("!AssetBundleManager.simulate"))
            {
                var editorScenes = UnityEditor.EditorBuildSettings.scenes;
                var sceneItem = Array.Find(editorScenes, x => System.IO.Path.GetFileNameWithoutExtension(x.path) == m_sceneName);
                if (sceneItem != null)
                {
                    sceneLoadOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(sceneItem.path, new LoadSceneParameters(m_loadSceneMode));
                }
            }
#endif
            if (sceneLoadOperation == null)
                sceneLoadOperation = SceneManager.LoadSceneAsync(this.m_sceneName, this.m_loadSceneMode);

            if (sceneLoadOperation != null)
            {
                sceneLoadOperation.completed += OnSceneLoadComplete;
                sceneLoadOperation.allowSceneActivation = allowSceneActivation;
                m_bundleOperation?.Lock(this);
            }
            else
            {
                SetFinish();
                Debug.LogError("failed load scene:" + m_sceneName);
            }
        }
        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        /// <param name="operation"></param>
        private void OnSceneLoadComplete(UnityEngine.AsyncOperation operation)
        {
            try
            {
                if (_recovered)
                {
                    if (m_loadSceneMode == UnityEngine.SceneManagement.LoadSceneMode.Additive)
                    {
                        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(m_sceneName);
                    }
                    return;
                }

                SetFinish();
                this.sceneLoadEvent?.Invoke(operation);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            this.sceneLoadEvent = null;
        }
        /// <summary>
        /// 释放资源锁定
        /// </summary>
        public override void Recover()
        {
            base.Recover();
            m_bundleOperation?.UnLock(this);
        }

        public override void Reset()
        {
            base.Reset();
            sceneLoadEvent = null;
            sceneLoadOperation = null;
            m_bundleOperation = null;
        }
    }

    /// <summary>
    /// 异步文件加载句柄
    /// </summary>
    [Preserve]
    public class AsyncFileBundleOperation : AsyncBundleOperation
    {
        public Queue<Action<string, string, AssetBundle>> loadBundleCallBack = new Queue<Action<string, string, AssetBundle>>();
        private AssetBundleCreateRequest request;
        private string filePath;
        public override float progress { get => GetProgress(); protected set => base.progress = value; }

        private float GetProgress()
        {
            if (request != null)
                return request.progress;
            return 0;
        }

        public AsyncFileBundleOperation(string filePath, string bundleName) : base(bundleName)
        {
            this.filePath = filePath;
        }
        public override void Reset()
        {
            base.Reset();
            request = null;
        }
        /// <summary>
        /// 开始请求
        /// </summary>
        public override void StartRequest()
        {
            if (_recovered)
                return;

            if (request != null)
                return;

            try
            {
                if (!LoadBundleFromMem())
                {
                    request = AssetBundle.LoadFromFileAsync(filePath);
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                if (!LoadBundleFromMem())
                {
                    Debug.LogException(e);
                }
            }
            if (request.isDone)
            {
                OnLoadBundleFromFile(request);
            }
            else
            {
                request.completed += OnLoadBundleFromFile;
            }
        }
        /// <summary>
        /// 文件加载回调
        /// </summary>
        /// <param name="operation"></param>
        private void OnLoadBundleFromFile(UnityEngine.AsyncOperation operation)
        {
            if (_recovered)
                return;

            if (operation.isDone)
            {
                base.SetAssetBundle(bundleName, request.assetBundle);
            }
        }
    }

    /// <summary>
    /// 从streamingAssetsPath加载资源包
    /// </summary>
    [Preserve]
    public class AsyncStreamingBundleOperation : AsyncBundleOperation
    {
        public Queue<Action<string, AssetBundle>> loadBundleCallBack = new Queue<Action<string, AssetBundle>>();
        public bool existsInStream;
        public bool memOnly;
        public long startTime;
        public string bundleMd5;
        private UnityWebRequestAsyncOperation urlRequest;
        private AssetBundleCreateRequest bundleRequest;
        private string uri;
        private string tempFilePath;
        private string tempFileCoping;
        private bool fallbackLoading;
        public override float progress { get => GetProgress(); protected set => base.progress = value; }
        private float GetProgress()
        {
            if (bundleRequest != null)
                return bundleRequest.progress;
            return 0;
        }

        public AsyncStreamingBundleOperation(string fileFolder, string bundleName) : base(bundleName)
        {
            this.uri = $"{Application.streamingAssetsPath}/{bundleName}";
            if (Application.platform != RuntimePlatform.Android)
                uri = $"file://{uri}";
            tempFilePath = $"{fileFolder}/{bundleName}";
            tempFileCoping = tempFilePath + ".downloading";
        }

        public override void Recover()
        {
            base.Recover();
            if (urlRequest != null)
            {
                urlRequest.webRequest?.Dispose();
                urlRequest = null;
            }
            if (bundleRequest != null)
            {
                bundleRequest = null;
            }
        }

        public override void Reset()
        {
            base.Reset();
            urlRequest = null;
        }
        /// <summary>
        /// 开始请求
        /// </summary>
        public override void StartRequest()
        {
            if (_recovered)
                return;

            if (bundleRequest != null)
                return;

            if (LoadBundleFromMem())
                return;

            fallbackLoading = existsInStream ? false : true;

            try
            {
                var file = new System.IO.FileInfo(tempFilePath);
                if (file.Exists && file.Length > 0)
                {
                    //md5相同、更新时间大、streaming不存在
                    if (file.LastWriteTime.Ticks > startTime || !existsInStream || (!string.IsNullOrEmpty(bundleMd5) && bundleMd5 == ComputeFileBase64(tempFilePath)))
                    {
                        LoadBundleFromFile(tempFilePath);
                        return;
                    }
                    else
                    {
                        Debug.Log($"ignore old bundle:{file.Name} modify:{file.LastWriteTime.Ticks} start:{startTime}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                if(existsInStream){
                    LoadFromStreamingAssets();
                }
                else{
                    SetAssetBundle(bundleName,null);
                }   
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 计算文件的Base64 MD5值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Base64编码的MD5值</returns>
        private string ComputeFileBase64(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return Convert.ToBase64String(hash);
                }
            }
        }
        /// <summary>
        /// 从文件加载资源包
        /// </summary>
        /// <param name="operation"></param>
        private void LoadBundleFromFileAfterCreate()
        {
            if (_recovered)
                return;

            try
            {
                var copingInfo = new System.IO.FileInfo(tempFileCoping);
                if (copingInfo.Exists && copingInfo.Length > 0)
                {
                    if(System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                    System.IO.File.Copy(tempFileCoping, tempFilePath, true);
                    copingInfo.Delete();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(new Exception("LoadBundleFromFileAfterCreate", e));
            }

            var fileInfo = new FileInfo(tempFilePath);
            if (fileInfo != null && fileInfo.Exists)
            {
                fileInfo.LastWriteTime = DateTime.Now;
                Debug.Log(bundleName + " write time:" + DateTime.Now.Ticks);
                LoadBundleFromFile(tempFilePath);
            }
        }

        /// <summary>
        /// 从文件加载资源包
        /// </summary>
        /// <param name="path"></param>
        private void LoadBundleFromFile(string path)
        {
            if (LoadBundleFromMem())
                return;

            if (memOnly)
            {
                try
                {
                    Debug.LogFormat("load to mem:{0}", path);
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        SetAssetBundle(bundleName, AssetBundle.LoadFromMemory(bytes));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(new Exception(path, e));
                }
            }
            else
            {
                try
                {
                    bundleRequest = AssetBundle.LoadFromFileAsync(path);

                    if (bundleRequest != null)
                    {
                        bundleRequest.completed += (x) =>
                        {
                            if (_recovered || bundleRequest == null)
                            {
                                return;
                            }

                            SetAssetBundle(bundleName, bundleRequest.assetBundle);
                        };
                    }
                    else
                    {
                        SetAssetBundle(bundleName, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(new Exception(path, e));
                }
            }
        }

        /// <summary>
        /// 从streamingAssets加载资源包
        /// </summary>
        private void LoadFromStreamingAssets()
        {
            if (!existsInStream)
            {
                SetAssetBundle(bundleName, null);
                return;
            }

            if (urlRequest != null)
                return;

            if (System.IO.File.Exists(tempFileCoping))
                System.IO.File.Delete(tempFileCoping);

            var dir = System.IO.Path.GetDirectoryName(tempFileCoping);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var webReq = new UnityWebRequest(uri);
            webReq.downloadHandler = new DownloadHandlerFile(tempFileCoping);
            urlRequest = webReq.SendWebRequest();
            if (urlRequest != null)
            {
                urlRequest.completed += (x) =>
                {
                    try
                    {
                        if (_recovered || x == null)
                        {
                            return;
                        }

                        if (x.isDone && webReq != null && webReq.result == UnityWebRequest.Result.Success)
                        {
                            LoadBundleFromFileAfterCreate();
                        }
                        else
                        {
                            SetAssetBundle(bundleName, null);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(new Exception("LoadFromStreamingAssets:" + bundleName, e));
                        SetAssetBundle(bundleName, null);
                    }
                };
            }
            else
            {
                SetAssetBundle(bundleName, null);
            }
        }

        /// <summary>
        /// 支持fallback加载一次
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetBundle"></param>
        public override void SetAssetBundle(string assetBundleName, AssetBundle assetBundle)
        {
            //如果加载失败，尝试从streaming加载一次
            if (assetBundle == null && !fallbackLoading && existsInStream)
            {
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                }
                catch (Exception ee)
                {
                    Debug.LogException(ee);
                }
                fallbackLoading = true;
                LoadFromStreamingAssets();
            }
            else
            {
                base.SetAssetBundle(assetBundleName, assetBundle);
            }
        }
    }

    /// <summary>
    /// 组加载异步句柄
    /// </summary>
    [Preserve]
    public class GroupLoadOperation : AsyncOperation
    {
        public int preloadCount { get; set; }
        public event Action<GroupLoadOperation> onProgressEvent;
        private HashSet<object> m_loaded;

        public GroupLoadOperation(int preloadNum)
        {
            preloadCount = preloadNum;
            if (preloadCount > 0)
            {
                m_loaded = new HashSet<object>(preloadNum);
            }
            else
            {
                SetFinish();
            }
        }
        /// <summary>
        /// 完成一步
        /// </summary>
        /// <param name="item"></param>
        public void CompleteOne(object item)
        {
            m_loaded.Add(item);
            progress = (float)m_loaded.Count / preloadCount;
            try
            {
                onProgressEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            if (m_loaded.Count == preloadCount)
            {
                SetFinish();
            }
        }
        /// <summary>
        /// 结束回调
        /// </summary>
        protected override void OnFinish()
        {
            if (_recovered)
                return;

            progress = 1;
            base.OnFinish();
        }

        #region UniTask
        [Preserve]
        public AsyncOperationAwaiter GetAwaiter()
        {
            return new AsyncOperationAwaiter(this);
        }
        /// <summary>
        /// UniTask异步句柄
        /// </summary>
        [Preserve]
        public struct AsyncOperationAwaiter : ICriticalNotifyCompletion
        {
            private GroupLoadOperation _asyncOperation;
            private Action<GroupLoadOperation> _continuationAction;

            public AsyncOperationAwaiter(GroupLoadOperation asyncOperation)
            {
                this._asyncOperation = asyncOperation;
                this._continuationAction = null;
            }

            public bool IsCompleted => _asyncOperation == null || _asyncOperation.isDone;
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            [Preserve]
            public int GetResult()
            {
                int preloadCount = _asyncOperation?.preloadCount ?? 0;
                if (_continuationAction != null)
                {
                    _asyncOperation?.RemoveComplete(OnFinish);
                    _continuationAction = null;
                    _asyncOperation = null;
                }
                else
                {
                    _asyncOperation = null;
                }
                return preloadCount;
            }

            private void OnFinish()
            {
                _continuationAction?.Invoke(_asyncOperation);
            }

            [Preserve]
            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }
            /// <summary>
            /// 多线程回调
            /// </summary>
            /// <param name="continuation"></param>
            [Preserve]
            public void UnsafeOnCompleted(Action continuation)
            {
                _continuationAction = PooledDelegate<GroupLoadOperation>.Create(continuation);
                _asyncOperation?.RegistComplete(OnFinish);
            }
        }
        #endregion UniTask
    }
}

