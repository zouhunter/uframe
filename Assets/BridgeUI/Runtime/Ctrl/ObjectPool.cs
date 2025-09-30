/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 对象池                                                                          *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public class ObjectPool<T>
    {
        public UnityEngine.Events.UnityAction<T> onCreate { get; set; }
        public UnityEngine.Events.UnityAction<T> onRelease { get; set; }
        protected virtual Func<T> createFunc { get; set; }
        protected Stack<T> stack;

        public ObjectPool(Func<T> createFunc)
        {
            this.createFunc = createFunc;
            this.stack = new Stack<T>();
        }

        internal T Allocate()
        {
            if (stack.Count == 0)
            {
#if BRIDGEUI_LOG
                Debug.Log("create new " + typeof(T).FullName);
#endif
                var instence = createFunc();
                if (onCreate != null)
                {
                    onCreate.Invoke(instence);
                }
                return instence;
            }
            else
            {
                return stack.Pop();
            }
        }
        internal void Release(T instence)
        {
#if BRIDGEUI_LOG
            Debug.Log("release: " + instence);
#endif
            stack.Push(instence);
            if (onRelease != null)
                onRelease.Invoke(instence);
        }
    }
}