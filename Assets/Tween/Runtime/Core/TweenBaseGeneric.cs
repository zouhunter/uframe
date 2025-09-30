using UnityEngine;
using System.Collections;
using System;

namespace UFrame.Tween
{
    public abstract class TweenBase<T> : TweenBase
    {
        public T from;
        public T to;

        protected T m_value;
        public virtual T value
        {
            get
            {
                return m_value;
            }
            set
            {
                if (m_value == null || !m_value.Equals(value))
                {
                    m_value = value;
                    onValueChanged?.Invoke(value);
                }
            }
        }
        public Action<T> onValueChanged { get; set; }
    }

    public abstract class TweenBaseDynimic<T> : TweenBase<T>
    {
        public Func<T> getValue;
        public Action<T> setValue { get; set; }

        public override T value
        {
            get
            {
                if(getValue == null)
                {
                    return m_value;
                }
                m_value = getValue.Invoke();
                return m_value;
            }
            set
            {
                if (m_value == null || !m_value.Equals(value))
                {
                    m_value = value;
                    onValueChanged?.Invoke(value);
                }
                setValue?.Invoke(value);
            }
        }

        public override void EnableTween()
        {
            base.EnableTween();
            if (getValue != null)
                from = getValue.Invoke();
        }
    }
}
