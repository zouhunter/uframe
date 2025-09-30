using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UFrame.BridgeUI;

namespace UFrame.BridgeUI
{
    public class InfoTextShow : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField]
        public int fontSize = 10;
        [SerializeField]
        public bool specialSize = false;
        [SerializeField]
        public Color fontColor = Color.white;
        [SerializeField]
        public bool specialColor = false;
        [SerializeField]
        public string textshow;
        [SerializeField]
        public Vector2 showPos;
        private bool isWorld;
        private Hashtable sender;
        private float plusOfScreen;

        public void Start()
        {
            if (string.IsNullOrEmpty(textshow))
            {
                textshow = name;
            }
            isWorld = !GetComponent<RectTransform>();
            plusOfScreen = Screen.width / 1600f;
            OnPointerEnter(null);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Vector3 position = Vector3.zero;

            if (!isWorld)
            {
                position += transform.position;
            }
            else
            {
                position += Camera.main.WorldToScreenPoint(transform.position);
            }

            position.x += showPos.x * plusOfScreen;
            position.y += showPos.y * plusOfScreen;

            if (sender == null) sender = new Hashtable();

            sender["position"] = position;
            sender["textInfo"] = textshow;

            if (specialColor) sender["fontColor"] = fontColor;
            if (specialSize) sender["fontSize"] = fontSize;
            //UIFacade.Instence.Open("TextInfoBehiaver", sender);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            //UIFacade.Instence.Close("TextInfoBehiaver");
        }
    }
}