using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UFrame.BehaviourTree
{
    public class GraphNodeView : MonoBehaviour
    {
        [SerializeField]
        private Toggle inPort;
        [SerializeField]
        private Toggle outPort;
        [SerializeField]
        private Text _title;
        [SerializeField]
        private Text _type;
        [SerializeField]
        private Image _colorImage;
        [SerializeField]
        private Toggle _conditionTog;
        [SerializeField]
        private Text _condtionMatchTypeText;

        [SerializeField]
        private GraphConditionView _conditionPfb;
        [SerializeField]
        private Image _groupBg;

        public const int WIDTH = 200;
        public const int MAX_HEIGHT = 300;
        private BTree _tree;
        public TreeInfo parentInfo { get; private set; }
        public TreeInfo treeInfo { get; private set; }
        public Action onReload { get; set; }
        public Action<GraphNodeView> onActive { get; set; }
        public Action<Vector2> onStartConnect { get; set; }
        private Vector2 _position;
        private List<GraphConditionView> _conditions;
        internal void Init(BTree bTree, TreeInfo parentInfo, TreeInfo info, Vector2 vector2Int)
        {
            _tree = bTree;
            this.parentInfo = parentInfo;
            this.treeInfo = info;
            _position = vector2Int - new Vector2(WIDTH * 0.5f, -20);
            _position.y = -_position.y;
            transform.localPosition = _position;
            inPort.interactable = false;
            outPort.interactable = false;
            _conditionPfb.gameObject.SetActive(false);
            _conditionTog.onValueChanged.AddListener(OnCondtionTogChanged);
            _groupBg.enabled = info.node && info.node is ParentNode;
            RefreshInfo();
        }

        private void OnCondtionTogChanged(bool arg0)
        {
            if (treeInfo != null)
            {
                treeInfo.condition.enable = arg0;
                RefreshInfo();
            }
        }

        private void RefreshInfo()
        {
            inPort.gameObject.SetActive(parentInfo != null);
            if (treeInfo.node is ParentNode parentNode || (treeInfo.subTrees != null && treeInfo.subTrees.Count > 0))
                outPort.gameObject.SetActive(true);
            else
                outPort.gameObject.SetActive(false);
            if (treeInfo.node)
            {
                _title.text = treeInfo.node.name;
                _type.text = treeInfo.node.GetType().Name;
            }
            ResetCondtionViews();
            if (treeInfo.condition != null && treeInfo.condition.enable && treeInfo.condition.conditions != null)
            {
                CreateConditionViews(treeInfo.condition);
            }
            _conditionTog.isOn = treeInfo.condition.enable;
            _condtionMatchTypeText.text = treeInfo.condition.matchType.ToString();
            _condtionMatchTypeText.enabled = treeInfo.condition.enable;
        }

        private void Update()
        {
            if (treeInfo != null)
            {
                GraphColorUtil.SetColorByState(_colorImage, treeInfo.status, treeInfo.tickCount == _tree.TickCount);
            }
        }

        public Vector2 GetInPotPos()
        {
            var inPortPos = _position;
            inPortPos.y += 20;
            return inPortPos;
        }
        public Vector2 GetOutPotPos()
        {
            var outPortPos = _position;
            if (outPort)
            {
                outPortPos.y += outPort.transform.localPosition.y;
            }
            return outPortPos;
        }
        private void ResetCondtionViews()
        {
            if (_conditions != null)
            {
                foreach (var condition in _conditions)
                {
                    Destroy(condition.gameObject);
                }
                _conditions.Clear();
            }
            else
            {
                _conditions = new List<GraphConditionView>();
            }
        }
        private void CreateConditionViews(ConditionInfo conditionInfo)
        {
            foreach (var condition in conditionInfo.conditions)
            {
                var conditonView = Instantiate(_conditionPfb);
                conditonView.gameObject.SetActive(true);
                conditonView.transform.SetParent(_conditionPfb.transform.parent, false);
                conditonView.Init(_tree, condition);
                _conditions.Add(conditonView);
            }
        }
    }
}
