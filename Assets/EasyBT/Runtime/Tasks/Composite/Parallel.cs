using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace UFrame.EasyBT.Tasks
{
    [AddComponentMenu("BehaviourTree/Composite/Parallel")]
    public class Parallel : Composite
    {
        private int _matchCount = 0;

        protected override NodeStatus OnUpdate()
        {
            if(_children.Count == 0)
                return NodeStatus.Inactive;

            var resultStatus = NodeStatus.Running;
            _matchCount = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                var childStatus = child.Execute();

                if (childStatus == NodeStatus.Inactive)
                    continue;

                if(abortType == MatchType.AnySuccess && childStatus == NodeStatus.Success)
                {
                    resultStatus = NodeStatus.Success;
                    continue;
                }
                else if (abortType == MatchType.AnyFailure && childStatus == NodeStatus.Failure)
                {
                    resultStatus = NodeStatus.Failure;
                    continue;
                }
                else if (abortType == MatchType.AllSuccess && childStatus == NodeStatus.Success)
                {
                    _matchCount++;
                }
                else if (abortType == MatchType.AllFailure && childStatus == NodeStatus.Failure)
                {
                    _matchCount++;
                }
            }
            if(_matchCount == _children.Count)
            {
                return abortType == MatchType.AllSuccess ? NodeStatus.Success: NodeStatus.Failure;
            }
            return resultStatus;
        }
    }
}
