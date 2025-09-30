/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 场景加载控制器                                                                        *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UFrame.SceneLoad
{
    public class GameSceneLoadCtrlGeneric<SID>
    {
        protected Dictionary<SID, IGameSceneGeneric<SID>> m_activeScenes = new Dictionary<SID, IGameSceneGeneric<SID>>();
        protected List<IUpdateable> m_updateableScenes = new List<IUpdateable>();
        protected Dictionary<SID, Type> m_sceneTypes = new Dictionary<SID, Type>();
        protected Dictionary<SID, IGameSceneGeneric<SID>> m_sceneCaches = new Dictionary<SID, IGameSceneGeneric<SID>>();
        public int ActiveSceneCount { get { return m_activeScenes.Count; } }
        public SID LastSceneId { get; protected set; }
        public SID CurrentSceneId { get; protected set; }

        public void StartWithActiveScene(SID sceneId,IGameSceneGeneric<SID> instence = null)
        {
            if (instence == null)
                m_sceneCaches.TryGetValue(sceneId, out instence);

            if (instence == null && m_sceneTypes.TryGetValue(sceneId, out Type sceneType))
            {
                instence = Activator.CreateInstance(sceneType) as IGameSceneGeneric<SID>;
                m_sceneCaches[sceneId] = instence;
            }
            if(instence != null && !m_activeScenes.ContainsKey(sceneId))
            {
                m_activeScenes.Add(sceneId, instence);
                if (instence is IUpdateable)
                {
                    m_updateableScenes.Add(instence as IUpdateable);
                }
                if (instence is IEntryableGeneric<SID>)
                {
                    (instence as IEntryableGeneric<SID>).OnEntry(sceneId);
                }
                CurrentSceneId = sceneId;
            }
        }

        public bool IsSceneActive(SID sceneID)
        {
            return m_activeScenes.ContainsKey(sceneID);
        }

        public bool Quit(SID sceneID)
        {
            if (IsSceneActive(sceneID))
            {
                IGameSceneGeneric<SID> scene = m_activeScenes[sceneID];
                scene.OnExit(sceneID, true);
                m_activeScenes.Remove(sceneID);
                if (scene is IUpdateable)
                {
                    m_updateableScenes.Remove(scene as IUpdateable);
                }
                LastSceneId = sceneID;
                if (m_activeScenes.Count > 0)
                {
                    CurrentSceneId = m_activeScenes.First().Key;
                }
                return true;
            }
            return false;
        }

        public bool Enter(SID sceneID, IGameSceneGeneric<SID> sceneInstance = null, bool alone = true)
        {
            if (sceneInstance == null)
                m_sceneCaches.TryGetValue(sceneID, out sceneInstance);

            if (m_activeScenes.TryGetValue(sceneID,out var oldScene) && (sceneInstance == null || oldScene == sceneInstance))
                return false;

            if (sceneInstance == null && m_sceneTypes.TryGetValue(sceneID, out Type sceneType))
            {
                sceneInstance = Activator.CreateInstance(sceneType) as IGameSceneGeneric<SID>;
                m_sceneCaches[sceneID] = sceneInstance;
            }

            if (sceneInstance != null)
            {
                EnterGameScene(sceneID, alone, sceneInstance);
                return true;
            }
            return false;
        }
        public bool Reload(SID sceneId)
        {
            if (m_activeScenes.TryGetValue(sceneId, out var scene) && scene is IReloadAbleGeneric<SID>)
            {
                (scene as IReloadAbleGeneric<SID>).OnReload(sceneId);
                return true;
            }
            return false;
        }

        protected void EnterGameScene(SID sceneID, bool alone, IGameSceneGeneric<SID> instence)
        {
            if (alone)
            {
                if (m_activeScenes.Count > 0)
                {
                    var sceneIds = new List<SID>(m_activeScenes.Keys);
                    for (int i = 0; i < sceneIds.Count; i++)
                    {
                        Quit(sceneIds[i]);
                    }
                }
            }

            if (!IsSceneActive(sceneID))
            {
                m_activeScenes.Add(sceneID, instence);
                if (instence is IUpdateable)
                {
                    m_updateableScenes.Add(instence as IUpdateable);
                }
                instence.OnEnter(sceneID, alone);
            }
            CurrentSceneId = sceneID;
        }

        public IGameSceneGeneric<SID> FindActive(SID sceneID)
        {
            if (m_activeScenes.TryGetValue(sceneID, out var activeScene))
            {
                return activeScene;
            }
            return null;
        }

        public virtual void Recover()
        {
            if (m_activeScenes.Count > 0)
            {
                using (var enumerator = m_activeScenes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var sceneId = enumerator.Current.Key;
                        enumerator.Current.Value.OnExit(sceneId, false);
                    }
                }
                m_activeScenes.Clear();
                m_updateableScenes.Clear();
            }
            ClearAllRecord();
        }

        protected virtual void ClearAllRecord()
        {
            m_activeScenes.Clear();
            m_updateableScenes.Clear();
            m_sceneTypes.Clear();
        }

        public virtual void OnUpdate()
        {
            if (m_updateableScenes.Count <= 0)
                return;

            for (int i = 0; i < m_updateableScenes.Count; i++)
            {
                var scene = m_updateableScenes[i];
                try
                {
                    scene.OnUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void RegistScene<T>(SID sceneID) where T : class, IGameSceneGeneric<SID>,new()
        {
            m_sceneTypes[sceneID] = typeof(T);
        }

        public void RegistScene(SID sceneID, IGameSceneGeneric<SID> sceneInstance)
        {
            m_sceneCaches[sceneID] = sceneInstance;
        }
    }
}