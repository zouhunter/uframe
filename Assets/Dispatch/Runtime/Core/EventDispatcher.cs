/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 事件派发                                                                        *
*//************************************************************************************/
using System;
using System.Collections.Generic;

namespace UFrame.EventDispatch
{
    public class EventDispatcher<Key> : IEventDispatcher<Key>
    {
        internal IDictionary<Key, EventObserver> m_observerMap;
        public Action<Key, string> messageNoHandle { get; set; }
        public Action<Key, Exception> messageException { get; set; }
        protected bool m_isRuning = false;
        internal Stack<EventDelegate> m_eventPool;
        protected int m_eventIdSpaner = 0;
        internal Queue<Tuple<Key, EventDelegate>> m_waitAdd;
        protected Queue<Tuple<Key, int>> m_waitDelete;

        public EventDispatcher()
        {
            m_observerMap = new Dictionary<Key, EventObserver>();
            m_eventPool = new Stack<EventDelegate>();
            m_waitAdd = new Queue<Tuple<Key, EventDelegate>>();
            m_waitDelete = new Queue<Tuple<Key, int>>();
        }

        internal EventDelegate GetEventDelegate()
        {
            EventDelegate eventAction = null;
            if (m_eventPool.Count > 0)
                eventAction = m_eventPool.Pop();
            else
                eventAction = new EventDelegate();
            eventAction.id = ++m_eventIdSpaner;
            return eventAction;
        }

        internal void SaveBackEventDelegate(EventDelegate eventAction)
        {
            if (eventAction != null)
            {
                eventAction.Clean();
                m_eventPool.Push(eventAction);
            }
        }

        protected virtual void NoMessageaction(Key eventKey, string err)
        {
            if (messageNoHandle != null)
            {
                messageNoHandle(eventKey, err);
            }
        }
        protected virtual void MessageException(Key eventKey, Exception e)
        {
            if (messageException != null)
            {
                messageException.Invoke(eventKey, e);
            }
        }
        public virtual void Clear()
        {
            m_observerMap.Clear();
        }

