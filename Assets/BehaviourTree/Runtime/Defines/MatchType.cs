using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    public enum MatchType
    {
        AllSuccess = 0,
        AllFailure = 1,
        AnySuccess = 2,
        AnyFailure = 3
    }

    public enum MatchStatus
    {
        Success = Status.Success,
        Failure = Status.Failure
    }
}
