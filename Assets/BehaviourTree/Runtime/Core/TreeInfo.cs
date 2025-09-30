using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    public interface ITreeInfo
    {
        public byte status { get; set; }
        public int tickCount { get; set; }
    }

    [System.Serializable]
    public class TreeInfo : ITreeInfo
    {
        public string id;
        public bool enable;
        public string desc;
        [SerializeReference]
        public BaseNode node;
        public ConditionInfo condition = new ConditionInfo();
        [SerializeReference]
        public List<TreeInfo> subTrees;
        public TreeInfo()
        {

        }
        public static TreeInfo Create(string id = default)
        {
            var treeInfo = new TreeInfo();
            treeInfo.id = id ?? System.Guid.NewGuid().ToString().Substring(0, 8);
            return treeInfo;
        }
        public byte status { get; set; }
        public int tickCount { get; set; }
        public int subIndex { get; set; }
    }

    [Serializable]
    public class ConditionInfo
    {
        public bool enable;
        public MatchType matchType = MatchType.AllSuccess;
        public List<ConditionItem> conditions = new List<ConditionItem>();
    }

    [Serializable]
    public class ConditionItem : ITreeInfo
    {
        public byte status { get; set; }
        public int tickCount { get; set; }

        public MatchType matchType = MatchType.AllSuccess;
        [SerializeReference]
        public ConditionNode node;
        public int state;
        public bool subEnable;
        public List<SubConditionItem> subConditions = new List<SubConditionItem>();
    }

    [Serializable]
    public class SubConditionItem : ITreeInfo
    {
        public byte status { get; set; }
        public int tickCount { get; set; }
        public int state;
        [SerializeReference]
        public ConditionNode node;
    }
}
