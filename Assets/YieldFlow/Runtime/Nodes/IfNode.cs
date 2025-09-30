using System;
using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>条件判断节点</summary>
    public class IfNode : FlowNode
    {
        public Func<bool> Condition;
        public FlowNode TrueNode;
        public FlowNode FalseNode;
        public IfNode(Func<bool> cond, FlowNode t, FlowNode f = null) { Condition = cond; TrueNode = t; FalseNode = f; }
        protected override IEnumerator Run() { if (Condition()) { if (TrueNode != null) yield return TrueNode.RunWrapper(); } else if (FalseNode != null) yield return FalseNode.RunWrapper(); }
    }
}