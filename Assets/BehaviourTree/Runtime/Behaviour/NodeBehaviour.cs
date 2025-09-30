using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    public class NodeBehaviour : MonoBehaviour
    {
        [SerializeReference]
        public BaseNode node;
        public ConditionInfo condition = new ConditionInfo();
        private TreeInfo _treeInfo;

        public byte status => _treeInfo == null ? Status.Inactive : _treeInfo.status;
        public TreeInfo treeInfo => _treeInfo;
        public TreeInfo CreateTreeInfo()
        {
            if (_treeInfo == null)
                _treeInfo = TreeInfo.Create(GetInstanceID().ToString());
            _treeInfo.enable = gameObject.activeSelf && enabled;
            _treeInfo.node = node;
            _treeInfo.condition = condition;
            if (transform.childCount == 0)
                return _treeInfo;
            _treeInfo.subTrees = new List<TreeInfo>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).GetComponent<NodeBehaviour>();
                if (!child)
                    continue;
                _treeInfo.subTrees.Add(child.CreateTreeInfo());
            }
            return _treeInfo;
        }

        private void OnEnable()
        {
            if (_treeInfo != null)
                _treeInfo.enable = true;
        }

        private void OnDisable()
        {
            if (_treeInfo != null)
                _treeInfo.enable = false;
        }

        public byte Execute()
        {
            if (_treeInfo != null && _treeInfo.node && _treeInfo.enable)
                return _treeInfo.node.Execute(_treeInfo);
            return Status.Inactive;
        }

        public void SetOwner(BTree tree)
        {
            if (_treeInfo != null && _treeInfo.node)
                _treeInfo.node.SetOwner(tree);
        }
    }
}
