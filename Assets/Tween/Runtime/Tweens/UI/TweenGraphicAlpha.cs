using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenGraphicAlpha : TweenBase<float>
    {

        public GameObject target;
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

        float mAlpha = 0f;

        public override void EnableTween()
        {
            base.EnableTween();
        }

        public float alpha
        {
            get
            {
                return mAlpha;
            }
            set
            {
                SetAlpha(target.transform, value);
                mAlpha = value;
            }
        }

        protected override void OnUpdate(float value, bool isFinished)
        {
            alpha = value;
        }

        void SetAlpha(Transform _transform, float _alpha)
        {
            foreach (var item in cachedGraphics)
            {
                Color color = item.color;
                color.a = _alpha;
                item.color = color;
            }
        }
        public TweenGraphicAlpha()
        {
        }

        public TweenGraphicAlpha(RectTransform trans, float from, float to, float duration = 1f, float delay = 0f)
        {
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