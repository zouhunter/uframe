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
    public class SizeAnimPlayer : PanelAnimPlayer
    {
        [SerializeField]
        private float sizeA = 0.8f;
        [SerializeField]
        private float sizeB = 1f;
     
        protected override TweenBase CreateTweener(RectTransform rectTrans)
        {
            return new TweenScale(rectTrans, Vector3.one * sizeA, Vector3.one * sizeB);
        }
    }

}