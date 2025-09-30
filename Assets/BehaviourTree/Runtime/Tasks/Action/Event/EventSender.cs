/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: event to send
 *_*/

using System.Collections.Generic;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("Event/Sender")]
    public class EventSender : ActionNode
    {
        public Ref<string> eventName;
        public Ref<object> data;

        protected override IEnumerable<IRef> GetRefVars()
        {
            return new IRef[] { eventName, data };
        }

        protected override byte OnUpdate()
        {
            if (string.IsNullOrEmpty(eventName.Value))
                return Status.Failure;

            Owner.SendEvent(eventName, data.Value);
            return Status.Success;
        }
    }
}
