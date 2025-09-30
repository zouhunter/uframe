/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 任务接口                                                                        *
*//************************************************************************************/

namespace UFrame.Tasks
{
    public interface ITask
    {
        bool Completed { get; }
        void OnStart();
        void OnUpdate();
        void OnCancel(string reason);
        void OnComplete();
    }
}