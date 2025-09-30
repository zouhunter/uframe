using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenPosition : TweenBase<Vector3>
    {
        public override bool Valid => target;
        public Transform target;
        public override Vector3 value
        {
            get { return target.position; }
            set { target.position = value; }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
        }
        public TweenPosition() { }
        public TweenPosition(Transform trans, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
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
    }
}
