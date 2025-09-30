//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-16 09:38:11
//* 描    述： 通用计算动画基类

//* ************************************************************************************
using UnityEngine;
using UFrame.Tween;

namespace UFrame.CodeAnimation
{
    public abstract class FloatTweenBase : CodeAnimBase
    {
        [SerializeField]
        protected Transform m_target;
        [SerializeField]
        protected float m_dureation = 1;
        [SerializeField]
        protected bool m_moveBack;
        [SerializeField]
        protected int m_loopCount = 1;
        protected TweenFloat m_tweenFloat;

        protected abstract float GetTargetValue();

        protected override void RecordState()
        {
            if (!m_target)
                m_target = transform;
            var taretValue = GetTargetValue();
            m_tweenFloat =new TweenFloat(0, taretValue, m_dureation, 0);
            m_tweenFloat.onUpdate += OnRefreshTween;
            m_tweenFloat.SetLoopCount(m_loopCount);
            if (m_moveBack)
                m_tweenFloat.style = TweenBase.Style.PingPong;
            else if (m_loopCount > 1)
                m_tweenFloat.style = TweenBase.Style.Loop;
        }

        protected abstract void OnRefreshTween();

        public override void Play()
        {
            if (m_tweenFloat != null && !m_tweenFloat.IsPlaying)
            {
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
        }
    }
}