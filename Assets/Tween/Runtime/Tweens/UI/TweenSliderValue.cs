using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]

    public class TweenSliderValue : TweenBase<float>
    {

        public Slider cacheSlider;

        /// <summary>
        /// The need carry.
        /// when is true, value==1 equal value=0
        /// </summary>
        public bool NeedCarry = false;

        public float sliderValue
        {
            set
            {
                if (NeedCarry)
                {
                    cacheSlider.value = (value >= 1) ? value - Mathf.Floor(value) : value;
                }
                else
                {
                    cacheSlider.value = (value > 1) ? value - Mathf.Floor(value) : value;
                }
            }
        }

        protected override void OnUpdate(float value, bool isFinished)
        {
            this.sliderValue = value;
        }
        public TweenSliderValue()
        {
        }
        public TweenSliderValue(Slider slider, float from, float to, float duration, float delay)
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
