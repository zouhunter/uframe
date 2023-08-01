//*************************************************************************************
//* 作    者： zht
//* 创建时间： 2023-05-10 10:30:34
//* 描    述： 单例

//* ************************************************************************************
namespace UFrame
{
    public class Singleton<Agent> : UFrame.Agent
        where Agent : Singleton<Agent>, new()
    {
        protected virtual BaseGameManage Manage => BaseGameManage.Single;
        protected static object m_locker = new object();
        protected static Agent m_manager = default(Agent);
        public static Agent GetInstance()
        {
            lock (m_locker)
            {
                if (m_manager == null)
                {
                    m_manager = new Agent();
                    m_manager.OnCreate();
                }
            }
            return m_manager;
        }
        public static Agent Instance
        {
            get
            {
                return GetInstance();
            }
        }
        public bool Started
        {
            get
            {
                return m_manager != null;
            }
        }
        public static bool Valid => m_manager != null;

        public static void Release()
        {
            if (m_manager != null)
            {
                m_manager.OnDispose();
            }
            m_manager = null;
        }

        protected void OnCreate()
        {
            if (Manage != null)
            {
                if (!Manage.RegistAgent(this))
                {
                    m_manager = null;
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
            m_manager = null;
        }

        protected override void OnInitialize()
        {
#if DEBUG
            UnityEngine.Debug.Log(m_manager + ".OnInitialize");
#endif
        }
        protected override void OnRecover()
        {
#if DEBUG
            UnityEngine.Debug.Log(m_manager + ".OnRecover");
#endif
        }
    }
}