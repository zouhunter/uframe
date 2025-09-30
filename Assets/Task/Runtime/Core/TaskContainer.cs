/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 任务容器                                                                        *
*//************************************************************************************/

namespace UFrame.Tasks
{
    public class TaskContainer
    {
        public const int DefaultPriority = 0;

        private int m_SerialId;
        private int m_Priority;
        public int m_Delay;
        private object m_Context;
        private TaskStatus m_Status;
        private ITask task;

        public TaskContainer()
        {
            m_SerialId = 0;
            m_Priority = DefaultPriority;
            m_Status = TaskStatus.Free;
        }

        public int SerialId
        {
            get
            {
                return m_SerialId;
            }
            set
            {
                m_SerialId = value;
            }
        }

        public int Priority
        {
            get
            {
                return m_Priority;
            }
            set
            {
                m_Priority = value;
            }
        }

        public TaskStatus Status
        {
            get
            {
                return m_Status;
            }
        }

        public object Context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }

        public void Start()
        {
            m_Status = TaskStatus.Running;
            task.OnStart();
        }

        public void Release()
        {
            m_SerialId = 0;
            m_Priority = DefaultPriority;
            m_Status = TaskStatus.Free;
            task = null;
        }

        public void Initialize(ITask task)
        {
            m_Status = TaskStatus.Waiting;
            this.task = task;
        }

        public void Update()
        {
            m_Status = TaskStatus.Running;
            if (task.Completed)
            {
                m_Status = TaskStatus.Completed;
                task.OnComplete();
            }
            else
            {
                task.OnUpdate();
            }
        }
        public void Cancel(string reason)
        {
            m_Status = TaskStatus.Canceled;
            task.OnCancel(reason);
        }
    }
}
