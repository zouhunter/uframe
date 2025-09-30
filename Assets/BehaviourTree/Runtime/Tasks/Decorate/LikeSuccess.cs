/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 不论对错，都返回成功
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Decorates
{
    [NodePath("纠正")]
    public class LikeSuccess : DecoratorNode
    {
        protected override byte OnUpdate(TreeInfo info)
        {
            var status = base.ExecuteChild(info);
            if (status == Status.Failure)
            {
                return Status.Success;
            }
            return status;
        }
    }
}
