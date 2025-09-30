/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 任务控制器                                                                      *
*//************************************************************************************/

using UFrame;
using System.Collections.Generic;

namespace UFrame.Tasks
{
    public class SimpleTaskManager : TaskCtrl, ITaskController,  IUpdate
    {
        private Dictionary<int, int> taskIDCatch;
        public float Interval => 0;
        public bool Started { get; protected set; }
        public bool Runing => Count > 0;

        public void OnRegist()
        {
            taskIDCatch = new Dictionary<int, int>();
        }

        public void OnUnRegist()
        {
            Shutdown();
            taskIDCatch.Clear();
            taskIDCatch = null;
        }

        public bool CancelTask(ITask task, string reason = null)
        {
            if (!Started) return false;
            var hashCode = task.GetHashCode();
            int id = -1;
            if (taskIDCatch.TryGetValue(hashCode, out id))
            {
                CancelTask(id, reason);
            }
            return false;
        }


        public void OnUpdate()
        {
            Update();
        }
    }
}