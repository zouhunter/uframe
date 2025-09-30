// ██╗░░░██╗███████╗██████╗    ░████╗  ░███╗░░░███╗███████╗
// ██║░░░██║██╔════╝██╔══██╗ ██╔═██╗░████╗░████║██╔════╝
// ██║░░░██║█████╗░░██████╔╝ ██████║░██╔████╔██║█████╗░░
// ██║░░░██║██╔══╝░░██╔══██╗ ██╔══██╗██║╚██╔╝██║██╔══╝░░
// ╚██████╔╝██║░░░░░██║░░██║ ██║░░██║██║░╚═╝░██║███████╗
// ░╚═════╝░╚═╝░░░░░╚═╝░░╚═╝ ╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-10
//* 描    述： 整数秒级计时器

//* ************************************************************************************
using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Timer
{
    /// <summary>
    /// 秒级时钟管理器
    /// </summary>
    public class ClockTimerCtrl
    {
        /// <summary>
        /// 秒级计时器状态信息
        /// </summary>
        internal class ClockTimerInfo
        {
            internal System.Action callback; // 普通回调
            internal System.Func<bool> funcCallback; // 返回bool的回调
            internal bool isLoop = false; // 是否循环
            internal int countLeft = 0; // 剩余循环次数
            internal int interval = 0; // 间隔（秒）

            /// <summary>
            /// 清空状态
            /// </summary>
            internal void Clear()
            {
                isLoop = false;
                countLeft = 0;
                callback = null;
                funcCallback = null;
                interval = 0;
            }
        }

        private Dictionary<int, ClockTimerInfo> m_timerDic = new Dictionary<int, ClockTimerInfo>(); // 计时器字典
        internal List<ClockNode> m_runingClock = new List<ClockNode>(); // 运行中的时钟节点
        internal Stack<ClockNode> m_clockPool = new Stack<ClockNode>(); // 时钟对象池
        private Stack<ClockTimerInfo> m_timerPool = new Stack<ClockTimerInfo>(); // 计时器对象池
        private int m_timerIdSpan = 0; // 计时器ID自增

        /// <summary>运行中时钟数量</summary>
        public int RuningClockCount => m_runingClock.Count;
        /// <summary>计时器数量</summary>
        public int TimerCount => m_timerDic.Count;
        /// <summary>时钟池数量</summary>
        public int ClockPoolCount => m_clockPool.Count;
        /// <summary>计时器池数量</summary>
        public int TimerPoolCount => m_timerPool.Count;
        /// <summary>自定义有效性检查</summary>
        public System.Func<int, bool> ValidCheck { get; set; }

        /// <summary>
        /// 更新所有时钟（需外部驱动）
        /// </summary>
        public void UpdateClock()
        {
            if (m_runingClock.Count <= 0)
                return;

            var second = UnityEngine.Time.realtimeSinceStartup;

            for (int i = 0; i < m_runingClock.Count; i++)
            {
                var clockNode = m_runingClock[i];
                if (clockNode.triggerTime < second)
                {
                    m_runingClock.RemoveAt(i);
                    TriggerTimeNode(clockNode);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 触发时钟节点下所有计时器
        /// </summary>
        /// <param name="clockNode">时钟节点</param>
        internal void TriggerTimeNode(ClockNode clockNode)
        {
            for (int i = 0; i < clockNode.timers.Count; i++)
            {
                var timerId = clockNode.timers[i];
                if (!m_timerDic.TryGetValue(timerId, out var clockTimerInfo))
                    continue;

                if (!CheckValid(timerId, clockTimerInfo))
                {
                    SaveBackClockTimer(timerId, clockTimerInfo);
                    continue;
                }

                if (clockTimerInfo.isLoop)
                {
                    bool finished = false;
                    try
                    {
                        finished = clockTimerInfo.funcCallback.Invoke();
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                    if (finished || (clockTimerInfo.countLeft > 0 && --clockTimerInfo.countLeft == 0))
                    {
                        SaveBackClockTimer(timerId, clockTimerInfo);
                        continue;
                    }
                    var triggerTime = clockTimerInfo.interval + clockNode.triggerTime;
                    InsertTimerToClock(timerId, triggerTime);
                }
                else
                {
                    try
                    {
                        clockTimerInfo.callback.Invoke();
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                    SaveBackClockTimer(timerId, clockTimerInfo);
                }
            }
            SaveBackClockNode(clockNode);
        }

        /// <summary>
        /// 检查计时器是否有效（可重写扩展）
        /// </summary>
        /// <param name="timerId">计时器ID</param>
        /// <param name="clockTimerInfo">计时器状态</param>
        /// <returns>是否有效</returns>
        internal virtual bool CheckValid(int timerId, ClockTimerInfo clockTimerInfo)
        {
            if (clockTimerInfo.isLoop && clockTimerInfo.funcCallback != null)
            {
                return true;
            }
            else if (!clockTimerInfo.isLoop && clockTimerInfo.callback != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 插入计时器到指定触发时间的时钟节点
        /// </summary>
        /// <param name="timerId">计时器ID</param>
        /// <param name="triggerTime">触发时间</param>
        private void InsertTimerToClock(int timerId, int triggerTime)
        {
            int lastIndex = -1;
            for (int i = 0; i < m_runingClock.Count; i++)
            {
                var clockItem = m_runingClock[i];
                if (clockItem.triggerTime < triggerTime)
                {
                    lastIndex = i;
                }
                else if (clockItem.triggerTime == triggerTime)
                {
                    clockItem.timers.Add(timerId);
                    return;
                }
                else
                {
                    var clockNode = GetOneClockNode(triggerTime);
                    clockNode.timers.Add(timerId);
                    m_runingClock.Insert(lastIndex + 1, clockNode);
                    return;
                }
            }
            var newNode = GetOneClockNode(triggerTime);
            newNode.timers.Add(timerId);
            m_runingClock.Add(newNode);
        }

        /// <summary>
        /// 获取一个空闲的时钟节点
        /// </summary>
        /// <param name="triggerTime">触发时间</param>
        /// <returns>时钟节点</returns>
        private ClockNode GetOneClockNode(int triggerTime)
        {
            if (m_clockPool.Count > 0)
            {
                var clockNode = m_clockPool.Pop();
                clockNode.triggerTime = triggerTime;
                return clockNode;
            }
            var node = new ClockNode();
            node.triggerTime = triggerTime;
            return node;
        }

        /// <summary>
        /// 获取一个空闲的计时器状态对象
        /// </summary>
        /// <returns>计时器状态</returns>
        private ClockTimerInfo GetClockTimer()
        {
            if (m_timerPool.Count > 0)
            {
                var timerItem = m_timerPool.Pop();
                return timerItem;
            }
            return new ClockTimerInfo();
        }

        /// <summary>
        /// 回收时钟节点到对象池
        /// </summary>
        /// <param name="clockNode">时钟节点</param>
        private void SaveBackClockNode(ClockNode clockNode)
        {
            clockNode.triggerTime = 0;
            clockNode.timers.Clear();
            m_clockPool.Push(clockNode);
        }

        /// <summary>
        /// 回收计时器到对象池
        /// </summary>
        /// <param name="timerId">计时器ID</param>
        /// <param name="timerState">计时器状态</param>
        private void SaveBackClockTimer(int timerId, ClockTimerInfo timerState)
        {
            m_timerDic.Remove(timerId);
            timerState.Clear();
            m_timerPool.Push(timerState);
        }

        /// <summary>
        /// 启动一个延迟执行的计时器
        /// </summary>
        /// <param name="action">回调</param>
        /// <param name="dely">延迟秒数</param>
        /// <returns>timerId</returns>
        public int DelyExecute(System.Action action, int dely)
        {
            var timerId = ++m_timerIdSpan;
            var timerInfo = GetClockTimer();
            timerInfo.interval = dely;
            timerInfo.isLoop = false;
            timerInfo.callback = action;
            m_timerDic[timerId] = timerInfo;

            int triggerTime = UnityEngine.Mathf.CeilToInt(UnityEngine.Time.realtimeSinceStartup) + dely;
            InsertTimerToClock(timerId, triggerTime);
            return timerId;
        }

        /// <summary>
        /// 注册一个循环计时器（无限循环）
        /// </summary>
        /// <param name="loopFunc">循环回调</param>
        /// <param name="interval">间隔秒数</param>
        /// <returns>timerId</returns>
        public int RegistLoop(System.Func<bool> loopFunc, int interval)
        {
            if (interval <= 0)
                return 0;

            var timerId = ++m_timerIdSpan;
            var timerInfo = GetClockTimer();
            timerInfo.interval = interval;
            timerInfo.isLoop = true;
            timerInfo.countLeft = -1;
            timerInfo.funcCallback = loopFunc;
            m_timerDic[timerId] = timerInfo;
            int triggerTime = UnityEngine.Mathf.CeilToInt(UnityEngine.Time.realtimeSinceStartup) + interval;
            InsertTimerToClock(timerId, triggerTime);
            return timerId;
        }

        /// <summary>
        /// 注册一个循环计时器（指定循环次数）
        /// </summary>
        /// <param name="loopFunc">循环回调</param>
        /// <param name="interval">间隔秒数</param>
        /// <param name="loopCount">循环次数</param>
        /// <returns>timerId</returns>
        public int RegistLoop(System.Func<bool> loopFunc, int interval, int loopCount)
        {
            if (interval <= 0)
                return 0;

            var timerId = ++m_timerIdSpan;
            var timerInfo = GetClockTimer();
            timerInfo.interval = interval;
            timerInfo.isLoop = true;
            timerInfo.countLeft = loopCount;
            timerInfo.funcCallback = loopFunc;
            m_timerDic[timerId] = timerInfo;
            int triggerTime = UnityEngine.Mathf.CeilToInt(UnityEngine.Time.realtimeSinceStartup) + interval;
            InsertTimerToClock(timerId, triggerTime);
            return timerId;
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        /// <param name="timerIndex">计时器ID</param>
        public void StopTimer(int timerIndex)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var clockTimer))
            {
                SaveBackClockTimer(timerIndex, clockTimer);
            }
        }

        /// <summary>
        /// 停止计时器并重置ID
        /// </summary>
        /// <param name="timerIndex">计时器ID</param>
        public void StopTimer(ref int timerIndex)
        {
            StopTimer(timerIndex);
            timerIndex = 0;
        }
    }
}