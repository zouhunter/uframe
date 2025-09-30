using UFrame;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.LitUI
{
    public class UIGroupBehaviour : MonoBehaviour
    {
        public UIGroupObject group;
        private Canvas _canvas;
        private void Awake()
        {
            //UIManager.Instance.RegistUIInfos(transform, group.infos.ToArray());
            //_canvas = GetComponent<Canvas>();
            //if (!_canvas)
            //    _canvas = GetComponentInParent<Canvas>();
            //UIManager.SetRootCanvas(_canvas);
        }
    }
}
