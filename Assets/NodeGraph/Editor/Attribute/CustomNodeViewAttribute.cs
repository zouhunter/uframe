/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 节点绘制器标记                                                                  *
*//************************************************************************************/

using System.Collections.Generic;

using System;
namespace UFrame.NodeGraph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeViewAttribute : Attribute
    {
        public List<Type> targetTypes;
        public CustomNodeViewAttribute(params Type[] types)
        {
            targetTypes = new List<Type>();
            this.targetTypes.AddRange(types);
        }
    }

}