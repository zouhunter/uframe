using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UFrame.EventCenter
{

    /// <summary>
    /// 事件中心 单例模式对象
    /// </summary>
    public class EventController
    {
        private Dictionary<string, EventGroup> eventDic = new Dictionary<string, EventGroup>();

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="name">事件的名字</param>
        /// <param name="action">准备用来处理事件 的委托函数</param>
        public IObserver Regist<T>(string name, Action<string, T> action)
        {
            if (!eventDic.TryGetValue(name, out var group))
            {
                group = eventDic[name] = new EventGroup();
            }
            return group.Append(action);
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="name">事件的名字</param>
        /// <param name="action">准备用来处理事件 的委托函数</param>
        public IObserver Regist<T>(string name, Action<T> action)
        {
            if (!eventDic.TryGetValue(name, out var group))
            {
                group = eventDic[name] = new EventGroup();
            }
            return group.Append(action);
        }

        /// <summary>
        /// 监听不需要参数传递的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public IObserver Regist(string name, Action<string> action)
        {
            if (!eventDic.TryGetValue(name, out var info))
                eventDic[name] = info = new EventGroup();
            return info.Append(action);
        }

        /// <summary>
        /// 监听不需要参数传递的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public IObserver Regist(string name, Action action)
        {
            if (!eventDic.TryGetValue(name, out var info))
                eventDic[name] = info = new EventGroup();
            return info.Append(action);
        }

        /// <summary>
        /// 移除对应的事件监听
        /// </summary>
        /// <param name="name">事件的名字</param>
        /// <param name="action">对应之前添加的委托函数</param>
        public void Remove<T>(string name, Action<string, T> action)
        {
            if (eventDic.TryGetValue(name, out var info))
                info.RemoveAt(action);
        }

        /// <summary>
        /// 移除不需要参数的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void Remove(string name, Action<string> action)
        {
            if (eventDic.TryGetValue(name, out var info))
                info.RemoveAt(action);
        }

        /// <summary>
        /// 移除不需要参数的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void Remove(string name, Action action)
        {
            if (eventDic.TryGetValue(name, out var info))
                info.RemoveAt(action);
        }

        /// <summary>
        /// 移除不需要参数的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void Remove(string name, IObserver observer)
        {
            if (eventDic.TryGetValue(name, out var info))
                info.Remove(observer);
        }

        /// <summary>
        /// 事件触发
        /// </summary>
        /// <param name="name">哪一个名字的事件触发了</param>
        public void Notify(string name, object info = null)
        {
            var index = name.IndexOf('.');
            while (index > 0)
            {
                var eventFirstName = name.Substring(0, index);
                if (eventDic.TryGetValue(eventFirstName, out var action))
                    action?.Trigger(name, info);
                index = name.IndexOf('.', index + 1);
            }
            if (eventDic.TryGetValue(name, out var eventInfo))
            {
                eventInfo?.Trigger(name, info);
            }
        }
    }

}
