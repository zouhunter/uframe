using System;
using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>执行Action的节点</summary>
    public class ActionNode : FlowNode
    {
        public Action Act;
        public ActionNode(Action act) { Act = act; }
        protected override IEnumerator Run() { Act?.Invoke(); yield break; }
    }
}