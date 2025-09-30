/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-13
 * Version: 1.0.0
 * Description: 
 *_*/
using UFrame.BehaviourTree.Actions;

using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEditorInternal;

using UnityEngine;
using UnityEngine.Networking;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public class TaskInfoDrawer
    {
        private TaskInfoDrawer _parentTree;
        private TaskInfo _treeInfo;
        private Dictionary<TaskInfo, TaskInfoDrawer> _subDrawers;
        private ReorderableList _subTreeList;
        private bool _changed;
        private string _deepth;
        private NTree _tree;
        private NTreeEditor _treeDrawer;
        private NTreeConditionDrawer _conditionDrawer;
        private CheckDrawer _checkDrawer;
        private EffectDrawer _effectDrawer;

        public bool changeTrigger
        {
            get
            {
                if (_changed)
                {
                    _changed = false;
                    return true;
                }
                if (_subDrawers != null)
                {
                    foreach (var pair in _subDrawers)
                    {
                        if (pair.Value.changeTrigger)
                            return true;
                    }
                }
                if (_conditionDrawer != null)
                {
                    if (_conditionDrawer.changed)
                    {
                        return true;
                    }
                }
                if (_effectDrawer != null && _effectDrawer.changed)
                    return true;
                if (_checkDrawer != null && _checkDrawer.changed)
                    return true;
                return false;
            }
        }
        public TaskInfoDrawer(NTreeEditor treeDrawer, TaskInfoDrawer parentTree, TaskInfo info)
        {
            _treeDrawer = treeDrawer;
            _treeInfo = info;
            _tree = (treeDrawer.target as NTree);
            _parentTree = parentTree;
            RebuildSubTreeList();
            RebuildConditionList();
            RebuildCheckList();
            RebuildEffectList();
        }

        private void RebuildConditionList()
        {
            if (_treeInfo == null)
                return;

            _treeInfo.condition = _treeInfo.condition ?? new ConditionInfo();
            _treeInfo.condition.conditions = _treeInfo.condition.conditions ?? new List<ConditionItem>();
            _conditionDrawer = new NTreeConditionDrawer(_treeDrawer.target as NTree, _treeInfo, _treeDrawer);
        }

        private void RebuildCheckList()
        {
            if (_treeInfo == null)
                return;
            _treeInfo.checks = _treeInfo.checks ?? new List<CheckInfo>();
            _checkDrawer = new CheckDrawer(_treeDrawer.target as NTree, _treeInfo, _treeDrawer);
        }
        private void RebuildEffectList()
        {
            if (_treeInfo == null)
                return;
            _treeInfo.effects = _treeInfo.effects ?? new List<EffectInfo>();
            _effectDrawer = new EffectDrawer(_treeDrawer.target as NTree, _treeInfo, _treeDrawer);
        }

        private void RebuildSubTreeList()
        {
            if (_treeInfo == null)
                return;

            _subDrawers = new Dictionary<TaskInfo, TaskInfoDrawer>();
            _treeInfo.subTrees = _treeInfo.subTrees ?? new List<TreeInfo>();
            foreach (var child in _treeInfo.subTrees)
            {
                if (child == null) continue;
                _subDrawers[child as TaskInfo] = new TaskInfoDrawer(_treeDrawer, this, child as TaskInfo);
            }
            bool canAddOrDelete = !(_treeDrawer.target is BVariantTree);
            _subTreeList = new ReorderableList(_treeInfo.subTrees, typeof(TaskInfo), true, true, canAddOrDelete, canAddOrDelete);
            _subTreeList.headerHeight = 0;
            _subTreeList.elementHeightCallback = OnSubTreeElementHeight;
            _subTreeList.drawElementCallback = OnDrawSubTreeElement;
            _subTreeList.onAddCallback = OnAddSubTreeElement;
            _subTreeList.onRemoveCallback = OnRemoveSubTreeElement;
        }

        private void OnRemoveSubTreeElement(ReorderableList list)
        {
            var baseTaskInfo = TaskInfoInBase(_treeInfo);
            if (baseTaskInfo != null)
            {
                EditorUtility.DisplayDialog("Error", "Can't delete base tree sub node", "OK");
                return;
            }

            RecordUndo("remove sub tree element");
            _treeInfo.subTrees.RemoveAt(list.index);
            _changed = true;
        }

        private void OnAddSubTreeElement(ReorderableList list)
        {
            var baseTaskInfo = TaskInfoInBase(_treeInfo);
            if (baseTaskInfo != null)
            {
                EditorUtility.DisplayDialog("Error", "Can't add base tree sub node", "OK");
                return;
            }

            RecordUndo("add sub tree element");
            if (_treeInfo.node is ParentNode)
            {
                var maxChildCount = (_treeInfo.node as ParentNode).maxChildCount;
                if (_treeInfo.subTrees != null && maxChildCount <= _treeInfo.subTrees.Count)
                    return;
            }
            var treeInfo = TaskInfo.Create();
            treeInfo.enable = true;
            _treeInfo.subTrees.Add(treeInfo);
            _subDrawers[treeInfo as TaskInfo] = new TaskInfoDrawer(_treeDrawer, this, treeInfo as TaskInfo);
            _changed = true;
        }

        private TaskInfoDrawer GetSubDrawer(TaskInfo info)
        {
            if (info == null)
                return null;
            if (!_subDrawers.TryGetValue(info, out var drawer))
            {
                drawer = new TaskInfoDrawer(_treeDrawer, this, info);
                _subDrawers[info] = drawer;
            }
            return drawer;
        }

        private float OnSubTreeElementHeight(int index)
        {
            var elementHeight = 4;
            if (_treeInfo.subTrees.Count > index)
            {
                var info = _treeInfo.subTrees[index];
                var drawer = GetSubDrawer(info as TaskInfo);
                return drawer?.GetHeight() ?? 0 + elementHeight;
            }
            return elementHeight;
        }

        private void OnDrawSubTreeElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                var innerRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
                GUI.Box(innerRect, GUIContent.none);
                if (_treeInfo.subTrees.Count > index && _subDrawers.Count > index)
                {
                    var info = _treeInfo.subTrees[index];
                    var drawer = GetSubDrawer(info as TaskInfo);
                    var disable = _treeInfo.node is ParentNode parentNode && parentNode.maxChildCount <= index;
                    using (var disableScope = new EditorGUI.DisabledGroupScope(disable))
                    {
                        var baseInfo = TaskInfoInBase(info as TaskInfo);
                        bool green = !EditorApplication.isPlaying && CheckTaskInfoChangedSelf(baseInfo, info as TaskInfo);
                        using (var colorScope = new ColorScope(green, new Color(0, 1, 0, 0.8f)))
                        {
                            drawer?.OnInspectorGUI(innerRect, _deepth + (1 + index));
                        }
                    }
                }
                if (changeCheck.changed)
                {
                    _changed = true;
                }
            }
        }

        private bool CheckTaskInfoChangedSelf(TaskInfo baseInfo, TaskInfo info)
        {
            if (baseInfo == null)
            {
                return true;
            }
            if (baseInfo.enable != info.enable || baseInfo.id != info.id)
            {
                return true;
            }
            if (baseInfo.condition.enable != info.condition.enable)
            {
                return true;
            }
            if (baseInfo.condition.matchType != info.condition.matchType)
            {
                return true;
            }
            return false;
        }

        public float GetHeight()
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (_treeInfo.enable)
            {
                float checkHeight = _checkDrawer.GetHeight();
                float effectHeight = _effectDrawer.GetHeight();
                float bothHeight = Mathf.Max(checkHeight, effectHeight);
                height += bothHeight;

                if (_treeInfo.condition.enable)
                {
                    height += _conditionDrawer.GetHeight();
                }

                var _needChild = _treeInfo.node is ParentNode || (_treeInfo.subTrees != null && _treeInfo.subTrees.Count > 0);
                if (_needChild)
                {
                    height += _subTreeList.GetHeight();
                }

            }
            return height;
        }

        public void OnInspectorGUI(Rect position, string deepth)
        {
            var fullRect = position;

            this._deepth = deepth;
            var nodeEnableRect = new Rect(position.x, position.y, 20, EditorGUIUtility.singleLineHeight);
            _treeInfo.enable = EditorGUI.Toggle(nodeEnableRect, _treeInfo.enable);

            var labelRect = new Rect(position.x + 20, position.y, 50, EditorGUIUtility.singleLineHeight);
            if (string.IsNullOrEmpty(_deepth))
            {
                EditorGUI.LabelField(labelRect, "Nodes");
            }
            else
            {
                EditorGUI.LabelField(labelRect, _deepth);
                labelRect.width = _deepth.Length * 8;
            }
            var nodeRect = new Rect(position.x + labelRect.width + 25, position.y, position.width - labelRect.width - 60, EditorGUIUtility.singleLineHeight);
            using (var disableScope = new EditorGUI.DisabledGroupScope(!_treeInfo.enable))
            {
                var baseInfo = TaskInfoInBase(_treeInfo);
                var green = !EditorApplication.isPlaying && CheckTaskInfoChangedSelf(baseInfo, _treeInfo);
                using (var colorScope = new ColorScope(green, Color.green))
                {
                    DrawCreateNodeContent(nodeRect, _treeInfo.node, n =>
                    {
                        RecordUndo("node changed!");
                        _treeInfo.node = n;
                    }, _treeDrawer.target as NTree, _treeInfo.status, _treeInfo.tickCount, _treeDrawer.serializedObject, _treeDrawer.GetPropPath(_treeInfo.node));
                }

                var yOffset = position.y + nodeRect.height;
                position.x += 30;
                position.width -= 30;

                var _needCondition = true;
                var _needChild = _treeInfo.node is ParentNode || (_treeInfo.subTrees != null && _treeInfo.subTrees.Count > 0);

                using (var disableScop = new EditorGUI.DisabledGroupScope(!_needCondition))
                {
                    var conditionEnableRect = new Rect(position.x + position.width - 30, position.y, 20, EditorGUIUtility.singleLineHeight);
                    _treeInfo.condition.enable = EditorGUI.Toggle(conditionEnableRect, new GUIContent("", "ConditionEnable"), _treeInfo.condition.enable, EditorStyles.radioButton);
                }

                if (_treeInfo.enable)
                {
                    float checkHeight = _checkDrawer.GetHeight();
                    float effectHeight = _effectDrawer.GetHeight();
                    float bothHeight = Mathf.Max(checkHeight, effectHeight);
                    float halfWidth = position.width / 2f - 2f;
                    var checkRect = new Rect(position.x, yOffset, halfWidth, bothHeight);
                    var effectRect = new Rect(position.x + halfWidth + 4f, yOffset, halfWidth, bothHeight);
                    _checkDrawer.OnGUI(checkRect);
                    _effectDrawer.OnGUI(effectRect);
                    yOffset += bothHeight;

                    if (_needCondition && _treeInfo.condition.enable)
                    {
                        var conditionRect = new Rect(position.x, yOffset, position.width, _conditionDrawer.GetHeight());
                        _conditionDrawer.OnGUI(conditionRect);

                        var countRect = new Rect(position.x + position.width - 15, yOffset, 20, EditorGUIUtility.singleLineHeight);
                        EditorGUI.LabelField(countRect, $"[{_treeInfo.condition.conditions.Count}]", EditorStyles.miniBoldLabel);

                        yOffset += _conditionDrawer.GetHeight();
                    }

                    if (_needChild)
                    {
                        DrawLineLink(_treeInfo, fullRect, yOffset);
                        var subTreeRect = new Rect(position.x, yOffset, position.width, _subTreeList.GetHeight());
                        _subTreeList.DoList(subTreeRect);
                    }
                    else if (_needCondition && _treeInfo.condition.conditions.Count > 0)
                    {
                        DrawWireBox(_treeInfo, new Rect(fullRect.x, fullRect.y, fullRect.width, fullRect.height));
                    }
                }
            }

            var menuRect = new Rect(position.x - 80, position.y, 60, EditorGUIUtility.singleLineHeight);
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Double (%d)"), false, Doublicat, 0);
                menu.AddItem(new GUIContent("Add Child"), false, (x) => OnAddSubTreeElement(_subTreeList), 0);
                menu.AddItem(new GUIContent("Insert Before"), false, InsertBefore, 0);
                menu.AddItem(new GUIContent("Insert Next"), false, InsertNext, 0);
                menu.AddItem(new GUIContent("Insert Parent"), false, InsertParent, 0);
                menu.AddItem(new GUIContent("Delete Self (del)"), false, DeleteSelf, 0);
                menu.AddItem(new GUIContent("Delete All"), false, DeleteAll, 0);
                menu.AddItem(new GUIContent("Hierarchy/Copy (%c)"), false, CopyNode, 0);
                menu.AddItem(new GUIContent("Hierarchy/Cut (%x)"), false, CopyNode, 1);
                if (TaskCopyPasteUtil.copyedTaskInfo != _treeInfo && _treeInfo != null)
                    menu.AddItem(new GUIContent("Hierarchy/Paste (%v)"), false, PasteNode, 0);
                menu.ShowAsContext();
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.D)
            {
                Doublicat(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.C)
            {
                CopyNode(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.X)
            {
                CopyNode(1);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.V)
            {
                if (TaskCopyPasteUtil.copyedTaskInfo != _treeInfo && _treeInfo != null)
                    PasteNode(0);
            }

            if (labelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy Node"), false, (x) =>
                {
                    TaskCopyPasteUtil.copyNode = _treeInfo.node;
                }, 0);
                if (TaskCopyPasteUtil.copyNode)
                {
                    menu.AddItem(new GUIContent("Paste Node"), false, (x) =>
                    {
                        RecordUndo("paste node");
                        _treeInfo.node = TaskCopyPasteUtil.copyNode;
                        _changed = true;
                    }, 0);
                }

                menu.ShowAsContext();
            }
        }

        public static void DrawCreateNodeContent<T>(Rect rect, T node, Action<T> onCreate, NTree tree, byte status, int tickCount, SerializedObject target = null, string propPath = null) where T : BaseNode
        {
            var nameRect = new Rect(rect.x, rect.y, rect.width - 50, rect.height);
            bool createTouched = false;
            if (node != null)
            {
                var iconX = nameRect.max.x + 5;
                NTree subTree = null;
                if (node is NTreeNode subNode)
                {
                    subTree = EditorApplication.isPlaying ? subNode.instaneTree : subNode.tree;
                    if (subTree)
                    {
                        var subRect = new Rect(nameRect.xMax - 20, nameRect.y, 20, EditorGUIUtility.singleLineHeight);
                        if (NTreeUtil.DrawSubTree(subRect, subTree))
                            nameRect.width -= 25;
                    }
                }
                if (!string.IsNullOrEmpty(propPath))
                {
                    var propX = nameRect.max.x - 32;
                    nameRect.width -= 15;
                    if (target != null && GUI.Button(new Rect(propX, rect.y, 35, EditorGUIUtility.singleLineHeight), "", EditorStyles.objectFieldMiniThumb))
                    {
                        var prop = target.FindProperty(propPath);
                        PropDrawerWindow.Show(prop);
                    }
                }
                using (var color = new ColorScope(node.Owner != null, GetNodeStateColor(node.Owner, status, tickCount)))
                {
                    node.name = EditorGUI.TextField(nameRect, node.name, EditorStyles.objectField);
                }
                if (node.Owner != null && node.Owner.TickCount != tickCount)
                {
                    DrawNodeBackground(status, nameRect);
                }
                var iconRect = new Rect(iconX, rect.y, 20, EditorGUIUtility.singleLineHeight);
                NTreeUtil.DrawIcon(iconRect, node);
            }
            else
            {
                using (var disableScope = new EditorGUI.DisabledScope(false))
                {
                    using (var colorScope = new ColorScope(true, Color.red))
                    {
                        if (GUI.Button(nameRect, "Null", EditorStyles.textField))
                        {
                            createTouched = true;
                        }
                    }
                }
            }
            using (var disableScope = new EditorGUI.DisabledScope(false))
            {
                var createRect = new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height);
                if (createTouched || GUI.Button(createRect, "", EditorStyles.popup))
                {
                    var nodes = new List<BaseNode>();
                    tree.CollectNodesDeepth(tree.rootTree, nodes);
                    CreateNTNodeWindow.Show(Event.current.mousePosition, (node) =>
                    {
                        if (!node.name.Contains("("))
                            node.name = $"{node.name} ({node.GetType().Name})";
                        onCreate?.Invoke(node as T);
                    }, typeof(T), nodes);
                }
            }
        }

        private static void DrawNodeBackground(byte status, Rect rect)
        {
            var color = Color.gray;
            var show = false;
            if (status == Status.Success)
            {
                show = true;
                color = Color.green;
            }
            else if (status == Status.Failure)
            {
                show = true;
                color = Color.red;
            }
            else if (status == Status.Running)
            {
                show = true;
                color = Color.yellow;
            }
            else if (status == Status.Interrupt)
            {
                show = true;
                color = Color.blue;
            }
            if (show)
            {
                color.a *= 0.5f;
                using (var colorScope = new ColorGUIScope(true, color))
                {
                    GUI.Box(rect, "");
                }
            }
        }
        public static Color GetStatusColor(byte status, bool active)
        {
            return GraphColorUtil.GetColorByState(status, active);
        }
        public static Color GetNodeStateColor(IOwner tree, byte status, int tickCount)
        {
            var color = Color.gray;
            if (tree == null)
                return color;
            return GetStatusColor(status, tickCount == tree.TickCount);
        }

        private void DrawWireBox(TaskInfo node, Rect rect)
        {
            if (!node.node || node.node.Owner == null || node.node.Owner.TickCount != node.tickCount)
                return;

            rect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);
            var leftRect = new Rect(rect.x, rect.y, 2, rect.height);
            var rightRect = new Rect(rect.x + rect.width - 2, rect.y, 2, rect.height);
            var topRect = new Rect(rect.x, rect.y, rect.width, 2);
            var bottomRect = new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2);
            var color = Color.gray;
            if (node != null)
                color = GetStatusColor(node.status, true);
            color.a = 0.5f;
            using (var colorScope = new ColorGUIScope(true, color))
            {
                GUI.DrawTexture(leftRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(rightRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(topRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(bottomRect, EditorGUIUtility.whiteTexture);
            }
        }

        private void DrawLineLink(TaskInfo node, Rect rect, float yOffset)
        {
            var active = true;
            if (!node.node || node.node.Owner == null || node.node.Owner.TickCount != node.tickCount)
            {
                active = false;
            }

            yOffset = yOffset - rect.y;
            float height = (rect.height - yOffset);
            var percent = (height * 0.5f + yOffset - 1.5f * EditorGUIUtility.singleLineHeight) / rect.height;
            var verticllLine = new Rect(rect.x + 5, rect.y + 15, 3, rect.height * percent);
            var horizontallLine = new Rect(verticllLine.x, verticllLine.yMax, 25, 3);
            var color = Color.gray;
            if (node != null)
            {
                color = GetStatusColor(node.status, active);
            }
            color.a = 0.5f;
            using (var colorScope = new ColorGUIScope(true, color))
            {
                GUI.DrawTexture(verticllLine, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(horizontallLine, EditorGUIUtility.whiteTexture);
            }
        }

        private void CopyNode(object userData)
        {
            TaskCopyPasteUtil.cut = (userData is int u) && u == 1;
            TaskCopyPasteUtil.copyedTaskInfo = _treeInfo;
            TaskCopyPasteUtil.copyedTaskInfoDrawer = this;
            if (_treeInfo.node)
            {
                TaskCopyPasteUtil.copyNode = _treeInfo.node;
            }
        }

        public void CopyNode(TaskInfo source, TaskInfo target)
        {
            RecordUndo("copy node");
            TaskCopyPasteUtil.CopyTaskInfo(source, target, target);
            TaskCopyPasteUtil.copyedTaskInfo = null;
            RebuildSubTreeList();
            RebuildConditionList();
            _changed = true;

        }

        private void PasteNode(object userData)
        {
            if (TaskCopyPasteUtil.copyedTaskInfo != _treeInfo)
            {
                RecordUndo("paste node");
                CopyNode(TaskCopyPasteUtil.copyedTaskInfo, _treeInfo);
                if (TaskCopyPasteUtil.copyedTaskInfoDrawer != null && TaskCopyPasteUtil.cut)
                {
                    TaskCopyPasteUtil.copyedTaskInfoDrawer.DeleteAll(null);
                }
            }
        }

        private void DeleteAll(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("remove all element");
                _parentTree._treeInfo.subTrees.Remove(_treeInfo);
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }

        private void InsertParent(object a)
        {
            RecordUndo("insert parent element");
            var treeInfo = TaskInfo.Create();
            treeInfo.enable = _treeInfo.enable;
            treeInfo.node = _treeInfo.node;
            treeInfo.condition = _treeInfo.condition;
            treeInfo.subTrees = _treeInfo.subTrees;
            _treeInfo.node = null;
            _treeInfo.condition = new ConditionInfo();
            _treeInfo.subTrees = new List<TreeInfo>() { treeInfo as TaskInfo };
            _subDrawers[treeInfo as TaskInfo] = new TaskInfoDrawer(_treeDrawer, this, treeInfo as TaskInfo);
            RebuildSubTreeList();
            RebuildConditionList();
            _changed = true;
        }
        private void Doublicat(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("duplicate node");

                // 创建新的TaskInfo
                var newInfo = TaskInfo.Create();

                // 复制当前节点的所有信息
                TaskCopyPasteUtil.CopyTaskInfo(_treeInfo, newInfo as TaskInfo, newInfo as TaskInfo);

                // 获取当前节点在父节点子树中的索引
                var index = _parentTree._treeInfo.subTrees.IndexOf(_treeInfo);

                // 在当前节点后面插入新节点
                _parentTree._treeInfo.subTrees.Insert(index + 1, newInfo);

                // 重建父节点的子树列表
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }

        private void InsertBefore(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("insert before node");

                // 创建新的TaskInfo
                var newInfo = TaskInfo.Create();
                newInfo.enable = true;

                // 获取当前节点在父节点子树中的索引
                var index = _parentTree._treeInfo.subTrees.IndexOf(_treeInfo);

                // 在当前节点前面插入新节点
                _parentTree._treeInfo.subTrees.Insert(index, newInfo);

                // 重建父节点的子树列表
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }

        private void InsertNext(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("insert next node");

                // 创建新的TaskInfo
                var newInfo = TaskInfo.Create();
                newInfo.enable = true;

                // 获取当前节点在父节点子树中的索引
                var index = _parentTree._treeInfo.subTrees.IndexOf(_treeInfo);

                // 在当前节点后面插入新节点
                _parentTree._treeInfo.subTrees.Insert(index + 1, newInfo);

                // 重建父节点的子树列表
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }
        private void DeleteSelf(object arg)
        {
            RecordUndo("delete self element");
            if (_parentTree != null)
            {
                _parentTree._treeInfo.subTrees.Remove(_treeInfo);
                if (_treeInfo.subTrees != null)
                {
                    _parentTree._treeInfo.subTrees.AddRange(_treeInfo.subTrees);
                }
                _parentTree.RebuildSubTreeList();
                _parentTree.RebuildConditionList();
                _changed = true;
            }
            else
            {
                if (_treeInfo.subTrees.Count > 1)
                {
                    EditorUtility.DisplayDialog("InValid", "root tree can`t delete self,child count>1", "ok");
                }
                else
                {
                    var bTree = _treeDrawer.target as NTree;
                    bTree.rootTree = _treeInfo.subTrees[0] as TaskInfo;
                    _treeDrawer.RebuildView();
                    _changed = true;
                }
            }
        }

        private void RecordUndo(string flag)
        {
            _changed = true;
            _treeDrawer.RecordUndo(flag);
        }

        protected TaskInfo TaskInfoInBase(TaskInfo info)
        {
            return null;
        }
    }

    public struct ColorScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.contentColor;

            if (_changeColor)
            {
                GUI.contentColor = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
                GUI.contentColor = _oldColor;
        }
    }
    public struct ColorGUIScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorGUIScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.color;
            if (_changeColor)
            {
                GUI.color = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
            {
                GUI.color = _oldColor;
            }
        }
    }
    public struct ColorBgScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorBgScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.backgroundColor;
            if (_changeColor)
            {
                GUI.backgroundColor = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
            {
                GUI.backgroundColor = _oldColor;
            }
        }
    }
}

