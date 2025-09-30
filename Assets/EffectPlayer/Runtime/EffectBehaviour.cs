using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Effects
{
    public class EffectBehaviour : MonoBehaviour
    {
        protected ParticleSystem[] particals;
        protected float m_recoverTime;

        public System.Action<GameObject> onRecover { get; set; }

        public virtual void Play(float delyHide)
        {
            m_recoverTime = Time.time + delyHide;
            if (particals == null)
            {
                particals = gameObject.GetComponentsInChildren<ParticleSystem>();
            }
            if (particals != null)
            {
                foreach (var item in particals)
                {
                    item.Play();
                }
            }
            gameObject.SetActive(true);
        }
        public virtual void Update()
        {
            if (Time.time > m_recoverTime)
            {
                gameObject.SetActive(false);
                onRecover?.Invoke(gameObject);
            }
        }
    }
}
