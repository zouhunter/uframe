using UnityEngine;
using UnityEditor;
using System;
using UFrame.BehaviourTree;
using System.Reflection;
using UFrame.BehaviourTree.Actions;
using System.Collections.Generic;

namespace UFrame.HTN.Editor.Graph
{
    public class PlanNodeView
    {
        private int _windowId;
        private NTree _tree;
        private PlanNode _info;
        private PlanNode _parentInfo;
        private Vector2 _position;
        private Rect _rect;
        public ref Rect Rect => ref _rect;
        public PlanNode Info => _info;
        private NTreeConditionDrawer _conditionDrawer;
        private bool _rectInited;
        private Vector2 _offset;
        public const int WIDTH = 300;
        public const int MIN_HEIGHT = 60;
        public const int MAX_HEIGHT = 250;
        public ref Vector2 Pos => ref _position;
        private bool _forceAddChild;
        public Action onReload { get; set; }
        public Action<PlanNodeView> onActive { get; set; }
        public Action<Vector2> onStartConnect { get; set; }
        public Func<TaskInfo, Vector2> posFunc { get; set; }

        public List<PlanNodeView> predecessors = new List<PlanNodeView>();
        public List<PlanNodeView> successors = new List<PlanNodeView>();

        // 添加节点类型的颜色定义
        private static readonly Color CompositeNodeColor = new Color(0.2f, 0.2f, 0.4f);
        private static readonly Color ActionNodeColor = new Color(0.6f, 0.4f, 0.2f);
        private static readonly Color DecoratorNodeColor = new Color(0.3f, 0.45f, 0.6f);
        private static readonly Color ConditionNodeColor = new Color(0.6f, 0.3f, 0.3f);
        private static readonly Color subNodeColor = new Color(0.2f, 0.3f, 0.1f);

        // 添加样式缓存
        private static GUIStyle _nodeStyle;
        private static GUIStyle _nodeTitleStyle;
        private static GUIStyle _nodeContentStyle;
        private static GUIStyle _rightAlignLabelStyle;
        private static GUIStyle _grayLableStyle;

        public PlanNodeView(NTree bTree, PlanNode info)
        {
            this._tree = bTree;
            this._info = info;
            this._windowId = info.GetHashCode();
            _conditionDrawer = new NTreeConditionDrawer(_tree, info, null);
        }

        internal void DrawNode(Rect contentRect)
        {
            if (!_rectInited)
            {
                var pos = _position;
                _rect = new Rect(pos.x, pos.y, WIDTH, GetHeight());
                _offset = new Vector2(contentRect.center.x, MAX_HEIGHT);
                _rect.position = _rect.position + _offset;
                _rectInited = true;
            }
            _rect.height = GetHeight();
            _rect = GUI.Window(_windowId, _rect, DrawThisNode, "");
        }

