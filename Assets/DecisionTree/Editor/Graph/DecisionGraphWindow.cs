using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEngine.UIElements;

namespace UFrame.Decision
{
    public class DecisionGraphWindow : EditorWindow
    {
        [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
        public static bool OnOpen(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is DecisionTree tree)
            {
                OpenWindow(tree);
                return true;
            }
            return false;
        }

        private DecisionTree nTree;
        private ScrollViewContainer _scrollViewContainer;
        private GraphBackground _background;
        private Rect _graphRegion;
        private List<DecisionNodeView> _nodes;
        private List<KeyValuePair<DecisionNodeView, DecisionNodeView>> _connections;
        private Rect _infoRegion;
        private bool _inConnect;
        private Vector2 _startConnectPos;
        private DecisionNodeView _activeNodeView;
        private SerializedObject serializedObject;
        private Vector2 _variableScroll;
        private Rect _scrollContentSize;
        private float _splitRatio = 0.7f;
        private bool _isResizing;
        private List<DecisionNodeView> _selectedNodes = new List<DecisionNodeView>();
        private bool _isSelecting = false;
        private Vector2 _selectionStartPos;
        private Rect _selectionRect;
        // 添加选择框的样式
        private GUIStyle _selectionBoxStyle;
        private long _enableTime;

        // 缓存纹理以提高性能
        private Dictionary<Color, Texture2D> _colorTextures = new Dictionary<Color, Texture2D>();
        private GUIStyle _iconButtonStyle;
        private Texture2D _hoverTex;
        public DecisionTreeNode _activeTaskInfo;

        private Dictionary<DecisionTreeNode, DecisionConditionDrawer> _checkDrawerCache = new Dictionary<DecisionTreeNode, DecisionConditionDrawer>();
        private Dictionary<DecisionTreeNode, DecisionResultDrawer> _effectDrawerCache = new Dictionary<DecisionTreeNode, DecisionResultDrawer>();

        private void OnEnable()
        {
            _background = new GraphBackground("UI/Default");
            _graphRegion = position;
            _scrollViewContainer = new ScrollViewContainer();
            _scrollViewContainer.Start(rootVisualElement, GetGraphRegion());
            _scrollViewContainer.onGUI = DrawScrollGraphContent;
            RefreshGraphRegion();
            LoadNodeInfos();
            InitStyles();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            // 清理缓存的纹理
            foreach (var texture in _colorTextures.Values)
            {
                DestroyImmediate(texture);
            }
            _colorTextures.Clear();

            // 原有的OnDisable代码
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }
        private void OnUndoRedoPerformed()
        {
            LoadNodeInfos();
        }

        private void InitStyles()
        {
            if (_selectionBoxStyle == null)
            {
                _selectionBoxStyle = new GUIStyle();
                _selectionBoxStyle.normal.background = EditorGUIUtility.whiteTexture;
                _selectionBoxStyle.border = new RectOffset(1, 1, 1, 1);
            }
            if (_iconButtonStyle == null)
            {
                _iconButtonStyle = new GUIStyle(GUI.skin.button);
                _iconButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                _iconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                _iconButtonStyle.fixedWidth = 0;
                _iconButtonStyle.fixedHeight = 0;
                _iconButtonStyle.normal.background = null;
                _iconButtonStyle.hover.background = GetHoverTex();
                _iconButtonStyle.active.background = GetHoverTex();
                _iconButtonStyle.normal.textColor = Color.white;
                _iconButtonStyle.hover.textColor = Color.cyan;
                _iconButtonStyle.active.textColor = Color.yellow;
            }
        }

        private Rect GetGraphRegion()
        {
            var graphRegion = new Rect(0, EditorGUIUtility.singleLineHeight, position.width * _splitRatio, position.height - 2 * EditorGUIUtility.singleLineHeight);
            return graphRegion;
        }

        private void LoadNodeInfos()
        {
            _nodes = new List<DecisionNodeView>();
            _connections = new List<KeyValuePair<DecisionNodeView, DecisionNodeView>>();
            if (nTree != null)
            {
                var rootTree = nTree.rootNode;
                var posMap = TreeNodeLayout.GetNodePositions(rootTree, DecisionNodeView.WIDTH + 10, DecisionNodeView.MAX_HEIGHT);
                CreateViewDeepth(nTree, rootTree, rootTree, posMap, _nodes, null);
                serializedObject = new SerializedObject(nTree);
            }
            Repaint();
        }

        private DecisionNodeView CreateViewDeepth(DecisionTree bTree, DecisionTreeNode rootInfo, DecisionTreeNode info, Dictionary<DecisionTreeNode, Vector2Int> posMap, List<DecisionNodeView> _nodes, Action<DecisionTreeNode> onNodeChanged)
        {
            var view = new DecisionNodeView(bTree, rootInfo as DecisionTreeNode, info as DecisionTreeNode, posMap[info]);
            view.onReload = LoadNodeInfos;
            view.onStartConnect = OnStartConnect;
            view.posFunc = OnGetNodePos;
            view.onActive = OnSetActiveView;
            view.onNodeChanged = onNodeChanged;
            this._nodes.Add(view);

            if (info.Children != null && info.Children.Count > 0)
            {
                for (var i = 0; i < info.Children.Count; ++i)
                {
                    var child = info.Children[i];
                    var index = i;
                    var childView = CreateViewDeepth(bTree, rootInfo, child, posMap, _nodes, (newNode) =>
                    {
                        if (info != null && info.Children != null)
                        {
                            info.Children[index] = newNode;
                        }
                    });
                    _connections.Add(new KeyValuePair<DecisionNodeView, DecisionNodeView>(view, childView));
                }
            }
            return view;
        }
        private Vector2 OnGetNodePos(DecisionTreeNode info)
        {
            foreach (var node in _nodes)
            {
                if (node.Info == info)
                    return node.Rect.position;
            }
            return Vector2.zero;
        }

