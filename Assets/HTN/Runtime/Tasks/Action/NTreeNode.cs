using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.BehaviourTree;
using UFrame.BehaviourTree.Actions;

namespace UFrame.HTN
{
    public class NTreeNode : BaseNode, IMethod
    {
        [Header("独立分解")]
        public bool independent;
        public NTree tree;
        [SerializeField]
        private NTree _instanceTree;
        public NTree instaneTree => _instanceTree;

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
            _instanceTree?.DoReset();
        }

        protected override byte OnUpdate()
        {
            return _instanceTree.Plan.Tick(_instanceTree.Owner);
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

        public SearchResult Search(TaskInfo taskInfo, Search search, List<string> predecessors)
        {
            if (independent)
            {
                _instanceTree.UpdateSearchPlan(false);
                if (_instanceTree.SearchState.result.success && _instanceTree.Plan != null && instaneTree.Plan.nodes.Count > 0)
                {
                    var searchResult = search.GetSearchResult();
                    _instanceTree.SearchState.result.CopyTo(searchResult);
                    searchResult.AddToPlan(taskInfo, predecessors);
                    return searchResult;
                }
                return null;
            }

            var child = _instanceTree.rootTree;

            if (_instanceTree.rootTree.node is IMethod method)
            {
                return method.Search(_instanceTree.rootTree, search, predecessors);
            }
            else
            {
                var searchResult = search.GetSearchResult();
                if (!child.VerifyCheck(search.workingState))
                {
                    searchResult.methodTraversalRecord.Increment();
                    child.status = Status.Failure;
                    search.SaveBack(searchResult);
                    return null;
                }
                searchResult.success = true;
                searchResult.AddToPlan(child, predecessors);
                return searchResult;
            }
        }
    }
}
