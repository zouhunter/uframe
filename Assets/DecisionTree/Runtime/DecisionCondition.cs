using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Decision
{
    [System.Serializable]
    public class DecisionCondition
    {
        public string desc;
        public string key;
        public CompireType checkCompire;
        public virtual bool Check(Dictionary<string, object> data) => true;
        public virtual bool Check(Func<string, object> func) => true;

    }

    public abstract class DecisionCondition<T> : DecisionCondition
    {
        public virtual object Value { get; }
        public override string ToString()
        {
            string flag = "=";
            switch (checkCompire)
            {
                case CompireType.Equal:
                    flag = "=";
                    break;
                case CompireType.Bigger:
                    flag = ">";
                    break;
                case CompireType.Lower:
                    flag = "<";
                    break;
                case CompireType.GreaterOrEqual:
                    flag = ">=";
                    break;
                case CompireType.NotEqual:
                    flag = "!=";
                    break;
                case CompireType.LessOrEqual:
                    flag = "<=";
                    break;
            }
            return key + flag + Value + (string.IsNullOrEmpty(desc) ? "" : "(" + desc + ")");
        }
        public override bool Check(Dictionary<string, object> data)
        {
            if (data.TryGetValue(key, out var obj) && obj is T value)
            {
                return Verify(value);
            }
            return false;
        }
        public override bool Check(Func<string, object> func)
        {
            var data = func(key);
            if (data != null && data is T value)
            {
                return Verify(value);
            }
            return false;
        }
        protected virtual bool Verify(T value) => false;
    }

    /// <summary>
    /// 比较方式
    /// </summary>
    public enum CompireType
    {
        Equal, // == 
        Bigger, // > 
        Lower,// <
        GreaterOrEqual, // >=
        NotEqual, // !=
        LessOrEqual // <=
    }
    [System.Serializable]
    public class DecisionIntCondition : DecisionCondition<int>
    {
        public int value;
        public override object Value => value;
        protected override bool Verify(int currentValue)
        {
            return checkCompire switch
            {
                CompireType.Equal => currentValue == value,
                CompireType.Bigger => currentValue > value,
                CompireType.Lower => currentValue < value,
                CompireType.GreaterOrEqual => currentValue >= value,
                CompireType.NotEqual => currentValue != value,
                CompireType.LessOrEqual => currentValue <= value,
                _ => false
            };
        }
    }
    [System.Serializable]
    public class DecisionBoolCondition : DecisionCondition<bool>
    {
        public bool value;
        public override object Value => value;
        protected override bool Verify(bool currentValue)
        {
            return checkCompire switch
            {
                CompireType.Equal => currentValue == value,
                CompireType.Bigger => currentValue && !value, // true > false
                CompireType.Lower => !currentValue && value,  // false < true
                CompireType.GreaterOrEqual => (currentValue == value) || (currentValue && !value), // true >= false, true == true
                CompireType.NotEqual => currentValue != value,
                CompireType.LessOrEqual => (currentValue == value) || (!currentValue && value), // false <= true, false == false
                _ => false
            };
        }
    }
    [System.Serializable]
    public class DecisionFloatCondition : DecisionCondition<float>
    {
        public float value;
        public override object Value => value;

        protected override bool Verify(float currentValue)
        {
            return checkCompire switch
            {
                CompireType.Equal => Mathf.Approximately(currentValue, value),
                CompireType.Bigger => currentValue > value,
                CompireType.Lower => currentValue < value,
                CompireType.GreaterOrEqual => currentValue > value || Mathf.Approximately(currentValue, value),
                CompireType.NotEqual => !Mathf.Approximately(currentValue, value),
                CompireType.LessOrEqual => currentValue < value || Mathf.Approximately(currentValue, value),
                _ => false
            };
        }
    }
    [System.Serializable]
    public class DecisionStringCondition : DecisionCondition<string>
    {
        public string value;
        public override object Value => value;

        protected override bool Verify(string currentValue)
        {
            return checkCompire switch
            {
                CompireType.Equal => currentValue == value,
                CompireType.Bigger => string.Compare(currentValue, value) > 0,
                CompireType.Lower => string.Compare(currentValue, value) < 0,
                CompireType.GreaterOrEqual => string.Compare(currentValue, value) >= 0,
                CompireType.NotEqual => currentValue != value,
                CompireType.LessOrEqual => string.Compare(currentValue, value) <= 0,
                _ => false
            };
        }
    }
}
