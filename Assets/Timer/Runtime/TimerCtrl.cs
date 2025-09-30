using System.Collections.Generic;
// ██╗░░░██╗███████╗██████╗    ░████╗  ░███╗░░░███╗███████╗
// ██║░░░██║██╔════╝██╔══██╗ ██╔═██╗░████╗░████║██╔════╝
// ██║░░░██║█████╗░░██████╔╝ ██████║░██╔████╔██║█████╗░░
// ██║░░░██║██╔══╝░░██╔══██╗ ██╔══██╗██║╚██╔╝██║██╔══╝░░
// ╚██████╔╝██║░░░░░██║░░██║ ██║░░██║██║░╚═╝░██║███████╗
// ░╚═════╝░╚═╝░░░░░╚═╝░░╚═╝ ╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-04-18
//* 描    述： 时间管理器-(动态设定精度)
//* ************************************************************************************

using UnityEngine;

namespace UFrame.Timer
{
    public class TimerCtrl
    {
        /// <summary>
        /// 计时器状态信息，包含回调、循环、间隔等属性
        /// </summary>
        internal class TimerStateInfo
        {
            internal bool cylce { get; set; } // 是否循环
            internal int interval { get; set; } // 间隔（毫秒）
            internal string flag { get; set; } // 标记
            internal int countLeft { get; set; } // 剩余循环次数
            internal bool paused { get; set; } // 是否暂停
            internal System.Action action { get; set; } // 普通回调
            internal System.Func<bool> function { get; set; } // 返回bool的回调

            /// <summary>
            /// 清空状态
            /// </summary>
            internal void Clear()
            {
                cylce = false;
                interval = 0;
                action = null;
                function = null;
                countLeft = 0;
                flag = null;
                paused = false;
            }
        }

        private bool m_paused; // 全局暂停标志
        private const int ACCURACY = 1000; // 精度（毫秒）
        private int m_timerIdSpan = 0; // 计时器ID自增
        internal List<ClockNode> m_fixedUpdateClock = new List<ClockNode>(); // 固定帧时钟
        internal List<ClockNode> m_updateClock = new List<ClockNode>(); // 普通帧时钟
        internal List<ClockNode> m_lateUpdateClock = new List<ClockNode>(); // LateUpdate时钟
        internal Dictionary<int, TimerStateInfo> m_timerDic = new Dictionary<int, TimerStateInfo>(); // 计时器字典
        internal Stack<ClockNode> m_clockPool = new Stack<ClockNode>(); // 时钟对象池
        internal Stack<TimerStateInfo> m_timerPool = new Stack<TimerStateInfo>(); // 计时器对象池
        /// <summary>当前计时器数量</summary>
        public int TimerCount => m_timerDic.Count;
        /// <summary>Update时钟数量</summary>
        public int UpdateClockCount => m_updateClock.Count;
        /// <summary>FixedUpdate时钟数量</summary>
        public int FixedUpdateClockCount => m_fixedUpdateClock.Count;
        /// <summary>LateUpdate时钟数量</summary>
        public int LateUpdateClockCount => m_lateUpdateClock.Count;
        /// <summary>时钟池数量</summary>
        public int ClockPoolCount => m_clockPool.Count;
        /// <summary>计时器池数量</summary>
        public int TimerPoolCount => m_timerPool.Count;
        /// <summary>是否全局暂停</summary>
        public bool Paused => m_paused;
        private bool m_checkTimerExists; // 是否检测ID冲突

        /// <summary>
        /// 切换全局暂停状态
        /// </summary>
        /// <param name="pause">是否暂停</param>
        public void TogglePause(bool pause)
        {
            m_paused = pause;
        }

        /// <summary>
        /// 固定帧更新（需外部驱动）
        /// </summary>
        public void OnFixedUpdate()
        {
            if (m_paused || m_fixedUpdateClock.Count < 0)
                return;
            UpdateClock(m_fixedUpdateClock, Time.fixedTime);
        }

        /// <summary>
        /// 普通帧更新（需外部驱动）
        /// </summary>
        public void OnUpdate()
        {
            if (m_paused || m_updateClock.Count < 0)
                return;
            UpdateClock(m_updateClock, Time.time);
        }

