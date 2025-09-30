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
    public class RotAnimPlayer : PanelAnimPlayer
    {
        [SerializeField]
        private float angleA = 500f;
        [SerializeField]
        private float angleB = 0f;
     
        protected override TweenBase CreateTweener(RectTransform rectTrans)
        {
            return new TweenRotation(rectTrans,Vector3.forward * angleA, Vector3.forward * angleB);
        }
    }

}