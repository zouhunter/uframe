using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.EasyBT
{
    /// <summary>
    /// 条件节点 (只返回成功或失败)
    /// </summary>
    public abstract class Condition : BaseNode
    {
        //取反
        public CompareType compareType;

        protected abstract bool CheckCondition();

        protected override NodeStatus OnUpdate()
        {
            if (CheckCondition())
            {
                return compareType == CompareType.Equal ? NodeStatus.Success:NodeStatus.Failure;
            }
            else
            {
                return compareType == CompareType.NotEqual ? NodeStatus.Success : NodeStatus.Failure;
            }
        }
    }
}
