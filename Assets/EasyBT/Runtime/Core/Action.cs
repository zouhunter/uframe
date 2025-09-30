using UnityEngine;
using UnityEngine.Events;

namespace UFrame.EasyBT
{
    [DisallowMultipleComponent]
    public abstract class Action : BaseNode
    {
        public UnityEvent<NodeStatus> onChange;

        private NodeStatus _lastStatus;
        internal override NodeStatus Execute()
        {
            var result = base.Execute();
            if (_lastStatus != result)
            {
                onChange?.Invoke(result);
                _lastStatus = result;
            }
            return result;
        }
    }
}
