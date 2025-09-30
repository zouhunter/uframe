/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-17                                                                   *
*  版本: 0.0.1                                                                        *
*  功能:                                                                              *
*   - 2d音乐+音效播放                                                                 *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Audio
{
    public class Audio2DPlayer : AudioPlayerBase, IAudio2DPlayer
    {
        protected Dictionary<int, AudioContext> m_musicDic = new Dictionary<int, AudioContext>();
        public Audio2DPlayer(GameObject owner) : base(owner) { }

        public override void SetMusicVolume(float volume)
        {
            base.SetMusicVolume(volume);
            foreach (var item in m_musicDic.Values)
            {
                item.TotleVolume = volume;
            }
        }

        public AudioContext GetMusicAudioContext(int channel)
        {
            if (!m_musicDic.TryGetValue(channel, out AudioContext context))
            {
                context = new AudioContext("music-2d:" + channel);
                context.SetParent(m_owner.transform);
                context.audioSource.spatialBlend = 0;//2d效果
                context.TotleVolume = m_sfxVolume;
                m_musicDic.Add(channel, context);
            }
            return context;
        }


        public AudioContext PlayMusic(string audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0,float pitch = 1)
        {
            var context = GetMusicAudioContext(channel);
            if (context.CanReplayAudio(audioId))
            {
                context.PlayMusic(context.audioSource.clip, isLoop, volumeScale, delay,pitch);
                return context;
            }

            FindAudioClip(audioId, false, (clip) =>
            {
                context.audioId = audioId;
                context.PlayMusic(clip, isLoop, volumeScale, delay, pitch);
            });
            return context;
        }

        public AudioContext PlayMusic(int audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0,float pitch = 1)
        {
            var context = GetMusicAudioContext(channel);
            if (context.CanReplayAudio(audioId))
            {
                context.PlayMusic(context.audioSource.clip, isLoop, volumeScale, delay, pitch);
                return context;
            }
            FindAudioClip(audioId, false, (clip) =>
            {
                context.audioId = audioId;
                context.PlayMusic(clip, isLoop, volumeScale, delay, pitch);
            });
            return context;
        }


        public AudioContext PauseMusic(bool isPause, int channel = 0)
        {
            if (m_musicDic.TryGetValue(channel, out AudioContext context))
            {
                context.Pause(isPause);
                return context;
            }
            return context;
        }

        public AudioContext GetSFXAudioContext()
        {
            RefreshSfxs();
            AudioContext context = null;
            if (m_audioAssetsPool.Count > 0)
            {
                context = m_audioAssetsPool.Pop();
                context.ResetData();
            }
            else
            {
                context = new AudioContext("audio-2d");
                context.SetParent(m_owner.transform);
            }
            context.audioSource.spatialBlend = 0;//2d效果
            context.TotleVolume = m_sfxVolume;
            m_sfxList.Add(context);
            return context;
        }

        public AudioContext PlaySFX(string audioId, float volumeScale = 1f, float delay = 0f, float pitch = 1)
        {
            AudioContext context = GetSFXAudioContext();
            if (context.CanReplayAudio(audioId))
            {
                context.PlaySFX(context.audioSource.clip, volumeScale, delay, pitch);
                return context;
            }
            FindAudioClip(audioId, true, (clip) =>
             {
                 context.PlaySFX(clip, volumeScale, delay, pitch);
             });
            return context;
        }


        public AudioContext PlaySFX(int audioId, float volumeScale = 1f, float delay = 0f, float pitch = 1)
        {
            AudioContext context = GetSFXAudioContext();
            if (context.CanReplayAudio(audioId))
            {
                context.PlaySFX(context.audioSource.clip, volumeScale, delay, pitch);
                return context;
            }
            FindAudioClip(audioId, true, (clip) =>
             {
                 context.PlaySFX(clip, volumeScale, delay, pitch);
             });
            return context;
        }


        public void PauseMusicAll(bool isPause)
        {
            foreach (int channel in m_musicDic.Keys)
            {
                PauseMusic(isPause, channel);
            }
        }

        public void StopMusic(int channel)
        {
            if (m_musicDic.ContainsKey(channel))
            {
                m_musicDic[channel].Stop();
            }
        }

        public void StopMusicAll()
        {
            foreach (int i in m_musicDic.Keys)
            {
                StopMusic(i);
            }
        }

        public void PauseSFXAll(bool isPause)
        {
            for (int i = 0; i < m_sfxList.Count; i++)
            {
                m_sfxList[i].Pause(isPause);
            }
        }
    }
}