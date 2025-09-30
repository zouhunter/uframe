/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-27                                                                   *
*  版本: master_                                                                      *
*  功能:                                                                              *
*   - 状态控制器                                                                      *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame.EventStates
{
    public class EventStateCtrl<StateId> where StateId : IEquatable<StateId>
    {
        protected Dictionary<StateId, IState<StateId>> m_stateMap = new Dictionary<StateId, IState<StateId>>();
        protected StateId m_state = default(StateId);
        protected StateChangeableCheck<StateId> m_stateChangeableCheck;
        protected StateEvent<StateId> m_onStateChanged;
        protected IState<StateId> m_currentState;

        public StateId CurrentState => m_state;

        public EventStateCtrl(StateId startState = default(StateId))
        {
            m_state = startState;
        }
        public virtual void SetChangeCheck(StateChangeableCheck<StateId> changeCheck)
        {
            m_stateChangeableCheck = changeCheck;
        }

        public virtual bool CanEnter(StateId stateId)
        {
            if (m_stateMap.TryGetValue(stateId, out var context))
            {
                return context.CanEnter;
            }
            return false;
        }

        public virtual void SetStateChange(StateEvent<StateId> onStateChanged)
        {
            this.m_onStateChanged = onStateChanged;
        }

        public virtual void SetEnterEvent(StateId stateId, StateEvent<StateId> onStart)
        {
            if (!m_stateMap.TryGetValue(stateId, out var context))
            {
                context = new State<StateId>();
                m_stateMap[stateId] = context;
            }
            if(context is State<StateId> state)
            {
                state.onStateEnter = onStart;
            }
        }

        public virtual bool CanExit(StateId stateId)
        {
            if (m_stateMap.TryGetValue(stateId, out var context))
            {
                return context.CanExit;
            }
            return false;
        }

        public virtual void SetExitEvent(StateId stateId, StateEvent<StateId> onExit)
        {
            if (!m_stateMap.TryGetValue(stateId, out var context))
            {
                context = new State<StateId>();
                m_stateMap[stateId] = context;
            }
            if (context is State<StateId> state)
            {
                state.onStateExit = onExit;
            }
        }

        public virtual bool NeedUpdate(StateId stateId)
        {
            if (m_stateMap.TryGetValue(stateId, out var context))
            {
                return context.NeedUpdate;
            }
            return false;
        }

        public virtual void SetUpdateEvent(StateId stateId, StateEvent<StateId> onUpdate)
        {
            if (!m_stateMap.TryGetValue(stateId, out var context))
            {
                context = new State<StateId>();
                m_stateMap[stateId] = context;
            }
            if (context is State<StateId> state)
            {
                state.onStateUpdate = onUpdate;
            }
        }

        public virtual void RegistState(StateId stateId, IState<StateId> events)
        {
            m_stateMap[stateId] = events;
        }

        public virtual void RemoveState(StateId stateId)
        {
            m_stateMap.Remove(stateId);
        }

        public virtual StateId UpdateState(StateId state)
        {
            bool equal = CurrentState.Equals(state);
            if (equal && m_currentState == null && (!m_stateMap.TryGetValue(state, out m_currentState) || m_currentState == null))
                return CurrentState;

            if(m_currentState != null && m_currentState.NeedUpdate)
                m_currentState.OnStateUpdate(state);

            //更新判断
            if (!equal)
            {
                // 判断状态是否可以过度
                if (m_stateChangeableCheck != null && !m_stateChangeableCheck.Invoke(CurrentState, state))
                    return CurrentState;

                // 状态不同且之前状态存在则退出
                if (m_stateMap.TryGetValue(CurrentState, out var oldContext))
                    oldContext.OnStateExit(CurrentState);

                //设置当前状态
                m_state = state;
                m_onStateChanged?.Invoke(state);

                // 新状态存在，则进入状态
                if (m_stateMap.TryGetValue(state, out m_currentState))
                    m_currentState.OnStateEnter(state);
            }
            return CurrentState;
        }

        public virtual void Release()
        {
            m_stateMap.Clear();
        }
    }
}
