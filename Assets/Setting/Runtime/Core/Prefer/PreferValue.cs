/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 用户数据 模板                                                                   *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.Setting.Prefer
{
    public abstract class PreferValue<T>
    {
        protected bool updated = false;
        protected T _value;
        public T value
        {
            get
            {
                if (updated){
                    updated = false;
                    _value = GetPreferValue();
                    //Debug.LogFormat("get prefer:{0} value:{1}", key,_value);
                }
                return _value;
            }
            set
            {
                if(!Equals(_value,value)) {
                    _value = value;
                    SetPreferValue(value);
                    //Debug.LogFormat("set prefer:{0} value:{1}", key, _value);
                    if (onValueChanged != null){
                        onValueChanged.Invoke(_value);
                    }
                }
            }
        }
        public string key;
        public event UnityAction<T> onValueChanged;
        public PreferValue(string key)
        {
            this.key = key;
            this.updated = true;
        }
        public virtual bool Exists()
        {
            return PlayerPrefs.HasKey(key);
        }
        public virtual void Reset()
        {
            PlayerPrefs.DeleteKey(key);
        }
        protected abstract void SetPreferValue(T value);
        protected abstract T GetPreferValue();
        protected abstract bool Equals(T a, T b);
    }

}