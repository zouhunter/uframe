/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 事件管理器                                                                      *
*//************************************************************************************/
using System;

namespace UFrame.EventDispatch
{
    public class SaftyEventController<Key> : EventDispatcher<Key>
    {
        public int RegistEvent(Key eventKey, Action action,IAlive alive)
        {
            var id = RegistEvent(eventKey, action);
            if (m_observerMap.TryGetValue(eventKey, out var observer))
            {
                observer.SetChecker(id, () => alive.Alive);
            }
            return id;
        }

        public int RegistEvent<T1>(Key eventKey, Action<T1> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2>( Key eventKey, Action<T1, T2> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, T3>( Key eventKey, Action<T1, T2, T3> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, T3, T4>( Key eventKey, Action<T1, T2, T3, T4> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, T3, T4, T5>( Key eventKey, Action<T1, T2, T3, T4, T5> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<R>( Key eventKey, Func<R> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, R>( Key eventKey, Func<T1, R> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, R>( Key eventKey, Func<T1, T2, R> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, T3, R>( Key eventKey, Func<T1, T2, T3, R> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, T3, T4, R>( Key eventKey, Func<T1, T2, T3, T4, R> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        public int RegistEvent<T1, T2, T3, T4, T5, R>(Key eventKey, Func<T1, T2, T3, T4, T5, R> action, IAlive alive)
        {
            return RegistEventInternal(alive, eventKey, action);
        }
        protected int RegistEventInternal(IAlive alive,Key eventKey,MulticastDelegate callback)
        {
            var eventId = FindEvent(eventKey, callback);
            if (eventId <= 0)
            {
                eventId = RegistEventInternal(eventKey, callback);
            }
            if(m_observerMap.TryGetValue(eventKey, out var observer))
            {
                observer.SetChecker(eventId,()=>alive.Alive);
            }
            return eventId;
        }
    }
}