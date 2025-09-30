using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.EasyBT.Tasks
{
    [AddComponentMenu("BehaviourTree/Decorate/Repeater")]
    public class Repeater : Decorate
    {
        [SerializeField,Tooltip("loop max count, -1 means ignore!")]
        private int _loopCount = 1;

        [SerializeField, Tooltip("should end if child on Failure!")]
        private bool _abortBySuccess;

        protected override NodeStatus OnUpdate()
        {
            var status = base.OnUpdate();
            if(status == NodeStatus.Failure && !_abortBySuccess)
            {
                return NodeStatus.Success;
            }
            else if (status == NodeStatus.Success && _abortBySuccess)
            {
                return NodeStatus.Success;
            }
            else if (_loopCount > 0 && --_loopCount == 0)
            {
                return NodeStatus.Success;
            }
            return NodeStatus.Running;
        }
    }
}
