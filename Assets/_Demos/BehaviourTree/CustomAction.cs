using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.BehaviourTree;

public class CustomAction : ActionNode
{
    protected override void OnStart()
    {
        base.OnStart();
        Debug.Log("OnStart");
    }
    protected override byte OnUpdate()
    {
        Debug.Log("OnUpdate");
        return Status.Running;
    }

    protected override void OnEnd()
    {
        base.OnEnd();
        Debug.Log("OnEnd");
    }
}
