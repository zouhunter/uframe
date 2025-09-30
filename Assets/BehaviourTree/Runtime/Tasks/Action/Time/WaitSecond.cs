/*-*-*-*
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Description: wait for seconds
 *-*-*/
using UnityEngine;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("Time/等待{waitSecond}秒")]
    public class WaitSecond : ActionNode
    {
        public Ref<float> waitSecond = new Ref<float> { Value = 1f };
        private float _triggerTime;

        protected void ResetTime()
        {
            _triggerTime = Time.time + waitSecond.Value;
        }

        protected override void OnStart()
        {
            ResetTime();
        }

        protected override byte OnUpdate()
        {
            if (_triggerTime < Time.time)
            {
                ResetTime();
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
