/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 输入信息查询                                                                    *
*//************************************************************************************/

namespace UFrame.Inputs
{
    public interface IVirtualValueGetter
    {
        bool AxisExists(string name);
        bool ButtonExists(string name);
        float GetAxis(string name);
        float GetAxisRaw(string name);
        bool GetButton(string name);
        bool GetButtonDown(string name);
        bool GetButtonUp(string name);
    }
}