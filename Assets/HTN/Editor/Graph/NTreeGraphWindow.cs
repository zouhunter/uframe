using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UFrame.BehaviourTree;
using System;
using UFrame.HTN.Editor.Graph;
using UnityEditor.Callbacks;
using System.Linq;

namespace UFrame.HTN
{
    public class NTreeGraphWindow : EditorWindow
    {
        [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
        public static bool OnOpen(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is NTree tree)
            {
                OpenWindow(tree);
                return true;
            }
            return false;
        }

        private NTree nTree;
        private ScrollViewContainer _scrollViewContainer;
        private GraphBackground _background;
        private Rect _graphRegion;
        private List<NTreeNodeView> _nodes;
        private List<KeyValuePair<NTreeNodeView, NTreeNodeView>> _connections;
        private Rect _infoRegion;
        private bool _inConnect;
        private Vector2 _startConnectPos;
        private NTreeNodeView _activeNodeView;
        private Dictionary<BaseNode, string> _propPaths;
        private SerializedObject serializedObject;
        private List<BaseNode> _activeNodes;
        private Vector2 _variableScroll;
        private Rect _scrollContentSize;
        private float _splitRatio = 0.7f;
        private bool _isResizing;
        private List<NTreeNodeView> _selectedNodes = new List<NTreeNodeView>();
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

        private int _variableTabIndex = 0; // 0: 变量集合, 1: 世界状态
        private static readonly string[] _variableTabs = new[] { "变量集合", "世界状态" };

        private CheckDrawer _checkDrawer;
        private EffectDrawer _effectDrawer;
        public TaskInfo _activeTaskInfo;

        private Dictionary<TaskInfo, CheckDrawer> _checkDrawerCache = new Dictionary<TaskInfo, CheckDrawer>();
        private Dictionary<TaskInfo, EffectDrawer> _effectDrawerCache = new Dictionary<TaskInfo, EffectDrawer>();

        // 显示模式 0: 树结构 1: 计划结构
        private int _graphMode = 0;
        private static readonly string[] _graphModes = new[] { "树结构", "计划结构" };
        // PlanNode可视化缓存
        private List<PlanNodeView> _planNodeViews = new List<PlanNodeView>();
        private List<KeyValuePair<PlanNodeView, PlanNodeView>> _planConnections = new List<KeyValuePair<PlanNodeView, PlanNodeView>>();

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
            _nodes = new List<NTreeNodeView>();
            _connections = new List<KeyValuePair<NTreeNodeView, NTreeNodeView>>();
            if (nTree != null)
            {
                var rootTree = nTree.rootTree;
                var posMap = TreeNodeLayout.GetNodePositions(rootTree, NTreeNodeView.WIDTH + 10, NTreeNodeView.MAX_HEIGHT);
                CreateViewDeepth(nTree, rootTree, rootTree, posMap, _nodes);
                var nodes = new List<BaseNode>();
                _propPaths = new Dictionary<BaseNode, string>();
                NTreeEditor.CollectNodeDeepth(nTree.rootTree, "rootTree", nodes, _propPaths);
                serializedObject = new SerializedObject(nTree);
            }

            if (nTree != null && nTree.Plan != null && nTree.Plan.nodes.Count > 0)
            {
                RefreshPlanNodes();
            }
            Repaint();
        }

