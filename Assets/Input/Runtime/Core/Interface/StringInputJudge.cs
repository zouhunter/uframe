/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 字符输入判断                                                                    *
*//************************************************************************************/

using System;

namespace UFrame.Inputs
{

    public abstract class StringInputJudge : InputHandleTemp<string, Action>
    {
        protected override void ExecuteInternal()
        {
            using (var enumerator = actionDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;

                    if (IsTriggered(key))
                    {
                        for (int i = 0; i < enumerator.Current.Value.Count; i++)
                        {
                            var currentEvent = enumerator.Current.Value[i];
                            try
                            {
                                currentEvent.Invoke();
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
        }
        protected abstract bool IsTriggered(string key);
    }

}