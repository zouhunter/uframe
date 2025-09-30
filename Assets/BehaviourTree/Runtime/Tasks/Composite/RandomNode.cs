/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 随机执行节点
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Composite
{
    public class RandomNode : CompositeNode
    {
        protected override void OnStart(TreeInfo info)
        {
            base.OnStart();
            if (info.subTrees != null && info.subTrees.Count > 0)
                info.subIndex = Random.Range(0, info.subTrees.Count);
        }

        protected override byte OnUpdate(TreeInfo info)
        {
            if (info.subTrees == null || info.subTrees.Count <= info.subIndex)
                return Status.Inactive;

            var child = info.subTrees[info.subIndex];
            if (!child.enable || !child.node)
                return Status.Inactive;

            return child.node.Execute(child);
        }
    }
}
