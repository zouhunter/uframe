/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源加载 异常                                                                   *
*//************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UFrame.Source
{
    public class SourceException : System.Exception
    {
        public SourceException(string message) : base(message) { }
    }
}