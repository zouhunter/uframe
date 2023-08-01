/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 启动器                                                                          *
*//************************************************************************************/

using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame
{
    public class BaseGameManage : MonoBehaviour
    {
        protected List<Agent> m_agents = new List<Agent>();
        protected List<RefreshContext> m_fixedUpdates = new List<RefreshContext>();
        protected List<RefreshContext> m_updates = new List<RefreshContext>();
        protected List<RefreshContext> m_lateUpdats = new List<RefreshContext>();
        private Stack<RefreshContext> m_deathContentPool = new Stack<RefreshContext>();
        public static BaseGameManage Single { get;protected set; }

        private bool m_inUnRegistLoop;

        #region Managers
        /// <summary>
        /// 优先级数值越大先启动
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority"></param>
        /// <param name="list"></param>
        /// <param name="obj"></param>
        protected virtual void InsetToOrderdList(int priority, IList<Agent> list, Agent obj)
        {
            int insertIndex = 0;
            for (; insertIndex < list.Count; insertIndex++)
            {
                var agent = list[insertIndex];
                if (agent.Priority < priority)
                {
                    break;
                }
            }
            if (insertIndex >= list.Count)
            {
                list.Add(obj);
            }
            else
            {
                list.Insert(insertIndex, obj);
            }
        }

        internal void InsetToOrderdContextList(int priority, IList<RefreshContext> list, IInterval interal,Agent agent, Action action)
        {
            int insertIndex = 0;
            for (; insertIndex < list.Count; insertIndex++)
            {
                var content = list[insertIndex];
                if (content.agent.Priority < priority)
                    break;
            }
            RefreshContext context = new RefreshContext();
            context.interval = interal;
            context.agent = agent;
            context.action = action;
            if (insertIndex >= list.Count)
            {
                list.Add(context);
            }
            else
            {
                list.Insert(insertIndex, context);
            }
        }

        protected virtual void OnRemoveAgent(Agent manager)
        {
            if (manager is IFixedUpdate)
            {
                MarkDeathUpdates(m_fixedUpdates, manager);
            }
            if (manager is IUpdate)
            {
                MarkDeathUpdates(m_updates, manager);
            }
            if (manager is ILateUpdate)
            {
                MarkDeathUpdates(m_lateUpdats, manager);
            }
            m_agents.Remove(manager);
            manager.Recover();
        }

        /// <summary>
        /// 标记死亡状态
        /// </summary>
        /// <param name="list"></param>
        /// <param name="target"></param>
        internal void MarkDeathUpdates(List<RefreshContext> list, Agent target)
        {
            foreach (var content in list)
            {
                if (content.interval == target)
                {
                    content.death = true;
                }
            }
        }

        /// <summary>
        /// 注册逻辑管理器
        /// </summary>
        public virtual bool RegistAgent(Agent agent)
        {
            if (agent == null)
                return false;
            if(m_inUnRegistLoop)
            {
                Debug.Log("should not regist agent in unregist loop!");
                return false;
            }
            int priority = agent.Priority;
            if(agent is IInterval)
            {
                var interval = agent as IInterval;
                if (agent is IFixedUpdate)
                {
                    var fixedUpdate = agent as IFixedUpdate;
                    var action = new Action(fixedUpdate.OnFixedUpdate);
                    InsetToOrderdContextList(priority, m_fixedUpdates, interval, agent, action);
                }
                if (agent is IUpdate)
                {
                    var update = agent as IUpdate;
                    var action = new Action(update.OnUpdate);
                    InsetToOrderdContextList(priority, m_updates, interval, agent, action);
                }
                if (agent is ILateUpdate)
                {
                    var lateUpdate = agent as ILateUpdate;
                    var action = new Action(lateUpdate.OnLateUpdate);
                    InsetToOrderdContextList(priority, m_lateUpdats, interval, agent, action);
                }
            }
            agent.Initialize();
            InsetToOrderdList(priority, m_agents, agent);
            return true;
        }

        //查找管理器
        public virtual T FindAgent<T>() where T:Agent
        {
            foreach (var agent in m_agents)
            {
                if(agent is T)
                {
                    return (T)agent;
                }
            }
            return default(T);
        }

        //注销管理器
        public virtual bool RemoveAgent(Agent agent)
        {
            if(m_agents.Remove(agent))
            {
                OnRemoveAgent(agent);
                return true;
            }
            return false;
        }

        protected virtual void OnEventException(Exception exception)
        {
            Debug.LogException(exception);
        }

        // 注册fixedUpdate
        protected virtual void FixedUpdateManagers()
        {
            for (int i = 0; i < m_fixedUpdates.Count; i++)
            {
                var updateItem = m_fixedUpdates[i];

                if (updateItem.death)
                {
                    m_deathContentPool.Push(updateItem);
                    continue;
                }

                if (!updateItem.interval.Runing)
                    continue;

                if (updateItem.interval.Interval > 0)
                {
                    if (Time.fixedTime < updateItem.nextTime)
                        continue;
                    updateItem.nextTime = Time.fixedTime + updateItem.interval.Interval;
                }

                try
                {
                    updateItem.action();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            while (m_deathContentPool.Count > 0)
            {
                var target = m_deathContentPool.Pop();
                m_fixedUpdates.Remove(target);
            }
        }

        // 注册Update
        protected virtual void UpdateManagers()
        {
            for (int i = 0; i < m_updates.Count; i++)
            {
                var updateItem = m_updates[i];

                if(updateItem.death)
                {
                    m_deathContentPool.Push(updateItem);
                    continue;
                }

                if (!updateItem.interval.Runing)
                    continue;

                if (updateItem.interval.Interval > 0)
                {
                    if (Time.time < updateItem.nextTime)
                        continue;
                    updateItem.nextTime = Time.time + updateItem.interval.Interval;
                }

                try
                {
                    updateItem.action();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            while (m_deathContentPool.Count > 0)
            {
                var target = m_deathContentPool.Pop();
                m_updates.Remove(target);
            }
        }

        // 注册lateUpdate
        protected virtual void LateUpdateManagers()
        {
            for (int i = 0; i < m_lateUpdats.Count; i++)
            {
                var updateItem = m_lateUpdats[i];

                if(updateItem.death)
                {
                    m_deathContentPool.Push(updateItem);
                    continue;
                }

                if (!updateItem.interval.Runing)
                    continue;

                if (updateItem.interval.Interval > 0)
                {
                    if (Time.time < updateItem.nextTime)
                        continue;
                    updateItem.nextTime = Time.time + updateItem.interval.Interval;
                }

                try
                {
                    updateItem.action();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            while (m_deathContentPool.Count > 0)
            {
                var target = m_deathContentPool.Pop();
                m_lateUpdats.Remove(target);
            }
        }

        // 注册程序退出
        protected virtual void UnregistAllManagers()
        {
            m_inUnRegistLoop = true;
            for (int i = m_agents.Count - 1; i >=0 && m_agents.Count > i; i--)
                OnRemoveAgent(m_agents[i]);
            m_fixedUpdates.Clear();
            m_lateUpdats.Clear();
            m_lateUpdats.Clear();
            m_agents.Clear();
            m_inUnRegistLoop = false;
        }
        #endregion Managers

        #region UnityAPI

        protected virtual void Awake()
        {
            Single = this;
        }
        protected virtual void FixedUpdate()
        {
            try
            {
                FixedUpdateManagers();
            }
            catch (Exception e)
            {
                this.OnEventException(e);
            }
        }

        protected virtual void Update()
        {
            try
            {
                UpdateManagers();
            }
            catch (Exception e)
            {
                this.OnEventException(e);
            }
        }

        protected virtual void LateUpdate()
        {
            try
            {
                LateUpdateManagers();
            }
            catch (Exception e)
            {
                this.OnEventException(e);
            }

        }

        protected virtual void OnApplicationQuit()
        {
            try
            {
                UnregistAllManagers();
            }
            catch (Exception e)
            {
                this.OnEventException(e);
            }
        }

        protected virtual void OnDestroy()
        {
            try
            {
                Single = null;
                UnregistAllManagers();
            }
            catch (Exception e)
            {
                this.OnEventException(e);
            }
        }
        #endregion UnityAPI
    }

    public class BaseGameManage<Manage> : BaseGameManage where Manage : BaseGameManage<Manage>
    {
        protected static Manage m_instance;
        protected static object m_locker = new object();
        public static bool Valid => m_instance != null;
        public static Manage Instance
        {
            get
            {
                lock (m_locker)
                {
                    if (m_instance == null)
                    {
                        m_instance = FindObjectOfType<Manage>();
                        if (m_instance == null)
                        {
                            var type = typeof(Manage);
                            m_instance = new GameObject(string.Format("[{0}]", type.FullName), type).GetComponent<Manage>();
                        }
                        GameObject.DontDestroyOnLoad(m_instance.gameObject);
                    }
                }
                return m_instance;
            }
        }
        protected override void Awake()
        {
            base.Awake();
            if (m_instance == null || m_instance == this)
            {
                m_instance = (Manage)this;
                OnCreate();
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
        protected virtual void OnCreate() {
            GameObject.DontDestroyOnLoad(gameObject);
        }
        public static void SetInstance(Manage manage)
        {
            m_instance = manage;
            m_instance.OnCreate();
        }

        protected T CreateGameBinding<T>(string sourcePath) where T:MonoBehaviour
        {
            T binding = FindObjectOfType<T>();
            if (!binding)
            {
                var prefab = Resources.Load<T>(sourcePath);
                if (prefab)
                {
                    binding = Instantiate<T>(prefab);
                }
            }
            if(binding)
            {
                binding.transform.SetParent(transform);
            }
            return binding;
        }
    }

    public class RefreshContext
    {
        public IInterval interval;
        public Agent agent;
        public float nextTime;
        public Action action;
        public bool death;
    }
}