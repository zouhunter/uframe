/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-12-15                                                                   *
*  功能:                                                                              *
*   - 触摸输入控件类                                                                  *
*//************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UFrame
{
    public class TouchInputBehaviour : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public UnityEvent<float> onScaleOffset;
        public UnityEvent<Vector2> onMoveOffset;
        public UnityEvent<Vector2> onClickPoint;

        private float m_touchDistance;
        private bool m_touchDown;
        private Vector2 m_dragPos;
        private bool uiDrag;
        private bool m_scaling;

        private void Awake()
        {
            uiDrag = GetComponent<Graphic>();
        }

        private void Update()
        {
            if (Application.isMobilePlatform)
            {
                UpdatePhone();
            }
            else
            {
                if (!uiDrag)
                {
                    if (Input.GetMouseButton(0) && !m_scaling)
                    {
                        if (!m_touchDown)
                        {
                            m_dragPos = Input.mousePosition;
                            m_touchDown = true;
                        }
                        else
                        {
                            var pos = Input.mousePosition;
                            var offset = new Vector2(pos.x, pos.y) - m_dragPos;
                            m_dragPos = pos;
                            onMoveOffset?.Invoke(offset);
                        }
                    }
                    else if (m_touchDown)
                    {
                        m_touchDown = false;
                    }
                }
                var scroll = Input.GetAxis("Mouse ScrollWheel") * Screen.width;
                if (scroll < -1 || scroll > 1)
                {
                    onScaleOffset?.Invoke(scroll * Screen.width);
                    m_scaling = true;
                }
            }
        }

        private void UpdatePhone()
        {
            if (Input.touchCount >= 2)
            {
                Touch newTouch1 = Input.GetTouch(0);
                Touch newTouch2 = Input.GetTouch(1);
                var distance = Vector2.Distance(newTouch1.position, newTouch2.position);
                if (m_touchDistance > 1 && onScaleOffset != null)
                {
                    onScaleOffset.Invoke(distance - m_touchDistance);
                }
                m_touchDistance = distance;
                m_scaling = true;
            }
            else if (!uiDrag && Input.touchCount == 1)
            {
                if (!m_touchDown)
                {
                    m_dragPos = Input.GetTouch(0).position;
                    m_touchDown = true;
                }
                else
                {
                    var pos = Input.GetTouch(0).position;
                    var offset = pos - m_dragPos;
                    m_dragPos = pos;
                    onMoveOffset?.Invoke(offset);
                }
                m_scaling = false;
                m_touchDistance = 0;
            }
            else if (m_touchDown)
            {
                m_touchDown = false;
                m_scaling = false;
                m_touchDistance = 0;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_dragPos = eventData.position;
            m_touchDown = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_scaling)
                return;

            var pos = eventData.position;
            var offset = pos - m_dragPos;
            m_dragPos = pos;
            onMoveOffset?.Invoke(offset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_touchDown = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClickPoint?.Invoke(eventData.position);
        }
    }
}