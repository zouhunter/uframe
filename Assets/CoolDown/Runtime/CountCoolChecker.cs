using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.CoolDown
{
    public class CountCoolChecker : ICoolDownChecker
    {
        [SerializeField]
        protected int m_count;
        protected int m_counter;

        public int Counter => m_counter;
        public int MaxCount => m_count;

        public float Precent => Counter / MaxCount;

        public CountCoolChecker(int count)
        {
            if (count <= 0)
                count = 1;
            m_count = count;
            m_counter = 0;
        }

        public void ResetState(bool coolEnd = false)
        {
            if (coolEnd)
            {
                m_counter = m_count;
            }
            else
            {
                m_counter = 0;
            }
        }

        public void ResetState(int maxTime)
        {
            m_count = maxTime;
            m_counter = 0;
        }

        public bool CoolCheck(bool autoReset = true)
        {
            m_counter += 1;
            if (m_counter >= m_count)
            {
                if (autoReset)
                {
                    m_counter = 0;
                }
                return true;
            }
            return false;
        }
    }
}
