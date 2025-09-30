using System.Collections;
using UnityEngine;

namespace UFrame.YieldFlow
{
    /// <summary>输出日志</summary>
    public class LogNode : FlowNode
    {
        public string Message;
        public LogNode(string msg) { Message = msg; }
        protected override IEnumerator Run() { Debug.Log(Message); yield break; }
    }
}