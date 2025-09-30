using System.Collections;
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-06
//* 描    述：分层网络树结构

//* ************************************************************************************
using UnityEngine;
using UFrame.BehaviourTree;
using System;
using System.Threading;
using System.Collections.Generic;

namespace UFrame.HTN
{
    public class NTree : OwnerBase
    {
        public WorldRefStates worldState;
        public TaskInfo rootTree;
        public Planner Plan { get; set; }
        public SearchState SearchState { get; set; }
        public bool autoPlan;
        private bool _treeStarted;
        private ulong _planIndex;
        private IOwner _ownerTree;
        public IOwner Owner
        {
            get
            {
                return _ownerTree ?? this;
            }
        }

        internal void DoReset()
        {
            throw new NotImplementedException();
        }

        private Search search;


        public event Action<NTree> SearchComplete;
        public event Action<NTree> PlanSet;
        public event Action<NTree> BlackboardChanged;
        public event Action<NTree, PlanNode, byte> PlanStateChanged;

        public NTree()
        {
            worldState = new WorldRefStates(_variableCenter);
            Plan = new Planner();
            SearchState = new SearchState(Plan);
            worldState.ValueChanged += OnBlackboardAttributeChanged;
            Plan.StatusChanged += OnPlanStateChanged;
        }

        public void CleanDeepth(TreeInfo info)
        {
            if (info.subTrees != null && info.subTrees != null)
            {
                foreach (var subInfo in info.subTrees)
                {
                    if (subInfo.enable)
                    {
                        CleanDeepth(subInfo);
                    }
                }
            }
            if (info.condition.enable && info.condition.conditions != null && info.node == this)
            {
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.subConditions != null)
                    {
                        for (int i = 0; i < condition.subConditions.Count; i++)
                        {
                            var condition2 = condition.subConditions[i];
                            condition2.node?.Clean();
                        }
                    }
                    condition.node?.Clean();
                }
            }
            info.node?.Clean();
        }

        public bool StartUp()
        {
            Debug.Log("StartUp:" + rootTree.node.name);
            if (rootTree != null && rootTree.node != null)
            {
                TickCount = 0;
                _ownerTree = this;
                TreeInfoUtil.SetOwnerDeepth(rootTree, this);
                SearchState.isDirty = true;
                _treeStarted = true;
                return true;
            }
            Debug.LogError("rootTree empty!" + (rootTree == null));
            return false;
        }

        public void Stop()
        {
            _treeStarted = false;
            TreeInfoUtil.CleanDeepth(rootTree);
        }

        public override void SetVariables(VariableCenter center)
        {
            base.SetVariables(center);
            worldState = new WorldRefStates(_variableCenter);
        }

        private void OnBlackboardAttributeChanged(string key, object value)
        {
            BlackboardChanged?.Invoke(this);
            SearchState.blackboardChanged = true;
            if (CheckNeedRePlanOnWorldStateChange(key))
            {
                SearchState.isDirty = true;
            }
        }

        /// <summary>
        /// 检查是否需要重新规划
        /// </summary>
        protected virtual bool CheckNeedRePlanOnWorldStateChange(string key)
        {
            if (SearchState.planFailed)
                return true;
            foreach (var node in Plan.nodes)
            {
                if (node.status != Status.Inactive)
                    continue;
                if (node.checks == null)
                    continue;
                foreach (var check in node.checks)
                {
                    if (check.Key == key)
                        return true;
                }
            }
            return false;
        }

        private void OnPlanStateChanged(PlanNode planNode, byte planState)
        {
            Debug.Log("OnPlanStateChanged:" + planState);
            switch (planState)
            {
                case Status.Failure:
                    SearchState.isDirty = true;
                    SearchState.planFailed = true;
                    break;

                case Status.Success:
                    if (Plan.IsComplete())
                    {
                        SearchState.isDirty = true;
                        SearchState.planDone = true;
                    }
                    break;
            }

            SearchState.hasPlanChanged = true;

            PlanStateChanged?.Invoke(this, planNode, planState);
        }

        public bool Search()
        {
            var root = rootTree;
            if (root == null)
                return false;

            search = search ?? new Search(this, worldState, Plan);
            var result = search.Run(root, SearchState.result);
            SearchComplete?.Invoke(this);
            return result;
        }

        public void SearchAsync()
        {
            var root = rootTree;
            if (root == null)
            {
                return;
            }

            var id = ++_planIndex;
            var syncContext = SynchronizationContext.Current;

            var callback = new SearchHandler(searchResult =>
                syncContext.Post(_ => { OnSearchComplete(searchResult); }, null));

            search = search ?? new Search(this, worldState, Plan);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = search.Run(root, SearchState.result);
                    SearchState.result.handle = id;
                    callback.Invoke(SearchState.result);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }

        public void SetPlan(SearchResult searchResult)
        {
            if (searchResult == null)
            {
                return;
            }

            var current = Plan.GetCurrent();
            if (current != null && current.Count > 0)
            {
                foreach (var item in current)
                {
                    item.status = Status.Interrupt;
                }
            }

            Plan.Set(searchResult);
            SearchState.hasPlanChanged = true;
            OnPlanSet();
        }

        public byte Tick()
        {
            if (_ownerTree == (IOwner)this && !_treeStarted)
                return Status.Inactive;
            if (autoPlan)
                UpdateSearchPlan(true);
            return Plan.Tick(this);
        }


        protected virtual void OnPlanSet()
        {
            PlanSet?.Invoke(this);
        }

        private void OnSearchComplete(SearchResult searchResult)
        {
            if (searchResult.handle != _planIndex)
            {
                // Ignore stale results
                return;
            }

            SearchState.isSearchingAsync = false;
            SearchComplete?.Invoke(this);

            if (searchResult.success)
            {
                ReplacePlan(searchResult);
            }
        }

        public void ReplacePlan(SearchResult searchResult)
        {
            bool isComplete = Plan.IsComplete();
            var isPartial = Plan.IsPartial(searchResult.plan);
            if (!isComplete && isPartial)
            {
                // Don't replace plan if new one is partial of the previous one.
                return;
            }

            bool isCurrentPlanValid = Plan.Verify(worldState);
            var hasNewPlanLowerMtr = searchResult.methodTraversalRecord.CompareTo(Plan.mtr) < 0;
            if (Plan.HasFailed() || isComplete || !isCurrentPlanValid || hasNewPlanLowerMtr)
            {
                SetPlan(searchResult);
            }
        }

        /// <summary>
        /// Searches for a plan if search state is dirty.
        /// </summary>
        public void UpdateSearchPlan(bool async)
        {
            if (!SearchState.isDirty || SearchState.isSearchingAsync)
            {
                // No need to search for a plan.
                return;
            }

            // Clear search state.
            SearchState.Clear();

            if (!async)
            {
                SearchState.isSearchingAsync = false;
                // Search for a plan syncronously.
                if (Search())
                {
                    // Replace the current plan if search successful.
                    ReplacePlan(SearchState.result);
                }
            }
            else
            {
                // Search for a plan on a different thread.
                SearchState.isSearchingAsync = true;
                SearchAsync();
            }
        }

        /// <summary>
        /// 收集节点
        /// </summary>
        /// <param name="allNodes"></param>
        public virtual void CollectNodesDeepth(TreeInfo info, List<BaseNode> nodes)
        {
            if (info.node && !nodes.Contains(info.node))
            {
                nodes.Add(info.node);
            }
            if (info.condition != null && info.condition.conditions != null)
            {
                int i = 0;
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.node && !nodes.Contains(condition.node))
                    {
                        nodes.Add(condition.node);
                    }

                    if (condition.subConditions != null)
                    {
                        int j = 0;
                        foreach (var subNode in condition.subConditions)
                        {
                            if (subNode != null && subNode.node && !nodes.Contains(subNode.node))
                            {
                                nodes.Add(subNode.node);
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            if (info.subTrees != null)
            {
                for (int i = 0; i < info.subTrees.Count; i++)
                {
                    var item = info.subTrees[i];
                    CollectNodesDeepth(item, nodes);
                }
            }
        }

        public void SetOwner(IOwner owner)
        {
            _ownerTree = owner;
            TreeInfoUtil.SetOwnerDeepth(rootTree, owner);
        }
    }
}

