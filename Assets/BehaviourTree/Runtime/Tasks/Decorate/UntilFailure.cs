/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 直到子节点返回失败
 *_*/

using UnityEngine;

namespace UFrame.BehaviourTree.Decorates
{
    [NodePath("直到失败=>{returnStatus}")]
    public class UntilFailure : DecoratorNode
    {
        [PrimaryArg(MatchStatus.Success, MatchStatus.Failure)]
        public MatchStatus returnStatus;
        protected override byte OnUpdate(TreeInfo info)
        {
            var childResult = base.ExecuteChild(info);
            if (childResult == Status.Failure)
            {
                return (byte)returnStatus;
            }
            return Status.Running;
        }
    }
}
