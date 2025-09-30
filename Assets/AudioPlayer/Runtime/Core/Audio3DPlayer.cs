/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-17                                                                   *
*  版本: 0.0.1                                                                        *
*  功能:                                                                              *
*   - 3d音乐+音效播放                                                                 *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Audio
{
    public class Audio3DPlayer : AudioPlayerBase, IAudio3DPlayer
    {
        protected Dictionary<int, Dictionary<int, AudioContext>> m_musicDic = new Dictionary<int, Dictionary<int, AudioContext>>();
        protected Dictionary<int, GameObject> m_musicOwnDic = new Dictionary<int, GameObject>();
        protected List<int> m_delyRemoveList = new List<int>();

        public Audio3DPlayer(GameObject owner) : base(owner) { }

        public override void SetMusicVolume(float volume)
        {
            base.SetMusicVolume(volume);
            foreach (var dics in m_musicDic.Values)
            {
                foreach (var item in dics.Values)
                {
                    item.TotleVolume = volume;
                }
            }
        }

        public override void SetSFXVolume(float volume)
        {
            base.SetSFXVolume(volume);
            for (int i = 0; i < m_sfxList.Count; i++)
            {
                m_sfxList[i].TotleVolume = volume;
            }
        }

        public AudioContext GetAudioContext(GameObject owner, int channel = 0)
        {
            var instanceId = owner.GetInstanceID();
            if (!m_musicDic.TryGetValue(instanceId, out Dictionary<int, AudioContext> tempDic))
            {
                tempDic = new Dictionary<int, AudioContext>();
                m_musicDic.Add(instanceId, tempDic);
                m_musicOwnDic[instanceId] = owner;
            }

            if (!tempDic.TryGetValue(channel, out AudioContext context) || !context.owner)
            {
                context = new AudioContext("music-3d:" + channel);
                context.SetParent(owner.transform);
                context.audioSource.spatialBlend = 1;
                context.TotleVolume = m_musicVolume;
                context.audioSource.spatialBlend = 1;
                tempDic[channel] = context;
            }
            return context;
        }

        public AudioContext PlayMusic(GameObject owner, string audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0f,float pitch = 1)
        {
            if (!owner)
            {
                Debug.LogError("can not play 3d player, owner is null");
                return null;
            }
            ClearDestroyObjectData();
            var context = GetAudioContext(owner, channel);
            FindAudioClip(audioId, false, (clip) =>
            {
                context.PlayMusic(clip, isLoop, volumeScale, delay,pitch);
            });
            return context;
        }


        public AudioContext PlayMusic(GameObject owner, int audioId, int channel = 0, bool isLoop = false, float volumeScale = 1, float delay = 0f,float pitch=1)
        {
            if (!owner)
            {
                Debug.LogError("can not play 3d player, owner is null");
                return null;
            }
            ClearDestroyObjectData();
            var context = GetAudioContext(owner, channel);
            if (context.CanReplayAudio(audioId))
            {
                context.PlayMusic(context.audioSource.clip,isLoop, volumeScale, delay,pitch);
                return context;
            }
            FindAudioClip(audioId, false, (clip) =>
            {
                context.PlayMusic(clip, isLoop, volumeScale, delay,pitch);
            });
            return context;
        }

        public AudioContext PauseMusic(GameObject owner, int channel, bool isPause)
        {
            if (!owner)
            {
                Debug.LogError("can not Pause , owner is null");
                return null;
            }
            var instanceId = owner.GetInstanceID();
            if (m_musicDic.TryGetValue(instanceId, out Dictionary<int, AudioContext> channelContexts))
            {
                if (channelContexts.ContainsKey(channel))
                {
                    AudioContext context = channelContexts[channel];
                    context.Pause(isPause);
                    return context;
                }
            }
            return null;
        }

        public void PauseMusicAll(bool isPause)
        {
            foreach (var instanceId in m_musicDic.Keys)
            {
                foreach (int channel in m_musicDic[instanceId].Keys)
                {
                    if (m_musicOwnDic.TryGetValue(instanceId, out GameObject go) && go)
                    {
                        PauseMusic(go, channel, isPause);
                    }
                }
            }
        }

        public AudioContext StopMusic(int instanceId, int channel)
        {
            if (m_musicDic.TryGetValue(instanceId, out Dictionary<int, AudioContext> tempDic))
            {
                if (tempDic.TryGetValue(channel, out AudioContext context))
                {
                    context.Stop();
                    return context;
                }
            }
            return null;
        }

        public void StopMusicOneAll(int instanceId)
        {
            if (m_musicDic.ContainsKey(instanceId))
            {
                List<int> list = new List<int>(m_musicDic[instanceId].Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    StopMusic(instanceId, list[i]);
                }
            }

        }

        public void StopMusicAll()
        {
            List<int> list = new List<int>(m_musicDic.Keys);
            for (int i = 0; i < list.Count; i++)
            {
                StopMusicOneAll(list[i]);
            }
        }

        public void ReleaseMusic(int instanceId)
        {
            if (m_musicDic.ContainsKey(instanceId))
            {
                StopMusicOneAll(instanceId);
                List<AudioContext> list = new List<AudioContext>(m_musicDic[instanceId].Values);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Release();
                }
                list.Clear();
            }
            m_musicDic.Remove(instanceId);
            m_musicOwnDic.Remove(instanceId);
        }

        public void ReleaseMusicAll()
        {
            List<int> list = new List<int>(m_musicDic.Keys);
            for (int i = 0; i < list.Count; i++)
            {
                ReleaseMusic(list[i]);
            }
            m_musicDic.Clear();
        }

        public AudioContext PlaySFX(string audioId, Vector3 pos, float volumeScale = 1f, float delay = 0f, float pitch = 1)
        {
            AudioContext context = GetSFXAudioContext();
            if(context.CanReplayAudio(audioId))
            {
                context.SetPosition(pos);
                context.PlaySFX(context.audioSource.clip, volumeScale, delay, pitch);
                return context;
            }
            FindAudioClip(audioId, true, (clip) =>
            {
                context.audioId = audioId;
                context.SetPosition(pos);
                context.PlaySFX(clip, volumeScale, delay, pitch);
            });
            return context;
        }

        public AudioContext PlaySFX(int audioId, Vector3 pos, float volumeScale = 1f, float delay = 0f, float pitch = 1)
        {
            AudioContext context = GetSFXAudioContext();
            if (context.CanReplayAudio(audioId))
            {
                context.SetPosition(pos);
                context.PlaySFX(context.audioSource.clip, volumeScale, delay, pitch);
                return context;
            }
            FindAudioClip(audioId, true, (clip) =>
             {
                 context.SetPosition(pos);
                 context.PlaySFX(clip, volumeScale, delay, pitch);
             });
            return context;
        }

        public void PauseSFXAll(bool isPause)
        {
            for (int i = 0; i < m_sfxList.Count; i++)
            {
                m_sfxList[i].Pause(isPause);
            }
        }

        public void ReleaseSFXAll()
        {
            for (int i = 0; i < m_sfxList.Count; i++)
            {
                m_sfxList[i].Stop();
                m_audioAssetsPool.Push(m_sfxList[i]);
            }
            m_sfxList.Clear();
        }

        protected void ClearDestroyObjectData()
        {
            foreach (var pair in m_musicOwnDic)
            {
                if (!pair.Value)
                {
                    m_delyRemoveList.Add(pair.Key);
                }
            }

            if (m_delyRemoveList.Count > 0)
            {
                for (int i = 0; i < m_delyRemoveList.Count; i++)
                {
                    var id = m_delyRemoveList[i];
                    m_musicDic.Remove(id);
                    m_musicOwnDic.Remove(id);
                }
                m_delyRemoveList.Clear();
            }
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
                context = new AudioContext("audio-3d");
            }
            context.SetParent(m_owner.transform);
            context.audioSource.spatialBlend = 1;
            context.TotleVolume = m_sfxVolume;
            m_sfxList.Add(context);
            return context;
        }
    }


}