/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 主机端输入                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Inputs.PlatformSpecific
{
    public class StandaloneInput : VirtualInput
    {
        public override float GetAxis(string name, bool raw)
        {
            if (AxisExists(name))
            {
                return GetVirtualAxis(name, raw);
            }
            else
            {
                return raw ? Input.GetAxisRaw(name) : Input.GetAxis(name);
            }
        }

        public override bool GetButton(string name)
        {
            if (ButtonExists(name))
            {
                return GetVirtualButton(name);
            }
            else
            {
                return Input.GetButton(name);
            }
        }

        public override bool GetButtonDown(string name)
        {
            if ( ButtonExists(name))
            {
                return GetVirtualButtonDown(name);
            }
            else
            {
                return Input.GetButtonDown(name);
            }
        }


        public override bool GetButtonUp(string name)
        {
            if (ButtonExists(name))
            {
                return GetVirtualButtonUp(name);
            }
            else
            {
                return Input.GetButtonUp(name); ;
            }
        }

        public override Vector3 MousePosition()
        {
            return Input.mousePosition;
        }
    }
}