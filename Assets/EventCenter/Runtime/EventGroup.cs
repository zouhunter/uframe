//*************************************************************************************
//* 作    者： zouhangte
//* 创建时间： 2024-02-28
//* 描    述： 事件中心

//* ************************************************************************************
using System;
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.EventCenter
{
    /// <summary>
    /// 事件注册组信息
    /// </summary>
    public class EventGroup
    {
        private List<IObserver> _observers;

        public EventGroup()
        {
            this._observers = new List<IObserver>();
        }

        public IObserver Append<T>(Action<string, T> action)
        {
            if (action == null)
                return null;
            var observer = _observers.Find(x => x.Exists(action));
            if (observer == null)
            {
                observer = new EventObserver<T>(action, Remove);
                _observers.Add(observer);
            }
            return observer;
        }

        public IObserver Append<T>(Action<T> action)
        {
            if (action == null)
                return null;
            var observer = _observers.Find(x => x.Exists(action));
            if (observer == null)
            {
                observer = new EventObserver0<T>(action, Remove);
                _observers.Add(observer);
            }
            return observer;
        }

        public IObserver Append(Action action)
        {
            if (action == null)
                return null;
            var observer = _observers.Find(x => x.Exists(action));
            if (observer == null)
            {
                observer = new EventObserver0(action, Remove);
                _observers.Add(observer);
            }
            return observer;
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IObserver Append(Action<string> action)
        {
            var observer = _observers.Find(x => x.Exists(action));
            if (observer == null)
            {
                observer = new EventObserver(action, Remove);
                _observers.Add(observer);
            }
            return observer;
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAt(Action<string> action)
        {
            var observer = _observers?.Find(x => x.Exists(action));
            if (observer != null)
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAt(Action action)
        {
            var observer = _observers?.Find(x => x.Exists(action));
            if (observer != null)
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void RemoveAt<T>(Action<string, T> action)
        {
            var observer = _observers?.Find(x => x.Exists(action));
            if (observer != null)
            {
                Remove(observer);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="observer"></param>
        public void Remove(IObserver observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="info"></param>
        public void Trigger(string eventName, object info)
        {
            if (_observers != null)
            {
                foreach (var action in _observers)
                {
                    try
                    {
                        action?.Trigger(eventName, info);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="info"></param>
        public void Trigger(string eventName)
        {
            if (_observers != null)
            {
                foreach (var action in _observers)
                {
                    try
                    {
                        action?.Trigger(eventName,null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }

}

