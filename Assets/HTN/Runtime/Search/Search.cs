using System;
using System.Collections.Generic;
using UFrame.BehaviourTree;
using UFrame.BehaviourTree.Composite;

namespace UFrame.HTN
{
	/// <summary>
	/// Searches a node for a plan of action.
	/// </summary>
	public class Search
	{
		public readonly IOwner context;
		public readonly WorldCopyStates workingState;
		public readonly Planner planner;
		private Stack<SearchResult> _resultPool;

		/// <summary>
		/// Constructor
		/// </summary>
		public Search(IOwner context, WorldRefStates state, Planner planner)
		{
			this.workingState = new WorldCopyStates();
			state.MakeCopy(workingState);
			this.context = context;
			this.planner = planner;
			_resultPool = new Stack<SearchResult>();
		}

		public SearchResult GetSearchResult()
		{
			SearchResult result = null;
			if (_resultPool.Count > 0)
				result = _resultPool.Pop();
			if (result == null)
				result = new SearchResult(planner);
			result.plan.Clear();
			result.success = true;
			result.lastPredecessors.Clear();
			return result;
		}

		public void SaveBack(SearchResult result)
		{
			if (result == null)
				return;
			result.plan.Clear();
			result.lastPredecessors.Clear();
			_resultPool.Push(result);
		}

		/// <summary>
		/// Search for action plan in a node. Searches the nodes children
		/// recursively by a depth-first search.
		/// </summary>
		public bool Run(TaskInfo taskInfo, SearchResult searchResult)
		{
			if (!taskInfo.enable)
				return false;
			var node = taskInfo.node;
			if (node is IMethod method)
			{
				searchResult.success = false;
				var tempSearch = method.Search(taskInfo, this, null);
				if (tempSearch != null)
				{
					tempSearch.CopyTo(searchResult);
					SaveBack(tempSearch);
				}
			}
			else
			{
				searchResult.AddToPlan(taskInfo, null);
				searchResult.lastPredecessors.Add(taskInfo.id);
			}
			if (searchResult != null && !searchResult.success)
			{
				searchResult.plan.Clear();
				return false;
			}
			return true;
		}
	}
}
