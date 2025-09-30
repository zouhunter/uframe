/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源加载控制器模板                                                              *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UFrame.Pool;

namespace UFrame.Source
{
    public abstract class SourceLoadCtrl<Handle> : ISourceLoadCtrl where Handle : SourceLoadHandle, new()
    {
        protected Dictionary<string, Object> runtimePrefab;//预制体缓存
        protected SourceCacheCtrl sourceCatchCtrl;
        protected Dictionary<string, Handle> loadingDic;
        protected Dictionary<string, List<Handle>> reLoadingDic;
        protected ObjectPool<Handle> sourceLoadHandlePool;
        protected ObjectPool<AsyncLoadInfo> asyncLoadInfoPool;
        protected SourceLoadedHandle loadedHandle;
        protected List<string> loadingPaths;
        public System.Action<UnityEngine.Object, bool> onSetObjectState { get; set; }
        protected bool m_inited;
        public void Init(SourceCacheCtrl sourceCatchCtrl)
        {
            if(m_inited)
                return;

            runtimePrefab = new Dictionary<string, Object>();
            loadedHandle = new SourceLoadedHandle();
            this.sourceCatchCtrl = sourceCatchCtrl;
            loadingPaths = new List<string>();
            loadingDic = new Dictionary<string, Handle>();
            reLoadingDic = new Dictionary<string, List<Handle>>();
            sourceLoadHandlePool = new ObjectPool<Handle>(10, ()=> new Handle());
            asyncLoadInfoPool = new ObjectPool<AsyncLoadInfo>(10, () => new AsyncLoadInfo());
            m_inited = true;
        }

        #region Public
        public void Dispose()
        {
            if(m_inited)
            {
                runtimePrefab.Clear();
                loadingDic.Clear();
                sourceCatchCtrl.Dispose();
                m_inited = false;
            }
        }
        /// <summary>
        /// 更新加载状态
        /// </summary>
        public virtual bool UpdateLoadStates()
        {
            if (loadingDic.Count > 0)
            {
                loadingPaths.Clear();
                loadingPaths.AddRange(loadingDic.Keys);

                using (var enumerator = loadingPaths.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var handle = loadingDic[enumerator.Current];
                        Object asset = null;

                        bool complete = ProcessHandle(handle, out asset);

                        if (complete)
                        {
                            loadingDic.Remove(enumerator.Current);
                            ProcessReloadingHandles(handle.path, asset);
                            ReleaseReLoadingHandles();
                            reLoadingDic.Remove(enumerator.Current);
                        }
                    }

                }
            }
            return loadingDic.Count > 0;
        }
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public T LoadSource<T>(string path, SourceLoadOption option) where T : UnityEngine.Object
        {
            var cacheObject = sourceCatchCtrl.AllocateSourceObjectFromCatch<T>(path);
            if (cacheObject == null)
            {
                var prefab = LoadPrefabRuntime<T>(path);

                if (prefab == null)
                {
                    prefab = LoadPrefabInternal<T>(path);
                }

                if (prefab != null)
                {
                    cacheObject = OnSourceLoad(path, prefab, option);
                }
            }

            if (cacheObject != null)
            {
                SetObjectState(cacheObject, true);
            }
            return cacheObject;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="option"></param>
        /// <param name="callBack"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ICustomSourceHandle LoadSourceAsync<T>(string path, SourceLoadOption option, LoadSourceCallBack<T> callBack, object context) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new SourceException("路径为空，不支持！");
            }

            if (callBack == null)
            {
                throw new SourceException("回调函数为空，不支持！");
            }

            var cacheObject = sourceCatchCtrl.AllocateSourceObjectFromCatch<T>(path);

