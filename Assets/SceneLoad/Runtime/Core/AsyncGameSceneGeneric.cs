//*************************************************************************************
//* 作    者： 
//* 创建时间： 2021-08-08 09:48:00
//* 描    述：  

//* ************************************************************************************
namespace UFrame.SceneLoad
{
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 异步场景场景模板
    /// <summary>
    public class AsyncGameSceneGeneric<SID,T> : IGameSceneGeneric<SID>, IAsyncSceneLoader, IUpdateable, IEntryableGeneric<SID>,IAlive where T : AsyncGameSceneGeneric<SID,T>
    {
        public static T Current { get; protected set; }
        protected Dictionary<SID, T> m_sceneMap = new Dictionary<SID, T>();
        protected HashSet<ISceneHandle> loadingHandles = null;
        protected Stack<ISceneHandle> waitAdd = null;
        protected Stack<ISceneHandle> waitRemove = null;
        protected bool m_loading;
        public virtual bool Runing => m_loading;
        public bool Loading => m_loading;

        public bool Alive { get; protected set; }

        public static implicit operator bool(AsyncGameSceneGeneric<SID, T> scene)
        {
            return scene != null && scene.Alive;
        }

        public virtual void OnEntry(SID sceneId)
        {
            Current = this as T;
            m_sceneMap[sceneId] = Current;
            Alive = true;
        }

        public virtual void OnEnter(SID sceneId, bool alone)
        {
            Current = this as T;
            m_sceneMap[sceneId] = Current;
            Alive = true;
        }

        public virtual void OnExit(SID sceneId, bool alone)
        {
            if(m_sceneMap.Remove(sceneId))
            {
                using (var enumerator = m_sceneMap.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        Current = enumerator.Current.Value;
                    }
                    else
                    {
                        Current = default(T);
                    }
                }
            }
            if (m_loading)
            {
                if(loadingHandles?.Count > 0)
                {
                    foreach (var handle in loadingHandles)
                    {
                        handle.Cansale();
                    }
                    loadingHandles.Clear();
                }
                m_loading = false;
            }
            Alive = false;
        }

        public virtual void OnUpdate()
        {
            if(m_loading)
            {
                UpdateLoading();

                if (loadingHandles.Count == 0 && waitAdd.Count == 0 && waitRemove.Count == 0)
                {
                    m_loading = false;
                }
            }
        }

        public ISceneHandle LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            m_loading = true;
            var sceneHandle = new SceneLoadHandle(sceneName);
            sceneHandle.loadSceneMode = mode;
            sceneHandle.onComplete = PushToWaitRemove;
            PushToWaitAdd(sceneHandle);
            return sceneHandle;
        }

        public ISceneHandle LoadSceneAsync<Loader>(string bundleName, string sceneName, LoadSceneMode mode) where Loader : CustomSceneLoadHandle, new()
        {
            m_loading = true;
            var sceneHandle = new Loader();
            sceneHandle.assetBundleName = bundleName;
            sceneHandle.sceneName = sceneName;
            sceneHandle.loadSceneMode = mode;
            sceneHandle.onComplete = PushToWaitRemove;
            PushToWaitAdd(sceneHandle);
            return sceneHandle;
        }

        protected virtual void UpdateLoading()
        {
            if (waitAdd?.Count > 0)
            {
                var item = waitAdd.Pop();
                PushToLoading(item);
            }

            if (loadingHandles != null)
            {
                using (var enumerator = loadingHandles.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        if (item.Started)
                        {
                            enumerator.Current.UpdateState();
                        }
                    }
                }
            }
            if (waitRemove?.Count > 0)
            {
                var item = waitRemove.Pop();
                loadingHandles.Remove(item);
            }
        }

        private void PushToLoading(ISceneHandle sceneHandle)
        {
            if (loadingHandles == null)
                loadingHandles = new HashSet<ISceneHandle>();
            loadingHandles.Add(sceneHandle);
            if (!sceneHandle.Started)
                sceneHandle.StartLoad();
        }

        private void PushToWaitAdd(ISceneHandle sceneHandle)
        {
            if (waitAdd == null)
                waitAdd = new Stack<ISceneHandle>();
            waitAdd.Push(sceneHandle);
        }

        private void PushToWaitRemove(ISceneHandle sceneHandle)
        {
            if (waitRemove == null)
                waitRemove = new Stack<ISceneHandle>();
            waitRemove.Push(sceneHandle);
        }

    }
}