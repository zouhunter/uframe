//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:09:32
//* 描    述：

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.LitUI
{
    public class UIController : IUICtrl
    {
        private IUILoader m_loader;
        private Dictionary<string, UIInfo> m_infos;
        private HashSet<string> m_asyncOperations;
        private Dictionary<UIView, UIInfo> m_createdViews;
        private List<UIView> m_stackedViews;
        private bool m_inited;
        private Dictionary<string, UIAsyncOperation> m_inUseOperations;
        private Transform m_root;
        private Transform m_defaultRoot;
        public event Action<UIInfo> onUIOpenEvent;
        public event Action<UIInfo> onUICloseEvent;

        public bool Vaild
        {
            get
            {
                if (!m_inited || m_loader == null)
                    return false;
                return true;
            }
        }

        public void Initialize(IUILoader loader)
        {
            this.m_loader = loader;
            if (!m_inited)
            {
                m_infos = new Dictionary<string, UIInfo>();
                m_asyncOperations = new HashSet<string>();
                m_createdViews = new Dictionary<UIView, UIInfo>();
                m_inUseOperations = new Dictionary<string, UIAsyncOperation>();
                m_stackedViews = new List<UIView>();
                m_inited = true;
            }
        }

        public void Release()
        {
            if (m_inited)
            {
                m_inited = false;
                foreach (var pair in m_inUseOperations)
                {
                    if (!pair.Value.isDone)
                        pair.Value.Cancel();
                }
                m_asyncOperations.Clear();
                m_inUseOperations.Clear();
                var views = new List<UIView>(m_createdViews.Keys);
                foreach (var pair in views)
                    pair.Close();
                m_createdViews.Clear();
                m_infos = null;
                m_asyncOperations = null;
                m_createdViews = null;
                m_inUseOperations = null;
                m_stackedViews = null;
                if (m_defaultRoot)
                    UnityEngine.Object.Destroy(m_defaultRoot.gameObject);
            }
        }

        public void RegistUIInfos(params UIInfo[] infos)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                if (info == null)
                    continue;

                if (!m_infos.TryGetValue(info.name,out var oldInfo)||oldInfo == null)
                {
                    m_infos[info.name] = info;
                }
                else
                {
                    info.CopyTo(oldInfo);
                }
            }
        }

        public UIAsyncOperation Open(string name, object arg = null, Transform parent = null)
        {
            if (!Vaild)
            {
                Debug.LogError("faild open ui:" + name);
                return null;
            }
            if (!m_infos.TryGetValue(name, out var uiInfo))
            {
                uiInfo = new UIInfo() { name = name};
                m_infos[name] = uiInfo;
            }
            return Open(uiInfo, arg, parent);
        }

        protected virtual void OnUILoadBack(UIAsyncOperation operation)
        {
            if (m_asyncOperations.Remove(operation.info.name))
            {
                if (operation.isDone && operation.view != null)
                {
                    m_stackedViews.Remove(operation.view);
                    TryStackMutex(operation.info);
                    InsertUIToRoot(operation);
                    m_createdViews.Add(operation.view, operation.info);
                    operation.view.onClose += OnUIClosed;
                    while (operation.argQueue.Count > 0)
                        operation.view.OnOpen(operation.argQueue.Dequeue());
                    if (operation.hideOnCreate)
                        operation.view.SetActive(false);
                    if (operation.stackOnCreate)
                        Stack(operation.info.name);
                    onUIOpenEvent?.Invoke(operation.info);
                }
                else
                {
                    m_inUseOperations.Remove(operation.info.name);
                }
            }
        }

        protected virtual void OnUIClosed(UIView view)
        {
            if (!Vaild)
                return;
            if (m_createdViews.TryGetValue(view, out var info))
            {
                TryUnStackMutex(info.layer);
                onUICloseEvent?.Invoke(info);
                m_createdViews.Remove(view);
                m_inUseOperations.Remove(info.name);
                m_asyncOperations.Remove(info.name);
                m_stackedViews.Remove(view);
            }
        }

        public UIAsyncOperation Open(UIInfo info, object arg = null, Transform parent = null)
        {
            Debug.Log("open:" + info.name);

            if (!Vaild)
                return null;

            if (m_inUseOperations.TryGetValue(info.name, out var operation))
            {
                if ((operation.isDone && operation.view) || !operation.isDone)
                {
                    OnReopenView(operation,arg);
                    return operation;
                }
                m_inUseOperations.Remove(info.name);
                if (operation != null)
                    operation.Cancel();
            }
            operation = new UIAsyncOperation(info);
            operation.argQueue.Enqueue(arg);
            operation.parent = parent;
            m_asyncOperations.Add(info.name);
            m_inUseOperations.Add(operation.info.name, operation);
            m_loader.Load(operation);
            if (!operation.isDone)
            {
                operation.RegistComplete(OnUILoadBack);
            }
            else
            {
                OnUILoadBack(operation);
            }
            return operation;
        }

        private void OnReopenView(UIAsyncOperation operation,object arg)
        {
            if (operation.view && !operation.view.isActiveAndEnabled)
            {
                operation.view.SetActive(true);
                operation.view.OnOpen(arg);
                onUIOpenEvent?.Invoke(operation.info);
                m_stackedViews.Remove(operation.view);
                TryStackMutex(operation.info);
            }
            if (!operation.isDone)
            {
                Debug.Log($"ui {operation.info.name} is opening!");
                operation.argQueue.Enqueue(arg);
            }
            else
            {
                operation.view.OnOpen(arg);
            }
        }

        public bool Close(string name)
        {
            if (!Vaild)
                return false;
            if (m_inUseOperations.TryGetValue(name, out var operation))
            {
                if (!operation.isDone || operation.view == null)
                {
                    operation.Cancel();

                }
                else
                {
                    operation.view.Close();
                }
                m_inUseOperations.Remove(name);
                m_asyncOperations.Remove(name);
                onUICloseEvent?.Invoke(operation.info);
                return true;
            }
            return false;
        }

        public UIView FindView(string name)
        {
            if (!Vaild)
                return null;
            if (m_inUseOperations.TryGetValue(name, out var operation))
            {
                return operation.view;
            }
            return null;
        }

        public UIView[] FindViews(byte layer)
        {
            if (!Vaild)
                return null;
            var count = 0;
            foreach (var pair in m_createdViews.Values)
            {
                if (pair.layer == layer)
                {
                    count++;
                }
            }
            var views = new UIView[count];
            count = 0;
            foreach (var pair in m_createdViews)
            {
                if (pair.Value.layer == layer)
                {
                    views[count++] = pair.Key;
                }
            }
            return views;
        }
        public void HideALL(byte layer = byte.MaxValue)
        {
            if (!Vaild)
                return;

            foreach (var pair in m_inUseOperations)
            {
                var info = pair.Value.info;
                if (layer == byte.MaxValue || info.layer != layer)
                    continue;

                if (m_asyncOperations.Contains(pair.Key) && !pair.Value.isDone)
                {
                    pair.Value.hideOnCreate = true;
                }
                else if (pair.Value.view != null)
                {
                    pair.Value.view.SetActive(false);
                }
            }
        }

        public void UnHideALL(byte layer = byte.MaxValue)
        {
            if (!Vaild)
                return;
            foreach (var pair in m_inUseOperations)
            {
                var info = pair.Value.info;
                if (layer == byte.MaxValue || info.layer != layer)
                    continue;
                if (m_asyncOperations.Contains(pair.Key) && !pair.Value.isDone)
                {
                    pair.Value.hideOnCreate = false;
                }
                else if (pair.Value.view != null)
                {
                    pair.Value.view.SetActive(true);
                }
            }
        }

        public void StackALL(byte layer)
        {
            if (!Vaild)
                return;
            foreach (var pair in m_inUseOperations)
            {
                var info = pair.Value.info;
                if (layer == byte.MaxValue || info.layer != layer)
                    continue;

                if (m_asyncOperations.Contains(pair.Key) && !pair.Value.isDone)
                {
                    pair.Value.stackOnCreate = true;
                }
                else if (pair.Value.view != null)
                {
                    if (!m_stackedViews.Contains(pair.Value.view))
                    {
                        pair.Value.view.SetActive(false);
                        m_stackedViews.Add(pair.Value.view);
                    }
                }

            }
        }

        public void UnStackAll(byte layer)
        {
            if (!Vaild)
                return;
            foreach (var pair in m_inUseOperations)
            {
                var info = pair.Value.info;
                if (layer == byte.MaxValue || info.layer != layer)
                    continue;

                if (m_asyncOperations.Contains(pair.Key) && !pair.Value.isDone)
                {
                    pair.Value.stackOnCreate = false;
                }
                else if (pair.Value.view != null)
                {
                    if (m_stackedViews.Contains(pair.Value.view))
                    {
                        pair.Value.view.SetActive(true);
                        m_stackedViews.Remove(pair.Value.view);
                    }
                }
            }
        }

        protected bool Stack(UIView view)
        {
            if (view == null)
                return false;
            if (m_stackedViews.Contains(view))
                return false;
            m_stackedViews.Add(view);
            view.SetActive(false);
            return true;
        }

        private bool Stack(UIAsyncOperation operation)
        {
            if (operation.view)
            {
                return Stack(operation.view);
            }
            else if (!operation.isDone)
            {
                operation.stackOnCreate = true;
                return true;
            }
            return false;
        }

        public bool Stack(string name)
        {
            if (!Vaild)
                return false;

            if (m_inUseOperations.TryGetValue(name, out var operation))
            {
                return Stack(operation);
            }
            return false;
        }

        protected bool UnStack(UIView view)
        {
            if (m_stackedViews.Contains(view))
            {
                m_stackedViews.Remove(view);
                view.SetActive(true);
                return true;
            }
            return false;
        }

        public bool UnStack(string name)
        {
            if (!Vaild)
                return false;
            if (m_inUseOperations.TryGetValue(name, out var operation))
            {
                if (!operation.isDone)
                {
                    operation.stackOnCreate = false;
                    return true;
                }
                else if (operation.view != null)
                {
                    return UnStack(operation.view);
                }
            }
            return false;
        }

        public void GetActiveViews(List<string> names, bool includeStack = false)
        {
            if (!Vaild)
                return;
            foreach (var pair in m_createdViews)
            {
                if (!includeStack && m_createdViews.ContainsKey(pair.Key))
                    continue;
                names.Add(pair.Value.name);
            }
            return;
        }

        public bool Hide(string name)
        {
            if (!Vaild)
                return false;
            var hideOk = false;
            if (m_inUseOperations.TryGetValue(name, out var operation))
            {
                if (operation.isDone && operation.view)
                {
                    if (operation.view.Showing)
                        operation.view.SetActive(false);
                    hideOk = true;
                }
                else if (!operation.isDone)
                {
                    operation.hideOnCreate = true;
                    hideOk = true;
                }
            }
            return hideOk;
        }

        public bool UnHide(string name)
        {
            if (!Vaild)
                return false;
            bool unHideOk = false;
            if (m_inUseOperations.TryGetValue(name, out var operation))
            {
                if (operation.isDone && operation.view)
                {
                    if (!operation.view.Showing)
                        operation.view.SetActive(true);
                    unHideOk = true;
                }
                else if (!operation.isDone)
                {
                    operation.hideOnCreate = false;
                    unHideOk = true;
                }
            }
            return unHideOk;
        }
        public void SetUIRoot(Transform root)
        {
            if (m_root != null)
            {
                while (m_root.childCount > 0)
                {
                    var child = m_root.GetChild(0);
                    child.transform.SetParent(root, false);
                }
                GameObject.Destroy(m_root.gameObject);
            }
            this.m_root = root;
        }

        protected void TryUnStackMutex(byte layer)
        {
            for (int i = m_stackedViews.Count - 1; i >= 0; i--)
            {
                var view = m_stackedViews[i];
                if (m_createdViews.TryGetValue(view, out var info))
                {
                    if (info.Mutex && info.layer == layer)
                    {
                        if (UnStack(view))
                            break;
                    }
                }
            }
        }
        public void CleanStacks(bool hideOnly = true, byte layer = byte.MaxValue)
        {
            if (!Vaild)
                return;

            var keys = new List<string>(m_inUseOperations.Keys);
            foreach (var key in keys)
            {
                var operation = m_inUseOperations[key];

                if (layer != byte.MaxValue && operation.info.layer != layer)
                    continue;

                if (m_asyncOperations.Contains(key) && !operation.isDone && operation.stackOnCreate)
                {
                    operation.Cancel();
                    m_inUseOperations.Remove(key);
                    m_asyncOperations.Remove(key);
                }

                var view = operation.view;
                if (view && m_stackedViews.Contains(operation.view))
                {
                    m_stackedViews.Remove(view);

                    if (hideOnly)
                    {
                        view.SetActive(false);
                    }
                    else
                    {
                        m_createdViews.Remove(view);
                        view.Close();
                        m_inUseOperations.Remove(key);
                    }
                }
            }
        }

        protected void TryStackMutex(UIInfo info)
        {
            foreach (var pair in m_inUseOperations)
            {
                var otherInfo = pair.Value.info;
                if (otherInfo == info)
                    continue;
                if (otherInfo.layer == info.layer && info.Mutex && otherInfo.Mutex)
                {
                    Stack(pair.Value);
                }
            }
        }

        protected void InsertUIToRoot(UIAsyncOperation operation)
        {
            if (!operation.view)
                return;

            var parent = operation.parent;
            if (parent == null)
            {
                if (m_root == null)
                {
                    if (m_defaultRoot == null)
                        m_defaultRoot = new GameObject("UIRoot").GetComponent<Transform>();
                    m_root = m_defaultRoot;
                }
                parent = m_root;
            }
            var uiInfo = operation.info;
            var placeIndex = 0;
            bool findLayer = false;
            foreach (Transform tran in parent)
            {
                if (tran == parent)
                    continue;
                var view = tran.GetComponent<UIView>();
                if (view && m_createdViews.TryGetValue(view, out var otherInfo))
                {
                    if (uiInfo.layer < otherInfo.layer)
                    {
                        findLayer = true;
                        break;
                    }
                }
                if (!findLayer)
                    placeIndex++;
            }
            operation.view.transform.SetParent(parent, false);
            if (parent.childCount > placeIndex)
            {
                operation.view.transform.SetSiblingIndex(placeIndex);
            }
        }
    }
}