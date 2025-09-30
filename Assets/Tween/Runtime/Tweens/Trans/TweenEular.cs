using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenEular : TweenBase<Vector3>
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

        protected override void OnUpdate(float _factor, bool _isFinished)
        {
            value = Vector3.Lerp(from, to, _factor);
            target.localEulerAngles = value;
        }
        public TweenEular() { }
        public TweenEular(Transform trans, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
        {
            this.target = trans;
            this.value = from;
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.delay = delay;
            this.enabled = true;
            if (duration <= 0)
            {
                this.Sample(1, true);
                this.enabled = false;
            }
        }
    }
}