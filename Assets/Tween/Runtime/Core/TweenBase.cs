using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Base of tween.
/// </summary>

namespace UFrame.Tween
{
    public abstract class TweenBase : ITween
    {
        public enum Style
        {
            Once,
            Loop,
            PingPong,
        }

        public EaseDelegate method;
        public Style style = Style.Once;
        public AnimationCurve animationCurve;
        public bool fixedUpdate = false;
        public float delay = 0f;
        public float duration = 1f;
        public bool enabled;

        public event UnityAction onFinished = null;

        public event UnityAction onUpdate = null;

        public event UnityAction<bool> onStepFinished = null;

        protected bool m_started = false;
        protected float m_startTime = 0f;
        protected float m_duration = 0f;
        protected float m_amountPerDelta = 1000f;
        protected float m_factor = 0f;
        protected float m_lastTime;
        protected int m_loopCount;
        protected int m_loopCounter;
        public bool Enabled => enabled;
        public bool IsPlaying => enabled && m_started;
        private TweenBehaviour m_dynimicTweenBehaviour;
        public static TweenCtrl tweenCtrl { get; set; }

        /// <summary>
        /// Amount advanced per delta time.
        /// </summary>

        public float amountPerDelta
        {
            get
            {
                if (m_duration != duration)
                {
                    m_duration = duration;
                    m_amountPerDelta = Mathf.Abs((duration > 0f) ? 1f / duration : 1000f) * Mathf.Sign(m_amountPerDelta);
                }
                return m_amountPerDelta;
            }
        }

        public float tweenFactor { get { return m_factor; } set { m_factor = Mathf.Clamp01(value); } }
        public virtual bool Valid => true;

        public bool IgnoreTimeScale => fixedUpdate;

        public virtual void EnableTween()
        {
            enabled = true;
        }

        public virtual void SetLoopCount(int loopCount)
        {
            m_loopCount = loopCount;
            m_loopCounter = 0;
        }

        public void Refresh()
        {
            if (!enabled)
                return;

            float time = fixedUpdate ? Time.fixedTime : Time.time;

            if (!m_started)
            {
                m_started = true;
                m_startTime = time + delay;
                m_lastTime = time;
            }

            if (time < m_startTime)
            {
                m_lastTime = time;
                return;
            }

            float delta = time - m_lastTime;
            m_lastTime = time;

            // Advance the sampling factor
            m_factor += amountPerDelta * delta;

            // Loop style simply resets the play factor after it exceeds 1.
            if (style == Style.Loop)
            {
                if (m_factor > 1f)
                {
                    m_factor -= Mathf.Floor(m_factor);

                    if (onStepFinished != null)
                    {
                        onStepFinished.Invoke(amountPerDelta > 0);
                    }

                    if (m_loopCount > 0 && ++m_loopCounter >= m_loopCount)
                    {
                        enabled = false;
                    }
                }
            }
            else if (style == Style.PingPong)
            {
                // Ping-pong style reverses the direction
                if (m_factor > 1f)
                {
                    m_factor = 1f - (m_factor - Mathf.Floor(m_factor));
                    m_amountPerDelta = -m_amountPerDelta;

                    if (onStepFinished != null)
                    {
                        onStepFinished.Invoke(true);
                    }
                }
                else if (m_factor < 0f)
                {
                    m_factor = -m_factor;
                    m_factor -= Mathf.Floor(m_factor);
                    m_amountPerDelta = -m_amountPerDelta;

                    if (onStepFinished != null)
                    {
                        onStepFinished.Invoke(false);
                    }

                    if (m_loopCount > 0 && ++m_loopCounter >= m_loopCount)
                    {
                        enabled = false;
                    }
                }
            }

            // If the factor goes out of range and this is a one-time tweening operation, disable the script
            if ((style == Style.Once) && (duration == 0f || m_factor > 1f || m_factor < 0f))
            {
                m_factor = Mathf.Clamp01(m_factor);
                enabled = false;
                Sample(m_factor, !enabled);
            }
            else
            {
                Sample(m_factor, !enabled);
            }
        }

