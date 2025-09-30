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
    [AddComponentMenu("UFrame/CodeAnimation/JumpRotateAnim")]
    public class JumpRotateAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected float m_jumpHeight = 1;
        [SerializeField]
        protected int m_rotateNum = 3;
        [SerializeField]
        protected int m_durection = 1;
        [SerializeField]
        protected Vector3 m_targetRot;
        [SerializeField]
        protected bool m_rotateBack;

        protected TweenEular m_rotateTween;
        protected TweenPosition m_positionTween;

        protected Vector3 m_startPos;
        protected Quaternion m_beginRot;

        public override bool IsPlaying => m_positionTween?.IsPlaying ?? false;

        protected override void RecordState()
        {
            m_startPos = m_target.transform.localPosition;
            m_beginRot = m_target.transform.localRotation;
        }

        public override void Play()
        {
            if (m_rotateTween == null)
            {
                if (m_rotateNum < 1)
                    m_rotateNum = 1;
                m_rotateTween = new TweenEular(m_target, m_beginRot.eulerAngles, m_targetRot, m_durection * 0.5f / m_rotateNum, m_durection * 0.5f);
                m_rotateTween.style = m_rotateBack ? TweenBase.Style.PingPong : TweenBase.Style.Loop;
                m_rotateTween.SetLoopCount(m_rotateNum);
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

            if (!m_positionTween.IsPlaying && !m_rotateTween.IsPlaying)
            {
                m_positionTween.ResetToBeginning();
                m_rotateTween.ResetToBeginning();
                StartTween(m_positionTween);
                StartTween(m_rotateTween);
            }
        }

        public override void ResetState()
        {
            m_target.transform.localPosition = m_startPos;
            m_target.transform.localRotation = m_beginRot;

            //if (m_positionTween != null && m_positionTween.IsPlaying)
            //{
            //    TweenAgent.Instance.StopTween(m_positionTween);
            //}
            //if (m_rotateTween != null && m_rotateTween.IsPlaying)
            //{
            //    TweenAgent.Instance.StopTween(m_rotateTween);
            //}
        }
    }
}