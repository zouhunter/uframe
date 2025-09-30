using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.EasyBT.Tasks
{
    [AddComponentMenu("BehaviourTree/Action/Unity/Debugger")]
    public class Debugger : BaseNode
    {
        public LogType logType;
        public string text;

        protected override NodeStatus OnUpdate()
        {
            Debug.unityLogger.Log(logType, text);
            return NodeStatus.Success;
        }
    }
}