        private void OnSetActiveView(DecisionNodeView nodeView)
        {
            if (!_selectedNodes.Contains(nodeView))
                _selectedNodes.Add(nodeView);
            _activeNodeView = nodeView;
            _activeTaskInfo = _activeNodeView.Info;
        }

        private void OnStartConnect(Vector2 pos)
        {
            _startConnectPos = pos;
            _inConnect = true;
        }

        private int GetSubtreeWidth(DecisionTreeNode node)
        {
            int horizontalSpacing = 10; // 水平间距
            int nodeWidth = DecisionNodeView.WIDTH; // 节点宽度
            if (node.Children == null || node.Children.Count == 0)
            {
                return nodeWidth; // 节点宽度
            }

            int width = 0;
            foreach (var child in node.Children)
            {
                width += GetSubtreeWidth(child) + horizontalSpacing; // 调整后的水平间距
            }

            return width - horizontalSpacing;
        }

        public void SelectBTree(DecisionTree tree)
        {
            this.nTree = tree;
            LoadNodeInfos();
            base.titleContent = new GUIContent(tree.name);
            EditorPrefs.SetInt("DecisionGraphWindow.bTree", tree.GetInstanceID());
        }

        private void OnGUI()
        {
            DrawVerticalLine();
            DrawSelectTree();
            _background?.Draw(_graphRegion, _scrollViewContainer.scrollOffset, _scrollViewContainer.ZoomSize);
            DrawInformations();
            DrawFootPrint();
        }

        private void DrawScrollGraphContent()
        {
            RefreshGraphRegion();
            DrawNodeGraphContent();
        }

        private void DrawVerticalLine()
        {
            float leftWidth = position.width * _splitRatio;
            var verticalSplitterRect = new Rect(leftWidth, 0, _isResizing ? 5 : 2, position.height);
            EditorGUI.DrawRect(verticalSplitterRect, _isResizing ? Color.gray : Color.black);
            verticalSplitterRect.width = 5;
            HandleSplitterDrag(verticalSplitterRect);
        }

        private void DrawSelectTree()
        {
            float barHeight = EditorGUIUtility.singleLineHeight;
            var titleBarRect = new Rect(0, 0, position.width, barHeight);
            EditorGUI.DrawRect(titleBarRect, new Color(0.2f, 0.2f, 0.2f));

            float buttonSize = barHeight - 2;
            float buttonSpacing = 4;
            float leftStartX = 10;
            float buttonY = 1;
            float x = leftStartX;
            float buttonWidth = buttonSize * 6.6f;
            var style = EditorStyles.toolbarButton;

            // 居中
            var centerIcon = EditorGUIUtility.IconContent("d_ViewToolMove");
            centerIcon.tooltip = "居中";
            if (GUI.Button(new Rect(x, buttonY, buttonWidth, buttonSize), new GUIContent("  居中", centerIcon.image, "居中"), style))
            {
                _scrollViewContainer?.ResetCenterOffset();
            }
            x += buttonWidth + buttonSpacing;

            // 刷新
            var refreshIcon = EditorGUIUtility.IconContent("d_Refresh");
            refreshIcon.tooltip = "刷新";
            if (GUI.Button(new Rect(x, buttonY, buttonWidth, buttonSize), new GUIContent("  刷新", refreshIcon.image, "刷新"), style))
            {
                LoadNodeInfos();
            }
            x += buttonWidth + buttonSpacing;

            // 保存
            var saveIcon = EditorGUIUtility.IconContent("d_SaveAs");
            saveIcon.tooltip = "保存";
            if (GUI.Button(new Rect(x, buttonY, buttonWidth, buttonSize), new GUIContent("  保存", saveIcon.image, "保存"), style))
            {
                if (nTree != null)
                {
                    EditorUtility.SetDirty(nTree);
                    AssetDatabase.SaveAssets();
                    ShowNotification(new GUIContent("保存成功"));
                }
            }
        }

        private Texture2D GetHoverTex()
        {
            if (_hoverTex == null)
            {
                _hoverTex = new Texture2D(1, 1);
                _hoverTex.SetPixel(0, 0, new Color(1, 1, 1, 0.15f));
                _hoverTex.Apply();
            }
            return _hoverTex;
        }

        private void RefreshGraphRegion()
        {
            var graphRegion = GetGraphRegion();
            bool regionChanged = _scrollViewContainer != null && _graphRegion != graphRegion;
            if (regionChanged)
            {
                _graphRegion = graphRegion;
                _scrollViewContainer.UpdateScale(graphRegion);
                _scrollViewContainer.Refesh();
                // 不要在这里ApplyOffset(true)，只ApplyOffset(false)
                _scrollViewContainer.ApplyOffset(false);
            }
            else
            {
                _scrollViewContainer?.ApplyOffset(false);
            }

            // 只在首次打开或特殊操作时才reset
            if (_enableTime == 0)
                _enableTime = (int)(DateTime.Now - DateTime.Today).TotalSeconds + 1;
            if (_enableTime > 0)
            {
                bool reset = _enableTime > (int)(DateTime.Now - DateTime.Today).TotalSeconds;
                if (reset)
                    _scrollViewContainer.ApplyOffset(true);
                else
                    _enableTime = -1;
            }
        }

        private void DrawConnections()
        {
            if (_connections == null)
                return;

            // 绘制连接线
            foreach (var connection in _connections)
            {
                var start = connection.Key;
                var end = connection.Value;
                DrawNodeCurve(start, end);
            }
        }

        void DrawNodeCurve(DecisionNodeView startView, DecisionNodeView endView)
        {
            if (_scrollViewContainer == null)
                return;
            var active = startView.Info != null && endView.Info != null;
            var start = new Vector2(startView.Rect.center.x + 2, startView.Rect.max.y - 2);
            var end = new Vector2(endView.Rect.center.x + 2, endView.Rect.min.y + 2);
            var color = Color.white;
            if (!active)
            {
                color = Color.gray;
            }
            else if (endView.Info.status == 1)
            {
                color = Color.green;
            }
            else if (endView.Info.status == 2)
            {
                color = Color.red;
            }
            else
            {
                color = Color.white;
            }
            DrawCurve(start, end, color);
        }

