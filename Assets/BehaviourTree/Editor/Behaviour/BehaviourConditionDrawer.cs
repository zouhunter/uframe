/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-07-24
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace UFrame.BehaviourTree
{
    public class BehaviourConditionDrawer
    {
        private ReorderableList _conditionList;
        private NodeBehaviour _nodeBehaviour;
        private Dictionary<int, ReorderableList> _subConditionListMap = new Dictionary<int, ReorderableList>();
        private Dictionary<BaseNode, string> _propPaths;
        private SerializedObject _serializedObject;
        private float _baseHeight;
        private GUIContent _conditionName;
        public BehaviourConditionDrawer(SerializedObject serializedObject, NodeBehaviour info)
        {
            _serializedObject = serializedObject;
            _nodeBehaviour = info;
            if (_nodeBehaviour.condition.conditions == null)
                _nodeBehaviour.condition.conditions = new List<ConditionItem>();
            _conditionList = new ReorderableList(_nodeBehaviour.condition.conditions, typeof(ConditionNode), true, true, true, true);
            _conditionList.drawHeaderCallback = OnDrawConditionHead;
            _conditionList.onAddCallback = OnAddCondition;
            _conditionList.onRemoveCallback = OnDeleteCondition;
            _conditionList.elementHeightCallback = OnDrawConditonHight;
            _conditionList.drawElementCallback = OnDrawCondition;
            _conditionList.onSelectCallback = OnSelectCondition;
            _propPaths = new Dictionary<BaseNode, string>();
            CollectConditionsProp(_nodeBehaviour, _propPaths);
        }

        private void OnSelectCondition(ReorderableList list)
        {
            var condition = _nodeBehaviour.condition.conditions[list.index];
            CollectConditionsProp(_nodeBehaviour, _propPaths);
        }

        private void OnSubConditionSelect(SubConditionItem condition)
        {
            CollectConditionsProp(_nodeBehaviour, _propPaths);
        }

        public float GetHeight()
        {
            _baseHeight = _conditionList.GetHeight();
            var height = _baseHeight;
            return height;
        }

        public void OnGUI(Rect rect)
        {
            _conditionList.DoList(rect);
        }

        private void OnDrawConditionHead(Rect rect)
        {
            var color = Color.yellow;
            if (_nodeBehaviour.node && _nodeBehaviour.status == Status.Success)
                color = Color.green;
            var condRect = new Rect(rect.x, rect.y, 60, rect.height);
            using (var c = new ColorScope(true, Color.yellow))
                EditorGUI.LabelField(condRect, "Conditions");

            var matchRect = new Rect(rect.x + rect.width - 75, rect.y, 75, EditorGUIUtility.singleLineHeight);
            _nodeBehaviour.condition.matchType = (MatchType)EditorGUI.EnumPopup(matchRect, _nodeBehaviour.condition.matchType, EditorStyles.linkLabel);
        }

        private float OnDrawConditonHight(int index)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (_nodeBehaviour.condition.conditions.Count > index)
            {
                var condition = _nodeBehaviour.condition.conditions[index];
                if (condition == null)
                    _nodeBehaviour.condition.conditions[index] = condition = new ConditionItem();

                if (condition.subEnable && condition.state < 2)
                {
                    height += EditorGUIUtility.singleLineHeight * 2.5f;
                    if (condition.subConditions != null && condition.subConditions.Count > 0)
                    {
                        height += EditorGUIUtility.singleLineHeight * condition.subConditions.Count;
                    }
                }
            }
            return height;
        }

        private void OnDrawCondition(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_nodeBehaviour.condition.conditions == null || _nodeBehaviour.condition.conditions.Count <= index)
                return;
            var condition = _nodeBehaviour.condition.conditions[index];
            var inverseRect = new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight);
            var normaIcon = EditorGUIUtility.IconContent("d_toggle_on_focus").image;
            var inverseIcon = IconCacheUtil.GetTextureByGUID("dd3247368b7f1bc43baf81039a01f4ee");
            var inValidIcon = EditorGUIUtility.IconContent("d_console.erroricon").image;
            var conditionIcon = (condition.state == 0) ? normaIcon : (condition.state == 1) ? inverseIcon : inValidIcon;
            if (GUI.Button(inverseRect, conditionIcon))
                condition.state = (++condition.state) % 3;

            if (condition == null)
                condition = _nodeBehaviour.condition.conditions[index] = new ConditionItem();
            var objectRect = new Rect(rect.x + 25, rect.y, rect.width - 55, EditorGUIUtility.singleLineHeight);
            var enableRect = new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight);

            using (var disableScope = new EditorGUI.DisabledScope(condition.state > 1))
            {
                var status = condition.status;
                int tickCount = condition.tickCount;
                string propPath = null;
                if (condition.node)
                    _propPaths.TryGetValue(condition.node, out propPath);
                NodeBehaviourDrawer.DrawCreateNodeContent(objectRect, condition.node, n =>
                {
                    RecordUndo("condition node changed!");
                    n.name = $"{n.name} ({n.GetType().Name})";
                    condition.node = n;
                }, _nodeBehaviour, status, tickCount, _serializedObject, propPath);
                condition.subEnable = EditorGUI.Toggle(enableRect, condition.subEnable, EditorStyles.radioButton);
            }

            var subConditionEnable = condition.subEnable && condition.state < 2;
            if (subConditionEnable)
            {
                var hashCode = condition.GetHashCode();
                if (!_subConditionListMap.TryGetValue(hashCode, out var subConditionList))
                {
                    if (condition.subConditions == null)
                        condition.subConditions = new List<SubConditionItem>();
                    subConditionList = _subConditionListMap[hashCode] = new ReorderableList(condition.subConditions, typeof(ConditionNode), true, true, true, true);
                    subConditionList.headerHeight = 0;
                    subConditionList.onSelectCallback = (list) => { OnSubConditionSelect(condition.subConditions[list.index]); };
                    subConditionList.drawElementCallback = (subRect, subIndex, subIsActive, subIsFocused) =>
                    {
                        var subNode = condition.subConditions[subIndex];
                        if (subNode == null)
                            subNode = new SubConditionItem();
                        var subInverseRect = new Rect(subRect.x, subRect.y, 20, EditorGUIUtility.singleLineHeight);
                        var subConditionIcon = (subNode.state == 0) ? normaIcon : (subNode.state == 1) ? inverseIcon : inValidIcon;
                        if (GUI.Button(subInverseRect, subConditionIcon))
                            subNode.state = (++subNode.state) % 3;

                        using (var disableScope2 = new EditorGUI.DisabledGroupScope(subNode.state > 1))
                        {
                            var subObjectRect = new Rect(subRect.x + 25, subRect.y, subRect.width - 25, EditorGUIUtility.singleLineHeight);
                            var status = subNode.status;
                            int tickCount = subNode.tickCount;
                            if (_nodeBehaviour.treeInfo != null)
                            {
                                status = _nodeBehaviour.treeInfo.status;
                                tickCount = _nodeBehaviour.treeInfo.tickCount;
                            }
                            string propPath = null;
                            if (subNode.node)
                                _propPaths.TryGetValue(subNode.node, out propPath);
                            NodeBehaviourDrawer.DrawCreateNodeContent(subObjectRect, subNode.node, n =>
                            {
                                RecordUndo("condition sub node changed!");
                                n.name = $"{n.name} ({n.GetType().Name})";
                                subNode.node = n;
                            }, _nodeBehaviour, status, tickCount, _serializedObject, propPath);
                            NodeBehaviourDrawer.DrawNodeState(subNode.node, subNode.tickCount, subNode.status, subObjectRect);
                        }

                        condition.subConditions[subIndex] = subNode;
                        var subMenuRect = new Rect(subRect.x - 100, subRect.y, 100, EditorGUIUtility.singleLineHeight);
                        if (subMenuRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
                        {
                            var menu = new GenericMenu();
                            if (CopyPasteUtil.copyNode && CopyPasteUtil.copyNode is ConditionNode cdn)
                            {
                                menu.AddItem(new GUIContent("Paste"), false, (x) =>
                                {
                                    RecordUndo("paste node");
                                    condition.subConditions[subIndex].node = cdn;
                                }, 0);
                            }
                            if (subNode.node)
                            {
                                menu.AddItem(new GUIContent("Copy"), false, (x) =>
                                {
                                    CopyPasteUtil.copyNode = subNode.node;
                                }, 0);
                            }
                            menu.ShowAsContext();
                        }
                    };
                    subConditionList.onAddCallback = (subList) =>
                    {
                        condition.subConditions.Add(null);
                    };
                    subConditionList.onRemoveCallback = (subList) =>
                    {
                        RecordUndo("remove sub condition element");
                        condition.subConditions.RemoveAt(subList.index);
                    };
                }
                subConditionList.DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, subConditionList.GetHeight()));
            }

            if (subConditionEnable)
            {
                var verticlOffset = EditorGUIUtility.singleLineHeight;
                if (condition.subConditions != null && condition.subConditions.Count > 0)
                {
                    verticlOffset += (EditorGUIUtility.singleLineHeight + 4) * condition.subConditions.Count;
                }

                if (condition.subConditions != null && condition.subConditions.Count > 0)
                {
                    var matchTypeRect = new Rect(rect.x + 20, rect.y + verticlOffset, 75, EditorGUIUtility.singleLineHeight);
                    condition.matchType = (MatchType)EditorGUI.EnumPopup(matchTypeRect, condition.matchType, EditorStyles.linkLabel);
                }
            }

            var subEnableRect1 = new Rect(rect.x - 100, rect.y, 100, EditorGUIUtility.singleLineHeight);
            if (subEnableRect1.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    if (CopyPasteUtil.copyNode && CopyPasteUtil.copyNode is ConditionNode cdn)
                    {
                        menu.AddItem(new GUIContent("Paste"), false, (x) =>
                        {
                            RecordUndo("paste node");
                            condition.node = cdn;
                        }, 0);
                    }
                    if (condition.node)
                    {
                        menu.AddItem(new GUIContent("Copy"), false, (x) =>
                        {
                            CopyPasteUtil.copyNode = condition.node;
                        }, 0);
                    }
                    menu.ShowAsContext();
                }
                else if (Event.current.control && Event.current.keyCode == KeyCode.C)
                {
                    CopyPasteUtil.copyNode = condition.node;
                }
                else if (Event.current.control && Event.current.keyCode == KeyCode.V && CopyPasteUtil.copyNode is ConditionNode cdn)
                {
                    RecordUndo("paste node");
                    condition.node = cdn;
                }
            }
        }

        private void OnDeleteCondition(ReorderableList list)
        {
            RecordUndo("remove condition element");
            _nodeBehaviour.condition.conditions.RemoveAt(list.index);
        }

        private void OnAddCondition(ReorderableList list)
        {
            RecordUndo("add condition element");
            _nodeBehaviour.condition.conditions.Add(new ConditionItem());
        }

        private void RecordUndo(string info)
        {
            Undo.RecordObject(_nodeBehaviour, info);
        }


        public static void CollectConditionsProp(NodeBehaviour info, Dictionary<BaseNode, string> _propPaths)
        {
            if (info.condition != null && info.condition.conditions != null)
            {
                int i = 0;
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.node && !_propPaths.ContainsKey(condition.node))
                    {
                        _propPaths[condition.node] = $"condition.conditions.Array.data[{i}].node";
                    }

                    if (condition.subConditions != null)
                    {
                        int j = 0;
                        foreach (var subNode in condition.subConditions)
                        {
                            if (subNode != null && subNode.node && !_propPaths.ContainsKey(subNode.node))
                            {
                                _propPaths[subNode.node] = $"condition.conditions.Array.data[{i}].subConditions.Array.data[{j}].node";
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
        }

    }
}

