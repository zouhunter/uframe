/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 复合节点
 *_*/

using System;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    public abstract class CompositeNode : ParentNode
    {
        [PrimaryArg(MatchStatus.Success, MatchStatus.Failure)]
        public MatchStatus matchStatus;
    }
}
