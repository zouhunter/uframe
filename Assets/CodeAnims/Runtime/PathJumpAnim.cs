//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-17 11:34:36
//* 描    述： 定点跳跃

//* ************************************************************************************
using UnityEngine;
using UFrame.Tween;
using System.Linq;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/PathJumpAnim")]
    public class PathJumpAnim : CodeAnimBase
    {
        [SerializeField, DefaultComponent]
        protected Transform m_target;
        [SerializeField]
        protected Transform[] m_paths;
        [SerializeField]
        protected float m_jumpHeight = 1;
        [SerializeField]
        protected float m_dureation = 1;
        [SerializeField]
        protected bool m_moveBack;
        [SerializeField]
        protected int m_loopCount = 1;

        protected TweenJumpPath m_tweenPath;
        protected Vector3 m_startPos;
        protected Vector3 m_startRot;
        public override bool IsPlaying => m_tweenPath?.IsPlaying ?? false;

        protected override void RecordState()
        {
            m_startPos = m_target.position;
            m_startRot = m_target.eulerAngles;
        }
        public override void Play()
        {
            if (m_tweenPath == null)
            {
                var paths = from pathNode in m_paths select pathNode.position;
                m_tweenPath = new TweenJumpPath(m_target, paths.ToArray(), m_jumpHeight, m_dureation, true);
                m_tweenPath.SetLoopCount(m_loopCount);
                if (m_moveBack)
                {
                    m_tweenPath.style = TweenBase.Style.PingPong;
                }
                else if (m_loopCount > 1)
                {
                    m_tweenPath.style = TweenBase.Style.Loop;
                }
            }

            if (!m_tweenPath.IsPlaying)
            {
                StartTween(m_tweenPath);
            }
        }
        public override void ResetState()
        {
            m_target.position = m_startPos;
            m_target.eulerAngles = m_startRot;

            if (m_tweenPath != null && m_tweenPath.IsPlaying)
            {
                StopTween(m_tweenPath);
            }
        }

    }
}