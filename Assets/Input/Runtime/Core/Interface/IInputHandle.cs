/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 输入句柄                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.Inputs {

    public interface IInputHandle
    {
        Action<IInputHandle> onEmpty { get; set; }
        Action<IInputHandle> onNotEmpty { get; set; }
        Action<string> onError { get; set; }
        bool HaveEvent();
        void Execute();
    }

}