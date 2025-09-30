/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 自定义UI接口                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.BridgeUI
{
    public interface ICustomUI
    {
        Transform Content { get;  }
        void Initialize(IUIPanel view);
        void Recover();
    }
}