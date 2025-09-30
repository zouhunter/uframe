/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: 画射线
 *_*/
using UnityEngine;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("Debug/画射线")]
    public class DrawRay : ActionNode
    {
        public Ref<Vector3> start;
        public Ref<Vector3> direction;
        public Ref<Color> color;

        protected override byte OnUpdate()
        {
            Debug.DrawRay(start.Value, direction.Value, color.Value);
            return Status.Success;
        }
    }
}

