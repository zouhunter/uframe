using UnityEngine;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenAlpha : TweenFloat
    {
        public CanvasGroup canvasGroup;

        public override bool Valid => canvasGroup;
 
        protected override void OnUpdate(float factor, bool isFinished)
        {
            base.OnUpdate(factor, isFinished);
            canvasGroup.alpha = value;
        }
        public TweenAlpha() { }
        public TweenAlpha(CanvasGroup group, float from, float to, float duration, float delay):base(from,to,duration,delay)
        {
            this.canvasGroup = group;
        }
    }
}
