/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 不论对错，都返回失败
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Decorates
{
    [NodePath("纠错")]
    public class LikeFailure : DecoratorNode
    {
        protected override byte OnUpdate(TreeInfo info)
        {
            var status = base.ExecuteChild(info);
            if (status == Status.Success)
            {
                return Status.Failure;
            }
            return status;
        }
    }
}
