//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-16 12:04:23
//* 描    述： 

//* ************************************************************************************
namespace UFrame.Tween
{
    using UnityEngine;
    public class TweenAxisRotate : TweenBase<float>
    {
        public Vector3 axis;
        public Quaternion QuaternionValue { get; protected set; }

        protected override void OnUpdate(float _factor, bool _isFinished)
        {
            value = Mathf.Lerp(from, to, _factor);
            QuaternionValue = Quaternion.AngleAxis(value, axis);
        }
        public TweenAxisRotate() { }
        public TweenAxisRotate(float from, float to, Vector3 axis, float duration = 1f, float delay = 0f)
        {
            this.axis = axis;
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
            else
            {
                this.enabled = true;
            }
        }
    }
}