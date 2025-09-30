//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-14 11:27:51
//* 描    述： 脚本动画通用类

//* ************************************************************************************
using UnityEngine;
using UFrame.Tween;
using System.Collections.Generic;

namespace UFrame.CodeAnimation
{
    public abstract class CodeAnimBase : MonoBehaviour
    {
        [SerializeField]
        protected bool m_playOnEnable;
        protected bool m_recorded;
        public static TweenCtrl tweenCtrl { get; set; }
        public abstract bool IsPlaying { get; }
        public static bool Forbid { get; set; }
        public virtual bool MoveAble { get; } = true;
        public ref bool PlayOnEnable => ref m_playOnEnable;
        public bool IgnoreForbid { get; set; }

        public virtual void OnEnable()
        {
            if (Forbid && !IgnoreForbid)
                return;

            if (!m_recorded)
            {
                RecordState();
                m_recorded = true;
            }

            if (m_playOnEnable)
            {
                Play();
            }
        }
        protected virtual void OnDisable()
        {
            ResetState();
        }
        protected abstract void RecordState();
        public abstract void Play();
        public abstract void ResetState();
        protected T SaftyGetComponent<T>() where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (!component)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
        public void StartTween(ITween tween)
        {
            if (tweenCtrl == null)
            {
                Debug.LogWarning("CodeAnimBase.tweenCtrl not reg,span new tweenCtrl!");
                tweenCtrl = TweenCtrlBehaviour.Instance.tweenCtrl;
            }
            tweenCtrl.StartTween(tween);
        }
        public void StopTween(ITween tween)
        {
            if (tweenCtrl != null)
                tweenCtrl.StopTween(tween);
        }
    }
}