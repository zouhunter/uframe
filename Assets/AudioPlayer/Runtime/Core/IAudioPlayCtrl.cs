/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 3d音乐+音效播放                                                                 *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Audio
{
    public interface IAudioPlayCtrl
    {
        IAudio2DPlayer Audio2D { get; }
        IAudio3DPlayer Audio3D { get; }
        GameObject Owner { get; }
        float TotleVolume { get; set; }
        float MusicVolume { get; set; }
        float SFXVolume { get; set; }
        void ChangeAudioClipLoader(IAudioClipLoader loader);
    }
}
