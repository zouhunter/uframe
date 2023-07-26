/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 生命周期接口                                                                    *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame
{
    public abstract class Agent: IAlive
    {
        public virtual int Priority => 0;
        public virtual bool Alive { get; protected set; }
        protected List<IDisposable> m_disposables;

        internal virtual void Initialize()
        {
            if (!Alive)
            {
                OnInitialize();
                Alive = true;
            }
        }
        internal virtual void Recover()
        {
            if (Alive)
            {
                OnBeforeRecover();
                OnRecover();
                OnAfterRecover();
                Alive = false;
            }
        }
        protected abstract void OnInitialize();
        protected abstract void OnRecover();
        public T New<T>() where T : IDisposable,new()
        {
            var t = new T();
            if (m_disposables == null)
                m_disposables = new List<IDisposable>(2);
            m_disposables.Add(t);
            return t;
        }
        public T New<T>(Func<T> createFunc) where T : IDisposable
        {
            var t = createFunc.Invoke();
            if (m_disposables == null)
                m_disposables = new List<IDisposable>(2);
            m_disposables.Add(t);
            return t;
        }
        protected abstract void OnAfterRecover();
        protected virtual void OnBeforeRecover()
        {
            if(m_disposables != null)
            {
                foreach (var recoverAble in m_disposables)
                {
                    recoverAble.Dispose();
                }
            }
            m_disposables = null;
        }
    }
}