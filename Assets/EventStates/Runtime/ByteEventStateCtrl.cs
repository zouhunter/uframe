/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-27                                                                   *
*  版本: master_                                                                      *
*  功能:                                                                              *
*   - 状态控制器                                                                      *
*//************************************************************************************/
using System.Collections.Generic;

namespace UFrame.EventStates
{
    public class ByteEventStateCtrl
    {
        internal Dictionary<byte, State<byte>> m_stateMap = new Dictionary<byte, State<byte>>();
        internal StateChangeableCheck<byte> m_stateChangeCheck;
        internal byte m_lastState = 255;
        internal State<byte> m_currentState;

        public void SetChangeCheck(StateChangeableCheck<byte> changeCheck)
        {
            m_stateChangeCheck = changeCheck;
        }

        public bool ExistEnterEvent(byte stateId)
        {
            if (m_stateMap.TryGetValue(stateId, out var context))
            {
                return context.onStateEnter != null;
            }
            return false;
        }

        public void SetEnterEvent(byte stateId, StateEvent<byte> onStart)
        {
            if (!m_stateMap.TryGetValue(stateId, out var context))
            {
                context = new State<byte>();
                m_stateMap[stateId] = context;
            }
            context.onStateEnter = onStart;
        }

        public bool ExistExitEvent(byte stateId)
        {
            if (m_stateMap.TryGetValue(stateId, out var context))
            {
                return context.onStateExit != null;
            }
            return false;
        }

        public void SetExitEvent(byte stateId, StateEvent<byte> onExit)
        {
            if (!m_stateMap.TryGetValue(stateId, out var context))
            {
                context = new State<byte>();
                m_stateMap[stateId] = context;
            }
            context.onStateExit = onExit;
        }

        public bool ExistUpdateEvent(byte stateId)
        {
            if (m_stateMap.TryGetValue(stateId, out var context))
            {
                return context.onStateUpdate != null;
            }
            return false;
        }

        public void SetUpdateEvent(byte stateId, StateEvent<byte> onUpdate)
        {
            if (!m_stateMap.TryGetValue(stateId, out var context))
            {
                context = new State<byte>();
                m_stateMap[stateId] = context;
            }
            context.onStateUpdate = onUpdate;
        }

        internal void RegistState(byte stateId, State<byte> events)
        {
            m_stateMap[stateId] = events;
        }

        public void RemoveState(byte stateId)
        {
            m_stateMap.Remove(stateId);
        }

        protected void UpdateCurrentState()
        {
            if (m_currentState != null && m_currentState.onStateUpdate != null)
                m_currentState.onStateUpdate(m_lastState);
        }

        public byte UpdateState(byte state)
        {
            if (m_lastState != state)
            {
                // 判断状态是否可以过度
                if (m_stateChangeCheck != null)
                {
                    bool canChange = m_stateChangeCheck(m_lastState, state);
                    if (!canChange)
                    {
                        UpdateCurrentState();
                        return m_lastState;
                    }
                }
                // 状态不同且之前状态存在则退出
                if (m_lastState != 255)
                {
                    if (m_stateMap.TryGetValue(m_lastState, out var oldContext) && oldContext.onStateExit != null)
                        oldContext.onStateExit(m_lastState);
                }
                // 新状态存在，则进入状态
                if (state != 255)
                {
                    if (m_stateMap.TryGetValue(state, out m_currentState) && m_currentState != null && m_currentState.onStateEnter != null)
                        m_currentState.onStateEnter(state);
                }
                m_lastState = state;
            }
            UpdateCurrentState();
            return m_lastState;
        }

        public void Release()
        {
            m_stateMap.Clear();
        }
    }
}
