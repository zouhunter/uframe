/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-15
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace UFrame.BehaviourTree
{
    [DisallowMultipleComponent]
    [CustomEditor(typeof(BVariantTree))]
    public class BVariantTreeDrawer : BTreeDrawer
    {
        private BVariantTree _bVariantTree;
        private BTree _baseTree;
        protected SerializedProperty _baseTreeDrawer;
        public bool drawBaseTree = true;
        protected override void OnEnable()
        {
            _bVariantTree = target as BVariantTree;
            _baseTree = _bVariantTree.baseTree;
            _baseTreeDrawer = serializedObject.FindProperty("baseTree");
            base.OnEnable();
            rootTree = _bVariantTree.rootTree;
            RebuildView();
        }

        protected override void OnDisable()
        {
            if (EditorApplication.isPlaying)
                return;
            CollectRealTreeInfo();
            base.OnDisable();
        }
        public override void OnInspectorGUI()
        {
            if (!_bVariantTree)
                return;

            base.OnInspectorGUI();
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                serializedObject.Update();
                bool changed = change.changed;
                if (drawBaseTree)
                {
                    using (var hor = new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(_baseTreeDrawer);
                        if (GUILayout.Button("Reset", EditorStyles.miniButtonLeft, GUILayout.Width(60)))
                        {
                            if (EditorUtility.DisplayDialog("Reset", "clear variant tree changes?", "Yes", "No"))
                            {
                                _bVariantTree.modifys.Clear();
                                EditorUtility.SetDirty(_bVariantTree);

                                _bVariantTree.BuildInstanceRootTree();
                                serializedObject.Update();
                                changed = true;
                            }
                        }
                        if (GUILayout.Button("Apply", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                        {
                            if (EditorUtility.DisplayDialog("Apply", "Apply variant tree to base tree?", "Yes", "No"))
                            {
                                _bVariantTree.ApplyToBaseTree();
                                EditorUtility.SetDirty(_baseTree);
                                changed = true;
                            }
                        }
                    }
                }
                if (changed)
                {
                    _baseTree = _baseTreeDrawer.objectReferenceValue as BTree;
                    _bVariantTree.baseTree = _baseTree;
                    RebuildView();
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void OnTreeInfoChanged()
        {
            base.OnTreeInfoChanged();
            CollectRealTreeInfo();
        }

        protected void CollectRealTreeInfo()
        {
            _bVariantTree.modifys = _bVariantTree.modifys ?? new List<TreeInfoModify>();
            _bVariantTree.modifys.Clear();
            _bVariantTree.rootTree = TreeInfo.Create("VariantRoot");
            if (_baseTree is BVariantTree bvt)
                bvt.BuildInstanceRootTree();
            CollectTreeInfo(rootTree);
            if (_bVariantTree)
                EditorUtility.SetDirty(_bVariantTree);
        }

        public void CollectTreeInfo(TreeInfo info)
        {
            if (info == null)
                return;

            if (info.subTrees != null)
            {
                foreach (var subTree in info.subTrees)
                {
                    CollectTreeInfo(subTree);
                }
            }
            var baseInfo = _baseTree?.FindTreeInfo(info.id);
            if (baseInfo == null)
            {
                Debug.LogError(_baseTree + " not exists:" + info.id);
                return;
            }
            var modify = CreateModify(baseInfo, info);
            if (modify != null)
            {
                _bVariantTree.modifys = _bVariantTree.modifys ?? new List<TreeInfoModify>();
                _bVariantTree.modifys.RemoveAll(x => x.id == modify.id);
                _bVariantTree.modifys.Add(modify);
            }
        }
        public static bool CheckNodeInfoEqual(BaseNode node1, BaseNode node2)
        {
            return JsonUtility.ToJson(node1) != JsonUtility.ToJson(node2);
        }

        public static TreeInfoModify CreateModify(TreeInfo baseInfo, TreeInfo treeInfo)
        {
            if (baseInfo == null)
                return null;

            var modify = new TreeInfoModify();
            modify.id = treeInfo.id;
            bool changed = false;
            if (baseInfo.enable != treeInfo.enable)
            {
                modify.enable = new InfoModify<bool>(treeInfo.enable);
                changed = true;
            }
            if (baseInfo.condition.enable != treeInfo.condition.enable)
            {
                modify.condition_enable = new InfoModify<bool>(treeInfo.condition.enable);
                changed = true;
            }
            if (baseInfo.condition.matchType != treeInfo.condition.matchType)
            {
                modify.condition_matchType = new InfoModify<MatchType>(treeInfo.condition.matchType);
                changed = true;
            }
            if (baseInfo.condition.conditions != null && treeInfo.condition.conditions != null)
            {
                bool needConditionModify = false;
                var conditionModifys = new List<ConditionInfoModify>();
                for (int i = 0; i < baseInfo.condition.conditions.Count && i < treeInfo.condition.conditions.Count; i++)
                {
                    var subCondition = treeInfo.condition.conditions[i];
                    var baseSubCondition = baseInfo.condition.conditions[i];

                    var subModify = CreateSubConditionModify(baseSubCondition, subCondition);
                    if (subModify != null)
                    {
                        conditionModifys.Add(subModify);
                        changed = true;
                        needConditionModify = true;
                    }
                    else
                    {
                        conditionModifys.Add(null);
                    }
                }
                if (needConditionModify)
                {
                    modify.condition_modifys = conditionModifys;
                }
            }
            return changed ? modify : null;
        }

        public static ConditionInfoModify CreateSubConditionModify(ConditionItem baseSubCondition, ConditionItem subCondition)
        {
            var modify = new ConditionInfoModify();
            bool changed = false;
            if (baseSubCondition.subEnable != subCondition.subEnable)
            {
                modify.subEnable = new InfoModify<bool>(subCondition.subEnable);
                changed = true;
            }
            if (baseSubCondition.matchType != subCondition.matchType)
            {
                modify.matchType = new InfoModify<MatchType>(subCondition.matchType);
                changed = true;
            }
            if (baseSubCondition.subConditions != null && baseSubCondition.subConditions.Count > 0
                && subCondition.subConditions != null && subCondition.subConditions.Count > 0)
            {
                bool needSubModify = false;
                List<InfoModify<int>> stateModifys = new List<InfoModify<int>>();
                for (int i = 0; i < baseSubCondition.subConditions.Count && i < subCondition.subConditions.Count; i++)
                {
                    if (baseSubCondition.subConditions[i].state != subCondition.subConditions[i].state)
                    {
                        var subMmodify = new InfoModify<int>(subCondition.subConditions[i].state);
                        stateModifys.Add(subMmodify);
                        needSubModify = true;
                        changed = true;
                    }
                    else
                    {
                        stateModifys.Add(null);
                    }
                }
                if (needSubModify)
                {
                    modify.sub_conditions = stateModifys;
                }
            }
            return changed ? modify : null;
        }
    }
}

