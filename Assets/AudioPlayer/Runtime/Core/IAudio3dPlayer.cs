/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 音效播放器接口-3d                                                               *
*//************************************************************************************/

using System;
using UnityEngine;

namespace UFrame.Audio
{
    public interface IAudio3DPlayer:IAudioPlayer
    {
        AudioContext GetAudioContext(GameObject owner, int channel = 0);
        AudioContext GetSFXAudioContext();
        AudioContext PlayMusic(GameObject owner, int audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0f, float pitch = 1);
        AudioContext PlayMusic(GameObject owner, string audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0f,float pitch = 1);
        AudioContext PauseMusic(GameObject owner, int channel, bool isPause);
        void PauseMusicAll(bool isPause);
        AudioContext StopMusic(int instanceId, int channel);
        void StopMusicOneAll(int instanceId);
        void StopMusicAll();
        void ReleaseMusic(int instanceId);
        void ReleaseMusicAll();
        AudioContext PlaySFX(string audioId, Vector3 pos, float volumeScale = 1f, float delay = 0f, float pitch = 1);
        AudioContext PlaySFX(int audioId, Vector3 pos, float volumeScale = 1f, float delay = 0f, float pitch = 1);
        void PauseSFXAll(bool isPause);
        void ReleaseSFXAll();
    }
}
