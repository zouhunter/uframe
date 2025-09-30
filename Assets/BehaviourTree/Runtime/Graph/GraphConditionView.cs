using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UFrame.BehaviourTree
{
    public class GraphConditionView : MonoBehaviour
    {
        [SerializeField]
        private GraphSubConditionView _subConditionViewPfb;
        private List<GraphSubConditionView> _conditions;
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private Toggle _subTog;
        [SerializeField]
        private Image _flag;
        [SerializeField]
        private Sprite[] _stateIcons;
        [SerializeField]
        private Image _colorImage;
        [SerializeField]
        private Text _condtionMatchTypeText;
        private ConditionItem _conditionItem;
        private Button _nextFlagBtn;
        private BTree _tree;

        void Awake()
        {
            _subConditionViewPfb.gameObject.SetActive(false);
            _subTog.onValueChanged.AddListener(OnSubToggleChanged);
            _nextFlagBtn = _flag.GetComponent<Button>();
            _nextFlagBtn.onClick.AddListener(OnNextFlag);
        }

        private void OnNextFlag()
        {
            _conditionItem.state++;
            if (_conditionItem.state > 2)
                _conditionItem.state = 0;
            Refresh();
        }

        private void OnSubToggleChanged(bool arg0)
        {
            if (_conditionItem != null)
            {
                _conditionItem.subEnable = arg0;
                Refresh();
            }
        }

        internal void Init(BTree tree, ConditionItem condition)
        {
            _tree = tree;
            _conditionItem = condition;
            Refresh();
        }

        private void Update()
        {
            if (_conditionItem != null)
            {
                GraphColorUtil.SetColorByState(_colorImage, _conditionItem.status, _conditionItem.tickCount == _tree.TickCount);
                if (_conditionItem.state == 1)
                {
                    var status = _conditionItem.status;
                    if (status == Status.Success)
                    {
                        status = Status.Failure;
                    }
                    else
                    {
                        status = Status.Success;
                    }
                    GraphColorUtil.SetColorByState(_flag, status, _conditionItem.tickCount == _tree.TickCount);
                }
                else
                {
                    _flag.color = Color.white;
                }
            }
        }

        public void Refresh()
        {
            var condition = _conditionItem;
            if (condition != null)
            {
                if (condition.node)
                {
                    _nameText.text = condition.node.name;
                }
                ResetConditionViews();
                _flag.sprite = _stateIcons[condition.state];
                if (condition.subEnable && condition.subConditions != null)
                    CreateConditionViews(condition);
                _condtionMatchTypeText.text = condition.matchType.ToString();
                _condtionMatchTypeText.enabled = condition.subEnable;
            }
        }

        private void ResetConditionViews()
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
                _conditions = new List<GraphSubConditionView>();
            }
        }

        private void CreateConditionViews(ConditionItem conditionInfo)
        {
            if (conditionInfo.subConditions != null && conditionInfo.subEnable)
            {
                foreach (var condition in conditionInfo.subConditions)
                {
                    var conditonView = Instantiate(_subConditionViewPfb);
                    conditonView.gameObject.SetActive(true);
                    conditonView.transform.SetParent(_subConditionViewPfb.transform.parent, false);
                    conditonView.Init(_tree, condition);
                    _conditions.Add(conditonView);
                }
            }
        }
    }
}
