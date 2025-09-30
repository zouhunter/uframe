using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using UFrame.BehaviourTree.Actions;

namespace UFrame.BehaviourTree
{
    [CustomEditor(typeof(NodeBehaviour))]
    [DisallowMultipleComponent]
    public class NodeBehaviourDrawer : Editor
    {
        private NodeBehaviour _nodeBehaviour;
        private ReorderableList _subTreeList;
        private bool _changed;
        private string _deepth;
        private BehaviourConditionDrawer _conditionDrawer;
        private Dictionary<int, ReorderableList> _subConditionListMap = new Dictionary<int, ReorderableList>();
        public bool changed
        {
            get
            {
                if (_changed)
                {
                    _changed = false;
                    return true;
                }
                return false;
            }
        }
        public BaseNode node
        {
            get
            {
                return _nodeBehaviour == null ? null : _nodeBehaviour.node;
            }
            set
            {
                if (_nodeBehaviour)
                    _nodeBehaviour.node = value;
            }
        }
        private SerializedProperty _nodeProp;
        private SerializedProperty _scriptProp;

        [MenuItem("GameObject/UFrame/BehaviourTree &b", priority = 0, validate = false)]
        private static void CreateBehaviourTree()
        {
            if (!Selection.activeTransform)
                return;

            var currentBehaviour = Selection.activeGameObject.GetComponent<NodeBehaviour>();

            List<BaseNode> customNodes = new List<BaseNode>();
            if (currentBehaviour)
                CollectNodesDeepth(GetRootBehaviour(currentBehaviour), customNodes);
            if (Event.current != null)
            {
                CreateNodeWindow.Show(Event.current.mousePosition, (node) =>
                {
                    var nodeBehaviour = new GameObject(node.name).AddComponent<NodeBehaviour>();
                    nodeBehaviour.transform.SetParent(Selection.activeTransform);
                    nodeBehaviour.node = node;
                    EditorUtility.SetDirty(nodeBehaviour);

                }, typeof(BaseNode), customNodes);
            }

        }

        private void OnEnable()
        {
            _nodeBehaviour = target as NodeBehaviour;
            RebuildConditionList();
            _nodeProp = serializedObject.FindProperty("node");
            _nodeProp.isExpanded = true;
            _scriptProp = serializedObject.FindProperty("m_Script");
            _conditionDrawer = new BehaviourConditionDrawer(serializedObject, _nodeBehaviour);
        }
        private void RebuildConditionList()
        {
            if (_nodeBehaviour == null)
                return;

            _nodeBehaviour.condition = _nodeBehaviour.condition ?? new ConditionInfo();
            _nodeBehaviour.condition.conditions = _nodeBehaviour.condition.conditions ?? new List<ConditionItem>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (var disableScope = new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_scriptProp);
            using (var hor = new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                if (node == null)
                    GUILayout.FlexibleSpace();
                var rect = hor.rect;
                rect.height = EditorGUIUtility.singleLineHeight;
                var contentRect = new Rect(rect.x, rect.y, rect.width - 25, EditorGUIUtility.singleLineHeight);
                var status = Status.Inactive;
                int tickCount = 0;
                if (_nodeBehaviour.treeInfo != null)
                {
                    status = _nodeBehaviour.treeInfo.status;
                    tickCount = _nodeBehaviour.treeInfo.tickCount;
                }
                DrawCreateNodeContent(contentRect, node, (x) =>
                {
                    node = x;
                    _nodeBehaviour.name = node.name;
                    _nodeProp.isExpanded = true;
                }, _nodeBehaviour, status, tickCount, serializedObject, "node");
                var condRect = new Rect(contentRect.xMax + 3, rect.y, 20, EditorGUIUtility.singleLineHeight);
                _nodeBehaviour.condition.enable = EditorGUI.Toggle(condRect, _nodeBehaviour.condition.enable, EditorStyles.radioButton);
            }
            if (node != null)
            {
                var nodeName = $"{_nodeBehaviour.name} ({node.GetType().Name})";
                if (node.name != nodeName)
                    node.name = nodeName;
                EditorGUILayout.PropertyField(_nodeProp, GUIContent.none, true);
            }
            if (_nodeBehaviour.condition.enable)
            {
                var height = _conditionDrawer.GetHeight();
                var crect = GUILayoutUtility.GetRect(0, height);
                _conditionDrawer.OnGUI(crect);
            }
            serializedObject.ApplyModifiedProperties();
        }

