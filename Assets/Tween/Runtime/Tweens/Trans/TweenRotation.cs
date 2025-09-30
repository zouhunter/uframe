using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenRotation : TweenBase<Quaternion>
    {
        public override bool Valid => m_target;
        [SerializeField]
        protected Transform m_target;
        public Transform target
        {
            get
            {
                return m_target;
            }
            set
            {
                m_target = value;
            }
        }
        public override Quaternion value { get => target.localRotation; set => target.localRotation = value; }
        protected override void OnUpdate(float _factor, bool _isFinished)
        {
            value = Quaternion.Lerp(from, to, _factor);
        }
        public TweenRotation() { }
        public TweenRotation(Transform trans, Quaternion from, Quaternion to, float duration = 1f, float delay = 0f)
        {
            this.target = trans;
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

        public TweenRotation(Transform trans, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f) : this(trans, Quaternion.Euler(from), Quaternion.Euler(to), duration, delay)
        {
        }
    }
}