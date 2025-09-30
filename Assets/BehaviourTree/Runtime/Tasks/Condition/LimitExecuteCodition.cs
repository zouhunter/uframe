/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 次数限制执行条件
 *_*/

namespace UFrame.BehaviourTree.Condition
{
    /// <summary>
    /// 限制次数执行条件
    /// </summary>
    [NodePath("次数限制")]
    public class LimitExecuteCodition : ConditionNode
    {
        public int limitCount = 1;

        private int _counter;

        protected override void OnReset()
        {
            base.OnReset();
            _counter = 0;
        }

        protected override bool CheckCondition()
        {
            if (_counter++ < limitCount)
            {
                return true;
            }
            return false;
        }
    }
}

