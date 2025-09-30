using UnityEngine;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenScale : TweenBase<Vector3>
    {
        [SerializeField]
        protected Transform m_target;
        public ref Transform target => ref m_target;

        public override Vector3 value
        {
            get { return m_target.localScale; }
            set { m_target.localScale = value; }
        }
        public override bool Valid => m_target;

       
        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
        }
        public TweenScale()
        {
            from = to = Vector3.one;
        }

        public TweenScale(Transform trans, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
        {
            this.m_target = trans;
            this.value = from;
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.delay = delay;
            if (duration <= 0)
            {
                this.Sample(1, true);
                this.enabled = false;
            }
        }
    }
}
