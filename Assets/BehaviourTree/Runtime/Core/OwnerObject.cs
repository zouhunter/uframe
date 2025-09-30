using UFrame.BehaviourTree.Actions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    [System.Serializable]
    public class OwnerBase : ScriptableObject, IOwner
    {
        public bool LogInfo { get; set; }
        public int TickCount { get; protected set; }

        #region Variables
        protected VariableCenter _variableCenter = new VariableCenter();

        public VariableCenter Variables => _variableCenter;
        public virtual void SetVariables(VariableCenter center)
        {
            _variableCenter = center;
        }
        /// <summary>
        /// 清理上下文
        /// </summary>
        /// <param name="includePersistent"></param>
        public void ClearCondition(bool includePersistent = true)
        {
            _variableCenter.ClearCondition(includePersistent);
            _eventCenter.Clear(includePersistent);
        }

        public void BindingExtraVariable(Func<string, Variable> variableGetter)
        {
            _variableCenter.BindingExtraVariable(variableGetter);
        }

        public Variable GetVariable(string name)
        {
            return _variableCenter.GetVariable(name);
        }

        public Variable<T> GetVariable<T>(string name)
        {
            return _variableCenter.GetVariable<T>(name);
        }

        public Variable<T> GetVariable<T>(string name, bool createIfNotExits)
        {
            return _variableCenter.GetVariable<T>(name, createIfNotExits);
        }

        public T GetValue<T>(string name)
        {
            return _variableCenter.GetValue<T>(name);
        }

        public bool TryGetVariable<T>(string name, out Variable<T> variable)
        {
            return _variableCenter.TryGetVariable(name, out variable);
        }

        public bool TryGetVariable(string name, out Variable variable)
        {
            return _variableCenter.TryGetVariable(name, out variable);
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            return _variableCenter.TryGetValue<T>(name, out value);
        }

        public void SetVariable(string name, Variable variable)
        {
            _variableCenter.SetVariable(name, variable);
        }
        public bool TrySetValue(string name, object data)
        {
            return _variableCenter.TrySetValue(name, data);
        }
        public bool SetValue<T>(string name, T data)
        {
            return _variableCenter.SetValue(name, data);
        }
        /// <summary>
        /// 持久变量
        /// </summary>
        /// <param name="variableName"></param>
        public void SetPersistentVariable(string variableName)
        {
            _variableCenter.SetPersistentVariable(variableName);
        }

        public void CopyTo(IVariableContent target, List<string> keys = null)
        {
            _variableCenter.CopyTo(target, keys);
        }
        #endregion Variables

        #region Events
        private EventContent _eventCenter = new EventContent();
        public void BindingEventMap(Dictionary<string, List<Action<object>>> map)
        {
            _eventCenter._events = map;
        }
        public void RegistEvent(string eventKey, Action<object> callback)
        {
            _eventCenter.RegistEvent(eventKey, callback);
        }
        public void RemoveEvent(string eventKey, Action<object> callback)
        {
            _eventCenter.RemoveEvent(eventKey, callback);
        }
        public void SendEvent(string eventKey, object arg = null)
        {
            _eventCenter.SendEvent(eventKey, arg);
        }
        /// <summary>
        /// 持久事件
        /// </summary>
        /// <param name="eventName"></param>
        public void SetPersistentEvent(string eventName)
        {
            _eventCenter.SetPersistentEvent(eventName);
        }
        #endregion

    }
}
