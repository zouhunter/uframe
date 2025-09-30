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
    public class FlyAnimPlayer : PanelAnimPlayer
    {
        [SerializeField]
        private Vector3 posA = Vector3.left * 100; 
        [SerializeField]
        private Vector3 posB = Vector3.zero;
     
        protected override TweenBase CreateTweener(RectTransform rectTrans)
        {
            return new TweenAnchoredPosition(rectTrans, posA, posB);
        }
    }

}