/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 资源加载控制器                                                                  *
*//************************************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Source
{
    public sealed class SourceController : ISyncSourceCtrl,IAsyncSyncSourceCtrl, IUpdate
    {
        #region protected Fields
        internal SourceCacheCtrl m_sourceCacheCtrl;
        internal ISourceLoadCtrl m_sourceLoadCtrl;
        internal SourceLoadType m_loadType = SourceLoadType.None;//资源加载
        internal FileLoadCtrl m_fileLoadCtrl;
        internal ResourceLoadCtrl m_resourceLoadCtrl;
        internal AssetBundleLoadCtrl m_bundleLoadCtrl;
        internal Dictionary<int, string> m_sourceDic;
        internal Transform m_cacheParent;
        internal bool m_asyncLoading;
        #endregion

        public float Interval => 0;

        #region Public Propertys
        public SourceLoadType sourceLoadType
        {
            get
            {
                return m_loadType;
            }
        }

        public bool Actived { get; internal set; }

        public bool Runing => m_asyncLoading;
        #endregion

        #region Management 
        public void OnRegist()
        {
            m_sourceDic = new Dictionary<int, string>();
            m_sourceCacheCtrl = new SourceCacheCtrl();
            SwitchResourceLoadCtrl<ResourceLoadCtrl>();
            this.m_cacheParent = new GameObject("SourceCachePool").GetComponent<Transform>();
            Object.DontDestroyOnLoad(m_cacheParent);
            Actived = true;
        }

        public void OnUnRegist()
        {
            m_sourceDic.Clear();
            m_sourceDic = null;
            m_sourceLoadCtrl = null;
            if (m_cacheParent && m_cacheParent.gameObject)
                Object.Destroy(m_cacheParent.gameObject);
            Actived = false;
        }
        #endregion

        #region Public Functions
        public void SwitchFileLoadCtrl<T>(string rootPath) where T : FileLoadCtrl, new()
        {
            if (m_fileLoadCtrl == null || !typeof(T).Equals(m_fileLoadCtrl.GetType()))
            {
                if (m_fileLoadCtrl != null)
                    m_fileLoadCtrl.Dispose();
                m_fileLoadCtrl = new T();
                m_fileLoadCtrl.SetRootPath(rootPath);
                m_fileLoadCtrl.Init(m_sourceCacheCtrl);
            }

            if (m_loadType != SourceLoadType.FileIO)
            {
                m_loadType = SourceLoadType.FileIO;
                OnSourceTypeChanged();
            }
        }

        public void SwitchResourceLoadCtrl<T>() where T : ResourceLoadCtrl, new()
        {
            if (m_resourceLoadCtrl == null || !typeof(T).Equals(m_resourceLoadCtrl.GetType()))
            {
                if (m_resourceLoadCtrl != null)
                    m_resourceLoadCtrl.Dispose();
                m_resourceLoadCtrl = new T();
                m_resourceLoadCtrl.Init(m_sourceCacheCtrl);
            }

            if (m_loadType != SourceLoadType.Resource)
            {
                m_loadType = SourceLoadType.Resource;
                OnSourceTypeChanged();
            }
        }


        public void SwitchBundleLoadCtrl<T>(UFrame.AssetBundles.IAssetBundleController aBundleCtrl) where T : AssetBundleLoadCtrl, new()
        {
            if (m_bundleLoadCtrl == null || !typeof(T).Equals(m_bundleLoadCtrl.GetType()))
            {
                if (m_bundleLoadCtrl != null)
                    m_bundleLoadCtrl.Dispose();
                m_bundleLoadCtrl = new T();
                m_bundleLoadCtrl.AssetBundleCtrl = aBundleCtrl;
                m_bundleLoadCtrl.Init(m_sourceCacheCtrl);
            }

            if (m_loadType != SourceLoadType.AssetBundle)
            {
                m_loadType = SourceLoadType.AssetBundle;
                OnSourceTypeChanged();
            }
        }

        public void OnUpdate()
        {
            if (Actived)
            {
                try
                {
                    m_asyncLoading = m_sourceLoadCtrl.UpdateLoadStates();
                }
                catch (SourceException e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public void RegistPrefab(int sourceID, GameObject prefab)
        {
            if (!Actived)
            {
                return;
            }
            var sourcePath = FindSourcePath(sourceID);

            try
            {
                m_sourceLoadCtrl.RegistRuntimePrefab(sourcePath, prefab);
            }
            catch (SourceException e)
            {
                Debug.LogError(e.Message);
            }
        }

        public T LoadSource<T>(string path, SourceLoadOption option = UFrame.Source.SourceLoadOption.None) where T : Object
        {
            if (!Actived) return null;

            try
            {
                if(typeof(Component).IsAssignableFrom(typeof(T)))
                {
                    var go = m_sourceLoadCtrl.LoadSource<GameObject>(path, option);
                    if (go)
                        return go.GetComponent<T>();
                    return null;
                }

                return m_sourceLoadCtrl.LoadSource<T>(path, option);
            }
            catch (SourceException e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public ICustomSourceHandle LoadGameObjectInstenceAsync(string sourcePath, LoadSourceCallBack<GameObject> callBack, object context = null)
        {
            try
            {
                m_asyncLoading = true;
                return LoadSourceAsync(sourcePath, callBack, SourceLoadOption.MakeCopy, context);
            }
            catch (SourceException e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }


        public ICustomSourceHandle LoadSourceAsync<T>(string sourcePath, LoadSourceCallBack<T> callBack, SourceLoadOption option = SourceLoadOption.None, object context = null) where T : UnityEngine.Object
        {
            if (!Actived) return null;
            try
            {
                m_asyncLoading = true;
                if (typeof(Component).IsAssignableFrom(typeof(T)))
                {
                    return m_sourceLoadCtrl.LoadSourceAsync<GameObject>(sourcePath, option, (go,context)=> {
                        T component = null;
                        if (go)
                            component = go.GetComponent<T>();
                        callBack?.Invoke(component, context);
                    }, context);
                }
                return m_sourceLoadCtrl.LoadSourceAsync<T>(sourcePath, option, callBack, context);
            }
            catch (SourceException e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public GameObject LoadGameObjectInstence(string sourcePath)
        {
            if (!Actived) return null;

            try
            {
                return m_sourceLoadCtrl.LoadSource<GameObject>(sourcePath, SourceLoadOption.MakeCopy);
            }
            catch (SourceException e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public void RecoverSource<T>(T obj) where T : Object
        {
            if (!Actived) return;
            try
            {
                SetObjectState(obj, false);
                if(obj is Component)
                {
                    m_sourceCacheCtrl.ReleaseSourceObjectToCatch<GameObject>((obj as Component).gameObject);
                }
                else
                {
                    m_sourceCacheCtrl.ReleaseSourceObjectToCatch<T>(obj);
                }
            }
            catch (SourceException e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 尝试设置对象的状态
        /// </summary>
        internal void SetObjectState(Object target, bool active)
        {
            if (target is GameObject || target is Component)
            {
                GameObject go;
                if (target is GameObject)
                {
                    go = target as GameObject;
                }
                else
                {
                    go = (target as Component).gameObject;
                }
                go.SetActive(active);

                if (!active)
                {
                    var haveRectTransfrom = go.transform is RectTransform;
                    go.transform.SetParent(m_cacheParent, !haveRectTransfrom);
                }
            }
        }

        public void ReleaseUnUsedSources()
        {
            if (Actived)
            {
                try
                {
                    if (m_sourceLoadCtrl != null)
                        m_sourceLoadCtrl.LowerMemeory();
                }
                catch (SourceException e)
                {
                    Debug.LogError(e.Message);
                }

            }
        }

        public string FindSourcePath(int sourceID)
        {
            m_sourceDic.TryGetValue(sourceID, out string sourcePath);
            return sourcePath;
        }
        #endregion

        #region protected Functions
        public void RegistSourcePath(int id, string path)
        {
            if (Actived)
            {
                if (!m_sourceDic.ContainsKey(id))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        m_sourceDic.Add(id, path);
                    }
                    else
                    {
                        UnityEngine.Debug.LogErrorFormat("资源路径不能为空：id{0}", id);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogErrorFormat("[重复注册资源]id:{0} path:{1}", id, path);
                }
            }
        }
        internal void OnSourceTypeChanged()
        {
            switch (m_loadType)
            {
                case SourceLoadType.Resource:
                    m_sourceLoadCtrl = m_resourceLoadCtrl;
                    break;
                case SourceLoadType.AssetBundle:
                    m_sourceLoadCtrl = m_bundleLoadCtrl;
                    break;
                case SourceLoadType.FileIO:
                    m_sourceLoadCtrl = m_fileLoadCtrl;
                    break;
                default:
                    break;
            }
            m_sourceLoadCtrl.onSetObjectState = SetObjectState;
        }

        #endregion
    }
}