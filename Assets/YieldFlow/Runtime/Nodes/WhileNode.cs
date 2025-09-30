using System;
using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>循环节点（条件为真时循环）</summary>
    public class WhileNode : FlowNode
    {
        public Func<bool> Condition;
        public FlowNode Body;
        public WhileNode(Func<bool> cond, FlowNode body) { Condition = cond; Body = body; }
        protected override IEnumerator Run() { while (Condition()) yield return Body.RunWrapper(); }
    }
}