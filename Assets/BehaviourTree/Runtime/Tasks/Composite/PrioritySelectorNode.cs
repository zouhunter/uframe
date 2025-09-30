/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 优先执行节点
 *_*/
using System.Collections.Generic;

namespace UFrame.BehaviourTree.Composite
{
    public class PrioritySelectorNode : SelectorNode
    {
        private List<TreeInfo> priorityList;

        protected override void OnStart(TreeInfo info)
        {
            base.OnStart(info);
            RefreshPriority(info);
        }

        protected override byte OnUpdate(TreeInfo info)
        {
            var subIndex = info.subIndex;
            var status = Selector(priorityList, ref subIndex);
            info.subIndex = subIndex;
            return status;
        }

        public void RefreshPriority(TreeInfo info)
        {
            if (priorityList == null)
                priorityList = new List<TreeInfo>();
            else
                priorityList.Clear();
            if (info.subTrees == null || info.subTrees.Count == 0)
                return;

            //插入排序
            for (int i = 0; i < info.subTrees.Count; i++)
            {
                var child = info.subTrees[i];
                for (int j = 0; j < priorityList.Count; j++)
                {
                    var current = priorityList[j];
                    if (child.node == null)
                        continue;

                    if (child.node.Priority > current.node.Priority)
                    {
                        priorityList.Insert(j, child);
                        child = null;
                        break;
                    }
                }
                if (child != null && child.node != null)
                {
                    priorityList.Add(child);
                }
            }
        }
    }
}
