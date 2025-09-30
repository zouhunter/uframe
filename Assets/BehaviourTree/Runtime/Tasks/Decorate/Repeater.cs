using UnityEngine;

namespace UFrame.BehaviourTree.Decorates
{
    [AddComponentMenu("BehaviourTree/Decorate/Repeater")]
    public class Repeater : DecoratorNode
    {
        [SerializeField, Tooltip("loop max count, -1 means ignore!")]
        private int _loopCount = 1;

        [SerializeField, Tooltip("should end if child on Failure!")]
        private MatchStatus _abortType;

        protected override byte OnUpdate(TreeInfo info)
        {
            var status = base.ExecuteChild(info);
            if (status == Status.Failure && _abortType == MatchStatus.Failure)
            {
                return Status.Success;
            }
            else if (status == Status.Success && _abortType == MatchStatus.Success)
            {
                return Status.Success;
            }
            if (_loopCount > 0 && --_loopCount == 0)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
