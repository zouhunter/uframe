/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 从resource 加载表的模板类                                                       *
*//************************************************************************************/

using System;
using System.Collections;

namespace UFrame.TableCfg
{
    public class AsyncTableOperation : IEnumerator
    {
        public bool finished { get; private set; }
        public event System.Action onFinish;
        public object Current => null;
        public static AsyncTableOperation Sync;
        static AsyncTableOperation()
        {
            Sync = new AsyncTableOperation();
            Sync.SetFinish();
        }
        public bool MoveNext()
        {
            return !finished;
        }
        public void Reset()
        {
            finished = false;
            onFinish = null;
        }
        public void SetFinish()
        {
            finished = true;
            onFinish?.Invoke();
            onFinish = null;
        }

        public void RegistFinish(Action onFinish)
        {
            if(finished)
            {
                onFinish?.Invoke();

            }
            else
            {
                this.onFinish += onFinish;
            }
        }
    }
}
