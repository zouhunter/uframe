using System.Collections.Generic;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
	public class SequenceMethodNode : MethodNode
	{
		public override SearchResult Search(TaskInfo taskInfo, Search search, List<string> predecessors)
		{
			SearchResult searchResult = search.GetSearchResult();
			searchResult.success = true;
			searchResult.lastPredecessors.Clear();
			if (predecessors != null)
				searchResult.lastPredecessors.AddRange(predecessors);

			foreach (TaskInfo child in taskInfo.subTrees)
			{
				if (!child.enable)
					continue;
				if (!child.VerifyCheck(search.workingState))
				{
					child.status = Status.Failure;
					searchResult.success = false;
					break;
				}
				if (child.node is IMethod)
				{
					var endNodes = searchResult.TraverseDepth(child, search, searchResult.lastPredecessors);
					if (endNodes == null)
					{
						child.status = Status.Failure;
						searchResult.success = false;
						break;
					}
					else
					{
						searchResult.lastPredecessors.Clear();
						searchResult.lastPredecessors.AddRange(endNodes);
					}
				}
				else
				{
					var planNode = searchResult.AddToPlan(child, searchResult.lastPredecessors);
					searchResult.lastPredecessors.Clear();
					searchResult.lastPredecessors.Add(planNode.id);
				}
				child.status = Status.Success;
				child.ApplyEffects(search.workingState);
			}
			taskInfo.status = searchResult.success ? Status.Success : Status.Failure;
			return searchResult;
		}
	}
}
