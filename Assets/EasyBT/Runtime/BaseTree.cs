using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.EasyBT
{
    public abstract class BaseTree: MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private bool _startOnEnable = true;
        [SerializeField]
        private bool _restartOnComplete;
        [SerializeField]
        private BaseNode _rootTask;
        [SerializeField]
        private float _interval;

        private bool _treeRuning;
        private float _intervalTimer;
        #endregion

        #region Properties
        public bool Running=>_treeRuning;
        public ref float Interval => ref _interval;
        public BaseNode rootTask => _rootTask;
        #endregion Properties

        #region Variables
        private Dictionary<string,SharedVariable> _variables = new Dictionary<string, SharedVariable>();
        public SharedVariable GetVariable(string name)
        {
            _variables.TryGetValue(name, out var variable);
            return variable; 
        }
        public SharedVariable<T> GetVariable<T>(string name)
        {
            if( _variables.TryGetValue(name, out var variable) && variable is SharedVariable<T> genVariable)
            {
                return genVariable;
            }
            return null;
        }
        public bool TryGetVariable<T>(string name, out SharedVariable variable)
        {
            var genVariable = GetVariable<T>(name);
            if(genVariable != null)
            {
                variable = genVariable;
                return true;
            }
            variable = null;
            return false;
        }
        public bool TryGetVariable(string name, out SharedVariable variable)
        {
            return _variables.TryGetValue(name, out variable);
        }
        public void SetVariable(string name, SharedVariable variable)
        {
            _variables[name] = variable;
        }
        public bool SetVariableValue(string name,object data)
        {
            if (TryGetVariable(name,out var variable))
            {
                variable.SetValue(data);
                return true;
            }
            return false;
        }
        #endregion Variables

        #region Events
        private Dictionary<string, List<Action<object>>> _events = new Dictionary<string, List<Action<object>>>();
        public void RegistEvent(string eventKey, Action<object> callback)
        {
            if(!_events.TryGetValue(eventKey,out var actions))
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
        public void SendEvent(string eventKey,object arg = null)
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
        #endregion

        #region TreeTraversal
        protected virtual void OnEnable()
        {
            _rootTask?.SetOwner(this);

            if (_startOnEnable)
            {
                StartTree();
            }
        }

        public virtual void StartTree()
        {
            if (_rootTask)
            {
                _treeRuning = true;
            }
        }

        public virtual void StopTree()
        {
            if (_rootTask)
            {
                _treeRuning = false;
            }
        }

        protected virtual void TraversalTree()
        {
            if (_interval > 0)
            {
                _intervalTimer += Time.deltaTime;
                if (_intervalTimer < _interval)
                    return;
                _intervalTimer = 0;
            }
            var status = _rootTask.Execute();
            if(status == NodeStatus.Success || status == NodeStatus.Failure)
            {
                if (!_restartOnComplete)
                    _treeRuning = false;
            }
        }

        protected virtual void Update()
        {
            if(_treeRuning)
            {
                TraversalTree();
            }
        }
        #endregion
    }
}
