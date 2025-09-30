using System;
using System.Collections.Generic;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
	/// <summary>
	/// A plan contains a list of action nodes that the agent will sequentially execute. The plan keep tracks of the
	/// method traversal index so that it can compare to cheaper options.
	/// </summary>
	[Serializable]
	public class Planner
	{
		/// <summary>
		/// Delegate for plan changes.
		/// </summary>
		public delegate void PlanHandler();

		/// <summary>
		/// Delegate for plan node Status changes.
		/// </summary>
		public delegate void PlanNodeHandler(PlanNode planNode, byte Status);

		/// <summary>
		/// Measurement of how many steps are needed to take to execute plan.
		/// </summary>
		public MethodTraversalRecord mtr;

		/// <summary>
		/// Action node list.
		/// </summary>
		public List<PlanNode> nodes;

		/// <summary>
		/// On plan cleared or replaced.
		/// </summary>
		public PlanHandler PlanChanged { get; set; }

		/// <summary>
		/// On plan node Status changed.
		/// </summary>
		public PlanNodeHandler StatusChanged { get; set; }

		protected Stack<PlanNode> _nodePool;
		protected Dictionary<PlanNode, List<PlanNode>> _planNodeMap;
		protected List<PlanNode> _runningNodes;
		protected byte _runingStatus;
		protected WorldCopyStates _worldCopyState;
		protected Dictionary<string, PlanNode> _planNodeIdMap;
		/// <summary>
		/// Constructor. Initiates a default PlanNodeFactory.
		/// </summary>
		public Planner()
		{
			_worldCopyState = new WorldCopyStates();
			mtr = new MethodTraversalRecord();
			nodes = new List<PlanNode>();
			_nodePool = new Stack<PlanNode>();
			_planNodeMap = new Dictionary<PlanNode, List<PlanNode>>();
			_runningNodes = new List<PlanNode>();
			_planNodeIdMap = new Dictionary<string, PlanNode>();
		}

		public PlanNode GetPlanNode(TaskInfo taskInfo)
		{
			PlanNode node = null;
			if (_nodePool.Count > 0)
			{
				node = _nodePool.Pop();
			}
			if (node == null)
			{
				node = new PlanNode();
			}
			node.SetTask(taskInfo);
			return node;
		}

		public void Release(PlanNode planNode)
		{
			_nodePool.Push(planNode);
		}
		/// <summary>
		/// Clears all nodes from the current plan.
		/// </summary>
		public void Clear()
		{
			mtr = null;
			if (nodes.Count > 0)
			{
				foreach (var item in nodes)
				{
					Release(item);
				}
				nodes.Clear();
			}
			_runningNodes.Clear();
			_planNodeMap.Clear();
			PlanChanged?.Invoke();
		}

		/// <summary>
		/// Get the node at the current plan index.
		/// </summary>
		public List<PlanNode> GetCurrent()
		{
			return _runningNodes;
		}

		/// <summary>
		/// Check if any node in the plan has failed
		/// </summary>
		public bool HasFailed()
		{
			foreach (PlanNode node in nodes)
			{
				if (node.status == Status.Failure)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Check if all nodes in the plan are successful
		/// </summary>
		public bool IsComplete()
		{
			if (IsEmpty())
			{
				return true;
			}
			if (_runningNodes.Count == 0)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Check if plan is empty of nodes.
		/// </summary>
		public bool IsEmpty()
		{
			return nodes.Count <= 0;
		}
		/// <summary>
		/// Check if action nodes are sub-part of the current plan.
		/// </summary>
		public bool IsPartial(List<PlanNode> actionNodes)
		{
			if (nodes.Count == 0 || actionNodes.Count == 0)
			{
				return false;
			}
			if (actionNodes.Count > nodes.Count)
			{
				return false;
			}
			foreach (PlanNode node in nodes)
			{
				if (node.status == Status.Failure)
				{
					return false;
				}
			}
			//TODO 修复模板判断方法
			return false;
		}

		/// <summary>
		/// Increment the plan index. Used to traverse the plan.
		/// </summary>
		public bool Next(PlanNode task)
		{
			if (_planNodeMap.TryGetValue(task, out var flowNodes))
			{
				foreach (var nextNode in flowNodes)
				{
					if (nodes.Contains(nextNode))
						continue;
					nodes.Add(nextNode);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Set a new plan from a search result. The nodes
		/// and MTR in the search result is added to the plan.
		/// </summary>
		public void Set(SearchResult searchResult)
		{
			Set(searchResult.plan);
			mtr = searchResult.methodTraversalRecord;
		}

		/// <summary>
		/// Set a new plan from a new set of action nodes.
		/// Previous plan is cleared and new nodes are added.
		/// </summary>
		public void Set(List<PlanNode> actionNodes)
		{
			Clear();
			_runingStatus = 0;
			nodes.AddRange(actionNodes);
			PlanChanged?.Invoke();
			InitPlanNodes(actionNodes);
		}

		/// <summary>
		/// 初始化依赖关系，找出所有根节点，并建立每个节点的后继节点字典。
		/// </summary>
		/// <param name="rootNodes">输出：所有无前置依赖的根节点</param>
		/// <returns>每个节点的后继节点字典</returns>
		private void InitPlanNodes(List<PlanNode> allNodes)
		{
			_runningNodes = _runningNodes ?? new List<PlanNode>();
			_runningNodes.Clear();
			_planNodeMap = _planNodeMap ?? new Dictionary<PlanNode, List<PlanNode>>();
			_planNodeMap.Clear();
			_planNodeIdMap.Clear();
			foreach (var node in allNodes)
			{
				_planNodeIdMap[node.id.ToString()] = node;
			}
			// 遍历所有任务节点，建立依赖关系
			foreach (var node in allNodes)
			{
				// 获取当前节点的前置节点列表
				var predecessors = node.Predecessors;

				// 如果没有前置节点，直接加入根节点列表
				if (predecessors == null || predecessors.Count == 0)
				{
					_runningNodes.Add(node);
					continue;
				}

				// 为每个前置节点建立后续节点关系
				foreach (var predId in predecessors)
				{
					var pred = _planNodeIdMap[predId];
					if (!_planNodeMap.ContainsKey(pred))
					{
						_planNodeMap[pred] = new List<PlanNode>();
					}
					_planNodeMap[pred].Add(node);
				}
			}
		}
		/// <summary>
		/// Updates the current plan. Validate the current node conditions and
		/// set it to running if it hasn't started. Execute inherited node
		/// custom logic if Status is 'Running' and let the node update the Status.
		/// Apply node effects if it was successful.
		/// </summary>
		public byte Tick(IOwner owner)
		{
			var context = owner as NTree;
			if (context.worldState == null)
			{
				return Status.Inactive;
			}
			if (IsComplete())
			{
				return Status.Inactive;
			}
			_runingStatus = Status.Running;
			for (int i = 0; i < _runningNodes.Count; i++)
			{
				var node = _runningNodes[i];
				if (node.CanExecute(_planNodeIdMap))
				{
					// 可执行则启动协程
					node.Execute(node);
				}
				// 执行成功，移除并激活后继节点
				if (node.status == Status.Success)
				{
					ApplyEffects(node, context.worldState, false);
					_runningNodes.Remove(node);
					i--;
					bool haveNext = Next(node);
					if (!haveNext)
						_runingStatus = Status.Success;
				}
				// 执行失败，移除
				else if (node.status == Status.Failure)
				{
					_runningNodes.Remove(node);
					i--;
					_runingStatus = Status.Failure;
				}
			}
			if (_runningNodes.Count > 0)
			{
				return Status.Running;
			}
			return _runingStatus;
		}

		/// <summary>
		/// Verify that the current plan is valid. Checks that each node
		/// condition is valid against the attributes in a blackboard.
		/// </summary>
		public bool Verify(WorldRefStates blackboard)
		{
			if (IsEmpty())
			{
				return true;
			}
			blackboard.CopyTo(_worldCopyState);
			for (int i = 0; i < _runningNodes.Count; i++)
			{
				PlanNode planNode = nodes[i];
				if (!TaskInfoUtil.VerifyCheck(planNode, _worldCopyState))
				{
					return false;
				}
				ApplyEffects(planNode, _worldCopyState, true);
			}
			return true;
		}

		/// <summary>
		/// Applies the effects in a node to the attributes in a blackboard.
		/// </summary>
		protected void ApplyEffects(TaskInfo node, IWorldStates states, bool plan)
		{
			foreach (EffectInfo effect in node.effects)
			{
				if (plan && effect.apply == EffectApply.Run)
					continue;
				else if (!plan && effect.apply == EffectApply.Plan)
					continue;
				effect.Apply(states);
			}
		}
	}
}
