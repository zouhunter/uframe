/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 节点异常                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.NodeGraph
{
    public class NodeException : Exception
    {
        public readonly string reason;
        public readonly string Id;

        public NodeException(string reason, string nodeId)
        {
            this.reason = reason;
            this.Id = nodeId;
        }
    }
}