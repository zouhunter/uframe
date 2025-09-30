using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenTextNum : TweenBase<float>
    {
        public Text cacheText;

        /// <summary>
        /// number after the digit point
        /// </summary>
        public int digits;

        protected override void OnUpdate(float value, bool isFinished)
        {
            cacheText.text = (System.Math.Round(value, digits)).ToString();
        }
        public TweenTextNum()
        {
        }
        public TweenTextNum(Text label, float from, float to, float duration, float delay)
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
