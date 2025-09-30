using System.Collections;
/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2024-07-22                                                                   *
*  版本:                                                                *
*  功能:                                                                              *
*   - editor                                                                          *
*//************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    public class BTreeWindow : EditorWindow
    {

        [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
        public static bool OnOpen(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is BTree tree)
            {
                OpenWindow(tree, true);
                return true;
            }
            return false;
        }

        private BTree bTree;
        private ScrollViewContainer _scrollViewContainer;
        private GraphBackground _background;
        private Rect _graphRegion;
        private List<NodeView> _nodes;
        private List<KeyValuePair<NodeView, NodeView>> _connections;
        private Rect _infoRegion;
        private bool _inConnect;
        private Vector2 _startConnectPos;
        private NodeView _activeNodeView;
        private Dictionary<BaseNode, string> _propPaths;
        private SerializedObject serializedObject;
        private List<BaseNode> _activeNodes;
        private Vector2 _variableScroll;
        private Rect _scrollContentSize;
        private float _splitRatio = 0.7f;
        private bool _isResizing;
        private List<NodeView> _selectedNodes = new List<NodeView>();
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
        private Dictionary<string, bool> _nodeExpandStates = new Dictionary<string, bool>();

        private void OnEnable()
        {
            _background = new GraphBackground("UI/Default");
            _graphRegion = position;
            _scrollViewContainer = new ScrollViewContainer();
            _scrollViewContainer.Start(rootVisualElement, GetGraphRegion());
            _scrollViewContainer.onGUI = DrawNodeGraphContent;
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
            _nodes = new List<NodeView>();
            _connections = new List<KeyValuePair<NodeView, NodeView>>();
            if (bTree != null)
            {
                var rootTree = bTree.rootTree;
                var posMap = TreeNodeLayout.GetNodePositions(rootTree, NodeView.WIDTH + 10, NodeView.MAX_HEIGHT);
                CreateViewDeepth(bTree, rootTree, rootTree, posMap, _nodes);
                var nodes = new List<BaseNode>();
                _propPaths = new Dictionary<BaseNode, string>();
                if (bTree is BVariantTree bvt)
                    bvt.BuildInstanceRootTree();
                BTreeDrawer.CollectNodeDeepth(bTree.rootTree, "_rootTree", nodes, _propPaths);
                serializedObject = new SerializedObject(bTree);
            }

            Repaint();
        }

        private NodeView CreateViewDeepth(BTree bTree, TreeInfo rootInfo, TreeInfo info, Dictionary<TreeInfo, Vector2Int> posMap, List<NodeView> _nodes)
        {
            var view = new NodeView(bTree, rootInfo, info, posMap[info]);
            view.onReload = LoadNodeInfos;
            view.onStartConnect = OnStartConnect;
            view.posFunc = OnGetNodePos;
            view.onActive = OnSetActiveView;
            this._nodes.Add(view);

            if (info.subTrees != null && info.subTrees.Count > 0)
            {
                for (var i = 0; i < info.subTrees.Count; ++i)
                {
                    var child = info.subTrees[i];
                    var childView = CreateViewDeepth(bTree, rootInfo, child, posMap, _nodes);
                    _connections.Add(new KeyValuePair<NodeView, NodeView>(view, childView));
                }
            }
            return view;
        }
        private Vector2 OnGetNodePos(TreeInfo info)
        {
            foreach (var node in _nodes)
            {
                if (node.Info == info)
                    return node.Rect.position;
            }
            return Vector2.zero;
        }

        private void OnSetActiveView(NodeView nodeView)
        {
            if (!_selectedNodes.Contains(nodeView))
                _selectedNodes.Add(nodeView);
            _activeNodeView = nodeView;
        }

        private void OnStartConnect(Vector2 pos)
        {
            _startConnectPos = pos;
            _inConnect = true;
        }

        private int GetSubtreeWidth(TreeInfo node)
        {
            int horizontalSpacing = 10; // 水平间距
            int nodeWidth = NodeView.WIDTH; // 节点宽度
            if (node.subTrees == null || node.subTrees.Count == 0)
            {
                return nodeWidth; // 节点宽度
            }

            int width = 0;
            foreach (var child in node.subTrees)
            {
                width += GetSubtreeWidth(child) + horizontalSpacing; // 调整后的水平间距
            }

            return width - horizontalSpacing;
        }

        public void SelectBTree(BTree tree)
        {
            this.bTree = tree;
            LoadNodeInfos();
            base.titleContent = new GUIContent(tree.name);
            EditorPrefs.SetInt("BTreeWindow.bTree", tree.GetInstanceID());
        }

        private void OnGUI()
        {
            DrawVerticalLine();
            DrawSelectTree();
            if (!bTree)
            {
                bTree = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("BTreeWindow.bTree")) as BTree;
                if (bTree)
                    SelectBTree(bTree);
                else
                    return;
            }
            _background?.Draw(_graphRegion, _scrollViewContainer.scrollOffset, _scrollViewContainer.ZoomSize);
            RefreshGraphRegion();
            DrawInformations();
            DrawFootPrint();
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
                if (bTree != null)
                {
                    EditorUtility.SetDirty(bTree);
                    AssetDatabase.SaveAssets();
                    ShowNotification(new GUIContent("保存成功"));
                }
            }
        }

        // 创建纯色纹理的辅助方法
        private Texture2D CreateColorTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private Texture2D GetColorTexture(Color color)
        {
            if (!_colorTextures.TryGetValue(color, out var texture))
            {
                texture = CreateColorTexture(color);
                _colorTextures[color] = texture;
            }
            return texture;
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

        void DrawNodeCurve(NodeView startView, NodeView endView)
        {
            if (_scrollViewContainer == null)
                return;
            var active = startView.Info.enable && endView.Info.enable;
            var start = new Vector2(startView.Rect.center.x + 2, startView.Rect.max.y - 2);
            var end = new Vector2(endView.Rect.center.x + 2, endView.Rect.min.y + 2);
            var color = Color.white;
            if (!active)
            {
                color = Color.gray;
            }
            else if (bTree)
            {
                color = GraphColorUtil.GetColorByState(endView.Info.status, endView.Info.tickCount == bTree.TickCount);
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
                NodeView targetView = null;
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
                        oldParent.subTrees.Remove(targetView.Info);
                        _activeNodeView.Info.subTrees.Add(targetView.Info);
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

        private bool CheckAsChildAble(TreeInfo parent, TreeInfo child)
        {
            if (parent == null || child == null)
                return false;

            if (child == parent) return false;

            if (child.subTrees != null && child.subTrees.Count > 0)
            {
                foreach (var subTree in child.subTrees)
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
            Undo.RecordObject(bTree, message);
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
            var name = bTree != null ? bTree.name : "(none)";
            var readMeRect = new Rect(contentRect.x + contentRect.width - 60, contentRect.y + 10, 60, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.LinkButton(readMeRect, "README"))
                Application.OpenURL("https://web-alidocs.dingtalk.com/i/nodes/QOG9lyrgJP3BQREofr345nLxVzN67Mw4?doc_type=wiki_doc");
            GUIContent content = new GUIContent($"ST行为树 <size=12><b><color=black>v1.0</color></b></size> <size=12><b>({name})</b></size>");
            if (GUI.Button(contentRect, content, centeredStyle) && bTree != null)
            {
                EditorGUIUtility.PingObject(bTree);
            }
            var lineRect = new Rect(contentRect.x, contentRect.y + contentRect.height, contentRect.width, 3);
            GUI.Box(lineRect, "");
        }

        private void CollectNodes()
        {
            _activeNodes = _activeNodes ?? new List<BaseNode>();
            _activeNodes.Clear();
            if (_activeNodeView != null)
            {
                if (_activeNodeView.Info.node)
                    _activeNodes.Add(_activeNodeView.Info.node);
                if (_activeNodeView.Info.condition.conditions != null)
                {
                    foreach (var condition in _activeNodeView.Info.condition.conditions)
                    {
                        _activeNodes.Add(condition.node);
                        if (condition.subConditions != null)
                        {
                            foreach (var item in condition.subConditions)
                            {
                                _activeNodes.Add(item.node);
                            }
                        }
                    }
                }
            }
        }
        private void DrawNodeProps(ref Rect rect)
        {
            serializedObject?.Update();
            int i = 0;
            foreach (var node in _activeNodes)
            {
                if (!node)
                    continue;

                // 标题获取逻辑
                string nodeTitle = null;
                var attr = node.GetType().GetCustomAttributes(typeof(NodePathAttribute), false).FirstOrDefault() as NodePathAttribute;
                if (attr != null && !string.IsNullOrEmpty(attr.desc))
                    nodeTitle = attr.desc;
                else if (!string.IsNullOrEmpty(node.name))
                    nodeTitle = node.name;
                else
                    nodeTitle = node.GetType().Name;

                // 第一个节点特殊样式
                if (i == 0)
                {
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
                    GUI.Label(new Rect(rect.x + 8, rect.y + 2, rect.width - 8, EditorGUIUtility.singleLineHeight + 2), nodeTitle, firstTitleStyle);
                    rect.y += EditorGUIUtility.singleLineHeight + 8;
                }
                else
                {
                    // 其他节点样式
                    var titleStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft,
                        normal = { textColor = new Color(0.7f, 0.8f, 1f) }
                    };
                    GUI.Label(new Rect(rect.x + 8, rect.y, rect.width - 8, EditorGUIUtility.singleLineHeight), $"{i}. {nodeTitle}", titleStyle);
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }
                i++;

                // 节点内容
                var nodeItemRect = rect;
                nodeItemRect.x += 20;
                nodeItemRect.width -= 20;
                TreeInfoDrawer.DrawCreateNodeContent(nodeItemRect, node, (x) =>
                {
                    _activeNodeView.Info.node = x;
                }, bTree, Status.Inactive, 0);

                if (node && _propPaths.TryGetValue(node, out var path))
                {
                    var drawer = serializedObject.FindProperty(path);
                    if (drawer != null)
                    {
                        string key = node.GetType().FullName + node.name;
                        if (!_nodeExpandStates.ContainsKey(key))
                            _nodeExpandStates[key] = true; // 首次默认展开
                        drawer.isExpanded = _nodeExpandStates[key];
                        var labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 120;
                        rect.height = EditorGUI.GetPropertyHeight(drawer, true);
                        EditorGUI.PropertyField(rect, drawer, GUIContent.none, true);
                        _nodeExpandStates[key] = drawer.isExpanded;
                        EditorGUIUtility.labelWidth = labelWidth;
                        rect.y += rect.height;
                    }
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
            if (_activeNodeView != null && _activeNodeView.Info != null)
            {
                var descRect = new Rect(_infoRegion.x + 10, rect.yMax, _infoRegion.width - 20, EditorGUIUtility.singleLineHeight * 3);
                _activeNodeView.Info.desc = EditorGUI.TextArea(descRect, _activeNodeView.Info.desc);
                if (string.IsNullOrEmpty(_activeNodeView.Info.desc))
                {
                    descRect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(descRect, "Description...", EditorStyles.centeredGreyMiniLabel);
                }
                rect.y += EditorGUIUtility.singleLineHeight * 5;
                CollectNodes();
                if (_activeNodes.Count > 0)
                {
                    DrawNodeProps(ref rect);
                }
            }
            var height = _infoRegion.height - (rect.yMax - contentRect.y);
            height = Mathf.Min(height, _infoRegion.height * 0.5f);
            var scrollRect = new Rect(_infoRegion.x, _infoRegion.y + _infoRegion.height - height, _infoRegion.width, height);
            DrawVariables(scrollRect);
        }

        private void DrawVariables(Rect rect)
        {
            var titleRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 0.8f);
            GUI.Box(titleRect, "");
            EditorGUI.LabelField(titleRect, "变量集合:", EditorStyles.boldLabel);

            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height -= EditorGUIUtility.singleLineHeight;
            _scrollContentSize.width = rect.width - 20;
            var fieldHeight = 0f;
            GUI.Box(rect, "");
            _variableScroll = GUI.BeginScrollView(rect, _variableScroll, _scrollContentSize);
            var index = 1;
            var variables = (bTree.Owner as BTree).Variables;
            using (var liter = variables.GetGetEnumerator())
            {
                while (liter.MoveNext())
                {
                    var lineHeight = EditorGUIUtility.singleLineHeight + 4;
                    fieldHeight += lineHeight;
                    using (var hori = new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight + 4)))
                    {
                        EditorGUILayout.LabelField(index++.ToString("00"), EditorStyles.miniBoldLabel, GUILayout.Width(20));
                        var widthLabel = _scrollContentSize.width * 0.4f;
                        EditorGUILayout.LabelField(liter.Current.Key + " : ", EditorStyles.miniBoldLabel, GUILayout.Width(widthLabel));
                        var value = liter.Current.Value.GetValue();
                        var layout = GUILayout.Height(EditorGUIUtility.singleLineHeight);
                        if (value == null)
                        {
                            var contentType = liter.Current.Value.GetType();
                            if (contentType.IsGenericType)
                            {
                                var subType = contentType.GetGenericArguments()[0];
                                EditorGUILayout.LabelField($"Null ({subType.Name})", layout);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("Null", layout);
                            }
                            continue;
                        }
                        var type = value.GetType();
                        if (value is UnityEngine.Object o)
                        {
                            EditorGUILayout.ObjectField(o, o.GetType(), false, layout);
                        }
                        else if (value is Vector2 v2)
                        {
                            EditorGUILayout.Vector2Field("", v2, layout);
                        }
                        else if (value is Vector3 v3)
                        {
                            EditorGUILayout.Vector3Field("", v3, layout);
                        }
                        else if (value is Vector4 v4)
                        {
                            EditorGUILayout.Vector4Field("", v4, layout);
                        }
                        else if (value is Rect r)
                        {
                            EditorGUILayout.RectField("", r, layout);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(value.ToString() + $" ({type.Name})", EditorStyles.textField, layout);
                        }
                    }
                }
            }
            _scrollContentSize.height = Mathf.Max(_scrollContentSize.height, rect.height, fieldHeight);
            GUI.EndScrollView();
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
            var nodesToDelete = new List<NodeView>(_selectedNodes);

            foreach (var nodeView in nodesToDelete)
            {
                // 获取父节点
                var parent = nodeView.RefreshParentNode();
                if (parent != null)
                {
                    // 从父节点的子树中移除
                    parent.subTrees.Remove(nodeView.Info);
                }
                else if (nodeView.Info == bTree.rootTree)
                {
                    // 如果是根节点，清空其子节点
                    Debug.LogWarning("Cannot delete root node, clearing its children instead.");
                    nodeView.Info.subTrees?.Clear();
                    break;
                }
            }

            // 清空选中节点列表
            _selectedNodes.Clear();
            _activeNodeView = null;

            // 标记为已修改
            EditorUtility.SetDirty(bTree);

            // 重新加载节点信息以更新视图
            LoadNodeInfos();
        }

        #endregion
        public static BTreeWindow[] GetWindows()
        {
            return Resources.FindObjectsOfTypeAll<BTreeWindow>();
        }

        public static BTreeWindow GetWindow(BTree tree)
        {
            var windows = GetWindows();
            foreach (var win in windows)
            {
                if (win != null && win.bTree == tree)
                {
                    win.Focus();
                    return win;
                }
            }
            return null;
        }

        public static BTreeWindow OpenWindow(BTree tree, bool tryGetCurrentWindow = true)
        {
            if (tree == null)
                return null;

            if (tryGetCurrentWindow)
            {
                var win = GetWindow(tree);
                if (win != null)
                    return win;
            }

            string title = tree != null ? tree.name : string.Empty;
            // Unity 2021+ 推荐用 CreateWindow<T>(title, params Type[])
            var btreeWindow = EditorWindow.CreateWindow<BTreeWindow>(title, typeof(BTreeWindow));
            btreeWindow.SelectBTree(tree);
            btreeWindow.Focus();
            return btreeWindow;
        }
    }
}