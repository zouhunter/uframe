using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenAnchoredPosition : TweenBase<Vector3>
    {
        public override bool Valid => cachedRectTransform;
        public RectTransform cachedRectTransform;

        public override Vector3 value
        {
            get { return cachedRectTransform.anchoredPosition; }
            set { cachedRectTransform.anchoredPosition = value; }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
        }
        public TweenAnchoredPosition() { }
        public TweenAnchoredPosition(RectTransform trans, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
        {
            this.cachedRectTransform = trans;
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
