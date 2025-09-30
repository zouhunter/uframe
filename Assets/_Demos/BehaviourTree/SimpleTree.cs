using System.Collections;
using System.Collections.Generic;
using UFrame.BehaviourTree;
using UnityEngine;

public class SimpleTree : TreeBehaviourRunner
{
    protected override void OnEnable()
    {
        base.OnEnable();
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }
}
