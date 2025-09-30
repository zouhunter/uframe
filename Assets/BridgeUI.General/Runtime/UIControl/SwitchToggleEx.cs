using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    [RequireComponent(typeof(Toggle))]
    public class SwitchToggleEx : MonoBehaviour
    {
        [SerializeField]
        protected Text label;
        [SerializeField]
        protected Color off_Lable_Color = Color.white;
        [SerializeField]
        protected Color on_Lable_Color = Color.white;
        private Toggle toggle;

        protected void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(SwitchSprite);
        }
        private void SwitchSprite(bool isOn)
        {
            toggle.targetGraphic.enabled = !isOn;
            if(label != null)
            {
                label.color = isOn ? on_Lable_Color : off_Lable_Color;
            }
        }
        
    }

}
