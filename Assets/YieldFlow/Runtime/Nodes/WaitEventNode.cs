using System;
using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>等待事件触发</summary>
    public class WaitEventNode : FlowNode
    {
        public Action<Action> Register;
        public WaitEventNode(Action<Action> register) { Register = register; }
        protected override IEnumerator Run() { bool done = false; Register(() => done = true); while (!done) yield return null; }
    }
}