/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 冷却装饰器
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Decorates
{
    public class Cooldown : DecoratorNode
    {
        [SerializeField, Tooltip("time cool run child!")]
        private float _coolTime = 1;
        [SerializeField]
        private bool _firstTimeCool = false;

        private float _coolTimer = 0;

        protected override byte OnUpdate(TreeInfo info)
        {
            if (_coolTimer < _coolTime && _firstTimeCool)
            {
                _coolTimer = Time.time + _coolTime;
                return Status.Running;
            }
            if (Time.time < _coolTimer)
            {
                return Status.Running;
            }
            _coolTimer = Time.time + _coolTime;
            return base.ExecuteChild(info);
        }
    }
}
