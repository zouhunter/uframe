//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-06
//* 描    述：对比检查

//* ************************************************************************************
using UFrame.BehaviourTree;
using UnityEngine;
using System;

namespace UFrame.HTN
{
    [Serializable]
    public class CheckInfo
    {
        public string Key;
        public int state;//0启用 1 取反 禁用 2
        public CheckCompire checkCompire;
        public virtual CheckCompire[] supportTypes { get; } = new CheckCompire[] { CheckCompire.Equal, CheckCompire.Bigger, CheckCompire.Lower };

        public CheckInfo()
        {
        }

        //
        // Summary:
        //     Constructor
        public CheckInfo(string key, CheckCompire compire)
        {
            Key = key;
            checkCompire = compire;
        }


        public virtual bool Verify(IVariableContent states)
        {
            return false;
        }
    }

    public class IntCheck : CheckInfo
    {
        public int Value;
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<int>(Key, out int currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => currentValue == Value,
                CheckCompire.Bigger => currentValue > Value,
                CheckCompire.Lower => currentValue < Value,
                _ => false
            };
        }
    }

    public class BoolCheck : CheckInfo
    {
        public bool Value;
        public override CheckCompire[] supportTypes { get; } = new CheckCompire[] { CheckCompire.Equal };
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<bool>(Key, out bool currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => currentValue == Value,
                _ => false
            };
        }
    }

    public class FloatCheck : CheckInfo
    {
        public float Value;
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<float>(Key, out float currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => Mathf.Approximately(currentValue, Value),
                CheckCompire.Bigger => currentValue > Value,
                CheckCompire.Lower => currentValue < Value,
                _ => false
            };
        }
    }

    public class StringCheck : CheckInfo
    {
        public string Value;
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<string>(Key, out string currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => currentValue == Value,
                CheckCompire.Bigger => string.Compare(currentValue, Value) > 0,
                CheckCompire.Lower => string.Compare(currentValue, Value) < 0,
                _ => false
            };
        }
    }

    public class GameObjectCheck : CheckInfo
    {
        public GameObject Value;
        public override CheckCompire[] supportTypes { get; } = new CheckCompire[] { CheckCompire.Equal };
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<GameObject>(Key, out GameObject currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => currentValue == Value,
                CheckCompire.Bigger => currentValue && !Value, // 非空 > 空
                CheckCompire.Lower => !currentValue && Value,  // 空 < 非空
                _ => false
            };
        }
    }

    public class ObjectCheck : CheckInfo
    {
        public UnityEngine.Object Value;
        public override CheckCompire[] supportTypes { get; } = new CheckCompire[] { CheckCompire.Equal };
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<UnityEngine.Object>(Key, out UnityEngine.Object currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => currentValue == Value,
                CheckCompire.Bigger => currentValue && !Value, // 非空 > 空
                CheckCompire.Lower => !currentValue && Value,  // 空 < 非空
                _ => false
            };
        }
    }

    public class NullableCheck : CheckInfo
    {
        public object Value;
        public override CheckCompire[] supportTypes { get; } = new CheckCompire[] { CheckCompire.Equal };
        public override bool Verify(IVariableContent states)
        {
            if (!states.TryGetValue<object>(Key, out object currentValue))
                return false;

            return checkCompire switch
            {
                CheckCompire.Equal => Equals(currentValue, Value),
                CheckCompire.Bigger => currentValue != null && Value == null, // 非空 > 空
                CheckCompire.Lower => currentValue == null && Value != null,  // 空 < 非空
                _ => false
            };
        }
    }

}

