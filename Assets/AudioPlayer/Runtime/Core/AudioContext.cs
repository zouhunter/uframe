/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 音效播放容器                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Audio
{
    public class AudioContext
    {
        public GameObject owner { get; private set; }
        public AudioSource audioSource { get; private set; }
        public object audioId { get; set; }
        public bool autoRelease { get; set; }
        /// <summary>
        /// 总音量
        /// </summary>
        private float m_totleVolume = 1;
        public float TotleVolume
        {
            get
            {
                return m_totleVolume;
            }

            set
            {

                m_totleVolume = value;
                Volume = TotleVolume * volumeScale;
            }
        }

        /// <summary>
        /// 当前AudioSource 实际音量
        /// </summary>
        public float Volume
        {
            get
            {
                if (audioSource)
                    return audioSource.volume;
                else
                {
                    return 0;
                }
            }
            set
            {
                if (audioSource)
                    audioSource.volume = value;
            }
        }

        /// <summary>
        /// 实际音量恢复到当前的最大
        /// </summary>
        public void ResetVolume()
        {
            Volume = TotleVolume * volumeScale;
        }

        public float GetMaxRealVolume()
        {
            return TotleVolume * volumeScale;
        }
        /// <summary>
        /// 相对于总音量当前当前AudioSource的音量缩放 Volume=TotleVolume * volumeScale
        /// </summary>
        private float volumeScale = 1f;
        public float VolumeScale
        {
            get { return volumeScale; }
            set
            {
                volumeScale = Mathf.Clamp01(value);
                ResetVolume();
            }
        }

        public void SetParent(Transform parent)
        {
            if (owner)
            {
                owner.transform.SetParent(parent);
                owner.transform.transform.localPosition = Vector3.zero;
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (audioSource)
                    return audioSource.isPlaying;
                else
                {
                    return false;
                }
            }
        }

        public AudioContext(string name)
        {
            owner = new GameObject(name);
            audioSource = owner.AddComponent<AudioSource>();
            autoRelease = true;
        }

        /// <summary>
        /// 重置某些参数，防止回收后再使用参数不对
        /// </summary>
        public void ResetData()
        {
            if (audioSource)
                audioSource.pitch = 1;
        }

        public void Pause(bool pause)
        {
            if (audioSource == null)
                return;

            if (pause)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.UnPause();
            }
        }

        public void Stop()
        {
            if (audioSource)
                audioSource.Stop();
        }

        public bool CanReplayAudio(object id)
        {
            if (audioSource != null && audioId == id)
            {
                return audioSource.clip != null;
            }
            return false;
        }

        public void Release()
        {
            if (audioSource)
            {
                Object.Destroy(audioSource.gameObject);
            }
        }

        public void SetPosition(Vector3 pos)
        {
            if (owner)
            {
                owner.transform.position = pos;
            }
        }

        public void Play(float delay)
        {
            if (audioSource == null)
                return;

            if (delay <= 0.01f)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.PlayDelayed(delay);
            }
        }

        public void Replay()
        {
            if (audioSource && audioSource.clip)
            {
                audioSource.timeSamples = 0;
                audioSource.Play();
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale, float delay, float pitch)
        {
            if (audioSource)
            {
                audioSource.timeSamples = 0;
                audioSource.clip = clip;
                audioSource.pitch = pitch;
                VolumeScale = volumeScale;
                Play(delay);
            }
        }

        public void PlayMusic(AudioClip clip, bool isLoop, float volumeScale, float delay,float pitch)
        {
            if (audioSource)
            {
                audioSource.timeSamples = 0;
                audioSource.clip = clip;
                audioSource.loop = isLoop;
                audioSource.pitch = pitch;
                VolumeScale = volumeScale;
                audioSource.timeSamples = 0;
                Play(delay);
            }
        }

        public static implicit operator bool(AudioContext context)
        {
            return context != null;
        }
    }
}