        public TweenBase AutoRefresh()
        {
            if(tweenCtrl != null)
            {
                tweenCtrl.StartTween(this);
            }
            else
            {
                if (!m_dynimicTweenBehaviour)
                    m_dynimicTweenBehaviour = new GameObject("TweenPlayer" + GetHashCode()).AddComponent<TweenBehaviour>();
                m_dynimicTweenBehaviour.SetTween(this);
                EnableTween();
            }
            return this;
        }

        protected void OnFinish()
        {
            if (onFinished != null)
            {
                onFinished.Invoke();
            }
            if(m_dynimicTweenBehaviour)
            {
                GameObject.Destroy(m_dynimicTweenBehaviour.gameObject);
                m_dynimicTweenBehaviour = null;
            }
        }

        public void RegistOnFinished(UnityAction finishedCallBack)
        {
            onFinished += finishedCallBack;
        }

        public void RemoveOnFinished(UnityAction finishedCallBack)
        {
            onFinished -= finishedCallBack;
        }

        public virtual void DisableTween()
        {
            enabled = false;
        }

        /// <summary>
        /// Sample the tween at the specified factor.
        /// </summary>

        public void Sample(float factor, bool isFinished)
        {
            // Calculate the sampling value
            float val = Mathf.Clamp01(factor);

            if(method != null)
                val = method.Invoke(0, 1, val);
            else if (animationCurve != null)
                val = animationCurve.Evaluate(val);

            // Call the virtual update
            if(Valid)
            {
                OnUpdate(val, isFinished);
            }
            else
            {
                isFinished = true;
            }

            if (onUpdate != null)
            {
                onUpdate.Invoke();
            }

            if (isFinished)
            {
                OnFinish();
            }
        }

        /// <summary>
        /// Play the tween forward.
        /// </summary>

        public void PlayForward(bool reset = false)
        {
            if (reset)
                ResetToBeginning();
            Play(true);
        }

        /// <summary>
        /// Play the tween in reverse.
        /// </summary>

        public void PlayReverse(bool reset = false)
        {
            if (reset)
                ResetToComplete();
            Play(false);
        }

        /// <summary>
        /// Manually activate the tweening process, reversing it if necessary.
        /// </summary>
        protected void Play(bool forward)
        {
            m_amountPerDelta = Mathf.Abs(amountPerDelta);
            if (!forward)
                m_amountPerDelta = -m_amountPerDelta;
            enabled = true;
            Refresh();
        }

        /// <summary>
        /// Manually reset the tweener's state to the beginning.
        /// If the tween is playing forward, this means the tween's start.
        /// If the tween is playing in reverse, this means the tween's end.
        /// </summary>

        public void ResetToBeginning()
        {
            m_started = false;
            m_factor = (amountPerDelta < 0f) ? 1f : 0f;
            m_loopCounter = 0;
            Sample(m_factor, false);
        }

        public void ResetToComplete()
        {
            m_started = true;
            m_factor = (amountPerDelta < 0f) ? 0f : 1f;
            m_loopCounter = 0;
            Sample(m_factor, true);
        }

        /// <summary>
        /// Manually start the tweening process, reversing its direction.
        /// </summary>

        public void Toggle()
        {
            if (m_factor > 0f)
            {
                m_amountPerDelta = -amountPerDelta;
            }
            else
            {
                m_amountPerDelta = Mathf.Abs(amountPerDelta);
            }
            enabled = true;
        }

        protected abstract void OnUpdate(float factor, bool isFinished);

        public TweenBase()
        {
            m_started = false;
            m_factor = 0f;
            m_amountPerDelta = Mathf.Abs(amountPerDelta);
            style = Style.Once;
            enabled = true;
        }
    }
    public interface ITween
    {
        bool IsPlaying { get; }
        bool Valid { get; }
        bool Enabled { get; }
        bool IgnoreTimeScale { get; }
        void Refresh();
        void EnableTween();
        void DisableTween();
    }
}