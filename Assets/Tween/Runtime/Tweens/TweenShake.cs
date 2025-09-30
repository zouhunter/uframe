using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenShake : TweenBase
    {
        public enum TweenTransOption
        {
            Position = 1,
            Rotation = 1 << 1,
            Scale = 1 << 2
        }

        public Transform target;

        private Vector3 localPosition = Vector3.zero;
        private Vector3 position = Vector3.zero;
        private Vector3 localScale = Vector3.zero;
        private Vector3 localEulerAngles = Vector3.zero;
        private Vector3 eulerAngles = Vector3.zero;

        public Vector3 limit;
        public Space space = Space.Self;
        public TweenTransOption shakeType = TweenTransOption.Position;

        private Vector3 mValue;
        public Vector3 value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                Shake();
            }
        }

        private void CacheTargetInfo()
        {
            localPosition = target.localPosition;
            position = target.position;
            localScale = target.localScale;
            localEulerAngles = target.localEulerAngles;
            eulerAngles = target.eulerAngles;
        }

        public override void EnableTween()
        {
            base.EnableTween();
            CacheTargetInfo();
        }


        protected override void OnUpdate(float factor, bool isFinished)
        {
            float x = limit.x * factor;
            float y = limit.y * factor;
            float z = limit.z * factor;
            mValue.x = UnityEngine.Random.Range(x * -1, x);
            mValue.y = UnityEngine.Random.Range(y * -1, y);
            mValue.z = UnityEngine.Random.Range(z * -1, z);
            value = mValue;
        }

        void Shake()
        {
            if (shakeType == TweenTransOption.Position)
            {
                if (space == Space.Self)
                {
                    target.localPosition = value + localPosition;
                }
                else
                {
                    target.position = value + position;
                }
            }
            else if (shakeType == TweenTransOption.Scale)
            {
                target.localScale = value + localScale;
            }
            else
            {
                if (space == Space.Self)
                {
                    target.localEulerAngles = value + localEulerAngles;
                }
                else
                {
                    target.eulerAngles = value + eulerAngles;
                }
            }
        }
        public TweenShake()
        {
        }
        public TweenShake(RectTransform trans, Vector3 from, float duration = 1f, float delay = 0f)
        {
            this.limit = from;
            this.duration = duration;
            this.delay = delay;
            if (duration <= 0)
            {
                Sample(1, true);
                enabled = false;
            }
        }
    }

}