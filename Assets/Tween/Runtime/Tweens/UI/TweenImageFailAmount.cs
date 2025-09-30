using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenImageFailAmount : TweenBase
    {
        [Range(0, 1)]
        public float from;
        [Range(0, 1)]
        public float to;

        float mValue;
        public float value
        {
            get { return mValue; }
            set
            {
                mValue = value;
            }
        }

        public Image mImage;
        public Image cacheImage
        {
            get
            {
                if (mImage.type != Image.Type.Filled)
                {
                    Debug.LogWarning("[uTweenImage] To use tween the image type must be [Image.Type.Filled]");
                }
                return mImage;
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
            cacheImage.fillAmount = value;
        }
        public TweenImageFailAmount()
        {
        }
        public TweenImageFailAmount(Image go, float from, float to, float duration, float delay)
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
