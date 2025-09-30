//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-07
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UFrame.BehaviourTree;
using System.Collections.Generic;

namespace UFrame.HTN
{
    public static class TaskInfoUtil
    {
        public static void CopyTo(this TaskInfo source, TaskInfo copy)
        {
            copy.node = source.node;
            copy.enable = source.enable;
            copy.condition = new ConditionInfo();
            copy.condition.enable = source.condition.enable;
            copy.condition.conditions = new List<ConditionItem>();
            copy.condition.matchType = source.condition.matchType;
            if (source.condition.conditions != null)
            {
                foreach (var item in source.condition.conditions)
                {
                    var conditionItem = new ConditionItem();
                    conditionItem.node = item.node;
                    conditionItem.subEnable = item.subEnable;
                    conditionItem.matchType = item.matchType;
                    conditionItem.state = item.state;
                    if (item.subConditions != null)
                        conditionItem.subConditions = new List<SubConditionItem>(item.subConditions);
                    copy.condition.conditions.Add(conditionItem);
                }
            }
            if (source.subTrees != null)
            {
                copy.subTrees = new List<TreeInfo>();
                foreach (var item in source.subTrees)
                {
                    var subTree = new TaskInfo();
                    CopyTo(item as TaskInfo, subTree);
                    copy.subTrees.Add(subTree);
                }
            }
            copy.checks = source.checks;
            copy.effects = source.effects;
        }

        public static bool VerifyCheck(this TaskInfo taskInfo, IWorldStates blackboard)
        {
            foreach (var info in taskInfo.checks)
            {
                if (info.state >= 2)//禁用
                    continue;
                var result = info.Verify((IVariableContent)blackboard);
                if (info.state == 1)
                    result = !result;
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }

        public static void ApplyEffects(this TaskInfo node, IWorldStates workingState)
        {
            foreach (EffectInfo effect in node.effects)
            {
                effect.Apply(workingState);
            }
        }
    }
}

