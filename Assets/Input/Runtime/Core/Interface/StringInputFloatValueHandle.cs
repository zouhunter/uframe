/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 字符输入模板                                                                    *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Inputs {

    public abstract class StringInputFloatValueHandle : InputHandleTemp<string, Action<float>>
    {
        protected override void ExecuteInternal()
        {
            using (var enumerator = actionDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;

                    var value = GetValue(key);

                    for (int i = 0; i < enumerator.Current.Value.Count; i++)
                    {
                        var currentEvent = enumerator.Current.Value[i];
                        try
                        {
                            currentEvent.Invoke(value);
                        }
                        catch (Exception e)
                        {
                            if (onError != null)
                                onError(e.Message);
                        }
                    }
                }
            }
        }
        protected abstract float GetValue(string key);
    }

}