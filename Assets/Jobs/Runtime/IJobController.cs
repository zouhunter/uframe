/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   -                                                                                 *
*//************************************************************************************/

namespace UFrame.Jobs
{
    public interface IJobController
    {
        void AppendJob(IJobContent job);
        void ScheduleJobs();
        void CompleteJobs();
    }
}