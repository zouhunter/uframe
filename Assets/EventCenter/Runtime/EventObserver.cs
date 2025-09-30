/*-*-* Copyright (c) uframe@asia
 * Author: zouhunter
 * Creation Date: 2024-02-28
 * Version: 1.0.0
 * Description: 
 *_*/

using System;

using Unity.VisualScripting.Antlr3.Runtime.Tree;

using UnityEngine;

namespace UFrame.EventCenter
{
    /// <summary>
    /// 事件观察者接口
    /// </summary>
    public interface IObserver : IDisposable
    {
        bool Exists(MulticastDelegate action);
        void Pause(bool pause);
        void Trigger(string eventName, object info);
    }

    /// <summary>
    /// 简单观察者
    /// </summary>
    public class EventObserver0 : IObserver
    {
        private Action _action0;
        private Action<IObserver> _removeFunc;
        protected bool _isPause;

        public EventObserver0(Action action, Action<IObserver> removeFunc)
        {
            _action0 = action;
            _removeFunc = removeFunc;
        }

        /// <summary>
        /// 暂停事件
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            _isPause = pause;
        }

        /// <summary>
        /// 存在性判断
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Exists(MulticastDelegate action)
        {
            if (action is Action action0 && _action0 == action0)
                return true;
            return false;
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="info"></param>
        public void Trigger(string eventName, object info)
        {
            if (!_isPause)
            {
                try
                {
                    _action0?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// 释放事件
        /// </summary>
        public void Dispose()
        {
            if (_action0 != null)
                _removeFunc?.Invoke(this);
        }
    }

    /// <summary>
    /// 事件观察者
    /// </summary>
    public class EventObserver : IObserver
    {
        private Action<string> _action;
        private Action<IObserver> _removeFunc;
        protected bool _isPause;

        public EventObserver(Action<string> action, Action<IObserver> removeFunc)
        {
            _action = action;
            _removeFunc = removeFunc;
        }

        /// <summary>
        /// 暂停事件
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            _isPause = pause;
        }

        /// <summary>
        /// 存在性判断
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Exists(MulticastDelegate action)
        {
            if(action is Action<string> action1 && _action == action1)
                return true;
            return false;
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="info"></param>
        public void Trigger(string eventName, object info)
        {
            if (!_isPause)
            {
                try
                {
                    _action?.Invoke(eventName);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// 释放事件
        /// </summary>
        public void Dispose()
        {
            if (_action != null)
                _removeFunc?.Invoke(this);
        }
    }

    /// <summary>
    /// 事件观察者（泛型）
    /// </summary>
    public class EventObserver<T> : IObserver
    {
        private Action<string, T> _action;
        private Action<IObserver> _removeFunc;
        private bool _isPause;

        public EventObserver(Action<string, T> action, Action<IObserver> removeFunc)
        {
            _action = action;
            _removeFunc = removeFunc;
        }

        /// <summary>
        /// 事件暂停
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            _isPause = pause;
        }

        /// <summary>
        /// 事件比对
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Exists(MulticastDelegate action)
        {
            return _action == (Action<string, T>)action;
        }

        //触发事件
        public void Trigger(string eventName, object info)
        {
            if (!_isPause)
            {
                if (info == null)
                {
                    _action?.Invoke(eventName, default(T));
                }
                else if (info is T t)
                {
                    _action?.Invoke(eventName, t);
                }
                else
                {
                    Debug.LogError("event arg type missmatch:" + eventName + "," + info.GetType());
                }
            }
        }
        /// <summary>
        /// 释放事件
        /// </summary>
        public void Dispose()
        {
            _removeFunc?.Invoke(this);
        }
    }


    /// <summary>
    /// 事件观察者（泛型）
    /// </summary>
    public class EventObserver0<T> : IObserver
    {
        private Action<T> _action;
        private Action<IObserver> _removeFunc;
        private bool _isPause;

        public EventObserver0(Action<T> action, Action<IObserver> removeFunc)
        {
            _action = action;
            _removeFunc = removeFunc;
        }

        /// <summary>
        /// 事件暂停
        /// </summary>
        /// <param name="pause"></param>
        public void Pause(bool pause)
        {
            _isPause = pause;
        }

        /// <summary>
        /// 事件比对
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Exists(MulticastDelegate action)
        {
            return _action == (Action<T>)action;
        }

        //触发事件
        public void Trigger(string eventName, object info)
        {
            if (!_isPause)
            {
                if (info == null)
                {
                    _action?.Invoke(default(T));
                }
                else if (info is T t)
                {
                    _action?.Invoke(t);
                }
                else
                {
                    Debug.LogError("event arg type missmatch:" + eventName + "," + info.GetType());
                }
            }
        }
        /// <summary>
        /// 释放事件
        /// </summary>
        public void Dispose()
        {
            _removeFunc?.Invoke(this);
        }
    }
}