        private NTreeNodeView CreateViewDeepth(NTree bTree, TreeInfo rootInfo, TreeInfo info, Dictionary<TreeInfo, Vector2Int> posMap, List<NTreeNodeView> _nodes)
        {
            var view = new NTreeNodeView(bTree, rootInfo as TaskInfo, info as TaskInfo, posMap[info]);
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
                    _connections.Add(new KeyValuePair<NTreeNodeView, NTreeNodeView>(view, childView));
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

        private void OnSetActiveView(NTreeNodeView nodeView)
        {
            if (!_selectedNodes.Contains(nodeView))
                _selectedNodes.Add(nodeView);
            _activeNodeView = nodeView;
            _activeTaskInfo = _activeNodeView.Info;
            CollectNodes();
        }

        private void OnStartConnect(Vector2 pos)
        {
            _startConnectPos = pos;
            _inConnect = true;
        }

        private int GetSubtreeWidth(TreeInfo node)
        {
            int horizontalSpacing = 10; // 水平间距
            int nodeWidth = NTreeNodeView.WIDTH; // 节点宽度
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

        public void SelectBTree(NTree tree)
        {
            this.nTree = tree;
            LoadNodeInfos();
            base.titleContent = new GUIContent(tree.name);
            EditorPrefs.SetInt("NTreeGraphWindow.bTree", tree.GetInstanceID());
        }

        private void OnGUI()
        {
            DrawVerticalLine();
            DrawSelectTree();
            _background?.Draw(_graphRegion, _scrollViewContainer.scrollOffset, _scrollViewContainer.ZoomSize);
            if (_graphMode == 1 && (nTree?.Plan == null || nTree.Plan.nodes == null || nTree.Plan.nodes.Count == 0))
            {
                EditorGUI.LabelField(new Rect(position.width * _splitRatio * 0.5f - 30, position.height * 0.5f, 200, 30), "当前无计划节点!", EditorStyles.boldLabel);
                return;
            }
            DrawInformations();
            DrawFootPrint();
        }

        private void DrawScrollGraphContent()
        {
            if (_graphMode == 0)
            {
                // 树结构原有绘制
                RefreshGraphRegion();
                DrawNodeGraphContent();
            }
            else
            {
                // 计划结构绘制
                DrawPlanGraph();
            }
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

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                var modeBarRect = new Rect(position.width * _splitRatio - 180, 0, 180, EditorGUIUtility.singleLineHeight);
                _graphMode = GUI.Toolbar(modeBarRect, _graphMode, _graphModes);

                if (change.changed)
                {
                    RefreshPlanNodes();
                }
            }
        }

        private void RefreshPlanNodes()
        {
            BuildPlanNodeViews();
            AutoLayoutPlanNodes(_graphRegion);
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

        void DrawNodeCurve(NTreeNodeView startView, NTreeNodeView endView)
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
            else if (nTree)
            {
                color = GraphColorUtil.GetColorByState(endView.Info.status, endView.Info.tickCount == nTree.TickCount);
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
                NTreeNodeView targetView = null;
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
            GUIContent content = new GUIContent($"Unity-HTH <size=12><b><color=black>v1.0</color></b></size> <size=12><b>({name})</b></size>");
            if (GUI.Button(contentRect, content, centeredStyle) && nTree != null)
            {
                EditorGUIUtility.PingObject(nTree);
            }
            var lineRect = new Rect(contentRect.x, contentRect.y + contentRect.height, contentRect.width, 3);
            GUI.Box(lineRect, "");
        }

        private void CollectNodes()
        {
            _activeNodes = _activeNodes ?? new List<BaseNode>();
            _activeNodes.Clear();
            if (_activeTaskInfo != null)
            {
                if (_activeTaskInfo.node)
                    _activeNodes.Add(_activeTaskInfo.node);
                if (_activeTaskInfo.condition.conditions != null)
                {
                    foreach (var condition in _activeTaskInfo.condition.conditions)
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
        private void DrawChecksAndEffects(TaskInfo info, ref Rect rect)
        {
            if (info == null) return;
            if (!_checkDrawerCache.TryGetValue(info, out var checkDrawer))
            {
                checkDrawer = new CheckDrawer(nTree, info, null);
                _checkDrawerCache[info] = checkDrawer;
            }
            if (!_effectDrawerCache.TryGetValue(info, out var effectDrawer))
            {
                effectDrawer = new EffectDrawer(nTree, info, null);
                _effectDrawerCache[info] = effectDrawer;
            }
            float checkHeight = checkDrawer.GetHeight();
            float effectHeight = effectDrawer.GetHeight();
            float bothHeight = Mathf.Max(checkHeight, effectHeight);
            float halfWidth = rect.width / 2f - 2f;
            var checkRect = new Rect(rect.x, rect.y, halfWidth, bothHeight);
            var effectRect = new Rect(rect.x + halfWidth + 4f, rect.y, halfWidth, bothHeight);
            checkDrawer.OnGUI(checkRect);
            effectDrawer.OnGUI(effectRect);
            rect.y += bothHeight + 8;
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

                // 第一个节点后绘制checks/effects
                if (i == 0 && _activeNodeView != null)
                {
                    DrawChecksAndEffects(_activeNodeView.Info, ref rect);
                }
                i++;

                // 节点内容
                var nodeItemRect = rect;
                nodeItemRect.x += 20;
                nodeItemRect.width -= 20;
                TaskInfoDrawer.DrawCreateNodeContent(nodeItemRect, node, (x) =>
                {
                    _activeNodeView.Info.node = x;
                }, nTree, Status.Inactive, 0);

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
            TaskInfo infoToShow = _activeTaskInfo ?? (_activeNodeView != null ? _activeNodeView.Info : null);
            if (infoToShow != null)
            {
                var descRect = new Rect(_infoRegion.x + 10, rect.yMax, _infoRegion.width - 20, EditorGUIUtility.singleLineHeight * 3);
                infoToShow.desc = EditorGUI.TextArea(descRect, infoToShow.desc);
                if (string.IsNullOrEmpty(infoToShow.desc))
                {
                    descRect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(descRect, "Description...", EditorStyles.centeredGreyMiniLabel);
                }
                rect.y += EditorGUIUtility.singleLineHeight * 5;
                _activeNodes = _activeNodes ?? new List<BaseNode>();
                _activeNodes.Clear();
                if (infoToShow.node)
                    _activeNodes.Add(infoToShow.node);
                if (infoToShow.condition.conditions != null)
                {
                    foreach (var condition in infoToShow.condition.conditions)
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

        // 公共标题背景绘制
        private void DrawSectionTitleBar(Rect rect, string title, Action<Rect> rightContent = null)
        {
            EditorGUI.DrawRect(rect, new Color(0.22f, 0.22f, 0.22f, 1f));
            var titleLabelRect = new Rect(rect.x + 10, rect.y, rect.width - 140, rect.height);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft
            };
            EditorGUI.LabelField(titleLabelRect, title, titleStyle);
            if (rightContent != null)
            {
                var rightRect = new Rect(rect.x + rect.width - 125, rect.y + (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f, 120, EditorGUIUtility.singleLineHeight);
                rightContent(rightRect);
            }
        }

        private void DrawVariables(Rect rect)
        {
            // 标题区美化，使用公共方法
            var titleBarRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight + 4);
            DrawSectionTitleBar(titleBarRect, "全局数据", rightRect =>
            {
                _variableTabIndex = EditorGUI.Popup(rightRect, _variableTabIndex, _variableTabs);
            });
            rect.y += EditorGUIUtility.singleLineHeight + 4;
            rect.height -= EditorGUIUtility.singleLineHeight + 4;

            if (_variableTabIndex == 0)
            {
                DrawVariableList(rect);
            }
            else
            {
                DrawWorldStateList(rect);
            }
        }

        // 公共变量行绘制
        private void DrawVariableLine(Rect rect, string key, object value)
        {
            var widthLabel = rect.width * 0.4f;
            var keyRect = new Rect(rect.x, rect.y, widthLabel, rect.height);
            var valueRect = new Rect(rect.x + widthLabel + 4, rect.y, rect.width - widthLabel - 4, rect.height);
            EditorGUI.LabelField(keyRect, key + " : ", EditorStyles.miniBoldLabel);
            if (value == null)
            {
                EditorGUI.LabelField(valueRect, "Null", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            var type = value.GetType();
            if (value is UnityEngine.Object o)
            {
                EditorGUI.ObjectField(valueRect, o, o.GetType(), false);
            }
            else if (value is Vector2 v2)
            {
                EditorGUI.Vector2Field(valueRect, "", v2);
            }
            else if (value is Vector3 v3)
            {
                EditorGUI.Vector3Field(valueRect, "", v3);
            }
            else if (value is Vector4 v4)
            {
                EditorGUI.Vector4Field(valueRect, "", v4);
            }
            else if (value is Rect r)
            {
                EditorGUI.RectField(valueRect, "", r);
            }
            else
            {
                EditorGUI.LabelField(valueRect, value.ToString() + $" ({type.Name})", EditorStyles.textField);
            }
        }

        private void DrawVariableList(Rect rect)
        {
            rect.y += 0;
            rect.height -= 0;
            _scrollContentSize.width = rect.width - 20;
            var fieldHeight = 0f;
            GUI.Box(rect, "");
            _variableScroll = GUI.BeginScrollView(rect, _variableScroll, _scrollContentSize);
            var variables = (nTree.Owner as NTree).Variables;
            using (var liter = variables.GetGetEnumerator())
            {
                while (liter.MoveNext())
                {
                    var lineHeight = EditorGUIUtility.singleLineHeight + 4;
                    fieldHeight += lineHeight;
                    var lineRect = new Rect(rect.x + 4, rect.y + fieldHeight - lineHeight, _scrollContentSize.width - 8, lineHeight);
                    DrawVariableLine(lineRect, liter.Current.Key, liter.Current.Value.GetValue());
                }
            }
            _scrollContentSize.height = Mathf.Max(_scrollContentSize.height, rect.height, fieldHeight);
            GUI.EndScrollView();
        }

        private void DrawWorldStateList(Rect rect)
        {
            rect.y += 0;
            rect.height -= 0;
            _scrollContentSize.width = rect.width - 20;
            var fieldHeight = 0f;
            GUI.Box(rect, "");
            _variableScroll = GUI.BeginScrollView(rect, _variableScroll, _scrollContentSize);
            var worldState = nTree.worldState;
            if (worldState.keys != null)
            {
                foreach (var key in worldState.keys)
                {
                    var variables = (nTree.Owner as NTree).GetVariable(key);
                    var value = variables?.GetValue();
                    var lineHeight = EditorGUIUtility.singleLineHeight + 4;
                    fieldHeight += lineHeight;
                    var lineRect = new Rect(rect.x + 4, rect.y + fieldHeight - lineHeight, _scrollContentSize.width - 8, lineHeight);
                    DrawVariableLine(lineRect, key, value);
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
            var nodesToDelete = new List<NTreeNodeView>(_selectedNodes);

            foreach (var nodeView in nodesToDelete)
            {
                // 获取父节点
                var parent = nodeView.RefreshParentNode();
                if (parent != null)
                {
                    // 从父节点的子树中移除
                    parent.subTrees.Remove(nodeView.Info);
                }
                else if (nodeView.Info == nTree.rootTree)
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
            EditorUtility.SetDirty(nTree);

            // 重新加载节点信息以更新视图
            LoadNodeInfos();
        }

        #endregion
        public static NTreeGraphWindow[] GetWindows()
        {
            return Resources.FindObjectsOfTypeAll<NTreeGraphWindow>();
        }

        public static NTreeGraphWindow GetWindow(NTree tree)
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

        public static NTreeGraphWindow OpenWindow(NTree tree)
        {
            if (tree == null)
                return null;

            var win = GetWindow(tree);
            if (win != null && win.nTree == tree)
                return win;

            string title = tree != null ? tree.name : string.Empty;
            // Unity 2021+ 推荐用 CreateWindow<T>(title, params Type[])
            var btreeWindow = EditorWindow.CreateWindow<NTreeGraphWindow>(title, typeof(NTreeGraphWindow));
            btreeWindow.SelectBTree(tree);
            btreeWindow.Focus();
            return btreeWindow;
        }

        public void SelectNTree(NTree tree)
        {
            nTree = tree;
            RefreshGraphRegion();
            LoadNodeInfos();
        }
        // PlanNode可视化主入口
        private void DrawPlanGraph()
        {
            foreach (var item in _planNodeViews)
            {
                var view = item;
                view.onActive = (x) =>
                {
                    _activeTaskInfo = view.Info;
                    CollectNodes();
                    Repaint();
                };
            }
            var contentRect = new Rect(Vector2.zero, _graphRegion.size / _scrollViewContainer.minZoomSize);
            foreach (var conn in _planConnections)
            {
                DrawPlanNodeCurve(conn.Key, conn.Value);
            }
            BeginWindows();
            for (int i = 0; i < _planNodeViews.Count; i++)
            {
                var view = _planNodeViews[i];
                view.DrawNode(contentRect);
            }
            EndWindows();
        }
        // 构建PlanNodeView及依赖
        private void BuildPlanNodeViews()
        {
            _planNodeViews.Clear();
            _planConnections.Clear();
            var nodeMap = new Dictionary<string, PlanNodeView>();
            foreach (var node in nTree.Plan.nodes)
            {
                var view = new PlanNodeView(nTree, node);
                nodeMap[node.id] = view;
                _planNodeViews.Add(view);
            }
            // 建立依赖
            foreach (var view in _planNodeViews)
            {
                foreach (var pred in view.Info.Predecessors)
                {
                    if (pred != null && nodeMap.TryGetValue(pred, out var predView))
                    {
                        view.predecessors.Add(predView);
                        predView.successors.Add(view);
                        _planConnections.Add(new KeyValuePair<PlanNodeView, PlanNodeView>(predView, view));
                    }
                }
            }
        }
        // 居中纵向分层布局：从_graphRegion中间顶部向下，深度越大Y值越大
        private void AutoLayoutPlanNodes(Rect region)
        {
            // 计算层级
            var layerMap = new Dictionary<PlanNodeView, int>();
            int maxLayer = 0;
            foreach (var view in _planNodeViews)
            {
                int layer = GetPlanNodeLayer(view, new HashSet<PlanNodeView>());
                layerMap[view] = layer;
                if (layer > maxLayer) maxLayer = layer;
            }
            // 每层分布
            var layers = new List<List<PlanNodeView>>();
            for (int i = 0; i <= maxLayer; i++) layers.Add(new List<PlanNodeView>());
            foreach (var kv in layerMap) layers[kv.Value].Add(kv.Key);
            // 布局参数
            float regionCenterX = region.x + region.width * 0.5f - PlanNodeView.WIDTH * 4 / 3f;
            float y0 = region.y - PlanNodeView.MIN_HEIGHT * 0.5f;
            float yStep = PlanNodeView.MAX_HEIGHT + 30;
            for (int l = 0; l <= maxLayer; l++)
            {
                var nodes = layers[l];
                float totalWidth = nodes.Count * PlanNodeView.WIDTH + (nodes.Count - 1) * 60;
                float xStart = regionCenterX - totalWidth * 0.5f;
                float y = y0 + l * yStep;
                for (int i = 0; i < nodes.Count; i++)
                {
                    float x = xStart + i * (PlanNodeView.WIDTH + 10);
                    nodes[i].Pos = new Vector2(x, y);
                }
            }
        }
        // 递归获取层级
        private int GetPlanNodeLayer(PlanNodeView view, HashSet<PlanNodeView> visited)
        {
            if (view.predecessors.Count == 0) return 0;
            if (!visited.Add(view)) return 0; // 防环
            int max = 0;
            foreach (var pred in view.predecessors)
            {
                int l = GetPlanNodeLayer(pred, visited);
                if (l > max) max = l;
            }
            return max + 1;
        }
        // 连线
        private void DrawPlanNodeCurve(PlanNodeView from, PlanNodeView to)
        {
            // 起点为from下边缘中心，终点为to上边缘中心
            var start = new Vector2(from.Rect.center.x, from.Rect.yMax);
            var end = new Vector2(to.Rect.center.x, to.Rect.yMin);
            var color = GraphColorUtil.GetColorByState(to.Info.status, from.Info.tickCount == nTree.TickCount);
            Handles.BeginGUI();
            // 贝塞尔控制点竖直偏移
            Handles.DrawBezier(start, end, start + Vector2.up * 40, end + Vector2.down * 40, color, null, 4);
            Handles.EndGUI();
        }
    }
}
