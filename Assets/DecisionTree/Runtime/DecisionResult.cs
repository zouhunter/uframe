using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Decision
{
    [System.Serializable]
    public class DecisionResult
    {
        public string key;
        public string desc;
        public virtual object Value { get; }
        public override string ToString()
        {
            return key + "->" + Value + (string.IsNullOrEmpty(desc) ? "" : "(" + desc + ")");
        }
    }

    public class DecisionIntResult : DecisionResult
    {
        public int value;
        public override object Value => value;
    }
    public class DecisionBoolResult : DecisionResult
    {
        public bool value;
        public override object Value => value;
    }
    public class DecisionStringResult : DecisionResult
    {
        public string value;
        public override object Value => value;
    }
    public class DecisionFloatResult : DecisionResult
    {
        public float value;
        public override object Value => value;
    }
}
