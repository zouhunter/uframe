/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 数据异常                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.NodeGraph
{
    public class DataModelException : Exception
    {
        public DataModelException(string message) : base(message)
        {
        }
    }
}