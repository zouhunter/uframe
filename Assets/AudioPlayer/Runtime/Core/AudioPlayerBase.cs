/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-17                                                                   *
*  版本: 0.0.1                                                                        *
*  功能:                                                                              *
*   - 音效播放模板                                                                    *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Audio
{
    public abstract class AudioPlayerBase : IAudioPlayer
    {
        protected GameObject m_owner = null;
        protected Stack<AudioContext> m_audioAssetsPool = new Stack<AudioContext>();
        protected List<AudioContext> m_sfxList = new List<AudioContext>();
        protected float m_musicVolume = 1f;
        protected float m_sfxVolume = 1f;
        protected bool m_musicOn;
        protected bool m_sfxOn;
        public float SFXVolume => m_sfxVolume;
        public float MusicVolume => m_musicVolume;
        public bool MusicOn => m_musicOn;
        public bool SFXOn => m_sfxOn;

        protected IAudioClipLoader m_clipLoader;

        public AudioPlayerBase(GameObject owner)
        {
            this.m_owner = owner;
            m_musicOn = true;
            m_sfxOn = true;
        }

        public virtual void SetMusicOn(bool on)
        {
            m_musicOn = true;
        }

        public virtual void SetSFXOn(bool on)
        {
            m_sfxOn = on;
        }

        public virtual void SetMusicVolume(float volume)
        {
            m_musicVolume = volume;
        }

        public virtual void SetSFXVolume(float volume)
        {
            m_sfxVolume = volume;
            for (int i = 0; i < m_sfxList.Count; i++)
            {
                m_sfxList[i].TotleVolume = volume;
            }
        }

        public void SetAudioClipLoader(IAudioClipLoader audioLoader)
        {
            m_clipLoader = audioLoader;
        }

        public void FindAudioClip(string audioId, bool isSfx, Action<AudioClip> callback)
        {
            if (m_clipLoader == null)
                return;
            if (isSfx && !m_sfxOn)
                return;
            if (!isSfx && !m_musicOn)
                return;
            m_clipLoader.FindClip(audioId, isSfx, callback);
        }

        public void FindAudioClip(int audioId, bool isSfx, Action<AudioClip> callback)
        {
            if (m_clipLoader == null)
                return;
            if (isSfx && !m_sfxOn)
                return;
            if (!isSfx && !m_musicOn)
                return;
            m_clipLoader.FindClip(audioId, isSfx, callback);
        }

        protected virtual void RefreshSfxs()
        {
            if (m_sfxList.Count == 0)
                return;

            for (int i = m_sfxList.Count - 1; i >= 0; i--)
            {
                var audioAsset = m_sfxList[i];
                if (!audioAsset.IsPlaying && audioAsset.autoRelease)
                {
                    m_sfxList.RemoveAt(i);
                    if (audioAsset.owner)
                    {
                        m_audioAssetsPool.Push(audioAsset);
                    }
                }
            }
        }
    }
}