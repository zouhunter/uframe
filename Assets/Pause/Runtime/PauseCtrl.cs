/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 暂停控制器                                                                      *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame.Pause
{
    public class PauseCtrl: IPauseCtrl
    {
        public event Action<bool> pauseEvent;
        protected float m_lastTimeScale;
        protected HashSet<byte> m_pauseLock = new HashSet<byte>();
        public bool IsPaused => m_pauseLock.Count == 0;

        public virtual void Pause(byte flag)
        {
            var currentPaused = IsPaused;
            m_pauseLock.Add(flag);
            if (!currentPaused)
            {
                pauseEvent?.Invoke(true);
            }
        }

        public virtual void UnPause(byte flag)
        {
            var currentPaused = IsPaused;
            m_pauseLock.Remove(flag);
            if (currentPaused)
            {
                pauseEvent?.Invoke(false);
            }
        }

        public virtual void RegistPauseEvent(Action<bool> pausedCallBack)
        {
            pauseEvent += pausedCallBack;
        }

        public virtual void RemovePauseEvent(Action<bool> pausedCallBack)
        {
            pauseEvent -= pausedCallBack;
        }
    }
}