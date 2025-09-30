/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 条件任务                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.Tasks
{
    public sealed class ConditionalTask : ITask 
    {
        public Predicate<ConditionalTask> condition { get; set; }
        public System.Action<ConditionalTask> onStartAction { get; set; }
        public System.Action<ConditionalTask, string> onCancelAction { get; set; }
        public System.Action<ConditionalTask> onCompleteAction { get; set; }
        public System.Action<ConditionalTask> onUpdateAction { get; set; }

        public bool Completed
        {
            get
            {
                return condition.Invoke(this);
            }
        }

        public event Action<string> onFailure;

        public void OnStart()
        {
            if (condition == null)
            {
                OnFailure("Condition is invalid.");
                return;
            }

            if (onStartAction == null)
            {
                OnFailure("Start action is invalid.");
                return;
            }

            onStartAction.Invoke(this);
        }
        public void OnUpdate()
        {
            if(onUpdateAction != null)
            {
                onUpdateAction.Invoke(this);
            }
        }
        public void OnComplete()
        {
            if(onCompleteAction == null)
            {
                OnFailure("Complete action is invalid.");
                return;
            }
            onCompleteAction(this);
        }
        public void OnFailure(string reason)
        {
            if (onFailure != null)
            {
                onFailure(reason);
            }
        }
        public void OnCancel(string reason)
        {
            if (onCancelAction != null)
            {
                onCancelAction(this, reason);
            }
        }
    }
}
