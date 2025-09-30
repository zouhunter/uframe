//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-06
//* 描    述：

//* ************************************************************************************
using UFrame.HTN;
using UFrame.BehaviourTree;
using UnityEngine;
using UFrame.BehaviourTree.Composite;
using UFrame.BehaviourTree.Actions;
using System.Collections.Generic;

namespace UFrame
{
    public class TestHTN : NTreeRunnerBase
    {
        [ContextMenu("TestPlan")]
        public void TestPlan()
        {
            _instanceTree = GameObject.Instantiate(_nTree);
            _instanceTree.StartUp();
            _instanceTree.Plan.Set(new List<PlanNode>());
            _instanceTree.UpdateSearchPlan(false);
        }
        protected override NTree CreateInstanceTree()
        {
            return GameObject.Instantiate(_nTree);
        }
    }
}