        /// <summary>
        /// LateUpdate帧更新（需外部驱动）
        /// </summary>
        public void OnLateUpdate()
        {
            if (m_paused || m_lateUpdateClock.Count < 0)
                return;
            UpdateClock(m_lateUpdateClock, Time.time);
        }

        /// <summary>
        /// 更新指定时钟列表
        /// </summary>
        /// <param name="runingClock">时钟列表</param>
        /// <param name="timeNow">当前时间</param>
        internal void UpdateClock(List<ClockNode> runingClock, float timeNow)
        {
            float timeWorped = timeNow * ACCURACY;
            for (int i = 0; i < runingClock.Count; i++)
            {
                var clockNode = runingClock[i];
                if (clockNode.triggerTime < timeWorped)
                {
                    runingClock.RemoveAt(i);
                    TriggerTimeNode(clockNode, runingClock, timeWorped);
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
        /// <param name="runingClock">所属时钟列表</param>
        /// <param name="timeNow">当前时间</param>
        internal void TriggerTimeNode(ClockNode clockNode, List<ClockNode> runingClock, float timeNow)
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

                if (clockTimerInfo.cylce)
                {
                    bool finished = false;
                    if (!clockTimerInfo.paused && clockTimerInfo.function != null)
                    {
                        try
                        {
                            finished = clockTimerInfo.function.Invoke();
                        }
                        catch (System.Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (!clockTimerInfo.paused && clockTimerInfo.action != null)
                    {
                        try
                        {
                            clockTimerInfo.action.Invoke();
                        }
                        catch (System.Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    if (finished || (clockTimerInfo.countLeft > 0 && --clockTimerInfo.countLeft == 0))
                    {
                        SaveBackClockTimer(timerId, clockTimerInfo);
                        continue;
                    }
                    var triggerTime = clockTimerInfo.interval + clockNode.triggerTime;
                    if (triggerTime < timeNow && clockTimerInfo.interval > 0)
                        triggerTime = UnityEngine.Mathf.CeilToInt(timeNow) + clockTimerInfo.interval;
                    InsertTimerToClock(timerId, triggerTime, runingClock);
                }
                else
                {
                    if (!clockTimerInfo.paused)
                    {
                        try
                        {
                            clockTimerInfo.action.Invoke();
                        }
                        catch (System.Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                        SaveBackClockTimer(timerId, clockTimerInfo);
                    }
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
        internal virtual bool CheckValid(int timerId, TimerStateInfo clockTimerInfo)
        {
            if (clockTimerInfo.function != null || clockTimerInfo.action != null)
                return true;
            return false;
        }

        /// <summary>
        /// 回收时钟节点到对象池
        /// </summary>
        /// <param name="clockNode">时钟节点</param>
        internal void SaveBackClockNode(ClockNode clockNode)
        {
            clockNode.Clear();
            m_clockPool.Push(clockNode);
        }

        /// <summary>
        /// 回收计时器到对象池
        /// </summary>
        /// <param name="timerId">计时器ID</param>
        /// <param name="timerState">计时器状态</param>
        internal virtual void SaveBackClockTimer(int timerId, TimerStateInfo timerState)
        {
            m_timerDic.Remove(timerId);
            timerState.Clear();
            m_timerPool.Push(timerState);
        }

        /// <summary>
        /// 插入计时器到指定时钟列表
        /// </summary>
        /// <param name="timerId">计时器ID</param>
        /// <param name="triggerTime">触发时间</param>
        /// <param name="runingClock">时钟列表</param>
        internal void InsertTimerToClock(int timerId, int triggerTime, List<ClockNode> runingClock)
        {
            int lastIndex = -1;
            for (int i = 0; i < runingClock.Count; i++)
            {
                var clockItem = runingClock[i];
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
                    runingClock.Insert(lastIndex + 1, clockNode);
                    return;
                }
            }
            var newNode = GetOneClockNode(triggerTime);
            newNode.timers.Add(timerId);
            runingClock.Add(newNode);
        }

        /// <summary>
        /// 获取一个空闲的时钟节点
        /// </summary>
        /// <param name="triggerTime">触发时间</param>
        /// <returns>时钟节点</returns>
        internal ClockNode GetOneClockNode(int triggerTime)
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
        internal TimerStateInfo GetTimerInfo()
        {
            if (m_timerPool.Count > 0)
            {
                var timerItem = m_timerPool.Pop();
                return timerItem;
            }
            return new TimerStateInfo();
        }

        /// <summary>
        /// 按类型插入计时器到对应时钟
        /// </summary>
        /// <param name="timerId">计时器ID</param>
        /// <param name="dely">延迟</param>
        /// <param name="updateType">更新类型</param>
        internal void InsertTimeClockByType(int timerId, float dely, TimerUpdateType updateType)
        {
            switch (updateType)
            {
                case TimerUpdateType.FixedUpdate:
                    {
                        int triggerTime = UnityEngine.Mathf.CeilToInt((UnityEngine.Time.fixedTime + dely) * ACCURACY);
                        InsertTimerToClock(timerId, triggerTime, m_fixedUpdateClock);
                    }
                    break;
                case TimerUpdateType.Update:
                    {
                        int triggerTime = UnityEngine.Mathf.CeilToInt((UnityEngine.Time.time + dely) * ACCURACY);
                        InsertTimerToClock(timerId, triggerTime, m_updateClock);
                    }
                    break;
                case TimerUpdateType.LateUpdate:
                    {
                        int triggerTime = UnityEngine.Mathf.CeilToInt((UnityEngine.Time.time + dely) * ACCURACY);
                        InsertTimerToClock(timerId, triggerTime, m_lateUpdateClock);
                    }
                    break;
                default:
                    break;
            }
        }

        public ITimer DelyExecute(System.Action action, float dely, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timerId = SpanTimerId();
            var timerInfo = GetTimerInfo();
            var interval = UnityEngine.Mathf.CeilToInt(dely * ACCURACY);
            timerInfo.interval = interval;
            timerInfo.cylce = false;
            timerInfo.action = action;
            timerInfo.flag = flag;
            m_timerDic[timerId] = timerInfo;
            InsertTimeClockByType(timerId, dely, updateType);
            return new Timer(timerId, PauseTimer, StopTimer);
        }

        public ITimer StartLoop(System.Func<bool> loopFunc, float loopTime, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timer = RegistLoop(loopFunc, loopTime, updateType, flag);
            timer.Pause(false);
            return timer;
        }

        public ITimer StartLoop(System.Func<bool> loopFunc, float loopTime, int loopCount, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timer = RegistLoop(loopFunc, loopTime, loopCount, updateType, flag);
            timer.Pause(false);
            return timer;
        }

        public ITimer StartLoop(System.Action loopFunc, float loopTime, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timer = RegistLoop(loopFunc, loopTime, updateType, flag);
            timer.Pause(false);
            return timer;
        }

        public ITimer StartLoop(System.Action loopFunc, float loopTime, int loopCount, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timer = RegistLoop(loopFunc, loopTime, loopCount, updateType, flag);
            timer.Pause(false);
            return timer;
        }

        public ITimer RegistLoop(System.Func<bool> loopFunc, float loopTime, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timerId = SpanTimerId();
            var timerInfo = GetTimerInfo();
            int interval = UnityEngine.Mathf.CeilToInt(loopTime * ACCURACY);
            timerInfo.interval = interval;
            timerInfo.cylce = true;
            timerInfo.paused = true;
            timerInfo.countLeft = -1;
            timerInfo.function = loopFunc;
            timerInfo.flag = flag;
            m_timerDic[timerId] = timerInfo;
            InsertTimeClockByType(timerId, loopTime, updateType);
            return new Timer(timerId, PauseTimer, StopTimer);
        }

        public ITimer RegistLoop(System.Func<bool> loopFunc, float loopTime, int loopCount, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timerId = SpanTimerId();
            var timerInfo = GetTimerInfo();
            int interval = UnityEngine.Mathf.CeilToInt(loopTime * ACCURACY);
            timerInfo.interval = interval;
            timerInfo.cylce = true;
            timerInfo.paused = true;
            timerInfo.countLeft = loopCount;
            timerInfo.function = loopFunc;
            timerInfo.flag = flag;
            m_timerDic[timerId] = timerInfo;
            InsertTimeClockByType(timerId, loopTime, updateType);
            return new Timer(timerId, PauseTimer, StopTimer);
        }

        public ITimer RegistLoop(System.Action loopFunc, float loopTime, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timerId = SpanTimerId();
            var timerInfo = GetTimerInfo();
            int interval = UnityEngine.Mathf.CeilToInt(loopTime * ACCURACY);
            timerInfo.interval = interval;
            timerInfo.cylce = true;
            timerInfo.paused = true;
            timerInfo.countLeft = -1;
            timerInfo.action = loopFunc;
            timerInfo.flag = flag;
            m_timerDic[timerId] = timerInfo;
            InsertTimeClockByType(timerId, loopTime, updateType);
            return new Timer(timerId, PauseTimer, StopTimer);
        }

        public ITimer RegistLoop(System.Action loopFunc, float loopTime, int loopCount, TimerUpdateType updateType = TimerUpdateType.Update, string flag = default)
        {
            var timerId = SpanTimerId();
            var timerInfo = GetTimerInfo();
            int interval = UnityEngine.Mathf.CeilToInt(loopTime * ACCURACY);
            timerInfo.interval = interval;
            timerInfo.cylce = true;
            timerInfo.paused = true;
            timerInfo.countLeft = loopCount;
            timerInfo.action = loopFunc;
            timerInfo.flag = flag;
            m_timerDic[timerId] = timerInfo;
            InsertTimeClockByType(timerId, loopTime, updateType);
            return new Timer(timerId, PauseTimer, StopTimer);
        }

        private int SpanTimerId()
        {
            if (m_timerIdSpan >= int.MaxValue)
            {
                m_timerIdSpan = 0;
                m_checkTimerExists = true;
            }
            if (m_checkTimerExists)
            {
                while (m_timerIdSpan < int.MaxValue)
                {
                    m_timerIdSpan++;
                    if (!m_timerDic.ContainsKey(m_timerIdSpan))
                        break;
                }
            }
            else
            {
                ++m_timerIdSpan;
            }
            return m_timerIdSpan;
        }

        public bool ExistsTimer(int timerIndex)
        {
            return m_timerDic.ContainsKey(timerIndex);
        }

        public void StartTimer(int timerIndex)
        {
            PauseTimer(timerIndex, false);
        }

        public void PauseTimer(int timerIndex)
        {
            PauseTimer(timerIndex, true);
        }

        protected void PauseTimer(int timerIndex, bool paused)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var timer))
            {
                timer.paused = paused;
            }
        }

        public void StopTimer(int timerIndex)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var clockTimer))
            {
                SaveBackClockTimer(timerIndex, clockTimer);
            }
        }


        public void StopTimer(ref int timerIndex)
        {
            StopTimer(timerIndex);
            timerIndex = 0;
        }

        public bool ChangeTimerInterval(int timerIndex, float timeSpan)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var clockTimer))
            {
                clockTimer.interval = UnityEngine.Mathf.CeilToInt(timeSpan * ACCURACY);
                return true;
            }
            return false;
        }
        public bool ChangeTimerLoopCount(int timerIndex, int loopCount)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var clockTimer))
            {
                clockTimer.countLeft = loopCount;
                return true;
            }
            return false;
        }

        public bool ChangeTimerCallBack(int timerIndex, System.Action callback)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var clockTimer))
            {
                clockTimer.action = callback;
                return true;
            }
            return false;
        }
        public bool ChangeTimerCallBack(int timerIndex, System.Func<bool> callback)
        {
            if (m_timerDic.TryGetValue(timerIndex, out var clockTimer))
            {
                clockTimer.function = callback;
                return true;
            }
            return false;
        }
        public void ClearAll()
        {
            m_timerDic.Clear();
            m_clockPool.Clear();
            m_timerPool.Clear();
            m_timerIdSpan = 0;
        }

        public void ClearCatchs()
        {
            m_clockPool.Clear();
            m_timerPool.Clear();
        }
    }
}