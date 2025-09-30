using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Audio
{
    public abstract class AudioClipLoader : IAudioClipLoader
    {
        protected Dictionary<object, AudioClip> m_audioMap = new Dictionary<object, AudioClip>();
        public virtual void ClearAllLoaded()
        {
            m_audioMap.Clear();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        public virtual void FindClip(string audioId, bool isSfx, Action<AudioClip> onLoad)
        {
            FindOrLoadAudioClip(audioId, isSfx, onLoad);
        }
        public virtual void FindClip(int audioId, bool isSfx, Action<AudioClip> onLoad)
        {
            FindOrLoadAudioClip(audioId, isSfx, onLoad);
        }
        protected virtual void FindOrLoadAudioClip(object audioId, bool isSfx, Action<AudioClip> onLoad)
        {
            if (onLoad == null)
                return;

            if (m_audioMap.TryGetValue(audioId, out AudioClip clip))
            {
                onLoad(clip);
            }
            else
            {
                LoadAudioClip(audioId, isSfx, (clip) =>
                 {
                     if (clip)
                     {
                         m_audioMap[audioId] = clip;

                         try
                         {
                             if (onLoad != null)
                                 onLoad.Invoke(clip);
                         }
                         catch (Exception e)
                         {
                             Debug.LogException(e);
                         }
                     }
                     else
                     {
                         Debug.LogError("failed load clip :" + audioId);
                     }
                 });
            }
        }
        protected abstract void LoadAudioClip(object audioId, bool isSfx, Action<AudioClip> onLoad);
    }
}