using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UFrame.Pool
{
    public class GameObjectPoolCtrl
    {
        protected Dictionary<string, IGameObjectPoolContext> m_contextDic;
        protected Transform m_prefabRoot;
        protected Transform m_poolRoot;
        protected Vector3 m_virtualHidePos = Vector3.down * 1000;
        public bool Inited { get; protected set; }

        public void Init()
        {
            if (!Inited)
            {
                m_contextDic = new Dictionary<string, IGameObjectPoolContext>();
                m_prefabRoot = new GameObject("[GameObject Prefab]").transform;
                m_prefabRoot.gameObject.SetActive(false);
                m_poolRoot = new GameObject("[GameObject Pool]").transform;
                m_poolRoot.position = m_virtualHidePos;
                GameObject.DontDestroyOnLoad(m_prefabRoot);
                GameObject.DontDestroyOnLoad(m_poolRoot);
                Inited = true;
            }
        }

        public void Release()
        {
            Inited = false;
            if (m_contextDic != null)
            {
                foreach (var pair in m_contextDic)
                {
                    pair.Value.Recover();
                }
                m_contextDic.Clear();
            }
            if (m_prefabRoot != null)
            {
                GameObject.Destroy(m_prefabRoot.gameObject);
            }
            if (m_poolRoot != null)
            {
                GameObject.Destroy(m_poolRoot.gameObject);
            }
        }

        public GameObjectPoolContext GetPoolContext(string flag)
        {
            return GetPoolContext<GameObjectPoolContext>(flag);
        }

        public T GetPoolContext<T>(string flag) where T : IGameObjectPoolContext, new()
        {
            if (!Inited)
            {
                Debug.LogError("!Inited");
                return default(T);
            }

            m_contextDic.TryGetValue(flag, out IGameObjectPoolContext poolContext);
            if (poolContext == null || !(poolContext is T))
            {
                poolContext = CreatePoolContext<T>(flag);
            }
            return (T)poolContext;
        }

        protected T CreatePoolContext<T>(string flag) where T : IGameObjectPoolContext, new()
        {
            Transform prefabRoot = new GameObject(flag).transform;
            Transform poolRoot = new GameObject(flag).transform;
            prefabRoot.SetParent(m_prefabRoot);
            poolRoot.SetParent(m_poolRoot);
            poolRoot.localPosition = Vector3.zero;
            var poolContext = new T();
            poolContext.Initialize(flag, prefabRoot, poolRoot);
            m_contextDic[flag] = poolContext;
            return poolContext;
        }
    }
}