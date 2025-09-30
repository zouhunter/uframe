//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-16 05:09:50
//* 描    述： 尺寸缩放动画

//* ************************************************************************************
using System;
using UnityEngine;
using UFrame.Tween;
namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/PingpongScaleAnim")]
    public class PingpongScaleAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected Vector3 m_targetScale = Vector3.one;
        [SerializeField]
        protected float m_dureation = 1;
        [SerializeField]
        protected int m_loopCount = 1;
        protected Vector3 m_startScale;
        protected int m_counter;
        protected TweenScale m_scaleTween;
        public override bool IsPlaying => m_scaleTween?.IsPlaying ?? false;

        protected override void RecordState()
        {
            if (!m_target)
                m_target = transform;
            m_startScale = m_target.localScale;
            m_scaleTween = new TweenScale(m_target, m_startScale, m_targetScale, m_dureation);
            m_scaleTween.style = TweenBase.Style.PingPong;
            m_scaleTween.onStepFinished += OnTweenFinished;
            m_counter = 0;
        }

        public override void ResetState()
        {
            m_scaleTween.target.localScale = m_startScale;
            m_counter = 0;
        }

        private void OnTweenFinished(bool forward)
        {
            if (!forward)
            {
                if (++m_counter >= m_loopCount)
                {
                    StopTween(m_scaleTween);
                }
            }
        }

        public override void Play()
        {
            //var addOk = TweenAgent.Instance.StartTween(m_scaleTween);
            //if (!addOk)
            //{
            //    m_counter = 0;
            //}
        }
    }
}