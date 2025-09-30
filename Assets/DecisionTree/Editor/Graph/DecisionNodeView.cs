using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace UFrame.Decision
{
    public class DecisionNodeView
    {
        private int _windowId;
        private DecisionTree _tree;
        private DecisionTreeNode _rootInfo;
        private DecisionTreeNode _info;
        private DecisionTreeNode _parentInfo;
        private Vector2 _position;
        private Rect _rect;
        public ref Rect Rect => ref _rect;
        public DecisionTreeNode Info => _info;
        private DecisionConditionDrawer _conditionDrawer;
        private DecisionResultDrawer _resultDrawer;
        private bool _rectInited;
        private Vector2 _offset;
        public const int WIDTH = 300;
        public const int HEIGHT = 60;
        public const int MAX_HEIGHT = 200;
        private bool _forceAddChild;
        public Action onReload { get; set; }
        public Action<DecisionNodeView> onActive { get; set; }
        public Action<Vector2> onStartConnect { get; set; }
        public Func<DecisionTreeNode, Vector2> posFunc { get; set; }
        public Action<DecisionTreeNode> onNodeChanged { get; set; }
        // 添加节点类型的颜色定义
        private static readonly Color rootNodeColor = new Color(0.2f, 0.2f, 0.4f);
        private static readonly Color leafNodeColor = new Color(0.6f, 0.4f, 0.2f);
        private static readonly Color selectNodeColor = new Color(0.6f, 0.3f, 0.3f);
        // 添加样式缓存
        private static GUIStyle _nodeStyle;
        private static GUIStyle _nodeTitleStyle;
        private static GUIStyle _nodeContentStyle;
        private static GUIStyle _rightAlignLabelStyle;
        private static GUIStyle _grayLableStyle;

        public DecisionNodeView(DecisionTree bTree, DecisionTreeNode rootInfo, DecisionTreeNode info, Vector2 pos)
        {
            this._tree = bTree;
            this._rootInfo = bTree.rootNode;
            this._info = info;
            this._position = pos;
            this._windowId = info.GetHashCode();
            _conditionDrawer = new DecisionConditionDrawer();
            ReloadResultDrawer();
        }

        private void ReloadResultDrawer()
        {
            if (_rootInfo is DecisionLeafNode leaf)
                _resultDrawer = new DecisionResultDrawer(leaf);
            else
                _resultDrawer = null;
        }

        internal void DrawNode(Rect contentRect)
        {
            if (!_rectInited)
            {
                var pos = _position;
                _rect = new Rect(pos.x, pos.y, WIDTH, GetHeight());
                _offset = new Vector2(contentRect.center.x, HEIGHT);
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
        // 获取节点颜色
        private Color GetNodeColor()
        {
            if (_info == null) return Color.gray;

            var nodeType = _info.GetType();
            Color baseColor;

            if (typeof(DecisionRootNode).IsAssignableFrom(nodeType))
                baseColor = rootNodeColor;
            else if (typeof(DecisionLeafNode).IsAssignableFrom(nodeType))
                baseColor = leafNodeColor;
            else if (typeof(DecisionSelectNode).IsAssignableFrom(nodeType))
                baseColor = selectNodeColor;
            else
                baseColor = Color.gray;
            return baseColor;
        }
        private float GetHeight()
        {
            float height = HEIGHT;
            if (_info is DecisionLeafNode)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            else if (Info is DecisionRootNode)
            {
                height -= EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        private DecisionTreeNode FindParentNode(DecisionTreeNode info, DecisionTreeNode match)
        {
            if (info.Children == null)
                return null;

            if (info.Children.Contains(match))
            {
                return info;
            }
            else
            {
                foreach (var subInfo in info.Children)
                {
                    var parent = FindParentNode(subInfo as DecisionTreeNode, match);
                    if (parent != null)
                    {
                        return parent;
                    }
                }
            }
            return null;
        }

        public DecisionTreeNode RefreshParentNode()
        {
            if (_rootInfo == _info)
                return null;

            if (_parentInfo == null || _parentInfo.Children == null || !_parentInfo.Children.Contains(_info))
            {
                _parentInfo = FindParentNode(_rootInfo, _info);
            }
            return _parentInfo;
        }

        private bool CheckNeedGreen(DecisionTreeNode info)
        {
            if (EditorApplication.isPlaying)
                return false;

            return false;
        }

        public void OnNodeChanged(DecisionTreeNode node)
        {
            if (_info != null)
            {
                RecordUndo("node changed!");
                _info.CopyTo(node);
                _info = node;
                ReloadResultDrawer();
                onNodeChanged?.Invoke(_info);
            }
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

            if (_rootInfo != _info)
            {
                var upPortRect = new Rect(2, rect.height - 14, 12, 12);
                EditorGUI.Toggle(upPortRect, true, EditorStyles.toggle);
            }
            var typeRect = new Rect(rect.x + 2, rect.y, rect.width - 4, EditorGUIUtility.singleLineHeight);

            var typeName = _info?.GetType()?.Name;
            if (_info is DecisionRootNode rootNode)
                typeName = rootNode.name;
            EditorGUI.LabelField(typeRect, typeName?.ToString(), _rightAlignLabelStyle);

            RefreshParentNode();
            var needGreen = CheckNeedGreen(_info);
            var nodeRect = new Rect(5, 20, _rect.width - 10, EditorGUIUtility.singleLineHeight);

            if (_info is not DecisionRootNode)
            {
                using (var colorScope = new ColorScope(needGreen, Color.green))
                {
                    nodeRect = DecisionTreeNodeDrawer.DrawCreateNodeContent(nodeRect, _info, OnNodeChanged);
                }
                float bothHeight = EditorGUIUtility.singleLineHeight + 8;
                float halfWidth = (_rect.width - 20) / 2f - 2f;
                _conditionDrawer.OnGUI(nodeRect, _info);
            }

            if (_info is DecisionSelectNode selectNode)
            {
                var descRect = new Rect(10, rect.height - 20, rect.width - 20, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(descRect, selectNode.question + "?", _grayLableStyle);
            }

            if (_resultDrawer == null && _info is DecisionLeafNode leafNode)
                _resultDrawer = new DecisionResultDrawer(leafNode);

            if (_resultDrawer != null)
            {
                var resultRect = nodeRect;
                resultRect.y += EditorGUIUtility.singleLineHeight + 5;
                _resultDrawer.OnGUI(resultRect);
            }

            if (_info.Children != null)
            {
                var downPortRect = new Rect(_rect.width * 0.5f - 6, _rect.height - 16, 20, 20);
                TryStartDrawLine(downPortRect);
                EditorGUI.Toggle(downPortRect, true, EditorStyles.radioButton);
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
        private void ReorderSubTrees(DecisionTreeNode parent)
        {
            if (parent.Children == null || parent.Children.Count <= 1)
                return;

            var beforeIndex = parent.Children.IndexOf(Info);
            // 根据X坐标排序
            parent.Children.Sort((a, b) =>
            {
                var posA = posFunc(a as DecisionTreeNode);
                var posB = posFunc(b as DecisionTreeNode);
                return posA.x.CompareTo(posB.x);
            });
            var afterIndex = parent.Children.IndexOf(Info);
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
                _info.Children?.ForEach(sub =>
                {
                    _parentInfo.Children.Add(sub);
                });
                _info.Children?.Clear();
                _parentInfo.Children.Remove(_info);
                onReload?.Invoke();
            }
        }

        private void AddCopyChild(DecisionTreeNode copyFrom)
        {
            var childInfo = System.Activator.CreateInstance(copyFrom.GetType()) as DecisionTreeNode;
            copyFrom.CopyTo(childInfo);
            _info.Children.Add(childInfo);
            onReload.Invoke();
        }

        private void AddCopySibling(DecisionTreeNode copyFrom)
        {
            var parent = RefreshParentNode();
            if (parent == null || parent.Children == null)
                return;

            var childInfo = System.Activator.CreateInstance(copyFrom.GetType()) as DecisionTreeNode;
            copyFrom.CopyTo(childInfo);
            int index = parent.Children.IndexOf(_info);
            if (index >= 0)
                parent.Children.Insert(index + 1, childInfo);
            else
                parent.Children.Add(childInfo);

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
                    if (_info != null && _info != null && !(_info is DecisionSelectNode) && Event.current.control)
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
                CopyPasteUtil.copyNode = _info;
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.X && Event.current.type == EventType.KeyDown)
            {
                CopyPasteUtil.copyNode = _info;
                DeleteSelf();
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.V && Event.current.type == EventType.KeyDown)
            {
                if (CopyPasteUtil.copyNode != null)
                {
                    RecordUndo("add child");
                    AddCopyChild(CopyPasteUtil.copyNode);
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

        }


        private void RecordUndo(string message)
        {
            Undo.RecordObject(_tree, message);
        }

        public void CreateAndAddChild()
        {
            DecisionTreeNodeDrawer.CreateDecisionTreeNode((childInfo) =>
             {
                 for (int i = 0; i < _info.Children.Count; i++)
                 {
                     var pos = posFunc(_info.Children[i]);
                     if (pos.x + WIDTH * 0.5f > _position.x)
                     {
                         _info.Children.Insert(i, childInfo);
                         break;
                     }
                 }
                 Debug.LogError("childInfo:" + childInfo);
                 if (!_info.Children.Contains(childInfo))
                     _info.Children.Add(childInfo);
                 onReload.Invoke();
             });
        }

        // 创建背景纹理
        private static Texture2D CreateBackgroundTexture()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return tex;
        }

        private void InsertEmptyNodeBetweenParent()
        {
            var parent = RefreshParentNode();
            if (parent == null || parent.Children == null)
                return;

            // 创建空节点
            var emptyInfo = new DecisionSelectNode();

            int index = parent.Children.IndexOf(_info);
            if (index >= 0)
            {
                // 1. 移除当前节点
                parent.Children.RemoveAt(index);
                // 2. 插入空节点
                parent.Children.Insert(index, emptyInfo);
                emptyInfo.Children.Add(_info);
            }
            else
            {
                // 如果找不到，直接加到末尾
                parent.Children.Add(emptyInfo);
                emptyInfo.Children.Add(_info);
            }

            onReload?.Invoke();
        }

        private void PromoteChildrenAndDeleteSelf()
        {
            var parent = RefreshParentNode();
            if (parent == null || parent.Children == null)
                return;

            int index = parent.Children.IndexOf(_info);
            if (index < 0)
                return;

            // 1. 将当前节点的所有子节点插入到父节点的 Children 中，插入到当前节点位置
            if (_info.Children != null && _info.Children.Count > 0)
            {
                parent.Children.InsertRange(index, _info.Children);
            }

            // 2. 移除当前节点
            parent.Children.RemoveAt(index + (_info.Children?.Count ?? 0)); // 因为前面插入了子节点，index要后移
            onReload?.Invoke();
        }
    }
}
