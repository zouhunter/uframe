/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接信息                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.NodeGraph.DataModel
{
    public struct CustomNodeInfo : IComparable
    {
        public CustomNodeAttribute node;
        public Type type;

        public CustomNodeInfo(Type t, CustomNodeAttribute n)
        {
            node = n;
            type = t;
        }

        public Node CreateInstance()
        {
            Node o = UnityEngine.ScriptableObject.CreateInstance(type) as Node;
            o.name = type.FullName;
            return o;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            CustomNodeInfo rhs = (CustomNodeInfo)obj;
            return node.OrderPriority - rhs.node.OrderPriority;
        }
    }
}