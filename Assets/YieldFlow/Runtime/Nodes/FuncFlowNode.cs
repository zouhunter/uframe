//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-30
//* 描    述：

//* ************************************************************************************
using System;
using System.Collections;

namespace UFrame.YieldFlow
{
    /// <summary>
    /// 执行一个自定义的协程任务。
    /// </summary>
    public class FuncTaskNode : FlowNode
    {
        private readonly Func<IEnumerator> _action;
        public FuncTaskNode(Func<IEnumerator> action) { _action = action; }
        protected override IEnumerator Run() => _action();
    }
}

