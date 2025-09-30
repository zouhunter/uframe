using System.Collections;
using UnityEngine;

namespace UFrame.YieldFlow
{
    /// <summary>随机分支节点</summary>
    public class RandomBranchNode : FlowNode
    {
        public FlowNode[] Branches;
        public RandomBranchNode(params FlowNode[] branches) { Branches = branches; }
        protected override IEnumerator Run() { if (Branches.Length > 0) yield return Branches[UnityEngine.Random.Range(0, Branches.Length)].RunWrapper(); }
    }
}