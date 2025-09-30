using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UFrame.YieldFlow;

public class TaskQueueTests
{
    // 简单Action任务节点
    class TestTaskNode : FlowNode
    {
        private readonly float _delay;
        private readonly bool _shouldFail;
        public bool Executed { get; private set; }
        public TestTaskNode(float delay = 0, bool shouldFail = false)
        {
            _delay = delay;
            _shouldFail = shouldFail;
        }
        protected override IEnumerator Run()
        {
            yield return new WaitForSeconds(_delay);
            Executed = true;
            if (_shouldFail)
                MarkFailed();
        }
    }

    [UnityTest]
    public IEnumerator 串行任务全部成功()
    {
        var t1 = new TestTaskNode(0.1f);
        var t2 = new TestTaskNode(0.1f);
        var builder = new FlowBuilder().Task(t1).Then(t2);
        var queue = new FlowQueue("test1", builder.Build(), null, null);
        yield return queue.Execute();
        Assert.AreEqual(Status.Success, t1.Status);
        Assert.AreEqual(Status.Success, t2.Status);
    }

    [UnityTest]
    public IEnumerator 并行任务全部成功()
    {
        var t1 = new TestTaskNode(0.1f);
        var t2 = new TestTaskNode(0.1f);
        var builder = new FlowBuilder().Parallel(t1, t2);
        var queue = new FlowQueue("test2", builder.Build(), null, null);
        yield return queue.Execute();
        Assert.AreEqual(Status.Success, t1.Status);
        Assert.AreEqual(Status.Success, t2.Status);
    }

    [UnityTest]
    public IEnumerator 依赖任务失败_后续不执行()
    {
        var t1 = new TestTaskNode(0.05f, shouldFail: true);
        var t2 = new TestTaskNode(0.05f);
        var builder = new FlowBuilder().Task(t1).Then(t2);
        var queue = new FlowQueue("test3", builder.Build(), null, null);
        yield return queue.Execute();
        Assert.AreEqual(Status.Failure, t1.Status);
        // t2依赖t1失败，理论上不会被执行
        Assert.AreEqual(Status.Inactive, t2.Status);
    }

    [UnityTest]
    public IEnumerator 并行任务部分失败()
    {
        var t1 = new TestTaskNode(0.05f, shouldFail: true);
        var t2 = new TestTaskNode(0.05f);
        var builder = new FlowBuilder().Parallel(t1, t2);
        var queue = new FlowQueue("test4", builder.Build(), null, null);
        yield return queue.Execute();
        Assert.AreEqual(Status.Failure, t1.Status);
        Assert.AreEqual(Status.Success, t2.Status);
    }
}