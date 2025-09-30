using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public class WorldButton : MonoBehaviour
    {
        public UnityEvent onClick = new UnityEvent();
        [SerializeField]
        private bool _interactable;
        public bool Interactable { get { return _interactable; } set { _interactable = value; } }

        private void OnMouseUpAsButton()
        {
            if (Interactable)
            {
                onClick.Invoke();
            }
        }
    }
}