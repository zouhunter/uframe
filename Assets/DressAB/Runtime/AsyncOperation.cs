//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace UFrame.DressAB
{
    public class AsyncOperation<T> : IDisposable, IEnumerator where T : AsyncOperation<T>
    {
        public bool isDone { get; protected set; }
        public string error { get; protected set; }

        public object Current => null;

        protected List<Action<T>> completed;
        protected List<Action> completed0;
        private event Action<float> m_onProgressEvent;

        protected void SetFinish()
        {
            if (!isDone)
            {
                isDone = true;
                SetProgress(1);
                OnFinish();
            }
            else
            {
                Debug.LogError("operation already finished!");
            }
        }

        protected void SetError(string error)
        {
            this.error = error;
        }

        public void RegistComplete(Action<T> callback)
        {
            if (isDone)
            {
                try
                {
                    callback?.Invoke((T)this);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (completed == null)
                    completed = new List<Action<T>>(1);
                this.completed.Add(callback);
            }
        }
        public void RegistComplete(Action callback)
        {
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
                if (completed0 == null)
                    completed0 = new List<Action>(1);
                this.completed0.Add(callback);
            }
        }


        public virtual void Dispose()
        {
            completed = null;
        }

        protected virtual void OnFinish()
        {
            if (completed != null)
            {
                foreach (var callback in completed)
                {
                    try
                    {
                        callback?.Invoke((T)this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                completed.Clear();
            }

            if (completed0 != null)
            {
                foreach (var callback in completed0)
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
                completed0.Clear();
            }
        }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
            //isDone = false;
        }


        public void SetProgress(float progress)
        {
            m_onProgressEvent?.Invoke(progress);
        }

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
                m_onProgressEvent += onProgress;
            }
        }
    }
    public class AsyncVersionOperation : AsyncOperation<AsyncVersionOperation>
    {
        public string verison;
        public void SetVerion(string verison)
        {
            this.verison = verison;
            SetFinish();
        }
    }
    public class AsyncCatlogOperation : AsyncOperation<AsyncCatlogOperation>
    {
        public string catlogPath;
        private List<Action<string>> pathComplated;

        public void SetCatlogPath(string catlogPath)
        {
            this.catlogPath = catlogPath;
            SetFinish();
        }


        public void RegistComplete(Action<string> callback)
        {
            if (isDone)
            {
                try
                {
                    callback?.Invoke(catlogPath);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (pathComplated == null)
                    pathComplated = new List<Action<string>>(1);
                pathComplated.Add(callback);
            }
        }

        protected override void OnFinish()
        {
            base.OnFinish();
            if (pathComplated != null)
            {
                foreach (var callback in pathComplated)
                {
                    try
                    {
                        callback?.Invoke(catlogPath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                pathComplated.Clear();
            }
        }
    }

    public class AsyncBundleOperation : AsyncOperation<AsyncBundleOperation>
    {
        public BundleItem abItem { get; private set; }
        public AssetBundle assetBundle;
        private List<object> m_locks = new List<object>();
        private Action<BundleItem, bool> m_unloadEvent;

        public bool InUse => m_locks.Count > 0;

        public AsyncBundleOperation(BundleItem abItem)
        {
            this.abItem = abItem;
        }

        public void ReleaseAssetBundle(bool unloadAll)
        {
            m_unloadEvent?.Invoke(abItem, unloadAll);
        }

        public void UnLock(object lockObj)
        {
            m_locks.Remove(lockObj);
            if (m_locks.Count <= 0)
                ReleaseAssetBundle(true);
        }

        public void Lock(object lockObj)
        {
            m_locks.Add(lockObj);
        }

        public void RegistUnloadBundle(Action<BundleItem, bool> callback)
        {
            m_unloadEvent = callback;
        }

        public void SetAssetBundle(string assetBundleName, AssetBundle assetBundle)
        {
            if (assetBundleName == abItem.bundleName)
            {
                this.assetBundle = assetBundle;
                SetFinish();
            }
        }
    }


    public class AsyncAssetsOperation<T> : AsyncOperation<AsyncAssetsOperation<T>> where T : UnityEngine.Object
    {
        public T[] assets;
        private AsyncBundleOperation m_bundleOperation;
        protected List<Action<T[]>> assetCompleted;

        public void SetAssetBundle(AsyncBundleOperation bundleOperation)
        {
            m_bundleOperation = bundleOperation;
            if (m_bundleOperation != null && m_bundleOperation.assetBundle != null)
            {
                var allAssets = m_bundleOperation.assetBundle.LoadAllAssets<T>();
                SetAsset(allAssets);
            }
        }

        protected void SetAsset(T[] assets)
        {
            if (assets != null)
            {
                m_bundleOperation?.Lock(this);
                this.assets = assets;
                SetFinish();
            }
        }

        public override void Dispose()
        {
            m_bundleOperation?.UnLock(this);
        }

        public void RegistComplete(System.Action<T[]> callback)
        {
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

        protected override void OnFinish()
        {
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
    }

    public class AsyncAssetOperation<T> : AsyncOperation<AsyncAssetOperation<T>> where T : UnityEngine.Object
    {
        public T asset;
        private AsyncBundleOperation m_bundleOperation;
        private string m_assetName;
        protected List<Action<T>> assetCompleted;

        public AsyncAssetOperation(string assetName = null)
        {
            this.m_assetName = assetName;
        }
        public void SetAssetBundle(AsyncBundleOperation bundleOperation)
        {
            m_bundleOperation = bundleOperation;
            if (m_bundleOperation != null && m_bundleOperation.assetBundle != null)
            {
                T asset = null;
                if (!string.IsNullOrEmpty(m_assetName))
                {
                    asset = m_bundleOperation.assetBundle.LoadAsset<T>(m_assetName);
                }
                if(!asset)
                {
                    var allAssets = m_bundleOperation.assetBundle.LoadAllAssets<T>();
                    asset = Array.Find(allAssets, x => x.name.ToLower().Contains(m_assetName.ToLower()));
                    if (!asset)
                    {
                        asset = allAssets[0];
                    }
                }
                if(asset)
                    SetAsset(asset);
            }
        }
        protected void SetAsset(T asset)
        {
            if (asset)
            {
                m_bundleOperation?.Lock(this);
                this.asset = asset;
                SetFinish();
            }
        }
        public override void Dispose()
        {
            m_bundleOperation?.UnLock(this);
        }

        public void RegistComplete(System.Action<T> callback)
        {
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

        protected override void OnFinish()
        {
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
    }

    public class AsyncSceneOperation : AsyncOperation<AsyncSceneOperation>
    {
        public AsyncOperation sceneLoadOperation;
        public bool success => sceneLoadOperation != null;
        private AsyncBundleOperation m_bundleOperation;
        private string m_sceneName;
        private UnityEngine.SceneManagement.LoadSceneMode m_loadSceneMode;
        private event Action<AsyncOperation> sceneLoadEvent;
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

        public AsyncSceneOperation(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            this.m_sceneName = sceneName;
            this.m_loadSceneMode = loadSceneMode;
        }

        public void SetAssetBundle(AsyncBundleOperation bundleOperation)
        {
            m_bundleOperation = bundleOperation;
            if (m_bundleOperation != null && m_bundleOperation.assetBundle != null)
            {
                if (string.IsNullOrEmpty(m_sceneName))
                {
                    var allScenePaths = bundleOperation.assetBundle.GetAllScenePaths();
                    if (allScenePaths != null && allScenePaths.Length > 0)
                    {
                        m_sceneName = allScenePaths[0];
                    }
                }
                LoadScene();
            }
        }
        public void RegistSceneLoadComplete(Action<AsyncOperation> callback)
        {
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
        protected void LoadScene()
        {
            if (string.IsNullOrEmpty(m_sceneName))
            {
                Debug.LogError("empty sceneName");
                return;
            }
            sceneLoadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(this.m_sceneName, this.m_loadSceneMode);
            sceneLoadOperation.completed += OnSceneLoadComplete;
            sceneLoadOperation.allowSceneActivation = allowSceneActivation;
            SetFinish();
            m_bundleOperation?.Lock(this);
        }

        private void OnSceneLoadComplete(AsyncOperation operation)
        {
            try
            {
                this.sceneLoadEvent?.Invoke(operation);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            this.sceneLoadEvent = null;
        }

        public override void Dispose()
        {
            m_bundleOperation?.UnLock(this);
        }
    }

    public class AsyncFileBundleOperation : AsyncOperation<AsyncFileBundleOperation>
    {
        public Queue<Action<BundleItem,string, AssetBundle>> loadBundleCallBack = new Queue<Action<BundleItem, string, AssetBundle>>();
        public AssetBundle assetbundle { get; private set; }
        public BundleItem bundleItem { get; private set; }
        private AssetBundleCreateRequest request;
        private string filePath;

        public AsyncFileBundleOperation(string filePath, BundleItem bundleItem)
        {
            this.filePath = filePath;
            this.bundleItem = bundleItem;
        }

        public void StartRequest()
        {
            if (request != null)
                return;
            try
            {
                request = AssetBundle.LoadFromFileAsync(filePath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            if (request.isDone)
            {
                OnLoadBundleFromFile(request.assetBundle);
            }
            else
            {
                request.completed += OnLoadBundleFromFile;
            }
        }

        private void OnLoadBundleFromFile(AsyncOperation operation)
        {
            if (operation.isDone)
            {
                OnLoadBundleFromFile(request.assetBundle);
            }
        }

        private void OnLoadBundleFromFile(AssetBundle assetbundle)
        {
            this.assetbundle = assetbundle;
            foreach (var action in loadBundleCallBack)
            {
                try
                {
                    action?.Invoke(bundleItem,filePath, assetbundle);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            loadBundleCallBack.Clear();
            SetFinish();
        }

        internal void RegistABLoadComplate(Action<BundleItem,string, AssetBundle> loadBundleCallBack)
        {
            if (isDone)
            {
                loadBundleCallBack?.Invoke(bundleItem, filePath, assetbundle);
            }
            else
            {
                this.loadBundleCallBack.Enqueue(loadBundleCallBack);
            }
        }
    }
    public class AsyncPreloadOperation : AsyncOperation<AsyncPreloadOperation>
    {
        public int preloadCount { get; private set; }
        public float progress { get; private set; }
        public event Action<AsyncPreloadOperation> onProgressEvent;
        private HashSet<string> m_loadedBundles;

        public AsyncPreloadOperation(int preloadNum)
        {
            preloadCount = preloadNum;
            if (preloadCount > 0)
            {
                m_loadedBundles = new HashSet<string>(preloadNum);
            }
            else
            {
                SetFinish();
            }
        }

        public void SetAssetBundle(string assetBundleName, object content)
        {
            m_loadedBundles.Add(assetBundleName);
            progress = (float)m_loadedBundles.Count / preloadCount;
            try
            {
                onProgressEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            if (m_loadedBundles.Count == preloadCount)
            {
                SetFinish();
            }
        }
        protected override void OnFinish()
        {
            progress = 1;
            base.OnFinish();
        }
    }

}

