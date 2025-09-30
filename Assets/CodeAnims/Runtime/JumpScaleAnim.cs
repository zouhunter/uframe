//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-17 11:34:36
//* 描    述： 翻跟斗式跳跃

//* ************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.Tween;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/JumpScaleAnim")]
    public class JumpScaleAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected float m_jumpHeight = 1;
        [SerializeField]
        protected int m_scaleNum = 3;
        [SerializeField]
        protected int m_durection = 1;
        [SerializeField]
        protected Vector3 m_targetScale;
        [SerializeField]
        protected bool m_scaleBack;

        protected TweenScale m_scaleTween;
        protected TweenPosition m_positionTween;

        protected Vector3 m_startPos;
        protected Vector3 m_startScale;
        public override bool IsPlaying => m_positionTween?.IsPlaying ?? false;

        protected override void RecordState()
        {
            m_startPos = m_target.transform.localPosition;
            m_startScale = m_target.transform.localScale;
        }

        public override void Play()
        {
            if (m_scaleTween == null)
            {
                if (m_scaleNum < 1)
                    m_scaleNum = 1;
                m_scaleTween = new TweenScale(m_target, m_startScale, m_targetScale, m_durection * 0.5f / m_scaleNum, m_durection * 0.5f);
                m_scaleTween.style = m_scaleBack ? TweenBase.Style.PingPong : TweenBase.Style.Loop;
                m_scaleTween.SetLoopCount(m_scaleNum);
            }

            if (m_positionTween == null)
            {
                var worldPos = m_startPos;
                if (m_target.parent)
                {
                    worldPos = m_target.parent.TransformPoint(m_startPos);
                }
                m_positionTween = new TweenPosition(m_target, worldPos, worldPos + Vector3.up * m_jumpHeight, m_durection);
                m_positionTween.style = TweenBase.Style.PingPong;
                m_positionTween.SetLoopCount(1);
            }

            if (!m_positionTween.IsPlaying && !m_scaleTween.IsPlaying)
            {
                m_positionTween.ResetToBeginning();
                m_scaleTween.ResetToBeginning();
                StartTween(m_positionTween);
                StartTween(m_scaleTween);
            }
        }

        public override void ResetState()
        {
            m_target.transform.localPosition = m_startPos;
            m_target.transform.localScale = m_startScale;

            //if (m_positionTween != null && m_positionTween.IsPlaying)
            //{
            //    TweenAgent.Instance.StopTween(m_positionTween);
            //}
            //if (m_scaleTween != null && m_scaleTween.IsPlaying)
            //{
            //    TweenAgent.Instance.StopTween(m_scaleTween);
            //}
        }
    }
}