using System.Collections.Generic;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public class ParallelMethodNode : MethodNode
    {
        public MatchType matchType;
        public override SearchResult Search(TaskInfo taskInfo, Search search, List<string> predecessors)
        {
            SearchResult searchResult = search.GetSearchResult();
            searchResult.success = true;
            if (taskInfo.subTrees == null || taskInfo.subTrees.Count == 0)
                return searchResult;
            searchResult.lastPredecessors.Clear();
            int successCount = 0;
            int failureCount = 0;
            int total = taskInfo.subTrees.Count;
            foreach (TaskInfo child in taskInfo.subTrees)
            {
                if (!child.enable)
                {
                    total--;
                    continue;
                }
                if (!child.VerifyCheck(search.workingState))
                {
                    failureCount++;
                    searchResult.methodTraversalRecord.Increment();
                    child.status = Status.Failure;
                    continue;
                }
                bool childSuccess = true;
                if (child.node is IMethod)
                {
                    var endNodes = searchResult.TraverseDepth(child, search, predecessors);
                    if (endNodes != null)
                    {
                        childSuccess = true;
                        searchResult.lastPredecessors.AddRange(endNodes);
                    }
                    else
                    {
                        childSuccess = false;
                    }
                }
                else
                {
                    var planNode = searchResult.AddToPlan(child, predecessors);
                    searchResult.lastPredecessors.Add(planNode.id);
                }
                if (childSuccess)
                {
                    child.status = Status.Success;
                    child.ApplyEffects(search.workingState);
                    successCount++;
                }
                else
                {
                    child.status = Status.Failure;
                    failureCount++;
                }
            }

            // 支持多种并行策略
            switch (matchType)
            {
                case MatchType.AllSuccess:
                    searchResult.success = (successCount == total);
                    break;
                case MatchType.AllFailure:
                    searchResult.success = (failureCount == total);
                    break;
                case MatchType.AnySuccess:
                    searchResult.success = (successCount > 0);
                    break;
                case MatchType.AnyFailure:
                    searchResult.success = (failureCount > 0);
                    break;
            }
            return searchResult;
        }
    }
}
