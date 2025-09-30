using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Effects
{
    public class EffectPlayBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected bool m_enablePlay = true;
        public List<ParticleSystem> m_particals;

        public void OnEnable()
        {
            if (m_enablePlay)
                Play();
        }

        public void Play()
        {
            for (int i = 0; i < m_particals.Count; i++)
            {
                m_particals[i].Play();
            }
        }
    }
}