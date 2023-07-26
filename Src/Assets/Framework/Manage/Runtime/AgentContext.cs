/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 管理器模板                                                                      *
*//************************************************************************************/
namespace UFrame
{
    public abstract class AgentContext<AgentContainer> : Agent where AgentContainer : AgentContext<AgentContainer>, new()
    {
        protected virtual BaseGameManage Manage => BaseGameManage.Single;
        protected static object m_locker = new object();
        protected static AgentContainer m_context = default(AgentContainer);
        public static AgentContainer Context
        {
            get
            {
                lock (m_locker)
                {
                    if (m_context == null)
                    {
                        m_context = new AgentContainer();
                        m_context.OnCreate();
                    }
                }
                return m_context;
            }
        }

        public bool Started
        {
            get
            {
                return m_context != null;
            }
        }

        public static bool Valid => m_context != null;

        public static void Release()
        {
            if (m_context != null)
            {
                m_context.OnDispose();
            }
            m_context = null;
        }

        protected void OnCreate()
        {
            if (Manage != null)
            {
                if (!Manage.RegistAgent(this))
                {
                    m_context = null;
                }
            }
        }

        protected void OnDispose()
        {
            if (Manage != null)
            {
                Manage.RemoveAgent(this);
            }
        }

        protected override void OnAfterRecover()
        {
            m_context = null;
        }

        protected override void OnInitialize()
        {
#if DEBUG
            UnityEngine.Debug.Log(Context + ".OnInitialize");
#endif
        }
        protected override void OnRecover()
        {
#if DEBUG
            UnityEngine.Debug.Log(Context + ".OnRecover");
#endif
        }
    }

    public abstract class AgentContext<AgentContainer, IAgent> : UFrame.Agent
    where AgentContainer : AgentContext<AgentContainer, IAgent>, new()
    {
        protected virtual BaseGameManage Manage => BaseGameManage.Single;
        protected static object m_locker = new object();
        protected IAgent m_agent = default(IAgent);
        protected static AgentContainer m_context;
        public static IAgent GetInstance()
        {
            lock (m_locker)
            {
                if (Context.m_agent == null)
                {
                    Context.m_agent = Context.CreateAgent();
                }
            }
            return m_context.m_agent;
        }
        public static IAgent Instance
        {
            get
            {
                return GetInstance();
            }
        }
        public static bool Valid => m_context != null && m_context.m_agent != null;
        public static AgentContainer Context
        {
            get
            {
                lock (m_locker)
                {
                    if (m_context == null)
                    {
                        m_context = new AgentContainer();
                        m_context.OnCreate();
                    }
                }
                return m_context;
            }
        }
        public bool Started
        {
            get
            {
                return Context.m_agent != null;
            }
        }

        public static void Release()
        {
            if (m_context != null)
            {
                m_context.OnDispose();
            }
            m_context = null;
        }
        protected abstract IAgent CreateAgent();

        protected void OnCreate()
        {
            if (Manage != null)
            {
                if (!Manage.RegistAgent(this))
                {
                    m_context = default(AgentContainer);
                }
            }
        }
        protected void OnDispose()
        {
            if (Manage != null)
            {
                Manage.RemoveAgent(this);
            }
        }
        protected override void OnInitialize()
        {
            if (Instance != null && Instance is UFrame.Agent)
            {
                (Instance as UFrame.Agent).Initialize();
            }
        }

        protected override void OnRecover()
        {
            if (Instance != null && Instance is UFrame.Agent)
            {
                (Instance as UFrame.Agent).Recover();
            }
        }
        protected override void OnAfterRecover()
        {
            m_context = null;
        }
    }

    public abstract class AgentContext<AgentContainer, IAgent, Agent> : UFrame.Agent
    where AgentContainer : AgentContext<AgentContainer, IAgent, Agent>, new()
    where Agent : IAgent
    {
        protected virtual BaseGameManage Manage => BaseGameManage.Single;
        protected static object m_locker = new object();
        protected IAgent m_agent = default(Agent);
        protected static Agent AgentSelf => (Agent)Instance;
        protected static AgentContainer m_context;
        public static IAgent GetInstance()
        {
            lock (m_locker)
            {
                if (Context.m_agent == null)
                {
                    Context.m_agent = Context.CreateAgent();
                }
            }
            return m_context.m_agent;
        }
        public static IAgent Instance
        {
            get
            {
                return GetInstance();
            }
        }
        public static bool Valid => m_context != null && m_context.m_agent != null;

        public static AgentContainer Context
        {
            get
            {
                lock (m_locker)
                {
                    if (m_context == null)
                    {
                        m_context = new AgentContainer();
                        m_context.OnCreate();
                    }
                }
                return m_context;
            }
        }
        public bool Started
        {
            get
            {
                return Context.m_agent != null;
            }
        }

        public static void Release()
        {
            if (m_context != null)
            {
                m_context.OnDispose();
            }
            m_context = null;
        }

        protected abstract Agent CreateAgent();

        protected void OnCreate()
        {
            if (Manage != null)
            {
                if (!Manage.RegistAgent(this))
                {
                    m_context = default(AgentContainer);
                }
            }
        }

        protected void OnDispose()
        {
            if (Manage != null)
            {
                Manage.RemoveAgent(this);
            }
        }

        protected override void OnInitialize()
        {
            if (Instance != null && Instance is UFrame.Agent)
            {
                (Instance as UFrame.Agent).Initialize();
            }
        }

        protected override void OnRecover()
        {
            if (Instance != null && Instance is UFrame.Agent)
            {
                (Instance as UFrame.Agent).Recover();
            }
        }

        protected override void OnAfterRecover()
        {
            m_context = null;
        }
    }
}
