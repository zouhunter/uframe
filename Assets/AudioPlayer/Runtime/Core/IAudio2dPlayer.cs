/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 音效播放器接口                                                                  *
*//************************************************************************************/

using System;

using UnityEngine;


namespace UFrame.Audio
{
    public interface IAudioPlayer
    {
        float MusicVolume { get; }
        float SFXVolume { get; }
        bool MusicOn { get; }
        bool SFXOn { get; }
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetMusicOn(bool on);
        void SetSFXOn(bool on);
        void SetAudioClipLoader(IAudioClipLoader audioLoader);
        void FindAudioClip(string audioId, bool isSfx, Action<AudioClip> callback);
        void FindAudioClip(int audioId, bool isSfx, Action<AudioClip> callback);
    }

    public interface IAudio2DPlayer : IAudioPlayer
    {
        AudioContext GetMusicAudioContext(int channel = 0);
        AudioContext GetSFXAudioContext();
        AudioContext PlayMusic(string audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0, float pitch = 1);
        AudioContext PlayMusic(int audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0, float pitch = 1);
        AudioContext PauseMusic(bool isPause, int channel = 0);
        void PauseMusicAll(bool isPause);
        void StopMusic(int channel);
        void StopMusicAll();
        AudioContext PlaySFX(string audioId, float volumeScale = 1f, float delay = 0f, float pitch = 1);
        AudioContext PlaySFX(int audioId, float volumeScale = 1f, float delay = 0f, float pitch = 1);
        void PauseSFXAll(bool isPause);
    }
}
