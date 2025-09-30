/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 移动端输入                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Inputs.PlatformSpecific
{
    public class MobileInput : VirtualInput
    {
        public override float GetAxis(string name, bool raw)
        {
            return GetVirtualAxis(name, raw);
        }

        public override bool GetButton(string name)
        {
            return GetVirtualButton(name);
        }

        public override bool GetButtonDown(string name)
        {
            return GetVirtualButtonDown(name);
        }

        public override bool GetButtonUp(string name)
        {
            return GetVirtualButtonUp(name);
        }

        public override Vector3 MousePosition()
        {
            return virtualMousePosition;
        }

    }
}
