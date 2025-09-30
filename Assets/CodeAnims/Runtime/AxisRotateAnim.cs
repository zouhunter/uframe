//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-14 01:21:22
//* 描    述： 定轴转动动画

//* ************************************************************************************
using UnityEngine;
using UFrame.Tween;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/AxisRotateAnim")]
    public class AxisRotateAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected float m_dureation = 2;
        [SerializeField]
        protected Vector3 m_axis = Vector3.forward;
        [SerializeField]
        protected bool m_moveBack;
        [SerializeField]
        protected int m_angle = 360;
        [SerializeField]
        protected int m_count = 1;
        [SerializeField]
        protected bool m_modelCenter;
        protected TweenAxisRotate m_tweenRotate;
        protected Quaternion m_startRot;

        public ref Vector3 Axis => ref m_axis;
        public ref int Count => ref m_count;
        public ref bool MoveBack => ref m_moveBack;
        public ref float Durection => ref m_dureation;
        public ref bool ModelCenter => ref m_modelCenter;
        public override bool IsPlaying => m_tweenRotate?.IsPlaying ?? false;
        protected float m_lastAngle;
        protected Vector3 m_startPos;
        protected Vector3 m_center;

        protected override void RecordState()
        {
            if (!m_target)
                m_target = transform;
            m_startRot = m_target.localRotation;
            m_startPos = m_target.localPosition;
        }

        private void MakeSureTween()
        {
            if (m_tweenRotate == null)
            {
                m_tweenRotate = new TweenAxisRotate(0, m_angle, m_axis, m_dureation, 0);
                m_tweenRotate.onUpdate += OnTweenUpdate;

                if (m_moveBack)
                {
                    m_tweenRotate.style = TweenBase.Style.PingPong;
                    m_tweenRotate.SetLoopCount(m_count);
                }
                else if (m_count == 1)
                {
                    m_tweenRotate.style = TweenBase.Style.Once;
                }
                else
                {
                    m_tweenRotate.style = TweenBase.Style.Loop;
                    m_tweenRotate.SetLoopCount(m_count);
                }
            }
        }

        private Vector3 GetModelCenter()
        {
            var collider = GetComponent<Collider>();
            if (collider)
            {
                return collider.bounds.center;
            }
            return transform.position;
        }

        public override void Play()
        {
            MakeSureTween();
            if (!m_tweenRotate.IsPlaying)
            {
                ResetState();
                StartTween(m_tweenRotate);
            }
        }

        public void OnTweenUpdate()
        {
            if (m_tweenRotate != null && m_target)
            {
                if (m_modelCenter)
                {
                    var angle = Quaternion.Angle(Quaternion.identity, m_tweenRotate.QuaternionValue);
                    m_target.RotateAround(GetModelCenter(), m_axis, Mathf.Abs(angle - m_lastAngle));
                    m_lastAngle = angle;
                }
                else
                {
                    m_target.localRotation = m_tweenRotate.QuaternionValue * m_startRot;
                }
            }
        }

        public override void ResetState()
        {
            m_target.localRotation = m_startRot;
            transform.localPosition = m_startPos;
            if (m_tweenRotate != null && m_tweenRotate.enabled)
            {
                StopTween(m_tweenRotate);
            }
        }
    }
}