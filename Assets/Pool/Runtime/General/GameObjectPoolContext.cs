using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Pool
{
    public interface IGameObjectPoolContext
    {
        void Initialize(string flag, Transform prefabRoot, Transform poolRoot);
        void Recover();
    }

    public class GameObjectPoolContext<T> : IGameObjectPoolContext
    {
        protected Transform m_prefabRoot = null;
        protected Transform m_poolRoot = null;
        protected Dictionary<T, GameObject> m_prefabDic = new Dictionary<T, GameObject>();
        protected Dictionary<T, Stack<GameObject>> m_poolDic = new Dictionary<T, Stack<GameObject>>();
        public Transform Root => m_poolRoot;
        protected string m_flag;

        public void Initialize(string flag, Transform prefabRoot, Transform poolRoot)
        {
            m_flag = flag;
            m_prefabRoot = prefabRoot;
            m_poolRoot = poolRoot;
        }

        public void AddObjectPool(T id)
        {
            if (!m_poolDic.ContainsKey(id))
            {
                m_poolDic.Add(id, new Stack<GameObject>());
            }
        }

        protected GameObject CreateInstance(T id)
        {
            GameObject pfb = null;
            if (m_prefabDic != null)
            {
                m_prefabDic.TryGetValue(id, out pfb);
                if (pfb)
                {
                    var roadInstance = GameObject.Instantiate(pfb);
                    roadInstance.transform.SetParent(m_poolRoot.transform);
                    roadInstance.transform.localPosition = Vector3.zero;
                    roadInstance.name = pfb.name + "-copy";
                    return roadInstance;
                }
            }
            return null;
        }

        public void RemoveObjectPool(T id)
        {
            m_poolDic.TryGetValue(id, out var pool);
            if (pool != null)
            {
                var iter = pool.GetEnumerator();
                while (iter.MoveNext())
                {
                    if (iter.Current is GameObject)
                    {
                        GameObject.Destroy(iter.Current as GameObject);
                    }
                }
                pool.Clear();
                m_poolDic.Remove(id);
            }
        }

        public void Recover()
        {
            if (m_poolDic != null)
            {
                using (var pooliter = m_poolDic.GetEnumerator())
                {
                    while (pooliter.MoveNext())
                    {
                        var id = pooliter.Current.Key;
                        m_poolDic.TryGetValue(id, out var pool);
                        if (pool != null)
                        {
                            var iter = pool.GetEnumerator();
                            while (iter.MoveNext())
                            {
                                GameObject.Destroy(iter.Current as UnityEngine.Object);
                            }
                            pool.Clear();
                        }
                    }
                    m_poolDic.Clear();
                }

            }
            if (m_prefabDic != null)
            {
                m_prefabDic.Clear();
            }
            if(m_prefabRoot != null)
            {
                var childs = m_prefabRoot.GetComponentsInChildren<Transform>();
                foreach (var child in childs)
                {
                    if(child != m_prefabRoot)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
            }
        }
        public void SetPoolVisiable(bool v)
        {
            if (m_poolRoot)
            {
                m_poolRoot.gameObject.SetActive(v);
            }
        }
        public GameObject FindPrefab(T prefabId)
        {
            m_prefabDic.TryGetValue(prefabId, out GameObject prefab);
            return prefab;
        }

        public void SetPrefab(T prefabId, GameObject prefab, bool replace = true, bool resetParent = true)
        {
            if (prefab)
            {
                if (m_prefabDic.TryGetValue(prefabId, out var oldPrefab))
                {
                    if (oldPrefab == prefab)
                        return;

                    if (oldPrefab)
                    {
                        if (replace)
                            GameObject.Destroy(oldPrefab);
                        else
                            return;
                    }
                }
                if (resetParent)
                {
                    prefab.transform.SetParent(m_prefabRoot);
                }
                m_prefabDic[prefabId] = prefab;
                AddObjectPool(prefabId);
            }
        }

        public bool ExistPool(T prefabId)
        {
            if (m_poolDic != null)
            {
                return m_poolDic.ContainsKey(prefabId);
            }
            return false;
        }

        public GameObject GetInstance(T id, bool active = true)
        {
            m_poolDic.TryGetValue(id, out var pool);
            if (pool != null && pool.Count > 0)
            {
                var instance = pool.Pop();
                instance.gameObject.SetActive(active);
                return instance;
            }
            return CreateInstance(id);
        }

        public int GetPrefabCount()
        {
            return m_poolDic.Count;
        }

        public void SaveBack(T id, GameObject instance)
        {
            m_poolDic.TryGetValue(id, out var pool);
            if (pool == null)
            {
                SetPrefab(id, instance);
            }
            else
            {
                pool.Push(instance);
                instance.transform.SetParent(m_poolRoot.transform);
                instance.gameObject.SetActive(false);
            }
            instance.transform.localPosition = Vector3.zero;
        }
    }

    public class GameObjectPoolContext : GameObjectPoolContext<int> { }
}