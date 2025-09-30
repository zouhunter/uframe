using System;
using System.Collections.Generic;

namespace UFrame.NetSocket
{
    internal class ObjectPool<T>
    {
        private readonly Stack<T> m_pool;
        private Func<T> m_createFunc;

        public ObjectPool(Func<T> createFunc)
        {
            m_pool = new Stack<T>();
            m_createFunc = createFunc;
        }

        public void SetFunc(Func<T> createFunc)
        {
            m_createFunc = createFunc;
        }

        public T Pop()
        {
            lock (m_pool)
            {
                if (m_pool.Count > 0)
                    return m_pool.Pop();
                return m_createFunc();
            }
        }

        public void Push(T item)
        {
            if (item != null)
            {
                lock (m_pool)
                {
                    m_pool.Push(item);
                }
            }
        }
    }
}