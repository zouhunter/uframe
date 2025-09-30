//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-06
//* 描    述：任务信息

//* ************************************************************************************
using UnityEngine;
using System;
using System.Collections.Generic;
using UFrame.BehaviourTree;
using UFrame.HTN;

namespace UFrame
{
    [Serializable]
    public class TaskInfo : TreeInfo
    {
        [SerializeReference]
        public List<CheckInfo> checks = new List<CheckInfo>();
        [SerializeReference]
        public List<EffectInfo> effects = new List<EffectInfo>();
        public static new TaskInfo Create(string id = default)
        {
            var treeInfo = new TaskInfo();
            treeInfo.id = id ?? System.Guid.NewGuid().ToString().Substring(0, 8);
            return treeInfo;
        }
    }
    /// <summary>
    /// 比较方式
    /// </summary>
    public enum CheckCompire
    {
        Equal, // == 
        Bigger, // > 
        Lower,// <
    }

    public enum EffectType
    {
        Set, //->
        Add, // + 
        Mult, // *
    }

    public enum EffectApply
    {
        All,
        Plan,
        Run
    }
}

