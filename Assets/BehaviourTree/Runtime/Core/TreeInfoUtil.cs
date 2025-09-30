//*************************************************************************************
//* 作    者： 
//* 创建时间： 2025-05-06
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.BehaviourTree
{

    public class TreeInfoUtil
    {

        /// <summary>
        /// 深度绑定
        /// </summary>
        /// <param name="info"></param>
        /// <param name="owner"></param>
        public static void SetOwnerDeepth(TreeInfo info, IOwner owner)
        {
            info.node?.SetOwner(owner);
            info.status = Status.Inactive;
            info.tickCount = 0;
            info.subIndex = 0;
            if (info.condition.conditions != null/* && info.node == this*/)
            {
                foreach (var condition in info.condition.conditions)
                {
                    condition.status = Status.Inactive;
                    condition.tickCount = 0;
                    condition.node?.SetOwner(owner);
                    condition.subConditions?.ForEach(subNode =>
                    {
                        subNode.status = Status.Inactive;
                        subNode.tickCount = 0;
                        subNode?.node?.SetOwner(owner);
                    });
                }
            }
            if (info.subTrees != null && info.subTrees != null)
            {
                foreach (var subInfo in info.subTrees)
                {
                    SetOwnerDeepth(subInfo, owner);
                }
            }
        }

        public static void CleanDeepth(TreeInfo info)
        {
            if (info.subTrees != null && info.subTrees != null)
            {
                foreach (var subInfo in info.subTrees)
                {
                    if (subInfo.enable)
                    {
                        CleanDeepth(subInfo);
                    }
                }
            }
            if (info.condition.enable && info.condition.conditions != null /*&& info.node == this*/)
            {
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.subConditions != null)
                    {
                        for (int i = 0; i < condition.subConditions.Count; i++)
                        {
                            var condition2 = condition.subConditions[i];
                            condition2.node?.Clean();
                        }
                    }
                    condition.node?.Clean();
                }
            }
            info.node?.Clean();
        }
        /// <summary>
        /// 嵌套节点检查
        /// </summary>
        /// <param name="matchType"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public static bool CheckConditions(int tickCount, TreeInfo treeInfo, MatchType matchType, List<SubConditionItem> conditions)
        {
            if (conditions != null && conditions.Count > 0)
            {
                int matchCount = 0;
                int validCount = 0;

                foreach (var conditionNode in conditions)
                {
                    if (conditionNode != null && conditionNode.node && conditionNode.state < 2)
                        validCount++;
                    else
                        continue;

                    conditionNode.status = conditionNode.node.Execute(conditionNode);
                    conditionNode.tickCount = tickCount;

                    var result = conditionNode.status == Status.Success;
                    if (conditionNode.state == 1)
                        result = !result;
                    //Debug.Log("check sub condition:" + conditionNode.node.name + " ," + result);
                    switch (matchType)
                    {
                        case MatchType.AnySuccess:
                            if (result)
                                return true;
                            break;
                        case MatchType.AnyFailure:
                            if (!result)
                                return true;
                            break;
                        case MatchType.AllSuccess:
                            if (result)
                                matchCount++;
                            else
                                return false;
                            break;
                        case MatchType.AllFailure:
                            if (!result)
                                matchCount++;
                            else
                                return false;
                            break;
                        default:
                            matchCount = -1;
                            break;
                    }
                }
                if (matchCount >= 0 && matchCount != validCount)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 条件检查
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CheckConditions(int tickCount, TreeInfo treeInfo)
        {
            var conditionInfo = treeInfo.condition;
            if (conditionInfo.enable && conditionInfo.conditions.Count > 0)
            {
                int matchCount = 0;
                int validCount = 0;

                foreach (var condition in conditionInfo.conditions)
                {
                    if (condition != null && condition.node != null && condition.state < 2)
                        validCount++;
                    else
                        continue;

                    bool subResult = true;
                    if (condition.subEnable)
                        subResult = CheckConditions(tickCount, treeInfo, condition.matchType, condition.subConditions);

                    if (subResult)
                        condition.status = condition.node.Execute(condition);
                    else
                        condition.status = Status.Failure;

                    condition.tickCount = tickCount;
                    var checkResult = condition.status == (condition.state == 1 ? Status.Failure : Status.Success);

                    //Debug.Log("checking condition:" + condition.node.name + "," + checkResult + "," + conditionInfo.matchType);

                    switch (conditionInfo.matchType)
                    {
                        case MatchType.AllSuccess:
                            if (checkResult)
                                matchCount++;
                            else
                                return false;
                            break;
                        case MatchType.AllFailure:
                            if (!checkResult)
                                matchCount++;
                            else
                                return false;
                            break;
                        case MatchType.AnySuccess:
                            if (checkResult)
                                return true;
                            matchCount = -1;
                            break;
                        case MatchType.AnyFailure:
                            if (!checkResult)
                                return true;
                            matchCount = -1;
                            break;
                    }
                }
                return matchCount >= 0 && matchCount == validCount;
            }
            return true;
        }
    }
}

