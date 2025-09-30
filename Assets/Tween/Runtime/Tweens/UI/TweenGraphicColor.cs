using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenGraphicColor : TweenBase<Color>
    {

        public RectTransform target;
        public bool includeChildren = false;

        UnityEngine.UI.Graphic[] mGraphics;
        UnityEngine.UI.Graphic[] cachedGraphics
        {
            get
            {
                if (mGraphics == null)
                {
                    mGraphics = includeChildren ? target.GetComponentsInChildren<UnityEngine.UI.Graphic>() : target.GetComponents<UnityEngine.UI.Graphic>();
                }
                return mGraphics;
            }
        }

        Color mColor = Color.white;

        public override void EnableTween()
        {
            base.EnableTween();
        }

        public override Color value
        {
            get
            {
                return mColor;
            }
            set
            {
                SetColor(target.transform, value);
                mColor = value;
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = Color.Lerp(from, to, factor);
        }
        public TweenGraphicColor()
        {
        }

        public TweenGraphicColor(RectTransform trans, Color from, Color to, float duration = 1f, float delay = 0f)
        {
            this.target = trans;
            this.value = from;
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

        void SetColor(Transform _transform, Color _color)
        {
            foreach (var item in cachedGraphics)
            {
                item.color = _color;
            }
        }


    }
}