        void DrawCurve(Vector2 start, Vector2 end, Color color)
        {
            Handles.BeginGUI();
            Vector2 startTan = start + Vector2.up * 50; // 控制点向右延伸50像素
            Vector2 endTan = end + Vector2.down * 50; // 控制点向左延伸50像素
            Handles.DrawBezier(start, end, startTan, endTan, color, null, 5);
            Handles.EndGUI();
        }

        private void DrawNodeGraphContent()
        {
            var contentRect = new Rect(Vector2.zero, _graphRegion.size / _scrollViewContainer.minZoomSize);

            BeginWindows();
            _nodes?.ForEach(node =>
            {
                // 绘制选中状态
                if (_selectedNodes.Contains(node))
                {
                    var selRect = node.Rect;
                    selRect.x -= 2;
                    selRect.y -= 2;
                    selRect.width += 4;
                    selRect.height += 4;
                    EditorGUI.DrawRect(selRect, new Color(0.3f, 0.5f, 0.8f, 0.3f));
                }

                node.DrawNode(contentRect);
            });
            HandleDragNodes();
            EndWindows();

            // 处理选择框的事件
            HandleSelectionEvents(contentRect);

            // 绘制选择框
            if (_isSelecting)
            {
                Color selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.1f);
                EditorGUI.DrawRect(_selectionRect, selectionColor);
                Handles.color = new Color(0.3f, 0.5f, 0.8f, 1f);
                Handles.DrawPolyLine(_selectionRect.min,
                    new Vector2(_selectionRect.xMin, _selectionRect.yMax),
                    _selectionRect.max,
                    new Vector2(_selectionRect.xMax, _selectionRect.yMin),
                    _selectionRect.min);
            }

            DrawConnections();
        }

        private void HandleDragNodes()
        {
            if (_inConnect && Event.current.type == EventType.MouseUp)
            {
                _inConnect = false;
                DecisionNodeView targetView = null;
                foreach (var node in _nodes)
                {
                    if (node.Rect.Contains(Event.current.mousePosition))
                    {
                        targetView = node;
                        break;
                    }
                }
                if (targetView == null || targetView == _activeNodeView)
                {
                    RecordUndo("add child!");
                    _activeNodeView?.CreateAndAddChild();
                }
                else if (CheckAsChildAble(_activeNodeView.Info, targetView.Info))
                {
                    var oldParent = targetView.RefreshParentNode();
                    if (oldParent != null)
                    {
                        RecordUndo("modify parent!");
                        oldParent.Children.Remove(targetView.Info);
                        _activeNodeView.Info.Children.Add(targetView.Info);
                        LoadNodeInfos();
                    }
                }
            }

            if (_inConnect)
            {
                var start = _startConnectPos;
                var end = Event.current.mousePosition;
                DrawCurve(start, end, Color.cyan);
            }

            // 处理多选节点的拖动
            if (!_isSelecting && _selectedNodes.Count > 0 && Event.current.type == EventType.MouseDrag && Event.current.button == 0 && _activeNodeView != null)
            {
                foreach (var node in _selectedNodes)
                {
                    var rect = node.Rect;
                    rect.position += Event.current.delta;
                    node.Rect = rect;
                }
                Event.current.Use();
                Repaint();
            }
        }

        private bool CheckAsChildAble(DecisionTreeNode parent, DecisionTreeNode child)
        {
            if (parent == null || child == null)
                return false;

            if (child == parent) return false;

            if (child.Children != null && child.Children.Count > 0)
            {
                foreach (var subTree in child.Children)
                {
                    if (subTree == parent)
                        return false;
                    if (!CheckAsChildAble(parent, subTree))
                        return false;
                }
            }
            return true;
        }


        private void RecordUndo(string message)
        {
            Undo.RecordObject(nTree, message);
        }


        private void DrawFootPrint()
        {
            var region = new Rect(0, position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(region, "zouhangte@zht.cn,power by UFrame", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://alidocs.dingtalk.com/i/nodes/gvNG4YZ7Jne60YR3f2jYBoPyV2LD0oRE?doc_type=wiki_doc&orderType=SORT_KEY&rnd=0.48325224514433396&sortType=DESC");
            }
        }

        private void DrawTitleInfo(Rect contentRect)
        {
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredStyle.alignment = TextAnchor.MiddleLeft;
            centeredStyle.fontSize = 14; // 设置字体大小
            centeredStyle.richText = true;
            var name = nTree != null ? nTree.name : "(none)";
            var readMeRect = new Rect(contentRect.x + contentRect.width - 60, contentRect.y + 10, 60, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.LinkButton(readMeRect, "README"))
                Application.OpenURL("https://web-alidocs.dingtalk.com/i/nodes/QOG9lyrgJP3BQREofr345nLxVzN67Mw4?doc_type=wiki_doc");
            GUIContent content = new GUIContent($"Unity-DecisionTree <size=12><b><color=black>v1.0</color></b></size> <size=12><b>({name})</b></size>");
            if (GUI.Button(contentRect, content, centeredStyle) && nTree != null)
            {
                EditorGUIUtility.PingObject(nTree);
            }
            var lineRect = new Rect(contentRect.x, contentRect.y + contentRect.height, contentRect.width, 3);
            GUI.Box(lineRect, "");
        }


        private void DrawChecksAndEffects(DecisionTreeNode info, ref Rect rect)
        {
            if (info == null) return;
            if (!_checkDrawerCache.TryGetValue(info, out var checkDrawer))
            {
                checkDrawer = new DecisionConditionDrawer();
                _checkDrawerCache[info] = checkDrawer;
            }
            DecisionResultDrawer effectDrawer = null;
            if (info is DecisionLeafNode leaf && !_effectDrawerCache.TryGetValue(info, out effectDrawer))
            {
                effectDrawer = new DecisionResultDrawer(leaf);
                _effectDrawerCache[info] = effectDrawer;
            }
            var checkRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            var effectRect = checkRect;
            effectRect.y += EditorGUIUtility.singleLineHeight;
            checkDrawer.OnGUI(checkRect, info);
            effectDrawer?.OnGUI(effectRect);
        }

        private void DrawNodeProps(ref Rect rect)
        {
            serializedObject?.Update();
            var node = _activeTaskInfo;
            if (node != null)
            {
                // 标题获取逻辑
                string nodeTitle = null;
                var attr = node.GetType().GetCustomAttributes(typeof(NodePathAttribute), false).FirstOrDefault() as NodePathAttribute;
                if (attr != null && !string.IsNullOrEmpty(attr.desc))
                    nodeTitle = attr.desc;
                else
                    nodeTitle = node.GetType().Name;

                // 背景色
                var titleBgRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight + 4);
                EditorGUI.DrawRect(titleBgRect, new Color(0.2f, 0.4f, 0.7f, 0.18f));
                // 左侧竖线
                var lineRect = new Rect(rect.x, rect.y, 4, EditorGUIUtility.singleLineHeight + 4);
                EditorGUI.DrawRect(lineRect, new Color(0.2f, 0.5f, 1f, 0.8f));
                // 标题样式
                var firstTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new Color(0.9f, 0.95f, 1f) }
                };