        // 初始化样式
        private static void InitStyles()
        {
            if (_nodeStyle == null)
            {
                _nodeStyle = new GUIStyle();
                _nodeStyle.normal.background = CreateBackgroundTexture();
                _nodeStyle.border = new RectOffset(10, 10, 10, 10);
                _nodeStyle.padding = new RectOffset(10, 10, 10, 10);
                _nodeStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (_nodeTitleStyle == null)
            {
                _nodeTitleStyle = new GUIStyle(EditorStyles.boldLabel);
                _nodeTitleStyle.alignment = TextAnchor.MiddleCenter;
                _nodeTitleStyle.fontSize = 12;
                _nodeTitleStyle.normal.textColor = Color.white;
            }

            if (_nodeContentStyle == null)
            {
                _nodeContentStyle = new GUIStyle(EditorStyles.helpBox);
                _nodeContentStyle.padding = new RectOffset(5, 5, 5, 5);
                _nodeContentStyle.fontSize = 11;
            }
            if (_rightAlignLabelStyle == null)
            {
                _rightAlignLabelStyle = new GUIStyle(EditorStyles.whiteBoldLabel);
                _rightAlignLabelStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (_grayLableStyle == null)
            {
                _grayLableStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                _grayLableStyle.alignment = TextAnchor.MiddleRight;
            }
        }
        private float GetHeight()
        {
            float height = MIN_HEIGHT;
            // checks/effects区域高度
            height += EditorGUIUtility.singleLineHeight + 8;
            if (_info.condition.enable)
            {
                var conditionHeight = _conditionDrawer.GetHeight();
                height += conditionHeight;
            }
            return height;
        }

        private PlanNode FindParentNode(PlanNode info, PlanNode match)
        {
            if (info.subTrees == null)
                return null;

            if (info.subTrees.Contains(match))
            {
                return info;
            }
            else
            {
                foreach (var subInfo in info.subTrees)
                {
                    var parent = FindParentNode(subInfo as PlanNode, match);
                    if (parent != null)
                    {
                        return parent;
                    }
                }
            }
            return null;
        }

        public TaskInfo RefreshParentNode()
        {
            return _parentInfo;
        }

        private bool CheckNeedGreen(TaskInfo info)
        {
            if (EditorApplication.isPlaying)
                return false;

            return false;
        }

        private void DrawThisNode(int id)
        {
            var rect = new Rect(0, 0, _rect.width, _rect.height);
            InitStyles();

            // 绘制节点背景
            var nodeColor = GetNodeColor();
            GUI.backgroundColor = nodeColor;
            GUI.Box(new Rect(0, 0, _rect.width, EditorGUIUtility.singleLineHeight), "", _nodeStyle);
            GUI.backgroundColor = Color.white;

            // 绘制标题区域
            var titleRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            var titleBgRect = titleRect;
            titleBgRect.height += 2;
            EditorGUI.DrawRect(titleBgRect, nodeColor * 0.8f);

            var upPortRect = new Rect(2, rect.height - 14, 12, 12);
            _info.enable = EditorGUI.Toggle(upPortRect, _info.enable, EditorStyles.toggle);

            var typeRect = new Rect(rect.x + 2, rect.y, rect.width - 4, EditorGUIUtility.singleLineHeight);

            var attr = _info.node?.GetType()?.GetCustomAttribute<NodePathAttribute>();
            var typeName = _info.node?.GetType()?.Name;
            if (attr != null)
                typeName = attr.desc;
            EditorGUI.LabelField(typeRect, typeName?.ToString(), _rightAlignLabelStyle);

            RefreshParentNode();
            //DrawMoveLeftRight(rect);
            var needGreen = CheckNeedGreen(_info);

            using (var disableScope = new EditorGUI.DisabledScope(!_info.enable))
            {
                var nodeRect = new Rect(5, 20, _rect.width - 40, EditorGUIUtility.singleLineHeight);
                using (var colorScope = new ColorScope(needGreen, Color.green))
                {
                    TaskInfoDrawer.DrawCreateNodeContent(nodeRect, _info.node, n =>
                    {
                        RecordUndo("node changed!");
                        _info.node = n;
                    }, _tree, _info.status, _info.tickCount);
                }
                var conditionEnableRect = new Rect(nodeRect.x + nodeRect.width + 10, nodeRect.y, 20, 20);
                _info.condition.enable = EditorGUI.Toggle(conditionEnableRect, _info.condition.enable, EditorStyles.radioButton);

                // checks/effects区域，左右分栏，数量显示
                float bothHeight = EditorGUIUtility.singleLineHeight + 8;
                float halfWidth = (_rect.width - 20) / 2f - 2f;
                var checkRect = new Rect(10, nodeRect.yMax + 4, halfWidth, bothHeight);
                var effectRect = new Rect(10 + halfWidth + 4f, nodeRect.yMax + 4, halfWidth, bothHeight);
                GUI.Box(checkRect, "");
                GUI.Box(effectRect, "");
                EditorGUI.LabelField(new Rect(checkRect.x + 4, checkRect.y + 2, checkRect.width - 8, 18), $"Checks: {_info.checks?.Count ?? 0}", EditorStyles.miniBoldLabel);
                EditorGUI.LabelField(new Rect(effectRect.x + 4, effectRect.y + 2, effectRect.width - 8, 18), $"Effects: {_info.effects?.Count ?? 0}", EditorStyles.miniBoldLabel);
                // 预留后续详细编辑入口

                if (_info.condition.enable)
                {
                    var height = _conditionDrawer.GetHeight();
                    var conditonRect = new Rect(nodeRect.x + 10, effectRect.yMax + 4, _rect.width - 20, height);
                    _conditionDrawer.OnGUI(conditonRect);
                }
                if ((_info.subTrees != null && _info.subTrees.Count > 0) || (_info.node && _info.node is ParentNode) || _forceAddChild)
                {
                    var downPortRect = new Rect(_rect.width * 0.5f - 6, _rect.height - 16, 20, 20);
                    TryStartDrawLine(downPortRect);
                    EditorGUI.Toggle(downPortRect, true, EditorStyles.radioButton);
                }
                var descRect = new Rect(10, rect.height - 20, rect.width - 20, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(descRect, _info.desc, _grayLableStyle);
            }
            if (rect.Contains(Event.current.mousePosition))
            {
                ProcessEvents();
                var dragRect = new Rect(0, 0, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.MoveArrow);
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                OnEndDrag();
            }
        }

        protected virtual void OnEndDrag()
        {
            // 获取父节点
            if (_parentInfo != null)
            {
                ReorderSubTrees(_parentInfo);
            }
        }

        // 重新排序子树
        private void ReorderSubTrees(TaskInfo parent)
        {
            if (parent.subTrees == null || parent.subTrees.Count <= 1)
                return;

            var beforeIndex = parent.subTrees.IndexOf(Info);
            // 根据X坐标排序
            parent.subTrees.Sort((a, b) =>
            {
                var posA = posFunc(a as TaskInfo);
                var posB = posFunc(b as TaskInfo);
                return posA.x.CompareTo(posB.x);
            });
            var afterIndex = parent.subTrees.IndexOf(Info);
            if (beforeIndex != afterIndex)
            {
                onReload?.Invoke();
            }
        }

        private void TryStartDrawLine(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    onActive?.Invoke(this);
                    onStartConnect?.Invoke(_rect.position + rect.center);
                }
            }
        }

        private void DeleteSelf()
        {
            if (_parentInfo != null)
            {
                RecordUndo("delete child!");
                _info.subTrees?.ForEach(sub =>
                {
                    _parentInfo.subTrees.Add(sub);
                });
                _info.subTrees = new System.Collections.Generic.List<TreeInfo>();
                _parentInfo.subTrees.Remove(_info);
                onReload?.Invoke();
            }
        }

        private void AddCopyChild(TaskInfo copyFrom)
        {
            var childInfo = TaskInfo.Create();
            CopyPasteUtil.CopyTreeInfo(copyFrom, childInfo, childInfo);
            _info.subTrees.Add(childInfo);
            onReload.Invoke();
        }

        private void AddCopySibling(TaskInfo copyFrom)
        {
            var parent = RefreshParentNode();
            if (parent == null || parent.subTrees == null)
                return;

            var siblingInfo = TaskInfo.Create();
            CopyPasteUtil.CopyTreeInfo(copyFrom, siblingInfo, siblingInfo);

            int index = parent.subTrees.IndexOf(_info);
            if (index >= 0)
                parent.subTrees.Insert(index + 1, siblingInfo);
            else
                parent.subTrees.Add(siblingInfo);

            onReload?.Invoke();
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.clickCount == 2)
                {
                    // 双击
                    TryOpenSubTree();
                    Event.current.Use();
                }
                else
                {
                    if (_info != null && _info.node != null && !(_info.node is ParentNode) && Event.current.control)
                    {
                        _forceAddChild = !_forceAddChild;
                        Event.current.Use();
                    }
                    onActive?.Invoke(this);
                }
            }
            if (Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyUp)
            {
                DeleteSelf();
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.D && Event.current.type == EventType.KeyDown)
            {
                RecordUndo("double");
                AddCopySibling(_info);
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.C && Event.current.type == EventType.KeyDown)
            {
                CopyPasteUtil.copyedTreeInfo = _info;
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.X && Event.current.type == EventType.KeyDown)
            {
                CopyPasteUtil.copyedTreeInfo = _info;
                DeleteSelf();
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.V && Event.current.type == EventType.KeyDown)
            {
                if (TaskCopyPasteUtil.copyedTaskInfo != null)
                {
                    RecordUndo("add child");
                    //AddCopyChild(TaskInfo.Create(TaskCopyPasteUtil.copyedTreeInfo));
                }
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.I && Event.current.type == EventType.KeyDown)
            {
                RecordUndo("insert empty node");
                InsertEmptyNodeBetweenParent();
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyDown)
            {
                RecordUndo("promote children and delete self");
                PromoteChildrenAndDeleteSelf();
            }
        }

