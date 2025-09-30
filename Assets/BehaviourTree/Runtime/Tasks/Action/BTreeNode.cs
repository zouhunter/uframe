/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: Returns a Status of appoint.
 *_*/

using UFrame.BehaviourTree;
using UFrame;
using UnityEngine;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("BTreeNode")]
    public class BTreeNode : ActionNode
    {
        public BTree tree;
        [SerializeField]
        private BTree _instanceTree;
        public BTree instaneTree => _instanceTree;

        public override void SetOwner(IOwner owner)
        {
            base.SetOwner(owner);
            if (tree)
                _instanceTree = Object.Instantiate(tree);
            if (_instanceTree)
                _instanceTree.SetOwner(owner);
        }

        protected override void OnReset()
        {
            base.OnReset();
            _instanceTree?.OnReset();
        }
        protected override byte OnUpdate()
        {
            return _instanceTree?.Tick() ?? Status.Failure;
        }

        protected override void OnEnd(ITreeInfo info)
        {
            base.OnEnd(info);
            if (_instanceTree && _instanceTree.rootTree.node)
                _instanceTree.rootTree.node.DoEnd(_instanceTree.rootTree);
        }

        protected override void OnClear()
        {
            base.OnClear();
            if (_instanceTree != null)
                _instanceTree.CleanDeepth(_instanceTree.rootTree);
        }
    }
}
