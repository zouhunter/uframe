/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 事件接收条件
 *_*/

namespace UFrame.BehaviourTree.Condition
{
    [NodePath("事件接收条件")]
    public class ReceiveEventCondition : ConditionNode
    {
        public Ref<string> eventName;
        public Ref<object> eventArgReceive;

        private bool _registed;
        private bool _triggger;

        protected override void OnReset()
        {
            base.OnReset();
            if (!_registed && !string.IsNullOrEmpty(eventName.Value))
            {
                _registed = true;
                Owner.RegistEvent(eventName.Value, OnEventTrigger);
            }
        }

        protected override void OnClear()
        {
            base.OnClear();
            if (_registed)
            {
                _registed = false;
                Owner?.RemoveEvent(eventName.Value, OnEventTrigger);
            }
        }

        private void OnEventTrigger(object obj)
        {
            _triggger = true;
            eventArgReceive.Value = obj;
        }

        protected override bool CheckCondition()
        {
            return _triggger;
        }
    }
}
