/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: 画线
 *_*/
using UnityEngine;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("Debug/画线")]
    public class DrawLine : ActionNode
    {
        [Tooltip("The start position")]
        public Ref<Vector3> start;
        [Tooltip("The end position")]
        public Ref<Vector3> end;
        [Tooltip("The color")]
        public Ref<Color> color;
        [Tooltip("Duration the line will be visible for in seconds.\nDefault: 0 means 1 frame.")]
        public Ref<float> duration;
        [Tooltip("Whether the line should show through world geometry.")]
        public Ref<bool> depthTest;

        protected override byte OnUpdate()
        {
            Debug.DrawLine(start.Value, end.Value, color.Value, duration.Value, depthTest.Value);
            return Status.Success;
        }
    }
}
