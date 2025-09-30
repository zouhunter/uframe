/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 任务异常                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.Tasks
{
    public class TaskException : Exception
    {
        public TaskException(string message) : base(message) { }
    }
}