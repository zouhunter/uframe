using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace UFrame
{
    [Serializable]
    public abstract class Variable
    {
        public event Action<string> onValueChanged;
        public string _name;
        public ref string Name => ref _name;
        public abstract object GetValue();
        public abstract bool SetValue(object value, Type type = default);
        protected bool NeedCheckChange()
        {
            return onValueChanged != null;
        }
        protected void OnValueChanged()
        {
            onValueChanged?.Invoke(Name);
        }
    }

    [System.Serializable]
    public class Variable<T> : Variable
    {
        [UnityEngine.SerializeField]
        private T _value;
        public T Value
        {
            get
            {
                return GetValueData();
            }
            set
            {
                SetValueData(value);
            }
        }
        protected virtual T GetValueData()
        {
            return _value;
        }
        protected virtual bool SetValueData(T value)
        {
            if (!NeedCheckChange())
            {
                _value = value;
                return true;
            }
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnValueChanged();
                return true;
            }
            return false;
        }
        public override object GetValue()
        {
            return Value;
        }
        public override bool SetValue(object value, Type type = default)
        {
            if (value is T t)
                return SetValueData(t);
            else if (value == null)
                return SetValueData(default);
            else if (type != null)
            {
                return SetValueData((T)Convert.ChangeType(value, type));
            }
            else
            {
                UnityEngine.Debug.LogError($"类型转换失败: {value.GetType()} 转换为 {typeof(T)}");
                return false;
            }
        }

        public static explicit operator T(Variable<T> variable) { return variable == null ? variable.Value : default(T); }
    }
}
