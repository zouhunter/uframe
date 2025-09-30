/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-27                                                                   *
*  版本: master_                                                                      *
*  功能:                                                                              *
*   - 状态事件容器                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.EventStates
{
    public delegate void StateEvent<StateType>(StateType state);
    public delegate bool StateChangeableCheck<StateType>(StateType state, StateType newState);
    public interface IState<StateType>
    {
        bool CanEnter { get; }
        bool CanExit { get; }
        bool NeedUpdate { get; }
        void OnStateEnter(StateType type);//修改状态事件
        void OnStateUpdate(StateType type);//状态更新事件
        void OnStateExit(StateType type);//状态退出事件
    }
    public class State<StateType> : IState<StateType>
    {
        public StateEvent<StateType> onStateEnter { get; set; }//修改状态事件
        public StateEvent<StateType> onStateUpdate { get; set; }//状态更新事件
        public StateEvent<StateType> onStateExit { get; set; }//状态退出事件

        public virtual bool CanEnter => onStateEnter != null;

        public virtual bool CanExit => onStateExit != null;

        public virtual bool NeedUpdate => onStateUpdate != null;

        public State(StateEvent<StateType> onStateEnter = null,
            StateEvent<StateType> onStateUpdate = null,
            StateEvent<StateType> onStateExit = null)
        {
            this.onStateEnter = onStateEnter;
            this.onStateUpdate = onStateUpdate;
            this.onStateExit = onStateExit;
        }

        public void OnStateEnter(StateType type)
        {
            try
            {
                onStateEnter?.Invoke(type);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void OnStateUpdate(StateType type)
        {
            try
            {
                onStateUpdate?.Invoke(type);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void OnStateExit(StateType type)
        {
            try
            {
                onStateExit?.Invoke(type);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
