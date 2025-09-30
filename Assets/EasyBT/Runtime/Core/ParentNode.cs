using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.EasyBT
{
    public abstract class ParentNode : BaseNode
    {
        protected List<BaseNode> _children = new List<BaseNode>();
        protected virtual int maxChildCount { get => int.MaxValue; }

        public int ChildCount => _children.Count;

        /// <summary>
        /// 查找子节点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BaseNode GetChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                return null;
            return _children[index];
        }

        /// <summary>
        /// 设置owner
        /// </summary>
        /// <param name="owner"></param>
        public override void SetOwner(BaseTree owner)
        {
            base.SetOwner(owner);
            foreach (var child in _children)
                child.SetOwner(owner);
        }

        /// <summary>
        /// 注册子节点
        /// </summary>
        /// <param name="order"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool RegistNode(int order, BaseNode node)
        {
            if (Owner)
                node.SetOwner(Owner);

            if (maxChildCount <= _children.Count)
            {
                Debug.LogWarning("BHTParentTask:RegistNode: maxChildCount <= _children.Count");
                return false;
            }
            var registed = false;
            for (int i = 0; i < _children.Count; i++)
            {
                var index = _children[i].transform.GetSiblingIndex();
                if (index == order)
                {
                    _children[i] = MergeTask(_children[i],node);
                    registed = true;
                    break;
                }
                else if (index > order)
                {
                    _children.Insert(i, node);
                    registed = true;
                    break;
                }
            }
            if (!registed && !_children.Contains(node))
                _children.Add(node);
            return true;
        }

        /// <summary>
        /// 合并task
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private ConditionGroupNode MergeTask(BaseNode origin,BaseNode next)
        {
            Debug.Log("mergeTask:" + origin + " + " + next);
            if(origin is ConditionGroupNode groupTask)
            {
                if(next != groupTask)
                    groupTask.AddNode(next);
                return groupTask;
            }
            else
            {
                var group = origin.GetComponent<ConditionGroupNode>();
                if(group == null)
                    group = origin.gameObject.AddComponent<ConditionGroupNode>();
                group.AddNode(origin);
                if(next != group)
                    group.AddNode(next);
                return group;
            }
        }

        /// <summary>
        /// 移除子节点
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(BaseNode node)
        {
            var removed = _children.Remove(node);
            if(!removed)
            {
                foreach (var child in _children)
                {
                    if (child is ConditionGroupNode groupTask)
                    {
                        removed = groupTask.RemoveNode(node);
                        if (removed)
                            break;
                    }
                }
            }
        }
    }
}
