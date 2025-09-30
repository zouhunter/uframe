using System;
using UnityEngine;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenFloat : TweenBase<float>
    {
        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = Mathf.Lerp(from, to, factor);
        }
        public TweenFloat() { }
        public TweenFloat(float from, float to, float duration, float delay = 0)
        {
            this.value = from;
            this.from = from;
            this.to = to;
            this.delay = delay;
            this.duration = duration;
            if (duration <= 0)
            {
                this.Sample(1, true);
                this.enabled = false;
            }
        }
    }

    [System.Serializable]
    public class TweenFloatDynimic : TweenBaseDynimic<float>
    {
        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = Mathf.Lerp(from, to, factor);
        }
        public TweenFloatDynimic(Func<float> getValue, Action<float> setValue, float to, float duration, float delay = 0)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.to = to;
            this.duration = duration;
            this.method = EaseLib.linear;
            if (duration <= 0)
            {
                this.Sample(1, true);
                this.enabled = false;
            }
        }
    }
}
