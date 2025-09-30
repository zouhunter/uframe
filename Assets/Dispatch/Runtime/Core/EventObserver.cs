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
    internal class EventObserver
    {
        internal Dictionary<int, EventDelegate> m_eventMap = new Dictionary<int, EventDelegate>();
        internal HashSet<EventDelegate> m_eventSet = new HashSet<EventDelegate>();
        internal int Count => m_eventSet.Count;
        public HashSet<EventDelegate>.Enumerator GetEnumerator()
        {
            return m_eventSet.GetEnumerator();
        }
        internal EventDelegate Remove(int id)
        {
            if (m_eventMap.TryGetValue(id, out var eventAction))
            {
                m_eventSet.Remove(eventAction);
                m_eventMap.Remove(id);
                return eventAction;
            }
            return null;
        }
        internal int FindId(Action action)
        {
            foreach (var eventAction in m_eventSet)
            {
                if (eventAction.simple == action)
                {
                    return eventAction.id;
                }
            }
            return -1;
        }
        internal int FindId(MulticastDelegate action)
        {
            if (action == null)
                return -1;

            foreach (var eventAction in m_eventSet)
            {
                if (eventAction.complex != null && eventAction.complex.GetHashCode() == action.GetHashCode())
                {
                    return eventAction.id;
                }
            }
            return -1;
        }
        internal void Add(EventDelegate eventAction)
        {
            if (eventAction != null)
            {
                m_eventSet.Add(eventAction);
                m_eventMap[eventAction.id] = eventAction;
            }
        }

        internal void SetChecker(int id,Func<bool> checker)
        {
            if(m_eventMap.TryGetValue(id,out var eventAction))
            {
                eventAction.checker = checker;
            }
        }
    }

    internal class EventDelegate
    {
        internal int id;
        internal Action simple;
        internal MulticastDelegate complex;
        internal Func<bool> checker;

        internal void Clean()
        {
            simple = null;
            complex = null;
            checker = null;
            id = 0;
        }
    }
}