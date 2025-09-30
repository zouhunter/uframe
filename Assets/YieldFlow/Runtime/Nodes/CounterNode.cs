using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>计数器节点</summary>
    public class CounterNode : FlowNode
    {
        public int Count;
        public FlowNode Body;
        public CounterNode(int count, FlowNode body) { Count = count; Body = body; }
        protected override IEnumerator Run() { for (int i = 0; i < Count; i++) yield return Body.RunWrapper(); }
    }
}