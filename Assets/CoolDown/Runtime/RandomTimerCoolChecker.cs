using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.CoolDown
{
    public class RandomTimerCoolChecker : ICoolDownChecker
    {
        [SerializeField]
        protected float m_timeMin;
        [SerializeField]
        protected float m_timeEnd;

        protected float m_lastTime;
        protected float m_currentTimeSpan;
        protected bool m_firstReseted;
        protected bool m_random;

        public float MaxTime => m_timeEnd;
        public float MinTime => m_timeMin;
        public float CurrMaxTime => m_currentTimeSpan;
        public float LastTime => m_lastTime;
        public float Precent => (Time.time - m_lastTime) / m_currentTimeSpan;

        public RandomTimerCoolChecker(float timeMin, float timeMax)
        {
            m_timeMin = timeMin;
            m_currentTimeSpan = m_timeEnd = timeMax;
            m_random = true;
        }

        public void ResetState(bool coolEnd = false)
        {
            if (m_random)
                m_currentTimeSpan = Random.Range(m_timeMin, m_timeEnd);
            if (coolEnd)
                m_lastTime = Time.time;
            else
                m_lastTime = Time.time - m_currentTimeSpan;
            m_firstReseted = true;
        }

        public bool CoolCheck(bool autoReset)
        {
            if (Time.time - m_lastTime > m_currentTimeSpan)
            {
                if (!m_firstReseted)
                {
                    ResetState(false);
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