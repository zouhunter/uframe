using System.Collections;
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-30
//* 描    述：
// 任务队列全局管理器，支持多队列并发、自动回收、单例模式。
//* ************************************************************************************
using UnityEngine;
using System;
using System.Collections.Generic;

namespace UFrame.YieldFlow
{
    /// <summary>
    /// 任务队列全局管理器（非MonoBehaviour）。
    /// 用于统一调度和管理所有任务队列。
    /// </summary>
    public class FlowManager
    {
        /// <summary>
        /// 全局唯一实例。
        /// </summary>
        public static FlowManager Instance { get; private set; }

        /// <summary>
        /// 当前正在运行的所有任务队列（按ID索引）。
        /// </summary>
        private readonly Dictionary<string, FlowQueue> _runningQueues = new();

        private MonoBehaviour _runner;
        private GameObject _autoRunnerGo;

        /// <summary>
        /// 构造函数，支持外部传入MonoBehaviour作为runner。
        /// </summary>
        /// <param name="runner">用于协程调度的MonoBehaviour（可选）</param>
        public FlowManager(MonoBehaviour runner = null)
        {
            if (Instance != null)
                throw new Exception("TaskQueueManager 已经实例化");
            Instance = this;
            if (runner != null)
            {
                _runner = runner;
            }
        }

        /// <summary>
        /// 获取或自动创建MonoBehaviour runner。
        /// </summary>
        private MonoBehaviour GetRunner()
        {
            if (_runner != null)
                return _runner;
            if (_autoRunnerGo == null)
            {
                _autoRunnerGo = new GameObject("TaskQueueRunner(Auto)");
                _autoRunnerGo.hideFlags = HideFlags.HideAndDontSave;
                _runner = _autoRunnerGo.AddComponent<InternalTaskQueueRunner>();
                UnityEngine.Object.DontDestroyOnLoad(_autoRunnerGo);
            }
            return _runner;
        }

        /// <summary>
        /// 内部隐藏MonoBehaviour用于协程调度。
        /// </summary>
        private class InternalTaskQueueRunner : MonoBehaviour { }

        /// <summary>
        /// 启动一个新的任务队列。
        /// </summary>
        /// <param name="queueId">唯一队列ID</param>
        /// <param name="tasks">任务节点列表（已构建好依赖关系）</param>
        /// <param name="onComplete">队列全部完成时回调（参数：队列ID，是否全部成功）</param>
        public void StartQueue(string queueId, List<FlowNode> tasks, Action<string, bool> onComplete)
        {
            if (_runningQueues.ContainsKey(queueId))
            {
                Debug.LogWarning($"任务队列 {queueId} 已存在！");
                return;
            }
            // 包装回调，确保异常或正常都能移除队列
            void SafeOnComplete(string id, bool success)
            {
                try
                {
                    onComplete?.Invoke(id, success);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"TaskQueue回调异常: {ex}");
                }
                finally
                {
                    RemoveQueue(id);
                }
            }
            var runner = GetRunner();
            FlowQueue queue = new FlowQueue(queueId, tasks, SafeOnComplete, runner);
            _runningQueues.Add(queueId, queue);
            runner.StartCoroutine(queue.Execute());
        }

        /// <summary>
        /// 移除已完成的队列（内部调用）。
        /// </summary>
        /// <param name="id">队列ID</param>
        public void RemoveQueue(string id)
        {
            _runningQueues.Remove(id);
        }

        /// <summary>
        /// 取消指定队列
        /// </summary>
        public void CancelQueue(string queueId)
        {
            if (_runningQueues.TryGetValue(queueId, out var queue))
                queue.Cancel();
        }
    }
}