        #region 注册注销事件
        public int RegistEvent(Key key, Action action)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                var id = observer.FindId(action);
                if (id > 0)
                {
                    MessageException(key, new Exception("Repeat registration!"));
                    return id;
                }
            }
            var eventAction = GetEventDelegate();
            eventAction.simple = action;
            RegistEventInternal(key, eventAction);
            return eventAction.id;
        }
        public int RegistEvent<T>(Key key, Action<T> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2>(Key key, Action<T1, T2> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2, T3>(Key key, Action<T1, T2, T3> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2, T3, T4>(Key key, Action<T1, T2, T3, T4> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2, T3, T4, T5>(Key key, Action<T1, T2, T3, T4, T5> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<R>(Key key, Func<R> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, R>(Key key, Func<T1, R> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2, R>(Key key, Func<T1, T2, R> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2, T3, R>(Key key, Func<T1, T2, T3, R> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }
        public int RegistEvent<T1, T2, T3, T4, R>(Key key, Func<T1, T2, T3, T4, R> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }

        public int RegistEvent<T1, T2, T3, T4, T5, R>(Key key, Func<T1, T2, T3, T4, T5, R> action)
        {
            var id = FindEvent(key, action);
            if (id <= 0)
                id = RegistEventInternal(key, action);
            return id;
        }

        internal void RegistEventInternal(Key key, EventDelegate eventAction)
        {
            if (eventAction == null) return;

            if (!m_observerMap.ContainsKey(key))
            {
                m_observerMap.Add(key, new EventObserver());
            }

            if (m_isRuning)
            {
                m_waitAdd.Enqueue(new Tuple<Key, EventDelegate>(key, eventAction));
            }
            else
            {
                m_observerMap[key].Add(eventAction);
            }
        }

        protected int RegistEventInternal(Key key, MulticastDelegate action)
        {
            var eventAction = GetEventDelegate();
            eventAction.complex = action;
            RegistEventInternal(key, eventAction);
            return eventAction.id;
        }
        #endregion

        #region 移除事件
        public bool RemoveEvents(Key key)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var eventAction in observer)
                {
                    SaveBackEventDelegate(eventAction);
                }
                m_observerMap.Remove(key);
                return true;
            }
            return false;
        }

        public bool RemoveEvent(Key key, Action action)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var observer = m_observerMap[key];
                var id = observer.FindId(action);
                if (id > 0)
                {
                    var removed = observer.Remove(id);
                    if (removed != null)
                    {
                        SaveBackEventDelegate(removed);
                        return true;
                    }
                }
            }
            return true;
        }

        public bool RemoveEvent<T>(Key key, Action<T> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2>(Key key, Action<T1, T2> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, T3>(Key key, Action<T1, T2, T3> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, T3, T4>(Key key, Action<T1, T2, T3, T4> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, T3, T4, T5>(Key key, Action<T1, T2, T3, T4, T5> action)
        {
            return RemoveEventInternal(key, action);
        }

        public bool RemoveEvent<R>(Key key, Func<R> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, R>(Key key, Func<T1, R> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, R>(Key key, Func<T1, T2, R> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, T3, R>(Key key, Func<T1, T2, T3, R> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, T3, T4, R>(Key key, Func<T1, T2, T3, T4, R> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent<T1, T2, T3, T4, T5, R>(Key key, Func<T1, T2, T3, T4, T5, R> action)
        {
            return RemoveEventInternal(key, action);
        }
        public bool RemoveEvent(Key key, int id)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var observer = m_observerMap[key];
                var removed = observer.Remove(id);
                if (removed != null)
                {
                    SaveBackEventDelegate(removed);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        protected bool RemoveEventInternal(Key key, MulticastDelegate action)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var observer = m_observerMap[key];
                var id = observer.FindId(action);
                if (id > 0)
                {
                    var removed = observer.Remove(id);
                    if (removed != null)
                    {
                        SaveBackEventDelegate(removed);
                        return true;
                    }
                }
            }
            return true;
        }
        #endregion 移除事件

        #region 触发事件
        protected bool CheckParameters(MulticastDelegate action, object[] arguments, out object[] finalArguments)
        {
            finalArguments = null;
            var parameters = action.Method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (arguments.Length <= i)
                    return false;
                if (!parameters[i].ParameterType.IsAssignableFrom(arguments[i].GetType()))
                    return false;
            }
            if (arguments.Length == parameters.Length)
            {
                finalArguments = arguments;
            }
            else
            {
                finalArguments = new object[parameters.Length];
                Array.Copy(arguments, finalArguments, finalArguments.Length);
            }
            return true;
        }

        public void ExecuteAction(Key key)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action)
                    {
                        try
                        {
                            (action.complex as Action).Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public void ExecuteAction<T>(Key key, T arg0)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T>)
                    {
                        try
                        {
                            (action.complex as Action<T>).Invoke(arg0);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public void ExecuteAction<T1, T2>(Key key, T1 arg1, T2 arg2)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2>).Invoke(arg1, arg2);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1>)
                    {
                        try
                        {
                            (action.complex as Action<T1>).Invoke(arg1);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public void ExecuteAction<T1, T2, T3>(Key key, T1 arg1, T2 arg2, T3 arg3)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2, T3>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2, T3>).Invoke(arg1, arg2, arg3);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2>).Invoke(arg1, arg2);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1>)
                    {
                        try
                        {
                            (action.complex as Action<T1>).Invoke(arg1);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public void ExecuteAction<T1, T2, T3, T4>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2, T3, T4>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2, T3, T4>).Invoke(arg1, arg2, arg3, arg4);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2, T3>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2, T3>).Invoke(arg1, arg2, arg3);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2>).Invoke(arg1, arg2);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1>)
                    {
                        try
                        {
                            (action.complex as Action<T1>).Invoke(arg1);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public void ExecuteAction<T1, T2, T3, T4, T5>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2, T3, T4, T5>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2, T3, T4, T5>).Invoke(arg1, arg2, arg3, arg4, arg5);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2, T3, T4>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2, T3, T4>).Invoke(arg1, arg2, arg3, arg4);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2, T3>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2, T3>).Invoke(arg1, arg2, arg3);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1, T2>)
                    {
                        try
                        {
                            (action.complex as Action<T1, T2>).Invoke(arg1, arg2);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Action<T1>)
                    {
                        try
                        {
                            (action.complex as Action<T1>).Invoke(arg1);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public R[] ExecuteFunc<R>(Key key)
        {
            R[] results = null;
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                results = new R[list.Count];
                int index = -1;
                foreach (var action in list)
                {
                    index++;
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        results[index] = default(R);
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<R>).Invoke();
                            results[index] = result;
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
            return results;
        }
        public void ExecuteFunc<T1, R>(Key key, T1 arg1, List<R> results = null)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                int index = -1;
                foreach (var action in list)
                {
                    index++;
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, R>).Invoke(arg1);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<R>).Invoke();
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
        }
        public void ExecuteFunc<T1, T2, R>(Key key, T1 arg1, T2 arg2, List<R> results = null)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                int index = -1;
                foreach (var action in list)
                {
                    index++;
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, R>).Invoke(arg1, arg2);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, R>).Invoke(arg1);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<R>).Invoke();
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
        }
        public void ExecuteFunc<T1, T2, T3, R>(Key key, T1 arg1, T2 arg2, T3 arg3, List<R> results = null)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                int index = -1;
                foreach (var action in list)
                {
                    index++;
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, T3, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, T3, R>).Invoke(arg1, arg2, arg3);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, R>).Invoke(arg1, arg2);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, R>).Invoke(arg1);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<R>).Invoke();
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
        }
        public void ExecuteFunc<T1, T2, T3, T4, R>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, List<R> results = null)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                int index = -1;
                foreach (var action in list)
                {
                    index++;
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, T3, T4, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, T3, T4, R>).Invoke(arg1, arg2, arg3, arg4);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, T3, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, T3, R>).Invoke(arg1, arg2, arg3);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, R>).Invoke(arg1, arg2);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, R>).Invoke(arg1);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<R>).Invoke();
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
        }

        public void ExecuteFunc<T1, T2, T3, T4, T5, R>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, List<R> results = null)
        {
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                int index = -1;
                foreach (var action in list)
                {
                    index++;
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, T3, T4, T5, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, T3, T4, T5, R>).Invoke(arg1, arg2, arg3, arg4, arg5);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, T3, T4, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, T3, T4, R>).Invoke(arg1, arg2, arg3, arg4);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, T3, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, T3, R>).Invoke(arg1, arg2, arg3);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, T2, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, T2, R>).Invoke(arg1, arg2);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<T1, R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<T1, R>).Invoke(arg1);
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (action.complex is Func<R>)
                    {
                        try
                        {
                            var result = (action.complex as Func<R>).Invoke();
                            if (results != null)
                                results.Add(result);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
        }

        public void ExecuteEvents(Key key, params object[] arguments)
        {
            if (m_observerMap.TryGetValue(key, out var observer))
            {
                foreach (var action in observer)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (CheckParameters(action.complex, arguments, out var args))
                    {
                        try
                        {
                            action.complex.DynamicInvoke(args);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "not reg!");
            }
            RunEndAction();
        }

        public object[] ExecuteEventsReturn(Key key, params object[] arguments)
        {
            List<object> results = null;
            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                foreach (var action in list)
                {
                    if (action.checker != null && action.checker.Invoke() == false)
                    {
                        var removeId = action.id;
                        m_waitDelete.Enqueue(new Tuple<Key, int>(key, removeId));
                        continue;
                    }
                    if (action.simple != null)
                    {
                        try
                        {
                            action.simple.Invoke();
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                    else if (CheckParameters(action.complex, arguments, out var args))
                    {
                        try
                        {
                            var result = action.complex.DynamicInvoke(args);
                            if (result != null)
                            {
                                if (results == null)
                                    results = new List<object>();
                                results.Add(result);
                            }
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }
                }
            }
            else
            {
                NoMessageaction(key, "未注册");
            }

            RunEndAction();
            return results?.ToArray() ?? null;
        }
        #endregion

        #region 判断是否存在
        public bool ExistsEvent(Key key)
        {
            if (m_observerMap.ContainsKey(key))
            {
                return m_observerMap[key].Count > 0;
            }
            return false;
        }

        public bool ExistsEvent(Key key, MulticastDelegate action)
        {
            return FindEvent(key, action) > 0;
        }

        public bool ExistsEvent(Key key, Action action)
        {
            return FindEvent(key, action) > 0;
        }

        public int FindEvent(Key key, MulticastDelegate action)
        {
            if (action == null)
                return 0;

            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                return list.FindId(action);
            }
            return 0;
        }

        public int FindEvent(Key key, Action action)
        {
            if (action == null)
                return 0;

            if (m_observerMap.ContainsKey(key))
            {
                var list = m_observerMap[key];
                return list.FindId(action);
            }
            return 0;
        }
        #endregion

        protected void RunEndAction()
        {
            while (m_waitAdd.Count > 0)
            {
                var tuple = m_waitAdd.Dequeue();
                RegistEventInternal(tuple.Item1, tuple.Item2);
            }
            while (m_waitDelete.Count > 0)
            {
                var tuple = m_waitDelete.Dequeue();
                RemoveEvent(tuple.Item1, tuple.Item2);
            }
        }

        public void RegistMessageNoHandle(Action<Key, string> messageNoHandle)
        {
            if (messageNoHandle != null)
            {
                this.messageNoHandle += messageNoHandle;
            }
        }

        public void RegistMessageError(Action<Key, Exception> messageExceptionHandle)
        {
            if (messageExceptionHandle != null)
            {
                messageException += messageExceptionHandle;
            }
        }

        public void RemoveMessageNoHandle(Action<Key, string> messageNoHandle)
        {
            if (messageNoHandle != null)
            {
                this.messageNoHandle -= messageNoHandle;
            }
        }

        public void RemoveMessageError(Action<Key, Exception> messageExceptionHandle)
        {
            if (messageExceptionHandle != null)
            {
                messageException -= messageExceptionHandle;
            }
        }
    }
}