/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 任务控制器                                                                      *
*//************************************************************************************/

namespace UFrame.Tasks
{
    public interface ITaskController : ITaskCtrl
    {
        bool CancelTask(ITask task, string reason = null);
    }
}