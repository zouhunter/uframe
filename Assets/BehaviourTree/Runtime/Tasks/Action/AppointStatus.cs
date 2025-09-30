/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: Returns a Status of appoint.
 *_*/
using UFrame.BehaviourTree;
using UFrame;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("AppointStatus")]
    public class AppointStatus : ActionNode
    {
        public Ref<StatusE> status;
        protected override byte OnUpdate()
        {
            return (byte)status.Value;
        }
    }
}
