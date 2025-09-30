/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 音效文件加载器口                                                                *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Audio
{
    public interface IAudioClipLoader
    {
        void FindClip(string id,bool isSfx, Action<AudioClip> onLoad);
        void FindClip(int id, bool isSfx, Action<AudioClip> onLoad);
        void ClearAllLoaded();
    }

    public class ResourceClipLoader : IAudioClipLoader
    {
        protected Dictionary<string, AudioClip> m_audioMap = new Dictionary<string, AudioClip>();

        public virtual void ClearAllLoaded()
        {
            m_audioMap.Clear();
            Resources.UnloadUnusedAssets();
        }

        public virtual void FindClip(string audioId, bool isSfx, Action<AudioClip> onLoad)
        {
            if (onLoad == null)
                return;

            if (m_audioMap.TryGetValue(audioId, out AudioClip clip))
            {
                onLoad(clip);
            }
            else
            {
                clip = Resources.Load<AudioClip>(audioId);
                if (clip != null)
                {
                    m_audioMap[audioId] = clip;
                }
                else
                {
                    Debug.LogError("failed load clip :"+audioId);
                }
                onLoad(clip);
            }
        }

        public virtual void FindClip(int audioId, bool isSfx, Action<AudioClip> onLoad)
        {
            Debug.LogError("FindClip from int key is not supported by ResourceClipLoader,please choose other loader!");
        }
    }
}
