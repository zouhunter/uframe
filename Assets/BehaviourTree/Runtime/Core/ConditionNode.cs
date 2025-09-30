using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    /// <summary>
    /// 条件节点 (只返回成功或失败)
    /// </summary>
    public abstract class ConditionNode : ActionNode
    {
        protected abstract bool CheckCondition();
        protected override byte OnUpdate()
        {
            return CheckCondition() ? Status.Success : Status.Failure;
        }
    }
}
