/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 任务控制器                                                                      *
*//************************************************************************************/

using System.Collections.Generic;

namespace UFrame.Tasks
{
    public class TaskCtrl : ITaskCtrl
    {
        private readonly LinkedList<TaskContainer> m_Tasks;
        private int m_Serial;
        private Queue<TaskContainer> taskPool;

        public int Count
        {
            get
            {
                return m_Tasks.Count;
            }
        }

        public TaskCtrl()
        {
            m_Tasks = new LinkedList<TaskContainer>();
            taskPool = new Queue<TaskContainer>();
            m_Serial = 0;
        }

        public void Update()
        {
            LinkedListNode<TaskContainer> current = m_Tasks.First;
            while (current != null)
            {
                TaskContainer task = current.Value;
                if (task.Status == TaskStatus.Free)
                {
                    throw new TaskException("Task status is invalid.");
                }

                if (task.Status == TaskStatus.Waiting)
                {
                    task.Start();
                }

                if (task.Status == TaskStatus.Running)
                {
                    task.Update();
                    current = current.Next;
                }
                else
                {
                    LinkedListNode<TaskContainer> next = current.Next;
                    m_Tasks.Remove(current);
                    Release(task);
                    current = next;
                }
            }
        }
        public void Shutdown()
        {
            CancelAllTasks(null);

            foreach (TaskContainer task in m_Tasks)
            {
                Release(task);
            }

            m_Tasks.Clear();
        }
        public int AddTask<T>(T task, object context = null, int priority = 0) where T : class, ITask
        {
            var taskContainer = Acquire();
            taskContainer.SerialId = m_Serial++;
            taskContainer.Priority = priority;
            taskContainer.Context = context;
            taskContainer.Initialize(task);

            LinkedListNode<TaskContainer> current = m_Tasks.First;
            while (current != null)
            {
                if (taskContainer.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                m_Tasks.AddBefore(current, taskContainer);
            }
            else
            {
                m_Tasks.AddLast(taskContainer);
            }
            return taskContainer.SerialId;
        }
        public bool CancelTask(int serialId, string reason = null)
        {
            foreach (TaskContainer task in m_Tasks)
            {
                if (task.SerialId != serialId)
                {
                    continue;
                }

                if (task.Status != TaskStatus.Waiting && task.Status != TaskStatus.Running)
                {
                    return false;
                }

                task.Cancel(reason);
                return true;
            }

            return false;
        }

        public void CancelContextTasks(object context, string reason = null)
        {
            foreach (TaskContainer task in m_Tasks)
            {
                if (task.Context == null || task.Context != context)
                {
                    continue;
                }

                if (task.Status != TaskStatus.Waiting && task.Status != TaskStatus.Running)
                {
                    continue;
                }

                task.Cancel(reason);
            }

        }
        public void CancelAllTasks(string reason)
        {
            foreach (TaskContainer task in m_Tasks)
            {
                if (task.Status != TaskStatus.Waiting && task.Status != TaskStatus.Running)
                {
                    continue;
                }

                task.Cancel(reason);
            }
        }
        private TaskContainer Acquire()
        {
            TaskContainer task = null;
            if(taskPool.Count > 0)
            {
                task = taskPool.Dequeue();
            }
           
            if(task == null)
            {
                task = new TaskContainer();
            }

            return task;
        }
        private void Release(TaskContainer task) 
        {
            task.Release();

            if (!taskPool.Contains(task))
            {
                taskPool.Enqueue(task);
            }
        }
    }
}
