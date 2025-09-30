using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UFrame.BridgeUI;
using UFrame.Tween;
using System;

namespace UFrame.BridgeUI
{
    public abstract class PanelAnimPlayer : AnimPlayer
    {
        [SerializeField]
        protected float animTime = 1;
        [SerializeField]
        protected UnityEngine.AnimationCurve curve;
        protected TweenBase tween;

        protected override List<TweenBase> CreateTweeners()
        {
            var tweenList = new List<TweenBase>();
            var rectTrans = panel.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                var tweener = CreateTweener(rectTrans);
                tweener.duration = animTime;
                tweener.animationCurve = curve;
                tweenList.Add(tweener);
            }
            return tweenList;
        }

        protected abstract TweenBase CreateTweener(RectTransform rectTrans);
    }

}