/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 时间随机冷却条件
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Condition
{
    [NodePath("冷却随机时间条件")]
    public class RandomTimeCoolDownCondition : ConditionNode
    {
        [PrimaryArg(1)]
        public float coolTimeMin;
        [PrimaryArg(3, 5)]
        public float coolTimeMax;
        private float _timer;
        private float _coolTime;

        protected override void OnReset()
        {
            base.OnReset();
            ResetTime();
        }

        protected override bool CheckCondition()
        {
            if (Time.time - _timer > _coolTime)
            {
                ResetTime();
                return true;
            }
            return false;
        }

        private void ResetTime()
        {
            _coolTime = Random.Range(coolTimeMin, coolTimeMax);
            _timer = Time.time + _coolTime;
        }

    }
}
