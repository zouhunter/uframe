/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 随机顺序执行
 *_*/

using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BehaviourTree.Composite
{
    public class RandomSelectorNode : SelectorNode
    {
        private List<TreeInfo> randomList;

        protected override void OnStart(TreeInfo info)
        {
            base.OnStart(info);
            ShuffleChilden(info);
        }

        protected override byte OnUpdate(TreeInfo info)
        {
            var subIndex = info.subIndex;
            var status = Selector(randomList, ref subIndex);
            info.subIndex = subIndex;
            return status;
        }

        private void ShuffleChilden(TreeInfo info)
        {
            if (randomList == null)
                randomList = new List<TreeInfo>();
            else
                randomList.Clear();
            if (info.subTrees == null || info.subTrees.Count == 0)
                return;
            randomList.AddRange(info.subTrees);
            for (int i = randomList.Count; i > 0; --i)
            {
                int j = Random.Range(0, i);
                TreeInfo index = randomList[j];
                randomList[j] = randomList[i - 1];
                randomList[i - 1] = index;
            }
        }
    }
}
