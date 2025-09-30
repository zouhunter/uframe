using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UFrame.BehaviourTree
{
    public class GraphSubConditionView : MonoBehaviour
    {
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private Sprite[] _stateIcons;
        [SerializeField]
        private Image _flag;
        [SerializeField]
        private Image _colorImage;
        private SubConditionItem _conditionInfo;
        private Button _nextFlagBtn;
        private BTree _tree;

        private void Awake()
        {
            _nextFlagBtn = _flag.GetComponent<Button>();
            _nextFlagBtn.onClick.AddListener(OnNextFlag);
        }

        private void OnNextFlag()
        {
            _conditionInfo.state++;
            if (_conditionInfo.state > 2)
                _conditionInfo.state = 1;
            Refresh();
        }

        public void Init(BTree tree, SubConditionItem condition)
        {
            _tree = tree;
            _conditionInfo = condition;
            Refresh();
        }
        private void Update()
        {
            if (_conditionInfo != null)
            {
                GraphColorUtil.SetColorByState(_colorImage, _conditionInfo.status, _conditionInfo.tickCount == _tree.TickCount);
                if (_conditionInfo.state == 1)
                {
                    var status = _conditionInfo.status;
                    if (status == Status.Success)
                    {
                        status = Status.Failure;
                    }
                    else
                    {
                        status = Status.Success;
                    }
                    GraphColorUtil.SetColorByState(_flag, status, _conditionInfo.tickCount == _tree.TickCount);
                }
                else
                {
                    _flag.color = Color.white;
                }
            }
        }


        public void Refresh()
        {
            var condition = _conditionInfo;
            if(condition != null)
            {
                if (condition.node != null)
                {
                    _nameText.text = condition.node.name;
                }
                _flag.sprite = _stateIcons[condition.state];
            }
        }
    }
}
