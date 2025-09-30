/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-25
 * Version: 1.0.0
 * Description: 行为树的变体，可以在原有行为树的基础上进行修改
 *_*/

using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    public class BVariantTree : BTree
    {
        public BTree baseTree;
        public List<TreeInfoModify> modifys;
        public override TreeInfo rootTree
        {
            get
            {
                if (baseTree != null && baseTree.rootTree.id != _rootTree.id)
                {
                    BuildInstanceRootTree();
                }
                return _rootTree;
            }
            set
            {
                _rootTree = value;
            }
        }

        public void BuildInstanceRootTree()
        {
            CopyTreeInfo(Instantiate(baseTree).rootTree, _rootTree);
            _rootTree.id = baseTree.rootTree.id;
            var dic = new Dictionary<string, TreeInfo>();
            CollectToDic(_rootTree, dic);
            ProcessModify(dic);
        }

        public void ApplyToBaseTree()
        {
            var dic = new Dictionary<string, TreeInfo>();
            CollectToDic(baseTree.rootTree, dic);
            ProcessModify(dic);
        }

        public void CopyTreeInfo(TreeInfo source, TreeInfo target)
        {
            target.id = source.id;
            target.enable = source.enable;
            target.desc = source.desc;
            target.node = source.node;
            target.condition = source.condition;
            target.subTrees = source.subTrees;
        }

        public override TreeInfo FindTreeInfo(string id, bool deep = true)
        {
            var info = base.FindTreeInfo(id);
            if (info == null && baseTree != null && deep)
            {
                info = baseTree.FindTreeInfo(id);
            }
            return info;
        }

        protected void ProcessModify(Dictionary<string, TreeInfo> infoDic)
        {
            if (modifys != null && modifys.Count > 0)
            {
                foreach (var modify in modifys)
                {
                    if (infoDic.TryGetValue(modify.id, out var treeInfo))
                    {
                        ModifyTreeInfo(treeInfo, modify);
                    }
                }
            }
        }

        protected void ModifyTreeInfo(TreeInfo treeInfo, TreeInfoModify modify)
        {
            if (modify.enable != null && modify.enable.enable)
            {
                treeInfo.enable = modify.enable.value;
            }
            if (modify.condition_enable != null && modify.condition_enable.enable)
            {
                treeInfo.condition.enable = modify.condition_enable.value;
            }
            if (modify.condition_matchType != null && modify.condition_matchType.enable)
            {
                treeInfo.condition.matchType = modify.condition_matchType.value;
            }
            if (modify.condition_modifys != null)
            {
                for (int i = 0; i < modify.condition_modifys.Count; i++)
                {
                    var conditionModify = modify.condition_modifys[i];
                    if (conditionModify == null)
                        continue;

                    if (conditionModify.subEnable.enable)
                    {
                        treeInfo.condition.conditions[i].subEnable = conditionModify.subEnable.value;
                    }

                    if (conditionModify.state.enable)
                    {
                        treeInfo.condition.conditions[i].state = conditionModify.state.value;
                    }

                    if (conditionModify.matchType.enable)
                    {
                        treeInfo.condition.conditions[i].matchType = conditionModify.matchType.value;
                    }

                    if (conditionModify.sub_conditions != null)
                    {
                        for (int j = 0; j < conditionModify.sub_conditions.Count && j < treeInfo.condition.conditions[i].subConditions.Count; j++)
                        {
                            var subModify = conditionModify.sub_conditions[j];
                            if (subModify == null)
                                continue;
                            if (subModify.enable)
                            {
                                treeInfo.condition.conditions[i].subConditions[j].state = subModify.value;
                            }
                        }
                    }
                }
            }
        }
    }
}

