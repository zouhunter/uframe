// ██╗░░░██╗███████╗██████╗    ░████╗  ░███╗░░░███╗███████╗
// ██║░░░██║██╔════╝██╔══██╗ ██╔═██╗░████╗░████║██╔════╝
// ██║░░░██║█████╗░░██████╔╝ ██████║░██╔████╔██║█████╗░░
// ██║░░░██║██╔══╝░░██╔══██╗ ██╔══██╗██║╚██╔╝██║██╔══╝░░
// ╚██████╔╝██║░░░░░██║░░██║ ██║░░██║██║░╚═╝░██║███████╗
// ░╚═════╝░╚═╝░░░░░╚═╝░░╚═╝ ╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2024-03-27
//* 描    述： 定时器接口与实现
//* ************************************************************************************
using System;
using UnityEngine;

namespace UFrame.Timer
{
    /// <summary>
    /// 定时器接口，支持暂停与释放
    /// </summary>
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// 定时器唯一ID
        /// </summary>
        int Id { get; }
        /// <summary>
        /// 暂停/恢复定时器
        /// </summary>
        /// <param name="pause">true暂停，false恢复</param>
        void Pause(bool pause);
    }

    /// <summary>
    /// 定时器实现类，支持回调暂停与释放
    /// </summary>
    public class Timer : ITimer
    {
        /// <summary>定时器唯一ID</summary>
        public int Id { get; private set; }
        private Action<int> m_releaseCallBack; // 释放回调
        private Action<int, bool> m_pauseCallBack; // 暂停回调

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="timerId">定时器ID</param>
        /// <param name="pauseCallBack">暂停回调</param>
        /// <param name="releaseCallBack">释放回调</param>
        public Timer(int timerId, Action<int, bool> pauseCallBack, Action<int> releaseCallBack)
        {
            Id = timerId;
            m_releaseCallBack = releaseCallBack;
            m_pauseCallBack = pauseCallBack;
        }

        /// <summary>
        /// 暂停/恢复定时器
        /// </summary>
        /// <param name="pause">true暂停，false恢复</param>
        public void Pause(bool pause)
        {
            m_pauseCallBack?.Invoke(Id, pause);
        }

        /// <summary>
        /// 释放定时器（会自动停止）
        /// </summary>
        public void Dispose()
        {
            if (m_releaseCallBack != null)
            {
                m_releaseCallBack?.Invoke(Id);
            }
        }

    }
}

