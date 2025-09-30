using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UFrame.BehaviourTree;
using UnityEngine;

namespace UFrame.HTN
{
	/// <summary>
	/// Node used in agent plans.
	/// </summary>
	[Serializable]
	public class PlanNode : TaskInfo
	{
		/// <summary>
		/// Check if effects have been applied.
		/// </summary>
		public bool hasAppliedEffects;

		/// <summary>
		/// Local copy of source node.
		/// </summary>
		private TaskInfo _task;

		/// <summary>
		/// 前置依赖节点列表。
		/// </summary>
		private List<string> _predecessors = new();

		/// <summary>
		/// true: 全部前置节点成功才可执行；false: 任意一个前置成功即可执行。
		/// </summary>
		private bool _allPredecessorsRequired = true;

		public List<string> Predecessors => _predecessors;

		public TaskInfo BaseTask => _task;

		/// <summary>
		/// Empty constructor for serialization.
		/// </summary>
		public PlanNode()
		{
		}

		/// <summary>
		/// Create a new plan node and make a copy of an action node.
		/// </summary>
		public void SetTask(TaskInfo node)
		{
			_task = node;
			node.CopyTo(this);
			id = node.id;
			this.status = Status.Inactive;
		}

		/// <summary>
		/// 设置前置依赖模式（全部/任意）。
		/// </summary>
		public void SetAllPredecessorsRequired(bool allRequired) => _allPredecessorsRequired = allRequired;

		/// <summary>
		/// 状态变更事件（参数：当前节点）。
		/// </summary>
		public event Action<PlanNode> OnStatusChanged;

		//
		public bool HavePredecessor => _predecessors.Count > 0;
		/// <summary>
		/// 添加前置依赖节点。
		/// </summary>
		public void AddPredecessor(string id)
		{
			if (!_predecessors.Contains(id))
				_predecessors.Add(id);
		}

		/// <summary>
		/// 添加前置依赖节点。
		/// </summary>
		public void AddPredecessors(List<string> ids)
		{
			if (ids == null)
				return;

			foreach (var id in ids)
			{
				if (!_predecessors.Contains(id))
					_predecessors.Add(id);
			}
		}

		/// <summary>
		/// 判断当前节点是否可以执行（依赖全部满足）。
		/// </summary>
		public bool CanExecute(Dictionary<string, PlanNode> nodeMap)
		{
			if (status == YieldFlow.Status.Inactive)
			{
				if (_predecessors.Count == 0)
					return true;
				bool haveSuccess = false;
				for (int i = 0; i < _predecessors.Count; i++)
				{
					var status = nodeMap[_predecessors[i]].status;
					if (status == YieldFlow.Status.Inactive || status == YieldFlow.Status.Running)
						return false;
					if (status == YieldFlow.Status.Failure && _allPredecessorsRequired)
						return false;
					if (status == YieldFlow.Status.Success)
						haveSuccess = true;
				}
				if (haveSuccess)
				{//中断不需要的前置任务
					foreach (var item in _predecessors)
					{
						var node = nodeMap[item];
						if (node.status != Status.Success || node.status != Status.Failure)
						{
							node.status = Status.Interrupt;
						}
					}
				}
				return haveSuccess;
			}
			if (status == Status.Running)
				return true;
			return false;
		}

		/// <summary>
		/// 中断当前节点（状态变为Interrupt）。
		/// </summary>
		public void Interrupt()
		{
			if (status == YieldFlow.Status.Running || status == YieldFlow.Status.Inactive)
			{
				status = YieldFlow.Status.Interrupt;
				OnStatusChanged?.Invoke(this);
			}
		}

		/// <summary>
		/// 标记当前节点为失败（仅在Running时有效）。
		/// </summary>
		public void MarkFailed()
		{
			if (status == YieldFlow.Status.Running)
			{
				status = YieldFlow.Status.Failure;
				OnStatusChanged?.Invoke(this);
			}
		}

		/// <summary>
		/// Execute inherited logic in action node and update the
		/// status based on the result.
		/// </summary>
		public byte Execute(ITreeInfo context)
		{
			var nextStatus = node.Execute(context);
			if (status != nextStatus)
			{
				status = nextStatus;
				OnStatusChanged?.Invoke(this);
			}
			return status;
		}
	}
}
