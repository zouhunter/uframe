/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 鼠标输入判断                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Inputs
{
    public class MouseDownInputJudge : IntInputJudge
    {
        protected override bool IsTriggered(int key)
        {
            return Input.GetMouseButtonDown(key);
        }
    }
    public class MouseUpInutJudge : IntInputJudge
    {
        protected override bool IsTriggered(int key)
        {
            return Input.GetMouseButtonUp(key);
        }
    }
    public class MouseInputJudge : IntInputJudge
    {
        protected override bool IsTriggered(int key)
        {
            return Input.GetMouseButton(key);
        }
    }

}