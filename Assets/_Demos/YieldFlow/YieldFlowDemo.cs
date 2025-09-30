using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.YieldFlow;

/// <summary>
/// TaskQueue 框架典型用法演示Demo。
/// </summary>
public class YieldFlowDemo : MonoBehaviour
{
    private FlowManager _manager;
    private string _queueId = "demoQueue";
    private int _runCount = 0;

    void Start()
    {
        // 初始化任务队列管理器，传入本MonoBehaviour作为runner
        _manager = new FlowManager(this);
        Debug.Log("[Demo] TaskQueueManager 初始化完成");
        StartDemo();
    }

    void StartDemo()
    {
        // 构建任务流：串行->并行->串行
        var t1 = new DemoTaskNode("A", 1f, 0.9f); // 90%成功
        var t2 = new DemoTaskNode("B", 1f, 0.9f); // 90%成功
        var t3 = new DemoTaskNode("C", 1f, 0.8f); // 80%成功
        var t31 = new DemoTaskNode("C1", 1f, 0.3f); // 100%成功
        var t4 = new DemoTaskNode("D", 2f, 0.8f); // 80%成功
        var t41 = new DemoTaskNode("D1", 3f, 0.5f); // 100%成功
        var t5 = new DemoTaskNode("E", 1f, 1f);   // 100%成功

        var builder = new FlowBuilder()
            .Task(t1)
            .Then(t2);
        var paras = builder.Parallels(t3, t4);
        paras[0].Then(t31).Return();
        paras[1].Then(t41).Return();
        builder.Then(t5).SetRelaxed();

        // 启动队列
        Debug.Log("[Demo] 任务队列启动");
        _manager.StartQueue(_queueId, builder.Build(), OnQueueComplete);
    }

    void OnQueueComplete(string id, bool success)
    {
        _runCount++;
        Debug.Log($"[Demo] 队列{id} 执行完毕，结果：{success}，第{_runCount}次");
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 200), GUI.skin.box);
        GUILayout.Label($"TaskQueue Demo 控制面板 (第{_runCount}次)");

        if (GUILayout.Button("取消队列"))
        {
            Debug.Log("[Demo] 取消队列");
            _manager.CancelQueue(_queueId);
        }
        if (GUILayout.Button("立即重开队列"))
        {
            Debug.Log("[Demo] 立即重开队列");
            StartDemo();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// Demo用任务节点，支持延迟、日志和概率成功/失败
    /// </summary>
    class DemoTaskNode : FlowNode
    {
        private readonly float _delay;
        private readonly float _successProbability;
        public DemoTaskNode(string name, float delay, float successProbability = 1f)
        {
            this.name = name;
            _delay = delay;
            _successProbability = Mathf.Clamp01(successProbability);
        }
        protected override IEnumerator Run()
        {
            Debug.Log($"[DemoTask] {name} 开始");
            yield return new WaitForSeconds(_delay);
            bool isSuccess = UnityEngine.Random.value < _successProbability;
            if (isSuccess)
            {
                Debug.Log($"[DemoTask] {name} 成功");
            }
            else
            {
                Debug.LogWarning($"[DemoTask] {name} 失败");
                MarkFailed();
            }
            Debug.Log($"[DemoTask] {name} 完成");
        }
    }
}