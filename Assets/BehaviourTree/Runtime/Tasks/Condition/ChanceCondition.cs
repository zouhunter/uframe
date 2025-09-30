/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 概率条件
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Condition
{
    [NodePath("概率条件<")]
    public class ChanceCondition : ConditionNode
    {
        public Ref<float> percent;
        public Ref<int> randomSeed;

        protected override void OnStart()
        {
            base.OnStart();
            if (randomSeed.Value != 0)
                Random.InitState(randomSeed.Value);
        }

        protected override bool CheckCondition()
        {
            if (Random.Range(0f, 1f) < percent.Value)
            {
                return true;
            }
            return false;
        }
    }
}