        internal static void DrawCreateNodeContent<T>(Rect rect, T node, Action<T> onCreate, NodeBehaviour root, byte status, int tickCount, SerializedObject target = null, string propPath = null) where T : BaseNode
        {
            var nameRect = new Rect(rect.x, rect.y, rect.width - 50, rect.height);
            bool createTouched = false;
            if (node != null)
            {
                var iconX = nameRect.max.x + 5;
                BTree subTree = null;
                if (node is BTreeNode subNode)
                {
                    subTree = EditorApplication.isPlaying ? subNode.instaneTree : subNode.tree;
                    if (subTree)
                    {
                        var subRect = new Rect(nameRect.xMax - 20, nameRect.y, 20, EditorGUIUtility.singleLineHeight);
                        if (IconCacheUtil.DrawSubTree(subRect, subTree))
                            nameRect.width -= 25;
                    }
                }

                if (!string.IsNullOrEmpty(propPath))
                {
                    var propX = nameRect.max.x - 32;
                    nameRect.width -= 15;
                    if (GUI.Button(new Rect(propX, rect.y, 35, EditorGUIUtility.singleLineHeight), "", EditorStyles.objectFieldMiniThumb))
                    {
                        var prop = target.FindProperty(propPath);
                        PropDrawerWindow.Show(prop);
                        Event.current.Use();
                    }
                }

                var color = TreeInfoDrawer.GetNodeStateColor(node.Owner, status, tickCount);
                using (var colorScope = new ColorScope(node.Owner != null, color))
                {
                    if (root.node == node)
                    {
                        EditorGUI.SelectableLabel(nameRect, node.name, EditorStyles.textField);
                    }
                    else
                    {
                        node.name = EditorGUI.TextField(nameRect, node.name, EditorStyles.textField);
                    }
                }
                var iconRect = new Rect(iconX, rect.y, 20, EditorGUIUtility.singleLineHeight);
                IconCacheUtil.DrawIcon(iconRect, node);
            }
            else
            {
                using (var colorScope = new ColorScope(true, Color.red))
                {
                    if (GUI.Button(nameRect, "Null", EditorStyles.textField))
                    {
                        createTouched = true;
                    }
                }
            }
            var createRect = new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height);
            if (createTouched || GUI.Button(createRect, "", EditorStyles.popup))
            {
                List<BaseNode> customNodes = new List<BaseNode>();
                CollectNodesDeepth(GetRootBehaviour(root), customNodes);
                CreateNodeWindow.Show(Event.current.mousePosition, (node) =>
                {
                    onCreate?.Invoke(node as T);
                }, typeof(T), customNodes);
            }
        }

        public static NodeBehaviour GetRootBehaviour(NodeBehaviour info)
        {
            var parent = info.transform.parent;
            if (parent)
            {
                var parentNode = parent.GetComponent<NodeBehaviour>();
                if (parentNode)
                    return GetRootBehaviour(parentNode);
            }
            return info;
        }

        public static void CollectNodesDeepth(NodeBehaviour info, List<BaseNode> nodes)
        {
            if (info.node && !nodes.Contains(info.node))
            {
                nodes.Add(info.node);
            }
            if (info.condition != null && info.condition.conditions != null)
            {
                int i = 0;
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.node && !nodes.Contains(condition.node))
                    {
                        nodes.Add(condition.node);
                    }

                    if (condition.subConditions != null)
                    {
                        int j = 0;
                        foreach (var subNode in condition.subConditions)
                        {
                            if (subNode != null && subNode.node && !nodes.Contains(subNode.node))
                            {
                                nodes.Add(subNode.node);
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            if (info.transform.childCount > 0)
            {
                for (int i = 0; i < info.transform.childCount; i++)
                {
                    var item = info.transform.GetChild(i).GetComponent<NodeBehaviour>();
                    CollectNodesDeepth(item, nodes);
                }
            }
        }

        public static void DrawNodeState(BaseNode node, int tickCount, byte status, Rect rect)
        {
            var color = Color.gray;
            var show = false;
            var ownerTickCount = 0;
            if (node && node.Owner != null)
                ownerTickCount = node.Owner.TickCount;

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
            if (ownerTickCount != tickCount)
                color *= 0.5f;
            if (show)
            {
                using (var colorScope = new ColorGUIScope(true, color))
                {
                    GUI.Box(rect, "");
                }
            }
        }

    }
}
