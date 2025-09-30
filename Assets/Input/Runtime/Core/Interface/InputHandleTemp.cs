/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 输入句柄模板                                                                    *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Inputs
{
    public abstract class InputHandleTemp<Key, Action> : IInputHandle
    {
        public Action<string> onError { get; set; }
        public Action<IInputHandle> onEmpty { get; set; }
        public Action<IInputHandle> onNotEmpty { get; set; }
        protected Dictionary<Key, List<Action>> actionDic;
        protected Stack<KeyValuePair<Key, Action>> waitAdd;
        protected Stack<KeyValuePair<Key, Action>> waitRemove;
        private int _actionCount;
        protected int actionCount
        {
            get
            {
                return _actionCount;
            }
            set
            {
                if(_actionCount != 0 && value == 0)
                {
                    _actionCount = value;

                    if (onEmpty != null)
                    {
                        onEmpty.Invoke(this);
                    }
                }

                else if(_actionCount != 1 && value == 1)
                {
                    _actionCount = value;

                    if (onNotEmpty != null)
                    {
                        onNotEmpty.Invoke(this);
                    }
                }
                else
                {
                    _actionCount = value;
                }

            }
        }
        protected bool execution;

        public InputHandleTemp()
        {
            actionDic = new Dictionary<Key, List<Action>>();
            waitAdd = new Stack<KeyValuePair<Key, Action>>();
            waitRemove = new Stack<KeyValuePair<Key, Action>>();
            _actionCount = 0;
        }

        public virtual void Dispose()
        {
            actionDic.Clear();
            waitAdd.Clear();
            waitRemove.Clear();
            actionCount = 0;
        }

        public bool HaveEvent()
        {
            return actionCount > 0;
        }

        public void RegistEvent(Key mouseID, Action callBack)
        {
            List<Action> actions;
            if (!actionDic.TryGetValue(mouseID, out actions))
            {
                actions = actionDic[mouseID] = new List<Action>();
            }
            if (!actions.Contains(callBack))
            {
                if (execution)
                {
                    waitAdd.Push(new KeyValuePair<Key, Action>(mouseID, callBack));
                }
                else
                {
                    actions.Add(callBack);
                    actionCount++;
                }
            }
        }

        public void RemoveEvent(Key mouseID, Action callBack)
        {
            List<Action> actions;
            if (actionDic.TryGetValue(mouseID, out actions))
            {
                if (actions.Contains(callBack))
                {
                    if (execution)
                    {
                        waitRemove.Push(new KeyValuePair<Key, Action>(mouseID, callBack));
                    }
                    else
                    {
                        actions.Remove(callBack);
                        actionCount--;
                    }
                }
            }
        }

        public void Execute()
        {
            execution = true;
            ExecuteInternal();
            execution = false;

            while (waitAdd.Count > 0)
            {
                var item = waitAdd.Pop();
                actionDic[item.Key].Add(item.Value);
                actionCount++;
            }


            while (waitRemove.Count > 0)
            {
                var item = waitRemove.Pop();
                actionDic[item.Key].Remove(item.Value);
                actionCount--;
            }
        }
        
        protected abstract void ExecuteInternal();
    }

}