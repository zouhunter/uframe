//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-30
//* 描    述：

//* ************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.YieldFlow
{
    /// <summary>
    /// 任务流节点基类。支持依赖、状态流转、可扩展Run逻辑。
    /// </summary>
    public abstract class FlowNode
    {
        /// <summary>
        /// 节点名称（便于调试/日志）。
        /// </summary>
        public string name;
        /// <summary>
        /// 当前节点状态（参见Status定义）。
        /// </summary>
        public byte Status { get; protected set; } = YieldFlow.Status.Inactive;

        /// <summary>
        /// 前置依赖节点列表。
        /// </summary>
        private readonly List<FlowNode> _predecessors = new();
        public List<FlowNode> Predecessors => _predecessors;
        /// <summary>
        /// true: 全部前置节点成功才可执行；false: 任意一个前置成功即可执行。
        /// </summary>
        private bool _allPredecessorsRequired = true;
        /// <summary>
        /// 设置前置依赖模式（全部/任意）。
        /// </summary>
        public void SetAllPredecessorsRequired(bool allRequired) => _allPredecessorsRequired = allRequired;

        /// <summary>
        /// 状态变更事件（参数：当前节点）。
        /// </summary>
        public event Action<FlowNode> OnStatusChanged;

        /// <summary>
        /// 添加前置依赖节点。
        /// </summary>
        public void AddPredecessor(FlowNode node)
        {
            if (!_predecessors.Contains(node))
                _predecessors.Add(node);
        }

        /// <summary>
        /// 判断当前节点是否可以执行（依赖全部满足）。
        /// </summary>
        public bool CanExecute()
        {
            if (Status != YieldFlow.Status.Inactive)
                return false;
            if (_predecessors.Count == 0)
                return true;
            bool haveSuccess = false;
            for (int i = 0; i < _predecessors.Count; i++)
            {
                var status = _predecessors[i].Status;
                if (status == YieldFlow.Status.Inactive || status == YieldFlow.Status.Running)
                    return false;
                if (status == YieldFlow.Status.Failure && _allPredecessorsRequired)
                    return false;
                if (status == YieldFlow.Status.Success)
                    haveSuccess = true;
            }
            return haveSuccess;
        }

        /// <summary>
        /// 包装Run方法，自动处理状态流转（Running→Success），并触发事件。
        /// </summary>
        public IEnumerator RunWrapper()
        {
            if (Status != YieldFlow.Status.Inactive)
                yield break;

            Status = YieldFlow.Status.Running;
            OnStatusChanged?.Invoke(this);

            yield return Run();

            if (Status == YieldFlow.Status.Running)
            {
                Status = YieldFlow.Status.Success;
                OnStatusChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// 中断当前节点（状态变为Interrupt）。
        /// </summary>
        public void Interrupt()
        {
            if (Status == YieldFlow.Status.Running || Status == YieldFlow.Status.Inactive)
            {
                Status = YieldFlow.Status.Interrupt;
                OnStatusChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// 标记当前节点为失败（仅在Running时有效）。
        /// </summary>
        public void MarkFailed()
        {
            if (Status == YieldFlow.Status.Running)
            {
                Status = YieldFlow.Status.Failure;
                OnStatusChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// 判断本任务是否永远无法执行（如前置条件已全部处理但依赖失败等）。默认实现已覆盖常见依赖场景。
        /// </summary>
        public virtual bool ShouldNeverExecute()
        {
            if (_predecessors.Count == 0)
                return false;
            // 所有前置都已完成（非NotStarted/Running）
            bool allPredDone = true;
            for (int i = 0; i < _predecessors.Count; i++)
            {
                var s = _predecessors[i].Status;
                if (s == YieldFlow.Status.Inactive || s == YieldFlow.Status.Running)
                {
                    allPredDone = false;
                    break;
                }
            }
            if (!allPredDone)
                return false;
            if (_allPredecessorsRequired)
            {
                // 只要有一个不是成功就永远无法执行
                for (int i = 0; i < _predecessors.Count; i++)
                {
                    if (_predecessors[i].Status == YieldFlow.Status.Failure)
                        return true;
                }
            }
            else
            {
                // 任意一个成功即可，否则全部失败/中断就永远无法执行
                bool anySuccess = false;
                for (int i = 0; i < _predecessors.Count; i++)
                {
                    if (_predecessors[i].Status == YieldFlow.Status.Success)
                    {
                        anySuccess = true;
                        break;
                    }
                }
                if (!anySuccess)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 派生类需实现的具体任务逻辑（协程）。
        /// </summary>
        protected abstract IEnumerator Run();
    }

}

