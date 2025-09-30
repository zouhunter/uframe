using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace UFrame.BridgeUI
{
    public class PressHandler : BridgeUIControl, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {

        [SerializeField]
        private PointerSystemEvent _onPointDown;
        [SerializeField]
        private PointerSystemEvent _onPointUp;
        [SerializeField]
        private PointerSystemEvent _onPointClick;
        [SerializeField]
        private UnityEngine.UI.Toggle.ToggleEvent _onPress;
        [SerializeField]
        private PointerSystemEvent _onLongPointDown;
        [SerializeField]
        private PointerSystemEvent _onDoubleClick;

        private float dobuleClickTimeSpan = 0.5f;
        private float longPressTime = 1f;

        public PointerSystemEvent onPointDown
        {
            get
            {
                if (_onPointDown == null)
                {
                    _onPointDown = new PointerSystemEvent();
                }
                return _onPointDown;
            }
        }
        public PointerSystemEvent onPointUp
        {
            get
            {
                if (_onPointUp == null)
                {
                    _onPointUp = new PointerSystemEvent();
                }
                return _onPointUp;
            }
        }
        public PointerSystemEvent onPointClick
        {
            get
            {
                if (_onPointClick == null)
                {
                    _onPointClick = new PointerSystemEvent();
                }
                return _onPointClick;
            }
        }
        public PointerSystemEvent onDoubleClick
        {
            get
            {
                if (_onDoubleClick == null)
                {
                    _onDoubleClick = new PointerSystemEvent();
                }
                return _onDoubleClick;
            }
        }
        public PointerSystemEvent onLongPointDown
        {
            get
            {
                if (_onLongPointDown == null)
                {
                    _onLongPointDown = new PointerSystemEvent();
                }
                return _onLongPointDown;
            }
        }
        public UnityEngine.UI.Toggle.ToggleEvent onPress
        {
            get
            {
                if (_onPress == null)
                {
                    _onPress = new UnityEngine.UI.Toggle.ToggleEvent();
                }
                return _onPress;
            }
        }

        private float pressTimer;
        private Coroutine longPressCoroutine;

        /// <summary>
        /// 按下
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (onPointDown != null)
            {
                onPointDown.Invoke(eventData);
            }

            if (onPress != null )
            {
                onPress.Invoke(true);
            }

            if (onLongPointDown != null)
            {
                longPressCoroutine = StartCoroutine(DelyPress(eventData));
            }
        }

        /// <summary>
        /// 抬起
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (onPointUp != null)
            {
                onPointUp.Invoke(eventData);
            }

            if (onPress != null)
            {
                onPress.Invoke(false);
            }

            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                longPressCoroutine = null;
            }
        }

        /// <summary>
        /// 点击
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (onPointClick != null)
            {
                onPointClick.Invoke(eventData);
            }

            if (Time.time - pressTimer < dobuleClickTimeSpan)
            {
                if (onDoubleClick != null)
                {
                    onDoubleClick.Invoke(eventData);
                }
            }
            else
            {
                pressTimer = Time.time;
            }
        }

        private IEnumerator DelyPress(PointerEventData eventData)
        {
            yield return new WaitForSeconds(longPressTime);
            if (gameObject && gameObject.activeSelf && this.isActiveAndEnabled)
            {
                onLongPointDown.Invoke(eventData);
            }
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnRelease()
        {
        }
    }
}