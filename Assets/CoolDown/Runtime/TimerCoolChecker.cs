using UnityEngine;

namespace UFrame.CoolDown
{
    public class TimerCoolChecker : ICoolDownChecker
    {
        [SerializeField]
        protected float m_time;
        protected float m_lastTime;
        protected bool m_firstReseted;

        public float LastTime => m_lastTime;
        public float MaxTime => m_time;
        public float Precent => (Time.time - LastTime) / MaxTime;

        public TimerCoolChecker(float maxTime)
        {
            m_time = maxTime;
        }

        public void ResetState(float maxTime)
        {
            m_time = maxTime;
            m_lastTime = Time.time;
            m_firstReseted = true;
        }

        public void ResetState(bool coolEnd = false)
        {
            if (coolEnd)
            {
                m_lastTime = Time.time;
            }
            else
            {
                m_lastTime = Time.time - m_time;
            }
            m_firstReseted = true;
        }

        public bool CoolCheck(bool autoReset = true)
        {
            if (Time.time - m_lastTime >= m_time)
            {
                if (!m_firstReseted)
                {
                    ResetState();
                    return false;
                }
                if (autoReset)
                {
                    m_lastTime = Time.time;
                }
                return true;
            }
            return false;
        }
    }
}