using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenSizeDelta : TweenBase<Vector2>
    {
        public RectTransform mRectTransform;
        public RectTransform cacheRectTransform
        {
            get
            {
                return mRectTransform;
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
            cacheRectTransform.sizeDelta = value;
        }
        public TweenSizeDelta()
        {
        }
        public TweenSizeDelta(RectTransform go, Vector2 from, Vector2 to, float duration, float delay)
        {
            this.value = from;
            this.from = from;
            this.to = to;
            this.delay = delay;
            this.mRectTransform = go;

            if (duration <= 0)
            {
                this.Sample(1, true);
                this.enabled = false;
            }
            else
            {
                this.enabled = true;
            }
        }
    }
}
