/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 顺序执行，选中返回成功
 *_*/

using System.Collections.Generic;

namespace UFrame.BehaviourTree.Composite
{
    public class SelectorNode : CompositeNode
    {
        protected override byte OnUpdate(TreeInfo info)
        {
            var subIndex = info.subIndex;
            var status = Selector(info.subTrees, ref subIndex);
            info.subIndex = subIndex;
            return status;
        }

        protected byte Selector(List<TreeInfo> subTrees, ref int subIndex)
        {
            if (subTrees == null || subTrees.Count == 0)
                return Status.Inactive;

            var status = Status.Inactive;
            for (; subIndex < subTrees.Count; subIndex++)
            {
                var child = subTrees[subIndex];
                if (!child.enable || child.node == null)
                    continue;

                var childStatus = child.node.Execute(child);
                switch (childStatus)
                {
                    case Status.Inactive:
                        break;
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        if (matchStatus == MatchStatus.Failure)
                            return Status.Success;
                        status = Status.Failure;
                        break;
                    case Status.Success:
                        if (matchStatus == MatchStatus.Success)
                            return Status.Success;
                        status = Status.Failure;
                        break;
                    case Status.Interrupt:
                        if (matchStatus == MatchStatus.Failure)
                            return Status.Success;
                        return Status.Failure;
                    default:
                        break;
                }
            }
            return status;
        }
    }
}
