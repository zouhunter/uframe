// ██╗░░░██╗███████╗██████╗    ░████╗  ░███╗░░░███╗███████╗
// ██║░░░██║██╔════╝██╔══██╗ ██╔═██╗░████╗░████║██╔════╝
// ██║░░░██║█████╗░░██████╔╝ ██████║░██╔████╔██║█████╗░░
// ██║░░░██║██╔══╝░░██╔══██╗ ██╔══██╗██║╚██╔╝██║██╔══╝░░
// ╚██████╔╝██║░░░░░██║░░██║ ██║░░██║██║░╚═╝░██║███████╗
// ░╚═════╝░╚═╝░░░░░╚═╝░░╚═╝ ╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-05-02
//* 描    述： 安全计时控制器，支持IAlive对象存活检测
//* ************************************************************************************
using System.Collections.Generic;

namespace UFrame.Timer
{
    public class SaftyTimerCtrl : TimerCtrl
    {
        /// <summary>
        /// 计时器间隔（可重写）
        /// </summary>
        public virtual float Interval => 0;
        /// <summary>
        /// 是否正在运行（有计时器且未暂停）
        /// </summary>
        public virtual bool Runing => !Paused && m_timerDic.Count > 0;
        /// <summary>
        /// 存活检测映射表，key为timerId，value为IAlive对象
        /// </summary>
        private Dictionary<int, IAlive> m_aliveCheckMap;

        /// <summary>
        /// 初始化存活检测表
        /// </summary>
        public void Init()
        {
            m_aliveCheckMap = new Dictionary<int, IAlive>();
        }

        /// <summary>
        /// 释放所有计时器和存活检测表
        /// </summary>
        public void Release()
        {
            ClearAll();
            if (m_aliveCheckMap != null)
                m_aliveCheckMap.Clear();
        }

        /// <summary>
        /// 启动带存活检测的循环计时器（指定循环次数）
        /// </summary>
        /// <param name="alive">IAlive对象</param>
        /// <param name="loopFunc">循环回调</param>
        /// <param name="loopTime">循环间隔</param>
        /// <param name="loopCount">循环次数</param>
        /// <param name="updateType">更新类型</param>
        /// <param name="flag">标记</param>
        /// <returns>ITimer</returns>
        public ITimer StartLoopSafty(IAlive alive, System.Func<bool> loopFunc, float loopTime, int loopCount, TimerUpdateType updateType = TimerUpdateType.LateUpdate, string flag = default)
        {
            var timer = StartLoop(loopFunc, loopTime, loopCount, updateType, flag);
            if (alive != null && m_aliveCheckMap != null)
                m_aliveCheckMap[timer.Id] = alive;
            return timer;
        }

        /// <summary>
        /// 启动带存活检测的循环计时器（无限循环）
        /// </summary>
        public ITimer StartLoopSafty(IAlive alive, System.Func<bool> loopFunc, float loopTime, TimerUpdateType updateType = TimerUpdateType.LateUpdate, string flag = default)
        {
            var timer = StartLoop(loopFunc, loopTime, updateType, flag);
            if (alive != null && m_aliveCheckMap != null)
                m_aliveCheckMap[timer.Id] = alive;
            return timer;
        }

        /// <summary>
        /// 注册带存活检测的循环计时器（无限循环）
        /// </summary>
        public ITimer RegistLoopSafty(IAlive alive, System.Func<bool> loopFunc, float loopTime, TimerUpdateType updateType = TimerUpdateType.LateUpdate, string flag = default)
        {
            var timer = RegistLoop(loopFunc, loopTime, updateType, flag);
            if (alive != null && m_aliveCheckMap != null)
                m_aliveCheckMap[timer.Id] = alive;
            return timer;
        }

        /// <summary>
        /// 注册带存活检测的循环计时器（指定循环次数）
        /// </summary>
        public ITimer RegistLoopSafty(IAlive alive, System.Func<bool> loopFunc, float loopTime, int loopCount, TimerUpdateType updateType = TimerUpdateType.LateUpdate, string flag = default)
        {
            var timer = RegistLoop(loopFunc, loopTime, loopCount, updateType, flag);
            if (alive != null && m_aliveCheckMap != null)
                m_aliveCheckMap[timer.Id] = alive;
            return timer;
        }

        /// <summary>
        /// 启动带存活检测的延迟执行计时器
        /// </summary>
        public ITimer DelyExecuteSafty(IAlive alive, System.Action action, float dely, TimerUpdateType updateType = TimerUpdateType.LateUpdate, string flag = default)
        {
            var timer = DelyExecute(action, dely, updateType, flag);
            if (alive != null && m_aliveCheckMap != null)
                m_aliveCheckMap[timer.Id] = alive;
            return timer;
        }

        /// <summary>
        /// 检查计时器是否有效（重写，增加IAlive检测）
        /// </summary>
        internal override bool CheckValid(int timerId, TimerStateInfo clockTimerInfo)
        {
            if (m_aliveCheckMap != null && m_aliveCheckMap.TryGetValue(timerId, out var aliveChecker))
            {
                if (aliveChecker != null && !aliveChecker.Alive)
                    return false;
            }
            return base.CheckValid(timerId, clockTimerInfo);
        }

        /// <summary>
        /// 回收计时器并移除存活检测
        /// </summary>
        internal override void SaveBackClockTimer(int timerId, TimerStateInfo timerState)
        {
            base.SaveBackClockTimer(timerId, timerState);
            if (m_aliveCheckMap != null)
                m_aliveCheckMap.Remove(timerId);
        }
    }
}