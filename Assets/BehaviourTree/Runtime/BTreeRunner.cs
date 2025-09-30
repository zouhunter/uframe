/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 行为树行为脚本
 *_*/

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BehaviourTree
{
    public class BTreeRunner : BTreeRunnerBase
    {
        [SerializeField]
        protected BTree _bt;
        protected override BTree CreateInstanceTree()
        {
            return _bt?.CreateInstance();
        }
    }
}

