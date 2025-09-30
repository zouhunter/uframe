/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 顺序执行,全部成功/失败 => 成功
 *_*/

using System.Diagnostics;

namespace UFrame.BehaviourTree.Composite
{
    public class SequenceNode : CompositeNode
    {
        protected override byte OnUpdate(TreeInfo info)
        {
            if (info.subTrees == null || info.subTrees.Count == 0)
                return Status.Inactive;

            var status = Status.Inactive;
            for (; info.subIndex < info.subTrees.Count; info.subIndex++)
            {
                var child = info.subTrees[info.subIndex];
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
                        if (matchStatus == MatchStatus.Success)
                            return Status.Failure;
                        status = Status.Success;
                        break;
                    case Status.Success:
                        if (matchStatus == MatchStatus.Failure)
                            return Status.Failure;
                        status = Status.Success;
                        break;
                    case Status.Interrupt:
                        if (matchStatus == MatchStatus.Success)
                            return Status.Failure;
                        return Status.Success;
                    default:
                        break;
                }
            }
            return status;
        }
    }
}
