//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:18:46
//* 描    述： 异步ui加载信息

//* ************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.LitUI
{
    public class UIAsyncOperation : IEnumerator
    {
        public UIInfo info { get; private set; }
        public UIView view { get; private set; }
        public bool isDone { get; private set; }
        public bool isCancel { get; private set; }
        private Queue<UILoadEvent> completed = new Queue<UILoadEvent>();
        public object Current => view;
        public bool stackOnCreate { get; internal set; }
        public bool hideOnCreate { get; set; }
        public Queue<object> argQueue { get; set; }
        public Transform parent { get; set; }
        private event UICloseEvent closeEvents;

        public bool MoveNext()
        {
            return !isDone;
        }
        public void Reset()
        {
            isDone = false;
            completed.Clear();
        }
        public void Finish(UIView view)
        {
            this.view = view;
            if(view)
                view.onClose += OnUIClose;
            isDone = true;
            if (!isCancel)
            {
                while (completed.Count > 0)
                {
                    try
                    {
                        completed.Dequeue()?.Invoke(this);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                completed.Clear();
            }
            else
            {
                view.Close();
            }
        }

        public UIAsyncOperation(UIInfo info)
        {
            this.info = info;
            this.argQueue = new Queue<object>();
        }
        public void Cancel()
        {
            isCancel = true;
            isDone = true;
        }

        public UIAsyncOperation RegistComplete(UILoadEvent callback)
        {
            if (isDone)
            {
                callback?.Invoke(this);
            }
            else
            {
                completed.Enqueue(callback);
            }
            return this;
        }

        private void OnUIClose(UIView view)
        {
            closeEvents?.Invoke(view);
            closeEvents = null;
        }

        public UIAsyncOperation RegistClose(UICloseEvent callback)
        {
            if(callback != null)
            {
                closeEvents += callback;
            }
            return this;
        }
    }
}
