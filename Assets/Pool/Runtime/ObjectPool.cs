/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 对象池                                                                          *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame
{
    public class ObjectPool<T>
    {
        public delegate T CreateFunc();
        private CreateFunc m_createFunc;

        public ObjectPool()
        {

        }
        public ObjectPool(int poolSize, CreateFunc createFunc, Action<T> resetAction = null)
        {
            Init(poolSize, createFunc, resetAction);
        }
        public T GetObject()
        {
            lock (this)
            {
                if (m_objStack.Count > 0)
                {
                    T t = m_objStack.Pop();
                    return t;
                }
            }
            return m_createFunc.Invoke();
        }

        public void Init(int poolSize, CreateFunc createFunc, Action<T> resetAction = null)
        {
            m_createFunc = createFunc;
            m_objStack = new Stack<T>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                T item = m_createFunc.Invoke();
                m_objStack.Push(item);
            }
        }

        public void Store(T obj)
        {
            if (obj == null)
                return;
            lock (this)
            {
                m_objStack.Push(obj);
            }
        }

        // 少用，调用这个池的作用就没有了
        public void Clear()
        {
            if (m_objStack != null)
                m_objStack.Clear();
        }

        public int Count
        {
            get
            {
                if (m_objStack == null)
                    return 0;
                return m_objStack.Count;
            }
        }

        public Stack<T>.Enumerator GetIter()
        {
            if (m_objStack == null)
                return new Stack<T>.Enumerator();
            return m_objStack.GetEnumerator();
        }

        private Stack<T> m_objStack = null;
    }
}