/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 用户节点标记                                                                    *
*//************************************************************************************/

using System;

namespace UFrame.NodeGraph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomNodeAttribute : Attribute
    {
        public const int kDEFAULT_PRIORITY = 1000;
        public const string kDEFAULT_GROUP = "Empty";

        private string m_name;
        private int m_orderPriority;
        private string m_group;
        public string Name => m_name;
        public string Group => m_group;
        public int OrderPriority => m_orderPriority;

        public CustomNodeAttribute(string name, int orderPriority = kDEFAULT_PRIORITY, string group = kDEFAULT_GROUP)
        {
            m_name = name;
            m_orderPriority = orderPriority;
            m_group = group;
        }
    }
}
