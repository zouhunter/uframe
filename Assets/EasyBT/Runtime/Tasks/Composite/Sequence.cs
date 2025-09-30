using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.EasyBT.Tasks
{
    [AddComponentMenu("BehaviourTree/Composite/Sequence")]
    public class Sequence : Composite
    {
        private int _matchCount = 0;

        protected override NodeStatus OnUpdate()
        {
            if (_children.Count == 0)
                return NodeStatus.Inactive;

            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                var childStatus = child.Execute();

                switch (childStatus)
                {
                    case NodeStatus.Inactive:
                        continue;
                    case NodeStatus.Running:
                        return NodeStatus.Running;
                    case NodeStatus.Failure:
                        if (abortType == MatchType.AnyFailure || abortType == MatchType.AllSuccess)
                            return NodeStatus.Failure;
                        _matchCount++;
                        break;
                    case NodeStatus.Success:
                        if (abortType == MatchType.AnySuccess || abortType == MatchType.AllFailure)
                            return NodeStatus.Success;
                        _matchCount++;
                        break;
                    default:
                        break;
                }
            }
            if (_matchCount == _children.Count)
            {
                return abortType == MatchType.AllSuccess ? NodeStatus.Success : NodeStatus.Failure;
            }
            return NodeStatus.Running;
        }
    }
}
