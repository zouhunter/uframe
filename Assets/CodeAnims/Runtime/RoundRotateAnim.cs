//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-16 09:38:11
//* 描    述： 圆形状旋转

//* ************************************************************************************
using System;
using UnityEngine;
using UFrame.Tween;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/RoundRotateAnim")]
    public class RoundRotateAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected Vector3 m_axis;//轴向
        [SerializeField]
        protected Vector3 m_center;//中心点
        [SerializeField]
        protected int m_dureation = 1;
        [SerializeField]
        protected float m_angle = 360;//旋转角度
        [SerializeField]
        protected LookForwardType m_loopForward;
        [SerializeField]
        protected bool m_moveBack;
        [SerializeField]
        protected int m_loopCount = 1;

        protected Vector3 m_localEular;
        protected Vector3 m_localPos;
        protected TweenFloat m_tweenFloat;
        protected float m_lastAngle;
        protected Vector3 m_worldCenter;
        protected Vector3 m_worldAxis;
        protected int m_counter;
        protected Vector3 m_lastPos;
        public override bool IsPlaying => m_tweenFloat?.IsPlaying ?? false;

        public enum LookForwardType
        {
            None,
            Forward,
            Right,
            Up
        }

        protected override void RecordState()
        {
            if (!m_target)
                m_target = transform;
            m_localEular = m_target.localEulerAngles;
            m_localPos = m_target.localPosition;
            m_tweenFloat = new TweenFloat(0, m_angle, m_dureation, 0);
            m_tweenFloat.onUpdate += OnRefreshTween;
            m_tweenFloat.onStepFinished += OnStepFinished;
            if (m_moveBack)
                m_tweenFloat.style = TweenBase.Style.PingPong;
            else if (m_loopCount > 1)
                m_tweenFloat.style = TweenBase.Style.Loop;
        }

        private void OnRefreshTween()
        {
            var currentAngle = m_tweenFloat.value;
            var delt = currentAngle - m_lastAngle;
            m_lastAngle = currentAngle;
            m_target.RotateAround(m_worldCenter, m_worldAxis, delt);
            if (m_loopForward != LookForwardType.None)
            {
                var direction = m_target.position - m_lastPos;
                switch (m_loopForward)
                {
                    case LookForwardType.Forward:
                        m_target.forward = direction;
                        break;
                    case LookForwardType.Right:
                        m_target.right = direction;
                        break;
                    case LookForwardType.Up:
                        m_target.up = direction;
                        break;
                    default:
                        break;
                }
                m_lastPos = m_target.position;
            }
        }

        public override void Play()
        {
            if (m_tweenFloat != null && !m_tweenFloat.IsPlaying)
            {
                m_lastPos = m_localPos;
                ResetState();
                m_tweenFloat.ResetToBeginning();
                StartTween(m_tweenFloat);
            }
        }

        public override void ResetState()
        {
            if (m_tweenFloat != null && m_tweenFloat.enabled)
            {
                StopTween(m_tweenFloat);
            }
            if (m_target)
            {
                m_target.localEulerAngles = m_localEular;
                m_target.localPosition = m_localPos;
                m_worldCenter = m_target.parent ? m_target.parent.TransformPoint(m_center) : m_center;
                m_worldAxis = m_target.parent ? m_target.parent.TransformVector(m_axis) : m_axis;
            }
            m_counter = 0;
        }

        protected void OnStepFinished(bool forward)
        {
            if (!forward && m_moveBack)
            {
                m_counter++;
            }
            else if (forward && !m_moveBack)
            {
                m_counter++;
            }
            if (m_counter >= m_loopCount)
            {
                m_tweenFloat.DisableTween();
            }
        }
    }
}