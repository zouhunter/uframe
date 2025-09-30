using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public abstract class MethodNode : ParentNode, IMethod
    {
        public abstract SearchResult Search(TaskInfo taskInfo, Search search, List<string> predecessors);
    }
}
