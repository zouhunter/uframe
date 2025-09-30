using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace UFrame.BehaviourTree
{
    public class GraphView : MonoBehaviour
    {
        [SerializeField]
        public RectTransform content;
        [SerializeField]
        public GraphNodeView nodePrefab;
        [SerializeField]
        private Button closeBtn;
        private List<BTree> _treeList;
        [SerializeField]
        private BTree _activeTree;
        [SerializeField]
        private Dropdown _dropdown;
        public bool useDefaultScaleMethod { get; set; } = true;
        private ScrollRect _scrollRect;  // 关联的ScrollRect组件
        private List<GraphNodeView> _nodes;
        private List<KeyValuePair<GraphNodeView, GraphNodeView>> _connections;
        private UILineRenderer _uiLineRender;
        public event Action onClose;
        private bool _graphChanged;
        private float _delyChangeTimer;
        private void Awake()
        {
            _scrollRect = content.GetComponentInParent<ScrollRect>();
            _connections = new List<KeyValuePair<GraphNodeView, GraphNodeView>>();
            InitLineRender();
            closeBtn.onClick.AddListener(OnCloseClicked);
            _dropdown.onValueChanged.AddListener(OnTreeDropSelect);
            nodePrefab.gameObject.SetActive(false);
        }

        private void OnTreeDropSelect(int arg0)
        {
            Open(_treeList[arg0]);
        }

        private void OnCloseClicked()
        {
            if (onClose != null)
            {
                onClose?.Invoke();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void InitLineRender()
        {
            _uiLineRender = new GameObject("line", typeof(UILineRenderer), typeof(RectTransform), typeof(CanvasGroup)).GetComponent<UILineRenderer>();
            _uiLineRender.transform.SetParent(content, false);
            _uiLineRender.pointsList = new List<UILineGroup>();
            var rt = (_uiLineRender.transform as RectTransform);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 1);
            rt.localPosition = Vector2.zero;
            _uiLineRender.thickness = 1;
            _uiLineRender.iterations = 10;
        }

        private void OnEnable()
        {
            if (_activeTree != null)
            {
                ReloadActiveTree();
            }
        }

        public void Open(BTree tree)
        {
            if (_activeTree != tree && tree != null)
            {
                _activeTree = tree;
                ReloadActiveTree();
            }
        }

        public void ResetTreeList(List<BTree> treeList)
        {
            if (_treeList == null)
                _treeList = new List<BTree>();
            _treeList.Clear();
            _treeList.AddRange(treeList);
            if (_treeList.Count > 0)
            {
                _activeTree = _treeList[_treeList.Count - 1];
                ReloadActiveTree();
            }
        }

        private void RefreshDropDown()
        {
            _treeList.RemoveAll(x => x == null);
            var options = new List<Dropdown.OptionData>();
            foreach (var item in _treeList)
            {
                options.Add(new Dropdown.OptionData(
                    text: item.name));
            }
            if (options.Count > 0)
            {
                _dropdown.options = options;
                if (_dropdown.value >= options.Count)
                    _dropdown.value = 0;
            }
            if (_activeTree != null)
            {
                var index = _treeList.IndexOf(_activeTree);
                if (index > -1)
                    _dropdown.value = index;
            }
        }

        private void ReloadActiveTree()
        {
            if (_treeList == null)
                _treeList = new List<BTree>();
            if (!_treeList.Contains(_activeTree))
                _treeList.Add(_activeTree);
            _connections.Clear();
            RefreshDropDown();
            CreateGraphNodes();
        }

        public void OnScroll(float scaleOffset)
        {
            // 执行缩放操作
            var scale = content.localScale;
            scale.x = Mathf.Clamp(scale.x + scaleOffset / Screen.width, 0.5f, 2);
            scale.y = scale.x;
            content.localScale = scale;
        }

        private void ProcessDefaultScale()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 2)
                {
                    var touch1 = Input.GetTouch(0);
                    var touch2 = Input.GetTouch(1);

                    if (touch1.phase != TouchPhase.Moved && touch2.phase != TouchPhase.Moved)
                        return;

                    var lastDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                    var currentDistance = Vector2.Distance(touch1.position, touch2.position);
                    if (Vector2.Angle(touch1.deltaPosition, touch2.deltaPosition) > 0.01f)
                    {
                        var offset = (currentDistance - lastDistance);
                        OnScroll(offset);
                    }
                }
            }
            else
            {
                var axis = Input.GetAxis("Mouse ScrollWheel");
                if (axis != 0)
                {
                    OnScroll(axis * Screen.width);
                }
            }
        }

        private void Update()
        {
            if (useDefaultScaleMethod)
            {
                ProcessDefaultScale();
            }

            if (_graphChanged)
            {
                _graphChanged = false;
                CreateGraphLines();
                _uiLineRender.enabled = false;
                _uiLineRender.enabled = true;
                _uiLineRender.Rebuild(CanvasUpdate.Layout);
            }

            if (_delyChangeTimer < Time.time && _delyChangeTimer > 0)
            {
                _graphChanged = true;
                _delyChangeTimer = -1;
            }
        }


        private void CreateGraphNodes()
        {
            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    Destroy(node.gameObject);
                }
                _nodes.Clear();
            }
            else
            {
                _nodes = new List<GraphNodeView>();
            }
            var posMap = new Dictionary<TreeInfo, Vector2Int>();
            CalculateNodePositions(_activeTree.rootTree, 0, 0, posMap);
            MoveOffsetNodePostions(posMap);
            CreateViewDeepth(_activeTree, null, _activeTree.rootTree, posMap, _nodes);
            _delyChangeTimer = Time.time + Time.deltaTime * 2;
        }

        private void CreateGraphLines()
        {
            _uiLineRender.pointsList.Clear();
            foreach (var connect in _connections)
            {
                var parent = connect.Key;
                var child = connect.Value;
                var pos1 = parent.GetOutPotPos();
                var pos2 = child.GetInPotPos();
                var color = GraphColorUtil.GetColorByState(child.treeInfo.status, child.treeInfo.tickCount == _activeTree.TickCount);
                _uiLineRender.pointsList.Add(new UILineGroup(color, new List<UILinePoint>() {
                   new UILinePoint(pos1,new Vector2(0,100),new Vector2(0,-100)),
                   new UILinePoint(pos2,new Vector2(0,100),new Vector2(0,-100)),
                }));
            }
        }

        private GraphNodeView CreateViewDeepth(BTree bTree, TreeInfo rootInfo, TreeInfo info, Dictionary<TreeInfo, Vector2Int> posMap, List<GraphNodeView> _nodes)
        {
            var view = Instantiate(nodePrefab);
            view.gameObject.SetActive(true);
            view.transform.SetParent(content.transform, false);
            view.Init(bTree, rootInfo, info, posMap[info]);
            view.onReload = CreateGraphNodes;
            view.onStartConnect = OnStartConnect;
            view.onActive = OnSetActiveView;
            this._nodes.Add(view);

            if (info.subTrees != null && info.subTrees.Count > 0)
            {
                for (var i = 0; i < info.subTrees.Count; ++i)
                {
                    var child = info.subTrees[i];
                    var childView = CreateViewDeepth(bTree, info, child, posMap, _nodes);
                    _connections.Add(new KeyValuePair<GraphNodeView, GraphNodeView>(view, childView));
                }
            }
            return view;
        }

        private void OnSetActiveView(GraphNodeView node)
        {
            throw new NotImplementedException();
        }

        private void OnStartConnect(Vector2 vector)
        {
            throw new NotImplementedException();
        }

        private void MoveOffsetNodePostions(Dictionary<TreeInfo, Vector2Int> posMap)
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

        private void CalculateNodePositions(TreeInfo node, int depth, int offset, Dictionary<TreeInfo, Vector2Int> posMap)
        {
            Vector2Int pos = new Vector2Int(0, 0);
            int verticalSpacing = 10; // 垂直间距
            int horizontalSpacing = 10; // 水平间距

            pos.y = depth * (GraphNodeView.MAX_HEIGHT + verticalSpacing);
            int subtreeOffset = offset;

            if (node.subTrees != null && node.subTrees.Count > 0)
            {
                foreach (var child in node.subTrees)
                {
                    CalculateNodePositions(child, depth + 1, subtreeOffset, posMap);
                    subtreeOffset += GetSubtreeWidth(child) + horizontalSpacing;
                }

                int firstChildX = posMap[node.subTrees[0]].x;
                int lastChildX = posMap[node.subTrees[node.subTrees.Count - 1]].x;
                pos.x = (firstChildX + lastChildX) / 2;
            }
            else
            {
                pos.x = offset + GraphNodeView.WIDTH / 2;
            }

            posMap[node] = pos;
        }

        private int GetSubtreeWidth(TreeInfo node)
        {
            int horizontalSpacing = 10; // 水平间距
            int nodeWidth = GraphNodeView.WIDTH; // 节点宽度
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
    }

    public struct GraphLine
    {
        public Vector2Int startPos;
        public Vector2Int endPos;
    }
}
