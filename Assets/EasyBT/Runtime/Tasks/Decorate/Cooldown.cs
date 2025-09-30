using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.EasyBT.Tasks
{
    [AddComponentMenu("BehaviourTree/Decorate/Cooldown")]
    public class Cooldown : Decorate
    {
        [SerializeField, Tooltip("time cool run child!")]
        private float _coolTime = 1;
        [SerializeField]
        private bool _firstTimeCool = false;

        private float _coolTimer = 0;

        protected override NodeStatus OnUpdate()
        {
            if(_coolTimer < _coolTime && _firstTimeCool)
            {
                _coolTimer = Time.time + _coolTime;
                return NodeStatus.Running;
            }
            if(Time.time < _coolTimer)
            {
                return NodeStatus.Running;
            }
            _coolTimer = Time.time + _coolTime;
            return base.OnUpdate();
        }
    }
}
