/*************************************************************************************//*
*  作者: 邹杭特                                                                    *
*  时间: 2021-04-01                                                                   *
*  版本: 0.0.1                                                                        *
*  功能:                                                                              *
*   - 音乐+音效播放                                                                   *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Audio
{
    public class AudioPlayCtrl : IAudioPlayCtrl
    {
        // 平面音效播放器
        protected Audio2DPlayer m_2DPlayer;
        public IAudio2DPlayer Audio2D { get { return m_2DPlayer; } }

        // 场景音效播放器
        protected Audio3DPlayer m_3DPlayer;
        public IAudio3DPlayer Audio3D { get { return m_3DPlayer; } }

        // 音效加载器
        protected IAudioClipLoader m_audioLoader;
        public IAudioClipLoader AudioLoader { get { return m_audioLoader; } }

        private GameObject m_owner;
        public GameObject Owner { get { return m_owner; } }

        #region Volume
        private float totleVolume = 1f;
        public float TotleVolume
        {
            get { return totleVolume; }
            set
            {
                totleVolume = Mathf.Clamp01(value);
                OnMusicVolumeChanged();
                OnSFXVolumeChanged();
            }
        }

        private float musicVolume = 1f;
        public float MusicVolume
        {
            get
            {
                return musicVolume;
            }

            set
            {
                musicVolume = Mathf.Clamp01(value);
                OnMusicVolumeChanged();
            }
        }

        private float sfxVolume = 1f;
        public float SFXVolume
        {
            get
            {
                return sfxVolume;
            }
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                OnSFXVolumeChanged();
            }
        }

        private bool m_musicOn;
        public bool MusicOn
        {
            get
            {
                return m_musicOn;
            }
            set
            {
                m_musicOn = value;
                OnMusicOnChanged();
            }
        }

        private bool m_sfxOn;
        public bool SFXOn
        {
            get
            {
                return m_sfxOn;
            }
            set
            {
                m_sfxOn = value;
                OnSfxOnChanged();
            }
        }

        protected virtual void OnMusicVolumeChanged()
        {
            m_2DPlayer.SetMusicVolume(totleVolume * musicVolume);
            m_3DPlayer.SetMusicVolume(totleVolume * musicVolume);
        }
        protected virtual void OnSFXVolumeChanged()
        {
            m_2DPlayer.SetSFXVolume(totleVolume * sfxVolume);
            m_3DPlayer.SetSFXVolume(totleVolume * sfxVolume);
        }
        protected virtual void OnMusicOnChanged()
        {
            m_2DPlayer.SetMusicOn(m_musicOn);
            m_3DPlayer.SetMusicOn(m_musicOn);
        }
        protected virtual void OnSfxOnChanged()
        {
            m_2DPlayer.SetSFXOn(m_sfxOn);
            m_3DPlayer.SetSFXOn(m_sfxOn);
        }
        #endregion

        public virtual void Initialize(float totleVolume = 1, float musicVolume = 1, float sfxVolume = 1, IAudioClipLoader audioLoader = null)
        {
            if (audioLoader == null)
            {
                audioLoader = new ResourceClipLoader();
            }
            m_owner = new GameObject("[AudioSources]");
            Object.DontDestroyOnLoad(m_owner);
            m_2DPlayer = new Audio2DPlayer(m_owner);
            m_3DPlayer = new Audio3DPlayer(m_owner);
            m_2DPlayer.SetAudioClipLoader(audioLoader);
            m_3DPlayer.SetAudioClipLoader(audioLoader);
            TotleVolume = Mathf.Clamp01(totleVolume);
            MusicVolume = Mathf.Clamp01(musicVolume);
            SFXVolume = Mathf.Clamp01(sfxVolume);
            m_audioLoader = audioLoader;
        }

        public virtual void Recover()
        {
            if (m_audioLoader != null)
            {
                m_audioLoader.ClearAllLoaded();
            }
            if (m_owner)
            {
                Object.Destroy(m_owner);
            }
        }

        public virtual void ChangeAudioClipLoader(IAudioClipLoader loader)
        {
            if (m_audioLoader != null)
            {
                m_audioLoader.ClearAllLoaded();
            }
            m_audioLoader = loader;
            m_2DPlayer.SetAudioClipLoader(loader);
            m_3DPlayer.SetAudioClipLoader(loader);
        }
    }
}