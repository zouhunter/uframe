///*************************************************************************************//*
//*  作者: 邹杭特                                                                       *
//*  时间: 2021-05-02                                                                   *
//*  版本: master_aeee4                                                                 *
//*  功能:                                                                              *
//*   - 条件任务                                                                        *
//*//************************************************************************************/
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace UFrame.Task
//{
//    public abstract class ThreadTask : ITask
//    {
//        protected System.Threading.Tasks.Task m_task;
//        public bool Completed => m_task == null || m_task.IsCanceled || m_task.IsFaulted || m_task.IsCompleted;
//        public void OnStart()
//        {
//            m_task = new System.Threading.Tasks.Task(OnTaskThread);
//            m_task.Start();
//            m_task.ContinueWith(OnTaskFinished);
//        }
//        public void OnCancel(string reason)
//        {
//            if (m_task != null)
//            {
//                m_task.Dispose();
//            }
//        }

//        public abstract void OnComplete();
//        protected abstract void OnTaskFinished(System.Threading.Tasks.Task arg1);
//        public abstract void OnUpdate();
//        protected abstract void OnTaskThread();
//    }
//}
