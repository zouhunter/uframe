using System.Collections.Generic;
using System.Linq;
using UFrame.BehaviourTree;
using UFrame.BehaviourTree.Composite;

namespace UFrame.HTN
{
	public interface IMethod
	{
		SearchResult Search(TaskInfo taskInfo, Search search, List<string> predecessors);
	}
}
