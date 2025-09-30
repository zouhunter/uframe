/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 任务控制器接口                                                                  *
*//************************************************************************************/


namespace UFrame.Tasks
{
    public interface ITaskCtrl
    {
        int Count { get; }
        int AddTask<T>(T task,object context = null, int priority = 0) where T :class, ITask;
        bool CancelTask(int serialId, string reason = null);
        void CancelContextTasks(object context, string reason = null);
        void CancelAllTasks(string reason = null);
        void Update();
        void Shutdown();
    }
}
