/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 按键输入判断                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Inputs
{
    public class KeyInputJudge : IntInputJudge
    {
        protected override bool IsTriggered(int key)
        {
            return Input.GetKey((KeyCode)key);
        }
    }
    public class KeyUpInputJudge : IntInputJudge
    {
        protected override bool IsTriggered(int key)
        {
            return Input.GetKeyUp((KeyCode)key);
        }
    }
    public class KeyDownInputJudge : IntInputJudge
    {
        protected override bool IsTriggered(int key)
        {
            return Input.GetKeyDown((KeyCode)key);
        }
    }

}