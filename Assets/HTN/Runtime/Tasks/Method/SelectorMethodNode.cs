using System.Collections.Generic;
using UFrame.BehaviourTree;
using UFrame.BehaviourTree.Composite;

namespace UFrame.HTN
{
	public class SelectorMethodNode : MethodNode
	{
		public override SearchResult Search(TaskInfo task, Search search, List<string> predecessors)
		{
			SearchResult searchResult = search.GetSearchResult();
			searchResult.success = true;

			for (int i = 0; i < task.subTrees.Count; i++)
			{
				TaskInfo child = task.subTrees[i] as TaskInfo;
				if (!child.enable)
					continue;
				if (!child.VerifyCheck(search.workingState))
				{
					searchResult.methodTraversalRecord.Increment();
					continue;
				}

				if (child is IMethod)
				{
					var endNodes = searchResult.TraverseDepth(child, search, predecessors);
					if (endNodes == null)
					{
						searchResult.methodTraversalRecord.Increment();
						continue;
					}
					searchResult.lastPredecessors.Clear();
					searchResult.lastPredecessors.AddRange(endNodes);
				}
				else
				{
					var planNode = searchResult.AddToPlan(child, predecessors);
					searchResult.lastPredecessors.Clear();
					searchResult.lastPredecessors.Add(planNode.id);
				}

				child.ApplyEffects(search.workingState);
				searchResult.success = true;
				break;
			}
			if (task.subTrees == null || task.subTrees.Count == 0)
			{
				searchResult.success = true;
			}
			return searchResult;
		}
	}
}
