using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace UFrame.Tween
{
    [System.Serializable]

    public class TweenVector3 : TweenBase<Vector3>
    {
        protected override void OnUpdate(float factor, bool isFinished)
        {
            this.value = from + factor * (to - from);
        }
        public TweenVector3() { }

        public TweenVector3 (Vector3 from, Vector3 to, float duration, float delay)
        {
            this.value = from;
            this.from = from;
            this.to = to;
            this.delay = delay;
            if (duration <= 0)
            {
                this.Sample(1, true);
                this.enabled = false;
            }
        }
    }
}
