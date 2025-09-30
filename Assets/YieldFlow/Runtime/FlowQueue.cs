//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-30
//* 描    述：
// 单个任务队列的封装与调度逻辑。
//* ************************************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UFrame.YieldFlow
{
    /// <summary>
    /// 封装的单个队列执行体。
    /// 负责调度、执行、回收一组有依赖关系的任务节点。
    /// </summary>
    public class FlowQueue
    {
        /// <summary>
        /// 队列唯一ID。
        /// </summary>
        private readonly string _id;
        /// <summary>
        /// 该队列包含的所有任务节点（带依赖关系）。
        /// </summary>
        private readonly List<FlowNode> _tasks;
        /// <summary>
        /// 队列全部完成时的回调（参数：队列ID，是否全部成功）。
        /// </summary>
        private readonly Action<string, bool> _onComplete;
        /// <summary>
        /// 协程调度器（Mono管理器）。
        /// </summary>
        private readonly MonoBehaviour _runner;
        /// <summary>
        /// 是否已被取消。
        /// </summary>
        private bool _isCancelled = false;
        /// <summary>
        /// 当前所有正在运行的协程列表。
        /// </summary>
        private List<Coroutine> _runningCoroutines = new();

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">队列唯一ID</param>
        /// <param name="tasks">任务节点列表</param>
        /// <param name="onComplete">全部完成时回调</param>
        /// <param name="runner">Mono管理器（用于启动协程）</param>
        /// <param name="maxConcurrency">最大并发数</param>
        public FlowQueue(string id, List<FlowNode> tasks, Action<string, bool> onComplete, MonoBehaviour runner)
        {
            _id = id;
            _tasks = tasks;
            _onComplete = onComplete;
            _runner = runner;
        }

        /// <summary>
        /// 初始化依赖关系，找出所有根节点，并建立每个节点的后继节点字典。
        /// </summary>
        /// <param name="rootNodes">输出：所有无前置依赖的根节点</param>
        /// <returns>每个节点的后继节点字典</returns>
        private Dictionary<FlowNode, List<FlowNode>> InitFlowNodes(out List<FlowNode> rootNodes)
        {
            rootNodes = new List<FlowNode>();
            var followNodes = new Dictionary<FlowNode, List<FlowNode>>();

            // 遍历所有任务节点，建立依赖关系
            foreach (var task in _tasks)
            {
                // 获取当前节点的前置节点列表
                var predecessors = task.Predecessors;

                // 如果没有前置节点，直接加入根节点列表
                if (predecessors == null || predecessors.Count == 0)
                {
                    rootNodes.Add(task);
                    continue;
                }

                // 为每个前置节点建立后续节点关系
                foreach (var pred in predecessors)
                {
                    if (!followNodes.ContainsKey(pred))
                    {
                        followNodes[pred] = new List<FlowNode>();
                    }
                    followNodes[pred].Add(task);
                }
            }
            return followNodes;
        }

        /// <summary>
        /// 事件驱动式调度：遍历当前活跃节点列表，执行可执行节点，处理完成与失败，动态激活后继节点。
        /// </summary>
        /// <param name="tasks">当前活跃节点列表</param>
        /// <param name="folowNodes">后继节点字典</param>
        /// <param name="success">是否全部成功</param>
        /// <returns>是否还有节点在运行</returns>
        private bool RunNodes(List<FlowNode> tasks, Dictionary<FlowNode, List<FlowNode>> folowNodes, ref bool success)
        {
            success = false;
            int runningCount = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                // 未激活状态，判断是否可执行
                if (task.Status == Status.Inactive)
                {
                    if (task.ShouldNeverExecute())
                    {
                        // 永不执行的节点直接标记失败
                        task.MarkFailed();
                    }
                    else if (task.CanExecute())
                    {
                        // 可执行则启动协程
                        Coroutine co = _runner.StartCoroutine(task.RunWrapper());
                        _runningCoroutines.Add(co);
                        runningCount++;
                    }
                }
                // 正在运行，计数
                else if (task.Status == Status.Running)
                {
                    runningCount++;
                }
                // 执行成功，移除并激活后继节点
                else if (task.Status == Status.Success)
                {
                    tasks.Remove(task);
                    i--;
                    var haveNext = FlowNext(task, tasks, folowNodes);
                    if (!haveNext)
                        success = true;
                }
                // 执行失败，移除
                else if (task.Status == Status.Failure)
                {
                    tasks.Remove(task);
                    i--;
                }
            }
            return runningCount > 0;
        }

        /// <summary>
        /// 激活指定节点的所有后继节点（如果未在活跃列表中）。
        /// </summary>
        /// <param name="task">已完成的节点</param>
        /// <param name="nodes">当前活跃节点列表</param>
        /// <param name="followNodes">后继节点字典</param>
        /// <returns>是否有后继节点被激活</returns>
        private bool FlowNext(FlowNode task, List<FlowNode> nodes, Dictionary<FlowNode, List<FlowNode>> followNodes)
        {
            if (followNodes.TryGetValue(task, out var flowNodes))
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
        /// 取消队列，立即终止所有未完成任务，并回调失败。
        /// </summary>
        public void Cancel()
        {
            _isCancelled = true;
            // 立即终止所有未完成任务
            foreach (var task in _tasks)
            {
                if (task.Status == Status.Inactive || task.Status == Status.Running)
                    task.Interrupt();
            }
            // 立即回调，确保同帧移除
            _onComplete?.Invoke(_id, false);
        }

        /// <summary>
        /// 执行整个任务队列（自动调度依赖、并发、失败检测、回收）。
        /// </summary>
        /// <returns>协程迭代器</returns>
        public IEnumerator Execute()
        {
            bool success = true;
            // 初始化依赖关系，获取根节点和后继字典
            var followNodes = InitFlowNodes(out var activeNodes);
            while (!_isCancelled)
            {
                // 事件驱动调度，处理当前活跃节点
                var running = RunNodes(activeNodes, followNodes, ref success);
                if (!running)
                {
                    break;
                }
                else
                {
                    yield return null;
                }
            }

            // 等待所有子任务完成（理论上此时都已完成，仅做安全兜底）
            foreach (var co in _runningCoroutines)
            {
                if (co != null)
                    _runner.StopCoroutine(co);
            }

            // 队列完成时回调，由回调方决定是否回收队列
            if (!_isCancelled)
                _onComplete?.Invoke(_id, success);
        }
    }
}