            if (cacheObject == null)
            {
                var prefab = LoadPrefabRuntime<T>(path);

                if (prefab == null)
                {
                    var asyncInfo = asyncLoadInfoPool.GetObject();
                    asyncInfo.onSetObjectState = SetObjectState;
                    asyncInfo.onSourceLoad = OnSourceLoad;
                    asyncInfo.callBack = callBack;
                    asyncInfo.option = option;
                    asyncInfo.path = path;
                    asyncInfo.onRelease = asyncLoadInfoPool.Store;
                    var handle = LoadPrefabAsyncInternal<T>(path, asyncInfo.CallBack, asyncInfo.EmptyCallBack, context);
                    if(handle !=null)
                    {
                        RegistLoading(path, handle);
                        handle.needRecover = false;
                        handle.onRelease = OnReleaseHandle;
                    }
                    return handle;
                }
                else
                {
                    if (typeof(Component).IsAssignableFrom(typeof(T)) && prefab is GameObject)
                    {
                        var go = OnSourceLoad<GameObject>(path, prefab as GameObject, option);
                        if (go)
                            cacheObject = go.GetComponent<T>();
                    }
                    else
                    {
                        cacheObject = OnSourceLoad<T>(path, prefab, option);
                    }
                }
            }
            if(cacheObject != null)
            {
                callBack.Invoke(cacheObject, context);
                SetObjectState(cacheObject, true);
            }
            return loadedHandle;
        }
        /// <summary>
        /// 资源内存
        /// </summary>
        public virtual void LowerMemeory()
        {
            runtimePrefab.Clear();
            sourceCatchCtrl.OnLowMemory();
        }

        public void RegistRuntimePrefab(string sourcePath, Object prefab)
        {
            runtimePrefab[sourcePath] = prefab;
        }

        public void RemoveRuntimePrefab(string sourcePath)
        {
            runtimePrefab.Remove(sourcePath);
            sourceCatchCtrl.ClearCatchs(sourcePath);
        }
        /// <summary>
        /// 中止正在下载的处理机
        /// </summary>
        /// <param name="context"></param>
        public void CansaleLoadAllByContext(object context)
        {
            using (var enumerator = loadingDic.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var handle = enumerator.Current;
                    if(handle.context != null && handle.context == context)
                    {
                        handle.needRecover = true;
                    }
                }
            }

            using (var enumerator = reLoadingDic.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var listEnumerator = enumerator.Current.GetEnumerator();
                    while (listEnumerator.MoveNext())
                    {
                        var handle = listEnumerator.Current;
                        if(handle.context != null && handle.context == context)
                        {
                            handle.needRecover = true;
                        }
                    }
                }
            }
        }
        #endregion Public

        #region Protected

        /// <summary>
        /// 外部释放
        /// </summary>
        /// <param name="handle"></param>
        protected void OnReleaseHandle(SourceLoadHandle handle)
        {
            if (handle is Handle)
            {
                sourceLoadHandlePool.Store(handle as Handle);
            }
        }

        /// <summary>
        /// 回收重复加载的索引 
        /// </summary>
        protected void ReleaseReLoadingHandles()
        {
            using (var reloadings = reLoadingDic.Values.GetEnumerator())
            {
                while (reloadings.MoveNext())
                {
                    var array = reloadings.Current;
                    for (int i = 0; i < array.Count; i++)
                    {
                        var subhandle = array[i];
                        {
                            sourceLoadHandlePool.Store(subhandle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理异步加载节点
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        protected abstract bool ProcessHandle(Handle handle, out Object asset);

        /// <summary>
        /// 执行重复加载的资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="asset"></param>
        protected void ProcessReloadingHandles(string path, Object asset)
        {
            if (reLoadingDic.ContainsKey(path))
            {
                if (asset != null)
                {
                    using (var reloadings = reLoadingDic.Values.GetEnumerator())
                    {
                        while (reloadings.MoveNext())
                        {
                            var array = reloadings.Current;
                            for (int i = 0; i < array.Count; i++)
                            {
                                var handle = array[i];
                                try
                                {
                                    if (!handle.needRecover)
                                    {
                                        handle.callBack.DynamicInvoke(asset, handle.context);
                                    }
                                    else
                                    {
                                        handle.callBack.DynamicInvoke(null, handle.context);
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    throw new SourceException("回调执行失败:" + e);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 下载成功后处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="prefab"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        protected T OnSourceLoad<T>(string path, T prefab, SourceLoadOption option) where T : UnityEngine.Object
        {
            runtimePrefab[path] = prefab;

            T cacheObject = null;
            if (option == SourceLoadOption.MakeCopy)
            {
                cacheObject = Object.Instantiate(prefab);
                cacheObject.name = prefab.name;
                sourceCatchCtrl.RecordSourceObjectToCatch(path, cacheObject);
            }
            else
            {
                cacheObject = prefab;
                sourceCatchCtrl.RecordSourceObjectToCatch(path, cacheObject);
            }
            return cacheObject;
        }
        /// <summary>
        /// 记录正在下载的索引
        /// </summary>
        /// <param name="path"></param>
        /// <param name="handle"></param>
        protected void RegistLoading(string path, Handle handle)
        {
            if (!loadingDic.ContainsKey(path))
            {
                loadingDic.Add(path, handle);
            }
            else
            {
                if (!reLoadingDic.ContainsKey(path) || reLoadingDic[path] == null)
                {
                    reLoadingDic[path] = new List<Handle>();
                }
                reLoadingDic[path].Add(handle);
            }
        }

        /// <summary>
        /// 从缓存中查找预制体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        protected T LoadPrefabRuntime<T>(string path) where T : UnityEngine.Object
        {
            if (runtimePrefab.ContainsKey(path))
            {
                return runtimePrefab[path] as T;
            }
            return null;
        }
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        protected abstract T LoadPrefabInternal<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        ///异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract Handle LoadPrefabAsyncInternal<T>(string path, LoadSourceCallBack<T> callBack,System.Action emptyCallBack, object context) where T : Object;
        #endregion Protected

        private void SetObjectState<T>(T cacheObject, bool state) where T : UnityEngine.Object
        {
            if (onSetObjectState != null)
            {
                onSetObjectState.Invoke(cacheObject, state);
            }
        }

    }
}