/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表加载异常                                                                      *
*//************************************************************************************/

using System;

namespace UFrame.TableCfg
{
    public class TableException : Exception
    {
        public TableException(string message) : base(message) { }
    }
}