        private void TryOpenSubTree()
        {
            // 假设你的行为树节点类型为BTreeNode
            if (_info.node is BTreeNode btreeNode && btreeNode.tree != null)
            {
                // 通知窗口切换到子树
                BTreeWindow window = EditorWindow.GetWindow<BTreeWindow>();
                var tree = btreeNode.instaneTree ?? btreeNode.tree;
                Debug.LogError(tree);
                window.SelectBTree(tree);
            }
        }

        private void RecordUndo(string message)
        {
            Undo.RecordObject(_tree, message);
        }

        public void CreateAndAddChild()
        {
            var customNodes = new List<BaseNode>();
            if (_tree)
                _tree.CollectNodesDeepth(_tree.rootTree, customNodes);
            var nodePos = Event.current.mousePosition;
            CreateNTNodeWindow.Show(Event.current.mousePosition, (node) =>
            {
                var childInfo = TaskInfo.Create();
                childInfo.enable = true;
                childInfo.node = node;
                if (_info.subTrees == null)
                    _info.subTrees = new List<TreeInfo>();

                for (int i = 0; i < _info.subTrees.Count; i++)
                {
                    var pos = posFunc(_info.subTrees[i] as TaskInfo);
                    if (pos.x + WIDTH * 0.5f > nodePos.x)
                    {
                        _info.subTrees.Insert(i, childInfo);
                        break;
                    }
                }
                if (!_info.subTrees.Contains(childInfo))
                    _info.subTrees.Add(childInfo);
                onReload.Invoke();
            }, typeof(BaseNode), customNodes);
        }

