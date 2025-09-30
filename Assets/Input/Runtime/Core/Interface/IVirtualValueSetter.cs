/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 输入信息设定                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Inputs
{
    public interface IVirtualValueSetter 
    {
        void SetButtonDown(string name);
        void SetButtonUp(string name);
        void SetAxisPositive(string name);
        void SetAxisNegative(string name);
        void SetAxisZero(string name);
        void SetAxis(string name, float value);
        void UnRegisterVirtualAxis(string name);
        void SetVirtualMousePosition(Vector3 position);
        void SetVirtualMousePositionX(float f);
        void SetVirtualMousePositionY(float f);
        void SetVirtualMousePositionZ(float f);
        void UnRegisterVirtualButton(string name);
    }
}