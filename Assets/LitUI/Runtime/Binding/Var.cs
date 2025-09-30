using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.LitUI
{
    public interface IVar
    {
        void Bind(ViewModel vm);
        void UnBind(ViewModel vm);
    }
    public interface IEvt: IVar
    {
     
    }
    public class Var<T> : IVar
    {
        private BindableProperty<T> prop;
        public string name;
        public object _value;
        public Var(string name)
        {
            this.name = name;
            prop = null;
            _value = default;
        }
        public T Value
        {
            get
            {
                if (prop == null)
                    return default;
                return prop.Value;
            }
            set
            {
                if (prop != null)
                    prop.Value = value;
                this._value = value;
            }
        }
        public void Set(T value)
        {
            Value = value;
        }

        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<T>(name);
            if (_value != null && _value is T value)
                prop.Value = value;
        }

        public void UnBind(ViewModel vm)
        {
            prop = null;
            _value = default;
        }
    }
    public class Evt : IEvt
    {
        private string name;
        private BindableProperty<Action> prop;
        private Action callback;
        public Evt(string name)
        {
            this.name = name;
            prop = null;
            callback = null;
        }
        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<Action>(name);
            prop.Value += OnCallBack;
        }
        public void UnBind(ViewModel vm)
        {
            if (prop != null)
                prop.Value -= OnCallBack;
            prop = null;
        }
        private void OnCallBack()
        {
            callback?.Invoke();
        }
        public void Set(Action callback)
        {
            this.callback = callback;
        }
    }
    public class Evt<T> : IEvt
    {
        private BindableProperty<Action<T>> prop;
        public string name;
        public Action<T> callback;

        public Evt(string name)
        {
            this.name = name;
            prop = null;
            callback = null;
        }
        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<Action<T>>(name);
            prop.Value += OnCallBack;
        }
        public void UnBind(ViewModel vm)
        {
            if (prop != null)
                prop.Value -= OnCallBack;
            prop = null;
        }


        private void OnCallBack(T value)
        {
            callback?.Invoke(value);
        }

        public void Set(Action<T> callback)
        {
            this.callback = callback;
        }
    }
    public class Evt<T, T2> : IEvt
    {
        private BindableProperty<Action<T, T2>> prop;
        public string name;
        public Action<T, T2> callback;

        public Evt(string name)
        {
            this.name = name;
            prop = null;
            callback = null;
        }
        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<Action<T, T2>>(name);
            prop.Value += OnCallBack;
        }
        public void UnBind(ViewModel vm)
        {
            if (prop != null)
                prop.Value -= OnCallBack;
            prop = null;
        }

        private void OnCallBack(T t1,T2 t2)
        {
            callback?.Invoke(t1, t2);
        }

        public void Set(Action<T, T2> callback)
        {
            this.callback = callback;
        }
    }
    public class Evt<T, T2, T3> : IEvt
    {
        private BindableProperty<Action<T, T2, T3>> prop;
        public string name;
        public Action<T, T2, T3> callback;

        public Evt(string name)
        {
            this.name = name;
            prop = null;
            callback = null;
        }
        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<Action<T, T2, T3>>(name);
            prop.Value += OnCallBack;
        }

        private void OnCallBack(T t1, T2 t2, T3 t3)
        {
            callback?.Invoke(t1,t2,t3);
        }

        public void UnBind(ViewModel vm)
        {
            if (prop != null)
                prop.Value -= OnCallBack;
            prop = null;
        }

        public void Set(Action<T, T2, T3> callback)
        {
            this.callback = callback;
        }
    }
    public class Evt<T, T2, T3, T4> : IEvt
    {
        private BindableProperty<Action<T, T2, T3, T4>> prop;
        public string name;
        public Action<T, T2, T3, T4> callback;

        public Evt(string name)
        {
            this.name = name;
            prop = null;
            callback = null;
        }
        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<Action<T, T2, T3, T4>>(name);
            prop.Value += OnCallBack;
        }

        public void UnBind(ViewModel vm)
        {
            if (prop != null)
                prop.Value -= OnCallBack;
            prop = null;
        }

        private void OnCallBack(T t1, T2 t2, T3 t3, T4 t4)
        {
            callback?.Invoke(t1, t2, t3, t4);
        }

        public void Set(Action<T, T2, T3, T4> callback)
        {
            this.callback = callback;
        }
    }
    public class Evt<T, T2, T3, T4, T5> : IEvt
    {
        private BindableProperty<Action<T, T2, T3, T4, T5>> prop;
        public string name;
        public Action<T, T2, T3, T4, T5> callback;

        public Evt(string name)
        {
            this.name = name;
            prop = null;
            callback = null;
        }
        public void Bind(ViewModel vm)
        {
            prop = vm.GetBindableProperty<Action<T, T2, T3, T4, T5>>(name);
            prop.Value += OnCallBack;
        }

        public void UnBind(ViewModel vm)
        {
            if (prop != null && callback != null)
                prop.Value -= OnCallBack;
            prop = null;
        }

        private void OnCallBack(T t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            callback?.Invoke(t1, t2, t3, t4, t5);
        }

        public void Set(Action<T, T2, T3, T4, T5> callback)
        {
            this.callback = callback;
        }
    }
}
