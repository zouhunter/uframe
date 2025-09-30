using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    public class EventContent : IEventProvider
    {
        public HashSet<string> _persistentEvents = new HashSet<string>();
        public Dictionary<string, List<Action<object>>> _events = new Dictionary<string, List<Action<object>>>();

        public void BindingEventMap(Dictionary<string, List<Action<object>>> map)
        {
            this._events = map;
        }

        public void RegistEvent(string eventKey, Action<object> callback)
        {
            if (!_events.TryGetValue(eventKey, out var actions))
            {
                _events[eventKey] = new List<Action<object>>() { callback };
            }
            else
            {
                actions.Add(callback);
            }
        }

        public void RemoveEvent(string eventKey, Action<object> callback)
        {
            if (_events.TryGetValue(eventKey, out var actions))
            {
                actions.Remove(callback);
            }
        }

        public void SendEvent(string eventKey, object arg = null)
        {
            if (_events.TryGetValue(eventKey, out var actions))
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        actions[i]?.Invoke(arg);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
        public void SetPersistentEvent(string eventName)
        {
            _persistentEvents.Add(eventName);
        }

        public void Clear(bool includePersistent = true)
        {
            if (includePersistent)
            {
                _events.Clear();
            }
            else
            {
                var keys = new List<string>(_events.Keys);
                foreach (var key in keys)
                {
                    if (!_persistentEvents.Contains(key))
                    {
                        _events.Remove(key);
                    }
                }
            }
        }
    }
}