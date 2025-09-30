using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;
namespace UFrame.ResContent
{
    public abstract class ResBase<T> where T : Object
    {
        protected Stack<T> m_instancePool = null;
        public virtual T Value { get; protected set; }
        public virtual T Clone()
        {
            if (Value)
            {
                UnityEngine.Object.Instantiate(Value);
            }
            return default(T);
        }
        public virtual void SetRes(T value)
        {
            Value = value;
        }
        public virtual void SetRes(Object obj)
        {
            if (obj is T)
            {
                Value = (T)obj;
            }
        }
        public virtual void Clean()
        {
            while (m_instancePool != null && m_instancePool.Count > 0)
            {
                var instance = m_instancePool.Pop();
                if (instance)
                {
                    Object.Destroy(instance);
                }
            }
            m_instancePool = null;

            if (Value)
            {
                Object.Destroy(Value);
            }
            Value = null;
        }
        public virtual void SaveBack(T instance)
        {
            if (instance)
                m_instancePool.Push(instance);
        }
    }
}