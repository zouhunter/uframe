/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-28
 * Version: 1.0.0
 * Description: 引用类型变量
 *_*/

using System;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    public interface IRef
    {
        bool Exists { get; }
        void Binding(IVariableProvider provider);
    }

    [Serializable]
    public class Ref<T> : IRef
    {
        [SerializeField]
        private string _key;
        [SerializeField]
        private T _default;
        [SerializeField]
        private bool _autoCreate = true;

        private IVariableProvider _variablePrivider;
        private Variable<T> _variable = null;
        public string Key => _key;
        public bool Exists
        {
            get
            {
                if (string.IsNullOrEmpty(_key))
                    return false;
                else if (_variable != null)
                    return true;
                else if (_variablePrivider != null && !string.IsNullOrEmpty(_key))
                {
                    _variable = _variablePrivider.GetVariable<T>(_key, _autoCreate);
                    return _variable != null;
                }
                return false;
            }
        }

        public Ref() { }
        public Ref(string key)
        {
            this._key = key;
        }
        public Ref(string key, IVariableProvider provider)
        {
            this._key = key;
            Binding(provider);
        }

        public void Binding(IVariableProvider provider)
        {
            this._variablePrivider = provider;
            if (!string.IsNullOrEmpty(_key))
            {
                this._variable = _variablePrivider.GetVariable<T>(_key, false);
                if (_variable == null && _autoCreate)
                {
                    _variable = _variablePrivider.GetVariable<T>(_key, true);
                    _variable.SetValue(_default);
                }
            }
            else
            {
                _variable = null;
            }
        }

        public void SetValue(T value)
        {
            Value = value;
        }

        public T Value
        {
            get
            {

                if (Exists)
                {
                    return _variable.Value;
                }
                return _default;
            }
            set
            {
                if (Exists)
                {
                    _variable.Value = value;
                }
                else
                {
                    _default = value;
                }
            }
        }

        public static implicit operator T(Ref<T> r)
        {
            return r.Value;
        }
    }
}