        // 创建背景纹理
        private static Texture2D CreateBackgroundTexture()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return tex;
        }

        // 获取节点颜色
        private Color GetNodeColor()
        {
            if (_info?.node == null) return Color.gray;

            var nodeType = _info.node.GetType();
            Color baseColor;

            if (typeof(BTreeNode).IsAssignableFrom(nodeType))
                baseColor = subNodeColor;
            else if (typeof(ConditionNode).IsAssignableFrom(nodeType))
                baseColor = ConditionNodeColor;
            else if (typeof(CompositeNode).IsAssignableFrom(nodeType))
                baseColor = CompositeNodeColor;
            else if (typeof(DecoratorNode).IsAssignableFrom(nodeType))
                baseColor = DecoratorNodeColor;
            else if (typeof(ActionNode).IsAssignableFrom(nodeType))
                baseColor = ActionNodeColor;
            else
                baseColor = Color.gray;

            // 如果节点禁用，降低颜色亮度
            if (!_info.enable)
                baseColor = Color.Lerp(baseColor, Color.gray, 0.5f);

            return baseColor;
        }

        private void InsertEmptyNodeBetweenParent()
        {
            var parent = RefreshParentNode();
            if (parent == null || parent.subTrees == null)
                return;

            // 创建空节点
            var emptyInfo = TaskInfo.Create();
            emptyInfo.enable = true;
            emptyInfo.node = null; // 或 new SomeDefaultNode();

            int index = parent.subTrees.IndexOf(_info);
            if (index >= 0)
            {
                // 1. 移除当前节点
                parent.subTrees.RemoveAt(index);
                // 2. 插入空节点
                parent.subTrees.Insert(index, emptyInfo);
                // 3. 让当前节点成为空节点的唯一子节点
                if (emptyInfo.subTrees == null)
                    emptyInfo.subTrees = new List<TreeInfo>();
                emptyInfo.subTrees.Add(_info);
            }
            else
            {
                // 如果找不到，直接加到末尾
                parent.subTrees.Add(emptyInfo);
                if (emptyInfo.subTrees == null)
                    emptyInfo.subTrees = new List<TreeInfo>();
                emptyInfo.subTrees.Add(_info);
            }

            onReload?.Invoke();
        }

        private void PromoteChildrenAndDeleteSelf()
        {
            var parent = RefreshParentNode();
            if (parent == null || parent.subTrees == null)
                return;

            int index = parent.subTrees.IndexOf(_info);
            if (index < 0)
                return;

            // 1. 将当前节点的所有子节点插入到父节点的 subTrees 中，插入到当前节点位置
            if (_info.subTrees != null && _info.subTrees.Count > 0)
            {
                parent.subTrees.InsertRange(index, _info.subTrees);
            }

            // 2. 移除当前节点
            parent.subTrees.RemoveAt(index + (_info.subTrees?.Count ?? 0)); // 因为前面插入了子节点，index要后移
            onReload?.Invoke();
        }
    }
}
