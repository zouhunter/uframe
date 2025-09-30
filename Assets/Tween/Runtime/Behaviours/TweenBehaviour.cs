using UnityEngine;

namespace UFrame.Tween
{
    public class TweenBehaviour : MonoBehaviour
    {
        private TweenBase m_tween;
        public virtual TweenBase Tween => m_tween;
        protected bool m_autoEnable;

        protected virtual void OnEnable()
        {
            m_autoEnable |= Tween.enabled;
            if (m_autoEnable)
            {
                Tween.ResetToBeginning();
                Tween.EnableTween();
            }
        }

        public void SetTween(TweenBase tween)
        {
            m_tween = tween;
        }

        private void FixedUpdate()
        {
            if (Tween != null && Tween.fixedUpdate)
            {
                Tween.Refresh();
            }
        }

        private void Update()
        {
            if (Tween != null && !Tween.fixedUpdate)
            {
                Tween.Refresh();
            }
        }
    }

    public class TweenBehaviour<T>: TweenBehaviour where T : TweenBase
    {
        [SerializeField]
        private T m_tween;
        public override TweenBase Tween => m_tween;
    }
}