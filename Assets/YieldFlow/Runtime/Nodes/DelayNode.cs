using System.Collections;
using UnityEngine;

namespace UFrame.YieldFlow
{
    /// <summary>延迟指定秒数</summary>
    public class DelayNode : FlowNode
    {
        public float Seconds;
        public DelayNode(float seconds) { Seconds = seconds; }
        protected override IEnumerator Run() { yield return new WaitForSeconds(Seconds); }
    }
}