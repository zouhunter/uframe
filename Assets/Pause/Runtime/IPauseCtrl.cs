/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 暂停控制                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.Pause
{
    public interface IPauseCtrl
    {
        bool IsPaused { get; }
        void RegistPauseEvent(Action<bool> pausedCallBack);
        void RemovePauseEvent(Action<bool> pausedCallBack);
        void Pause(byte flag);
        void UnPause(byte flag);
	}
}
