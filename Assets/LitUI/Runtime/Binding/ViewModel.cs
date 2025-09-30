/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2024-12-18                                                                   *
*  版本: master                                                                       *
*  功能:                                                                              *
*   - binding                                                                         *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace UFrame.LitUI
{
    public abstract class ViewModel : IViewModel
    {
        private List<BindingView> _views = new List<BindingView>();
        public List<BindingView> Views { get { return _views; } }
        public BindingView View { get; private set; }
        protected object lastOpenArg { get; private set; }
        private readonly Dictionary<string, IBindableProperty> innerDic = new Dictionary<string, IBindableProperty>();
        protected IBindableProperty this[string key]
        {
            get
            {
                if (innerDic.ContainsKey(key))
                {
                    return innerDic[key];
                }
                return null;
            }
            set
            {
                innerDic[key] = value;
            }
        }
        public bool ContainsKey(string key)
        {
            return innerDic.ContainsKey(key);
        }
        public void SetActiveView(BindingView view)
        {
            View = view;
        }
        public virtual void SetBindableProperty(string keyword, IBindableProperty value)
        {
            if (this[keyword] == null)
            {
                this[keyword] = value;
            }
            else if (this[keyword] == value)
            {
                this[keyword].ValueBoxed = value.ValueBoxed;
            }
            else
            {
                this[keyword] = value;
            }
        }
        public virtual BindableProperty<T> GetBindableProperty<T>(string keyword)
        {
            if (!ContainsKey(keyword) || this[keyword] == null)
            {
                var prop = new BindableProperty<T>();
                this[keyword] = prop;
                return prop;
            }
            else if (this[keyword] is BindableProperty<T>)
            {
                return this[keyword] as BindableProperty<T>;
            }
            else 
            {
                throw new Exception("类型不一致,请检查！" + this[keyword].GetType());
            }
        }
        public virtual IBindableProperty GetBindableProperty(string keyword, System.Type type)
        {
            var fullType = typeof(BindableProperty<>).MakeGenericType(type);

            if (!ContainsKey(keyword) || this[keyword] == null)
            {
                this[keyword] = System.Activator.CreateInstance(fullType) as IBindableProperty;
            }
            if (this[keyword].GetType() == fullType)
            {
                return this[keyword] as IBindableProperty;
            }
            else
            {
                throw new Exception("类型不一致,请检查！" + this[keyword].GetType());
            }
        }
        protected virtual T GetValue<T>(string keyword)
        {
            return GetBindableProperty<T>(keyword).Value;
        }
        protected virtual void SetValue<T>(string keyword, T value)
        {
            GetBindableProperty<T>(keyword).Value = value;
        }

        public virtual void OnAfterBinding(BindingView panel) {
            this._views.Add(panel);
            View = panel;
        }

        public virtual void OnBeforeUnBinding(BindingView panel) {
            this._views.Remove(panel);
            if(_views.Count > 0)
                View = _views[_views.Count - 1];
        }
        public bool HaveDefultProperty(string keyword)
        {
            return innerDic.ContainsKey(keyword);
        }
        public virtual void OnOpen(object arg) { lastOpenArg = arg; }
        public virtual void OnClose() { }
        protected virtual void Close()
        {
            View?.Close();
        }
        public virtual void SetActive(bool active) { }
    }
}