                if (node is DecisionRootNode rootNode)
                {
                    rootNode.name = GUI.TextField(new Rect(rect.x + 8, rect.y + 2, rect.width - 8, EditorGUIUtility.singleLineHeight + 2), rootNode.name, firstTitleStyle);
                }
                else
                {
                    GUI.Label(new Rect(rect.x + 8, rect.y + 2, rect.width - 8, EditorGUIUtility.singleLineHeight + 2), nodeTitle, firstTitleStyle);
                }
                rect.y += EditorGUIUtility.singleLineHeight + 8;

                bool isRoot = _activeNodeView.Info is DecisionRootNode;
                // 第一个节点后绘制checks/effects
                if (_activeNodeView != null && !isRoot)
                {
                    DrawChecksAndEffects(_activeNodeView.Info, ref rect);
                }

                // 节点内容
                var nodeItemRect = rect;
                nodeItemRect.x += 20;
                nodeItemRect.width -= 20;
                if (_activeNodeView != null && !isRoot)
                {
                    DecisionTreeNodeDrawer.DrawCreateNodeContent(nodeItemRect, node, (x) =>
                     {
                         _activeNodeView.OnNodeChanged(node);
                     });
                }

                var endY = rect.y + rect.height;
                rect.y += 30;
                rect.height = EditorGUIUtility.singleLineHeight;
            }
            serializedObject?.ApplyModifiedProperties();
        }
        private void DrawInformations()
        {
            _infoRegion = new Rect(position.width * _splitRatio + 5, EditorGUIUtility.singleLineHeight, position.width * (1 - _splitRatio) - 5, position.height - 2 * EditorGUIUtility.singleLineHeight);
            var contentRect = new Rect(_infoRegion.x, 0, _infoRegion.width, 2 * EditorGUIUtility.singleLineHeight);
            DrawTitleInfo(contentRect);
            var infoRect = new Rect(_infoRegion.x, contentRect.yMax + 5, _infoRegion.width, _infoRegion.height - EditorGUIUtility.singleLineHeight * 2);
            var rect = new Rect(infoRect.x, infoRect.y, infoRect.width, EditorGUIUtility.singleLineHeight);
            DecisionTreeNode infoToShow = _activeTaskInfo ?? (_activeNodeView != null ? _activeNodeView.Info : null);
            if (infoToShow != null)
            {
                DrawNodeProps(ref rect);

                // 最后绘制 Sub Question 到_infoRegion底部
                if (infoToShow is DecisionSelectNode selectNode)
                {
                    float subQHeight = EditorGUIUtility.singleLineHeight * 3;
                    var subQRect = new Rect(
                        _infoRegion.x + 10,
                        _infoRegion.y + _infoRegion.height - subQHeight - 10, // 距底部10像素
                        _infoRegion.width - 20,
                        subQHeight
                    );

                    // 标题
                    var titleRect = new Rect(subQRect.x, subQRect.y, subQRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(titleRect, "Sub Question:", EditorStyles.boldLabel);

                    // 输入框
                    var inputRect = new Rect(subQRect.x, subQRect.y + EditorGUIUtility.singleLineHeight, subQRect.width, EditorGUIUtility.singleLineHeight * 2);
                    selectNode.question = EditorGUI.TextArea(inputRect, selectNode.question);
                    if (string.IsNullOrEmpty(selectNode.question))
                    {
                        EditorGUI.LabelField(inputRect, "Question...", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }
        private void HandleSplitterDrag(Rect verticalSplitterRect)
        {
            EditorGUIUtility.AddCursorRect(verticalSplitterRect, MouseCursor.ResizeHorizontal);
            if (_isResizing && (Event.current.button == 1 || Event.current.type == EventType.MouseUp))
            {
                _isResizing = false;
                Event.current.Use(); // 使用事件，防止传播
            }
            else if (_isResizing)
            {
                float mouseX = Event.current.mousePosition.x;
                _splitRatio = Mathf.Clamp(mouseX / position.width, 0.1f, 0.9f);
                Repaint();
            }
            else if (Event.current.type == EventType.MouseDown && verticalSplitterRect.Contains(Event.current.mousePosition))
            {
                _isResizing = true;
                Event.current.Use(); // 使用事件，防止传播
            }
        }
        #region  MutiSelect
        private void HandleSelectionEvents(Rect contentRect)
        {
            var currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0)
                    {
                        // 检查是否点击到了节点上
                        foreach (var node in _nodes)
                        {
                            var nodeSelectRect = node.Rect;
                            nodeSelectRect.height = EditorGUIUtility.singleLineHeight;
                            if (nodeSelectRect.Contains(currentEvent.mousePosition))
                            {
                                _isSelecting = false;
                                return;
                            }
                        }

                        // 开始框选
                        if (!currentEvent.shift)
                        {
                            _selectedNodes.Clear();
                        }
                        _isSelecting = true;
                        _selectionStartPos = currentEvent.mousePosition;
                        _selectionRect = new Rect();
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (_isSelecting && currentEvent.button == 0)
                    {
                        // 更新选择框
                        _selectionRect = GetSelectionRect(_selectionStartPos, currentEvent.mousePosition);

                        // 更新选中的节点
                        foreach (var node in _nodes)
                        {
                            if (_selectionRect.Overlaps(node.Rect))
                            {
                                if (!_selectedNodes.Contains(node))
                                {
                                    _selectedNodes.Add(node);
                                }
                            }
                            else if (!currentEvent.shift)
                            {
                                _selectedNodes.Remove(node);
                            }
                        }

                        currentEvent.Use();
                        Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (_isSelecting && currentEvent.button == 0)
                    {
                        _isSelecting = false;
                        currentEvent.Use();
                        Repaint();
                    }
                    break;

                case EventType.KeyDown:
                    if (currentEvent.keyCode == KeyCode.Escape)
                    {
                        _selectedNodes.Clear();
                        _isSelecting = false;
                        currentEvent.Use();
                        Repaint();
                    }
                    // 添加Delete键处理
                    else if (currentEvent.keyCode == KeyCode.Delete && _selectedNodes.Count > 0)
                    {
                        DeleteSelectedNodes();
                        currentEvent.Use();
                        Repaint();
                    }
                    break;
            }
        }

        private Rect GetSelectionRect(Vector2 start, Vector2 end)
        {
            Vector2 min = Vector2.Min(start, end);
            Vector2 max = Vector2.Max(start, end);
            return new Rect(min, max - min);
        }

        // 添加删除选中节点的方法
        private void DeleteSelectedNodes()
        {
            if (_selectedNodes.Count == 0)
                return;
            string message = _selectedNodes.Count == 1
                     ? "Are you sure you want to delete this node?"
                     : $"Are you sure you want to delete these {_selectedNodes.Count} nodes?";

            if (!EditorUtility.DisplayDialog("Confirm Delete", message, "Delete", "Cancel"))
                return;

            // 记录撤销
            RecordUndo("Delete Nodes");

            // 创建要删除的节点列表（避免在遍历时修改集合）
            var nodesToDelete = new List<DecisionNodeView>(_selectedNodes);

            foreach (var nodeView in nodesToDelete)
            {
                // 获取父节点
                var parent = nodeView.RefreshParentNode();
                if (parent != null)
                {
                    // 从父节点的子树中移除
                    parent.Children.Remove(nodeView.Info);
                }
                else if (nodeView.Info == nTree.rootNode)
                {
                    // 如果是根节点，清空其子节点
                    Debug.LogWarning("Cannot delete root node, clearing its children instead.");
                    nodeView.Info.Children?.Clear();
                    break;
                }
            }

            // 清空选中节点列表
            _selectedNodes.Clear();
            _activeNodeView = null;

            // 标记为已修改
            EditorUtility.SetDirty(nTree);

            // 重新加载节点信息以更新视图
            LoadNodeInfos();
        }

        #endregion
        public static DecisionGraphWindow[] GetWindows()
        {
            return Resources.FindObjectsOfTypeAll<DecisionGraphWindow>();
        }

        public static DecisionGraphWindow GetWindow(DecisionTree tree)
        {
            var windows = GetWindows();
            foreach (var win in windows)
            {
                if (win != null && win.nTree == tree)
                {
                    win.Focus();
                    return win;
                }
            }
            return null;
        }

        public static DecisionGraphWindow OpenWindow(DecisionTree tree)
        {
            if (tree == null)
                return null;

            var win = GetWindow(tree);
            if (win != null && win.nTree == tree)
                return win;

            string title = tree != null ? tree.name : string.Empty;
            // Unity 2021+ 推荐用 CreateWindow<T>(title, params Type[])
            var btreeWindow = EditorWindow.CreateWindow<DecisionGraphWindow>(title, typeof(DecisionGraphWindow));
            btreeWindow.SelectBTree(tree);
            btreeWindow.Focus();
            return btreeWindow;
        }

        public void SelectNTree(DecisionTree tree)
        {
            nTree = tree;
            RefreshGraphRegion();
            LoadNodeInfos();
        }
    }
    public class ScrollViewContainer
    {
        private Rect region;
        private ScrollView scrollView;
        private IMGUIContainer content;
        private VisualElement scrollViewContent;
        private float zoomSize = 1;
        public readonly float minZoomSize = 0.1f;
        public readonly float maxZoomSize = 1.3f;
        private ZoomManipulator zoomMa;
        private int resetOffsetCount;
        public Action onGUI { get; set; }
        public float ZoomSize
        {
            get
            {
                return zoomSize;
            }
            set
            {
                zoomSize = zoomMa.SetZoom(value);
            }
        }
        public Vector2 scrollOffset
        {
            get
            {
                return _scrollOffset;
            }
        }
        private Vector2 _scrollOffset;

        public void Start(VisualElement root, Rect region)
        {
            this.region = region;
            CreateScrollViewContent(region);
            CreateScrollViewContainer(region);
            CreateScrollView(region);
            CreateZoomManipulator(region);
            root.Add(scrollView);
            ResetCenterOffset();
        }

        public void ResetCenterOffset()
        {
            var offset = (zoomSize * region.size / minZoomSize - region.size) * 0.5f;
            offset.y = 0;
            _scrollOffset = offset;
            scrollView.scrollOffset = offset;
        }

        public void UpdateScale(Rect position)
        {
            var percent = _scrollOffset / region.size;

            region = position;
            scrollView.style.marginTop = position.y;
            scrollView.style.marginLeft = position.x;
            scrollView.style.width = position.width;
            scrollView.style.height = position.height;

            content.style.width = position.width / minZoomSize;//内部固定大小（但scale在作用下会实现与ScrollViewContent一样大）
            content.style.height = position.height / minZoomSize;

            scrollViewContent.style.width = position.width * zoomSize / minZoomSize;//缩放容器以动态改变ScrollView的内部尺寸
            scrollViewContent.style.height = position.height * zoomSize / minZoomSize;

            zoomMa.SetContentSize(position.size);
            _scrollOffset = percent * position.size;
        }

        private void CreateScrollView(Rect position)
        {
            scrollView = new ScrollView()
            {
                style =
                {
                     marginTop = position.y,
                     marginLeft = position.x,
                     width = position.width,
                     height = position.height,
                     backgroundColor = Color.clear
                 },
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
            };
            // 添加滚动事件监听
            scrollView.verticalScroller.valueChanged += OnScrollerValueChanged;
            scrollView.horizontalScroller.valueChanged += OnScrollerValueChanged;
            scrollView.mouseWheelScrollSize = 0;
            scrollView.Add(scrollViewContent);
            ResetCenterOffset();
        }
        // 添加滚动事件处理方法
        private void OnScrollerValueChanged(float newValue)
        {
            _scrollOffset = scrollView.scrollOffset;
            Refesh(); // 刷新视图
        }
        // 在需要清理时记得移除事件监听
        public void Cleanup()
        {
            if (scrollView != null)
            {
                scrollView.verticalScroller.valueChanged -= OnScrollerValueChanged;
                scrollView.horizontalScroller.valueChanged -= OnScrollerValueChanged;
            }
        }
        private void CreateScrollViewContainer(Rect position)
        {
            scrollViewContent = new VisualElement()
            {
                style = {
                    width = position.width / minZoomSize,
                    height = position.height / minZoomSize,
                    backgroundColor = Color.clear,
                    position = Position.Relative
                }
            };
            scrollViewContent.Add(content);
        }
        private void CreateScrollViewContent(Rect position)
        {
            content = new IMGUIContainer(OnGUI)
            {
                style =
                {
                  width =  position.width / minZoomSize,
                  height = position.height / minZoomSize,
                  backgroundColor = Color.clear,
                  position = Position.Absolute
                }
            };
        }
        private void CreateZoomManipulator(Rect position)
        {
            zoomMa = new ZoomManipulator(minZoomSize, maxZoomSize, content);
            zoomMa.SetContentSize(position.size);
            zoomMa.onZoomChanged = OnZoomValueChanged;
            zoomMa.onScrollMove = (arg1) =>
            {
                _scrollOffset = arg1;
                scrollView.scrollOffset = scrollOffset;
            };
            zoomMa.scrollPosGet = () =>
            {
                return scrollView.scrollOffset;
            };
            scrollView.AddManipulator(zoomMa);
        }
        private void OnGUI()
        {
            if (onGUI != null)
            {
                onGUI.Invoke();
            }
            else
            {
                Debug.Log("empty on Gui!");
            }
        }
        private void OnZoomValueChanged(float arg2)
        {
            zoomSize = arg2;

            var width = region.width * zoomSize / minZoomSize;
            var height = region.height * zoomSize / minZoomSize;
            scrollViewContent.style.width = width;//scrollViewContent的大小随着缩放变化
            scrollViewContent.style.height = height;

            //居中显示调整为居左上角显示
            content.style.left = -(region.width / minZoomSize - width) * 0.5f;
            content.style.top = -(region.height / minZoomSize - height) * 0.5f;
        }

        public void ApplyOffset(bool reset)
        {
            if (reset)
                ResetCenterOffset();
            scrollView.scrollOffset = _scrollOffset;
        }

        public void Refesh()
        {
            content.MarkDirtyRepaint();
            scrollViewContent.MarkDirtyRepaint();
            scrollView.MarkDirtyRepaint();
        }
    }

    public class ZoomManipulator : MouseManipulator, IManipulator
    {
        private VisualElement targetElement;
        private float minSize;
        private float maxSize;
        public readonly float zoomStep = 0.05f;

        public System.Action<float> onZoomChanged { get; set; }
        public System.Action<Vector2> onScrollMove { get; set; }
        public System.Func<Vector2> scrollPosGet { get; set; }
        private Vector2 _contentSize;
        public ZoomManipulator(float minSize, float maxSize, VisualElement element)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.targetElement = element;
            base.activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Alt
            });
        }

        public void SetContentSize(Vector2 size)
        {
            _contentSize = size;
        }

        public float SetZoom(float zoom)
        {
            var scale = Mathf.Clamp(zoom, minSize, maxSize);
            targetElement.transform.scale = Vector3.one * scale;
            if (onZoomChanged != null)
            {
                onZoomChanged.Invoke(scale);
            }
            return scale;
        }
        protected override void RegisterCallbacksOnTarget()
        {
            base.target.RegisterCallback<WheelEvent>(OnScroll, TrickleDown.NoTrickleDown);
            base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            base.target.UnregisterCallback<WheelEvent>(OnScroll, TrickleDown.NoTrickleDown);
            base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.altKey)
            {
                var offset = -scrollPosGet.Invoke();
                Vector2 delta = evt.mouseDelta;
                offset += delta;
                onScrollMove?.Invoke(-offset);
            }
        }

        private void OnScroll(WheelEvent e)
        {
            var anchorPos = VisualElementExtensions.ChangeCoordinatesTo(target, targetElement.parent.parent, e.localMousePosition);
            var pos = VisualElementExtensions.ChangeCoordinatesTo(target, targetElement, e.localMousePosition);
            float zoomScale = 1f - e.delta.y * zoomStep;
            var offset = -scrollPosGet.Invoke();
            var scale = Mathf.Clamp(this.targetElement.transform.scale.x * zoomScale, minSize, maxSize);
            this.targetElement.transform.scale = scale * Vector2.one;
            onZoomChanged?.Invoke(scale);

            var realPos = pos * targetElement.transform.scale;
            var offset0 = anchorPos - realPos;
            onScrollMove?.Invoke(realPos - anchorPos); e.StopPropagation();
        }
    }
    public class GraphBackground
    {

        protected const float kNodeGridSize = 12.0f;
        private const float kMajorGridSize = 120.0f;
        private static readonly Color kGridMinorColorDark = new Color(0f, 0f, 0f, 0.18f);
        private static readonly Color kGridMajorColorDark = new Color(0f, 0f, 0f, 0.28f);
        private static readonly Color kGridMinorColorLight = new Color(0f, 0f, 0f, 0.10f);
        private static readonly Color kGridMajorColorLight = new Color(0f, 0f, 0f, 0.15f);

        private Rect m_graphRegion;
        private Vector2 m_scrollPosition;

        private Material m_lineMaterial;
        private string m_shaderName;

        private static Color gridMinorColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return kGridMinorColorDark;
                else
                    return kGridMinorColorLight;
            }
        }
        private static Color gridMajorColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return kGridMajorColorDark;
                else
                    return kGridMajorColorLight;
            }
        }
        public GraphBackground(string shaderName)
        {
            this.m_shaderName = shaderName;
        }

        private Material CreateLineMaterial()
        {
            Shader shader = Shader.Find(m_shaderName);
            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;
            return m;
        }


        public void Draw(Rect position, Vector2 scroll, float scale)
        {
            m_graphRegion = position;
            m_scrollPosition = scroll;

            if (Event.current.type == EventType.Repaint)
            {
                UnityEditor.Graphs.Styles.graphBackground.Draw(position, false, false, false, false);
            }

            DrawGrid(scale);
        }

        private void DrawGrid(float scale)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (m_lineMaterial == null)
            {
                m_lineMaterial = CreateLineMaterial();
            }

            m_lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            DrawGridLines(kNodeGridSize * scale, gridMinorColor);
            DrawGridLines(kMajorGridSize * scale, gridMajorColor);

            GL.End();
            GL.PopMatrix();
        }

        private void DrawGridLines(float gridSize, Color gridColor)
        {
            GL.Color(gridColor);
            for (float x = m_graphRegion.xMin - (m_graphRegion.xMin % gridSize) - m_scrollPosition.x; x < m_graphRegion.xMax; x += gridSize)
            {
                if (x < m_graphRegion.xMin)
                {
                    continue;
                }
                DrawLine(new Vector2(x, m_graphRegion.yMin), new Vector2(x, m_graphRegion.yMax));
            }
            GL.Color(gridColor);
            for (float y = m_graphRegion.yMin - (m_graphRegion.yMin % gridSize) - m_scrollPosition.y; y < m_graphRegion.yMax; y += gridSize)
            {
                if (y < m_graphRegion.yMin)
                {
                    continue;
                }
                DrawLine(new Vector2(m_graphRegion.xMin, y), new Vector2(m_graphRegion.xMax, y));
            }
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }
    }

    public class TreeNodeLayout
    {
        public class DrawTreeNode
        {
            public DecisionTreeNode Info { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public List<DrawTreeNode> Children { get; set; }
            public DrawTreeNode Parent { get; set; }
            public DrawTreeNode Thread { get; set; }
            public double Mod { get; set; }
            public DrawTreeNode Ancestor { get; set; }
            public double Change { get; set; }
            public double Shift { get; set; }
            private DrawTreeNode _lmostSibling;
            public int Number { get; set; }

            public DrawTreeNode(DecisionTreeNode info, DrawTreeNode parent = null, int depth = 0, int number = 1)
            {
                Info = info;
                X = -1;
                Y = depth;
                Children = new List<DrawTreeNode>();

                // 处理子树
                if (info.Children != null)
                {
                    for (int i = 0; i < info.Children.Count; i++)
                    {
                        if (info.Children[i] == null)
                            continue;
                        Children.Add(new DrawTreeNode(info.Children[i], this, depth + 1, i + 1));
                    }
                }

                Parent = parent;
                Thread = null;
                Mod = 0;
                Ancestor = this;
                Change = 0;
                Shift = 0;
                _lmostSibling = null;
                Number = number;
            }

            public DrawTreeNode Right()
            {
                return Thread ?? (Children.Count > 0 ? Children[Children.Count - 1] : null);
            }

            public DrawTreeNode Left()
            {
                return Thread ?? (Children.Count > 0 ? Children[0] : null);
            }

            public DrawTreeNode LeftBrother()
            {
                DrawTreeNode n = null;
                if (Parent != null)
                {
                    foreach (var node in Parent.Children)
                    {
                        if (node == this)
                            return n;
                        n = node;
                    }
                }
                return n;
            }

            public DrawTreeNode GetLmostSibling()
            {
                if (_lmostSibling == null && Parent != null && this != Parent.Children[0])
                {
                    _lmostSibling = Parent.Children[0];
                }
                return _lmostSibling;
            }

            public DrawTreeNode LeftmostSibling => GetLmostSibling();
        }

        private static DrawTreeNode Firstwalk(DrawTreeNode v, float distance = 1)
        {
            if (v.Children.Count == 0)
            {
                if (v.LeftmostSibling != null)
                {
                    v.X = v.LeftBrother().X + distance;
                }
                else
                {
                    v.X = 0;
                }
            }
            else
            {
                var defaultAncestor = v.Children[0];
                foreach (var child in v.Children)
                {
                    Firstwalk(child);
                    defaultAncestor = Apportion(child, defaultAncestor, distance);
                }
                ExecuteShifts(v);

                double midpoint = (v.Children[0].X + v.Children[v.Children.Count - 1].X) / 2;
                var w = v.LeftBrother();
                if (w != null)
                {
                    v.X = w.X + distance;
                    v.Mod = v.X - midpoint;
                }
                else
                {
                    v.X = midpoint;
                }
            }
            return v;
        }

        private static DrawTreeNode Apportion(DrawTreeNode v, DrawTreeNode defaultAncestor, float distance)
        {
            var leftBrother = v.LeftBrother();
            if (leftBrother != null)
            {
                var vInnerRight = v;
                var vOuterRight = v;
                var vInnerLeft = leftBrother;
                var vOuterLeft = v.LeftmostSibling;

                var sInnerRight = v.Mod;
                var sOuterRight = v.Mod;
                var sInnerLeft = vInnerLeft.Mod;
                var sOuterLeft = vOuterLeft.Mod;

                while (vInnerLeft.Right() != null && vInnerRight.Left() != null)
                {
                    vInnerLeft = vInnerLeft.Right();
                    vInnerRight = vInnerRight.Left();
                    vOuterLeft = vOuterLeft.Left();
                    vOuterRight = vOuterRight.Right();

                    vOuterRight.Ancestor = v;

                    var shift = vInnerLeft.X + sInnerLeft - (vInnerRight.X + sInnerRight) + distance;
                    if (shift > 0)
                    {
                        var ancestor = Ancestor(vInnerLeft, v, defaultAncestor);
                        MoveSubtree(ancestor, v, shift);
                        sInnerRight += shift;
                        sOuterRight += shift;
                    }

                    sInnerLeft += vInnerLeft.Mod;
                    sInnerRight += vInnerRight.Mod;
                    sOuterLeft += vOuterLeft.Mod;
                    sOuterRight += vOuterRight.Mod;
                }

                if (vInnerLeft.Right() != null && vOuterRight.Right() == null)
                {
                    vOuterRight.Thread = vInnerLeft.Right();
                    vOuterRight.Mod += sInnerLeft - sOuterRight;
                }
                else if (vInnerRight.Left() != null && vOuterLeft.Left() == null)
                {
                    vOuterLeft.Thread = vInnerRight.Left();
                    vOuterLeft.Mod += sInnerRight - sOuterLeft;
                    defaultAncestor = v;
                }
            }
            return defaultAncestor;
        }

        private static void MoveSubtree(DrawTreeNode wl, DrawTreeNode wr, double shift)
        {
            var subtrees = wr.Number - wl.Number;
            wr.Change -= shift / subtrees;
            wr.Shift += shift;
            wl.Change += shift / subtrees;
            wr.X += shift;
            wr.Mod += shift;
        }

        private static void ExecuteShifts(DrawTreeNode v)
        {
            double shift = 0;
            double change = 0;
            for (int i = v.Children.Count - 1; i >= 0; i--)
            {
                var w = v.Children[i];
                w.X += shift;
                w.Mod += shift;
                change += w.Change;
                shift += w.Shift + change;
            }
        }

        private static DrawTreeNode Ancestor(DrawTreeNode vInnerLeft, DrawTreeNode v, DrawTreeNode defaultAncestor)
        {
            return v.Parent.Children.Contains(vInnerLeft.Ancestor) ? vInnerLeft.Ancestor : defaultAncestor;
        }

        private static float SecondWalk(DrawTreeNode v, double m = 0, int depth = 0, float? min = null)
        {
            v.X += m;
            v.Y = depth;

            if (!min.HasValue || v.X < min.Value)
            {
                min = (float)v.X;
            }

            foreach (var child in v.Children)
            {
                min = SecondWalk(child, m + v.Mod, depth + 1, min);
            }

            return min.Value;
        }

        private static void ThirdWalk(DrawTreeNode tree, double n)
        {
            tree.X += n;
            foreach (var child in tree.Children)
            {
                ThirdWalk(child, n);
            }
        }

        public static DrawTreeNode CalculateLayout(DecisionTreeNode rootInfo)
        {
            var tree = new DrawTreeNode(rootInfo);
            var dt = Firstwalk(tree);
            var min = SecondWalk(dt);
            if (min < 0)
            {
                ThirdWalk(dt, -min);
            }
            return dt;
        }

        // 可选：添加获取节点位置的辅助方法
        public static Vector2 GetNodePosition(DrawTreeNode node)
        {
            return new Vector2((float)node.X, (float)node.Y);
        }
        public static Dictionary<DecisionTreeNode, Vector2Int> GetNodePositions(
        DecisionTreeNode rootInfo,
        int horizontalSpacing = 100,
        int verticalSpacing = 100)
        {
            var positions = new Dictionary<DecisionTreeNode, Vector2Int>();
            var root = CalculateLayout(rootInfo);

            // 使用队列进行广度优先遍历，收集所有节点位置
            var queue = new Queue<DrawTreeNode>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                // 将浮点数坐标转换为整数坐标，并应用缩放因子
                var position = new Vector2Int(
                    Mathf.RoundToInt((float)node.X * horizontalSpacing),
                    Mathf.RoundToInt((float)node.Y * verticalSpacing)
                );

                positions[node.Info] = position;

                // 将子节点加入队列
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }

            MoveOffsetNodePostions(positions);
            return positions;
        }
        private static void MoveOffsetNodePostions(Dictionary<DecisionTreeNode, Vector2Int> posMap)
        {
            var posxArr = posMap.Values.Select(p => p.x).ToArray();
            var max = Mathf.Max(posxArr);
            var min = Mathf.Min(posxArr);
            var offset = Mathf.FloorToInt((max - min) * 0.5f);
            foreach (var node in posMap.Keys.ToArray())
            {
                var pos = posMap[node];
                pos.x -= offset;
                posMap[node] = pos;
            }
        }
    }
}