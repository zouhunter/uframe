/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 反转子节点的状态
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Decorates
{
    [NodePath("取反")]
    public class Inverter : DecoratorNode
    {
        protected override byte OnUpdate(TreeInfo info)
        {
            var status = base.ExecuteChild(info);
            if (status == Status.Failure)
            {
                return Status.Success;
            }
            else if (status == Status.Success)
            {
                return Status.Failure;
            }
            return status;
        }
    }
}
