using System;
using UnityEngine;

namespace UFrame.EasyBT
{
    public abstract class BaseNode : MonoBehaviour
    {
        private BaseTree _owner;
        public BaseTree Owner => _owner;
        public NodeStatus Status { get; private set; }
        private bool _started;
      
        protected virtual void OnEnable()
        {
            var parentTask = transform.parent.GetComponent<ParentNode>();
            if (parentTask != null && !parentTask.RegistNode(transform.GetSiblingIndex(), this))
                enabled = false;
        }

        protected virtual void OnDisable()
        {
            var parentTask = transform.parent.GetComponent<ParentNode>();
            if (parentTask != null)
                parentTask.RemoveNode(this);
        }

        public virtual void SetOwner(BaseTree owner)
        {
            if (_owner != owner && owner)
            {
                _owner = owner;
                OnReset();
            }
        }

        protected virtual void OnStart() 
        {
            Debug.Assert(Owner,"owner is empty！");
            Debug.Log("OnStart:" + name);
            _started = true;
        }

        protected virtual void OnReset()
        {
            Debug.Log("OnReset:" + name);
            _started = false;
        }

        protected abstract NodeStatus OnUpdate();

        internal virtual NodeStatus Execute()
        {
            if (!_started)
            {
                if (!enabled)
                    return NodeStatus.Inactive;
                OnStart();
            }
            Status = OnUpdate();
            return Status;
        }
    }
}
