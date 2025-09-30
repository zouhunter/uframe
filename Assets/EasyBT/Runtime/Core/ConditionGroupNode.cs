using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.EasyBT
{
    [AddComponentMenu("BehaviourTree/Condition/ConditionGroupNode")]
    [UnityEngine.DisallowMultipleComponent]
    public class ConditionGroupNode : BaseNode
    {
        protected BaseNode _mainTask;
        protected List<Condition> _conditions;

        [UnityEngine.SerializeField]
        protected MatchType matchType = MatchType.AllSuccess;

        public void AddNode(BaseNode task)
        {
            if(task is Condition condition)
            {
                if (_conditions == null)
                    _conditions = new List<Condition>();
                _conditions.Add(condition);
            }
            else
            {
                _mainTask = task;
            }
            if (Owner)
                task.SetOwner(Owner);
        }

        public bool RemoveNode(BaseNode task)
        {
            if (task is Condition condition)
            {
               return _conditions?.Remove(condition) ?? false;
            }
            else if((object)_mainTask == task)
            {
                _mainTask = null;
                return true;
            }
            return false;
        }

        public override void SetOwner(BaseTree owner)
        {
            base.SetOwner(owner);
            _mainTask?.SetOwner(owner);
            foreach (var condition in _conditions)
            {
                condition?.SetOwner(owner);
            }
        }

        protected override NodeStatus OnUpdate()
        {
            if (_conditions != null && _conditions.Count > 0)
            {
                int matchCount = 0;
                foreach (var condition in _conditions)
                {
                    var result = condition.Execute();
                    switch (matchType)
                    {
                        case MatchType.AllSuccess:
                            if (result == NodeStatus.Success)
                                matchCount++;
                            else if (result == NodeStatus.Failure)
                                return NodeStatus.Failure;
                            break;
                        case MatchType.AllFailure:
                            if (result == NodeStatus.Failure)
                                matchCount++;
                            else if (result == NodeStatus.Success)
                                return NodeStatus.Failure;
                            break;
                        default:
                            matchCount = -1;
                            break;
                    }
                }
                if (matchCount >= 0 && matchCount != _conditions.Count)
                    return NodeStatus.Failure;
            }
            if (_mainTask != null)
            {
                return _mainTask.Execute();
            }
            return NodeStatus.Success;
        }
    }
}
