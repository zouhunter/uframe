//=============================================================================
//
//          Copyright (c) 2021 Beijing iQIYI Intelligent Technologies Inc.
//                          All Rights Reserved.
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace UFrame.XR
{
    /// <summary>
    /// Fades the screen from black after a new scene is loaded. Fade can also be controlled mid-scene using SetUIFade and SetFadeLevel
    /// </summary>
    [AddComponentMenu("UFrame/XR/CameraScreenFade")]
    public class CameraScreenFade : MonoBehaviour
    {
        [Tooltip("Fade duration")]
        [SerializeField]
        protected float m_fadeTime = 1.0f;
        [Tooltip("Screen color at maximum fade")]
        [SerializeField]
        protected Color m_fadeColor = new Color(0.01f, 0.01f, 0.01f, 1.0f);
        [SerializeField]
        protected bool m_fadeOnStart = true;
        [SerializeField]
        protected float m_startAlpha;
        [SerializeField]
        protected Renderer m_mask;

        /// <summary>
        /// The render queue used by the fade mesh. Reduce this if you need to render on top of it.
        /// </summary>
        public int renderQueue = 5000;

        /// <summary>
        /// Renders the current alpha value being used to fade the screen.
        /// </summary>
        public float currentAlpha { get { return Mathf.Max(explicitFadeAlpha, animatedFadeAlpha, uiFadeAlpha); } }

        private float explicitFadeAlpha = 0.0f;
        private float animatedFadeAlpha = 0.0f;
        private float uiFadeAlpha = 0.0f;
        private Material fadeMaterial = null;
        private bool isFading = false;

        /// <summary>
        /// Automatically starts a fade in
        /// </summary>
        void Awake()
        {
            fadeMaterial = m_mask.sharedMaterial;
            explicitFadeAlpha = 0.0f;
            animatedFadeAlpha = m_startAlpha;
            uiFadeAlpha = 0.0f;

            if (m_fadeOnStart)
            {
                if (m_startAlpha < 0.5f)
                {
                    ScreenFadeOut();
                }
                else
                {
                    ScreenFadeIn();
                }
            }
            else
            {
                SetMaterialAlpha();
            }
        }

        public void ScreenFadeIn()
        {
            if (animatedFadeAlpha > 0.1f)
            {
                StartCoroutine(Fade(animatedFadeAlpha, 0, m_fadeTime * animatedFadeAlpha));
            }
            else
            {
                animatedFadeAlpha = 0;
                SetMaterialAlpha();
            }
        }

        public void ScreenFadeOut()
        {
            if (animatedFadeAlpha < 0.9f)
            {
                StartCoroutine(Fade(animatedFadeAlpha, 1, m_fadeTime));
            }
            else
            {
                animatedFadeAlpha = 1;
                SetMaterialAlpha();
            }
        }

        public void ScreenHide()
        {
            animatedFadeAlpha = 1;
            SetMaterialAlpha();
        }

        /// <summary>
        /// Fades alpha from 1.0 to 0.0
        /// </summary>
        IEnumerator Fade(float startAlpha, float endAlpha, float _fadeTime)
        {
            float elapsedTime = 0.0f;
            while (elapsedTime < _fadeTime)
            {
                elapsedTime += Time.deltaTime;
                animatedFadeAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / _fadeTime));
                SetMaterialAlpha();
                yield return new WaitForEndOfFrame();
            }
            animatedFadeAlpha = endAlpha;
            SetMaterialAlpha();
        }

        /// <summary>
        /// Update material alpha. UI fade and the current fade due to fade in/out animations (or explicit control)
        /// both affect the fade. (The max is taken)
        /// </summary>
        private void SetMaterialAlpha()
        {
            Color color = m_fadeColor;
            color.a = currentAlpha;
            isFading = color.a > 0;
            if (fadeMaterial != null)
            {
                fadeMaterial.color = color;
                fadeMaterial.renderQueue = renderQueue;
            }
            m_mask.gameObject.SetActive(currentAlpha > 0.01f);
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                ScreenFadeIn();
            }
        }
    }
}