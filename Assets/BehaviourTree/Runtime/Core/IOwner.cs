using UFrame.BehaviourTree.Actions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    public interface IOwner : IVariableProvider, IEventProvider
    {

        // 当前Tick计数
        int TickCount { get; }

        // 是否输出日志
        bool LogInfo { get; }
    }
}
