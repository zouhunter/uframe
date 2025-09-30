using System.Collections.Generic;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public class TaskCopyPasteUtil
    {
        public static BaseNode copyNode;
        public static TaskInfo copyedTaskInfo;
        public static bool cut;
        public static TaskInfoDrawer copyedTaskInfoDrawer;

        public static void CopyTaskInfo(TaskInfo source, TaskInfo target, TaskInfo rootTarget)
        {
            if (cut)
                target.id = source.id;

            target.node = source.node;
            target.enable = source.enable;
            target.condition = new ConditionInfo();
            target.condition.enable = source.condition.enable;
            target.condition.conditions = new List<ConditionItem>();
            target.condition.matchType = source.condition.matchType;
            if (source.checks != null)
            {
                target.checks = new List<CheckInfo>(source.checks);
            }
            if (source.effects != null)
            {
                target.effects = new List<EffectInfo>(target.effects);
            }
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
                    target.condition.conditions.Add(conditionItem);
                }
            }
            if (source.subTrees != null)
            {
                target.subTrees = new List<TreeInfo>();
                foreach (var item in source.subTrees)
                {
                    if (item == rootTarget)
                        continue;

                    var subTree = TaskInfo.Create(cut ? item.id : null);
                    CopyTaskInfo(item as TaskInfo, subTree as TaskInfo, rootTarget);
                    target.subTrees.Add(subTree);
                }
            }
        }
    }
}
