using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>等待指定帧数</summary>
    public class WaitFrameNode : FlowNode
    {
        public int FrameCount;
        public WaitFrameNode(int count) { FrameCount = count; }
        protected override IEnumerator Run() { for (int i = 0; i < FrameCount; i++) yield return null; }
    }
}