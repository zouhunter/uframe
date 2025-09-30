//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-16 11:59:37
//* 描    述： 树飘动动画

//* ************************************************************************************
using System;
using UnityEngine;
using UFrame.Tween;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/TreeWaveAnim")]
    public class TreeWaveAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected float m_angleMin = 1f;
        [SerializeField]
        protected float m_angleMax = 5f;
        [SerializeField]
        protected float m_dureation = 1;
        [SerializeField]
        protected int m_count = 3;

        protected Quaternion m_startRot;
        protected TweenFloat m_tween;
        protected float m_targetAngle;
        protected int m_counter;
        protected Vector3 m_randomAxis;
        public override bool IsPlaying => m_tween?.IsPlaying ?? false;

        protected override void RecordState()
        {
            if (m_target)
                m_startRot = m_target.transform.localRotation;
        }

        public override void Play()
        {
            if (m_tween == null)
            {
                ReCreateRandom();
                m_tween = new TweenFloat(0, m_targetAngle, m_dureation, 0);
                m_tween.style = TweenBase.Style.PingPong;
                m_tween.SetLoopCount(m_count);
                m_tween.onStepFinished += OnStepFinished;
                m_tween.onUpdate += OnTweenUpdate;
            }
            if (!m_tween.IsPlaying)
            {
                m_counter = 0;
                m_tween.ResetToBeginning();
                StartTween(m_tween);
            }
        }

        private void OnTweenUpdate()
        {
            if (m_target)
                m_target.localRotation = Quaternion.AngleAxis(m_tween.value, m_randomAxis) * m_startRot;
        }

        private void OnStepFinished(bool forward)
        {
            if (forward)
            {
                if (m_counter >= m_count - 1)
                {
                    m_targetAngle = 0;
                }
                else
                {
                    m_targetAngle = -m_targetAngle;
                }
                m_tween.from = m_targetAngle;
            }
            else
            {
                ++m_counter;
                ReCreateRandom();
                m_tween.to = m_targetAngle;
            }
        }

        protected void ReCreateRandom()
        {
            m_targetAngle = UnityEngine.Random.Range(m_angleMin, m_angleMax);
            m_randomAxis.x = UnityEngine.Random.Range(0.5f, 1);
            m_randomAxis.z = UnityEngine.Random.Range(0.5f, 1);
        }

        public override void ResetState()
        {
            if (m_target)
            {
                m_target.transform.localRotation = m_startRot;
            }

            if (m_tween != null && m_tween.IsPlaying)
            {
                StopTween(m_tween);
            }
        }
    }
}
