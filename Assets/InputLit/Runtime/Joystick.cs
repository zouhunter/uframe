//* *************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-06-16 13:58:15
//* 描    述： 摇杆输入

//* ************************************************************************************
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace UFrame.Inputs
{
    public class Joystick : MonoBehaviour
    {
        public RectTransform joyBG;
        public RectTransform joyHandle;
        public bool autoHide;
        public float handleRange = 1;
        public float activeOffset = 0.1f;

        private int m_ignoreLayers;
        private bool m_dragging;
        private bool m_dragging2;
        private Canvas m_canvas;
        private Vector2 m_offset;
        private Vector2 m_offset2;
        private Vector2 m_lastPosition2;
        private System.Action onStartDrag { get; set; }
        private System.Action onStopDrag { get; set; }
        private System.Action<Vector2> onDragMove { get; set; }

        private System.Action onStartDrag2 { get; set; }
        private System.Action onStopDrag2 { get; set; }
        private System.Action<Vector2> onDragMove2 { get; set; }

        public System.Func<bool> onCheckActiveForbid { get; set; }

        private bool m_checkDrag1;
        private bool m_checkDrag2;

        private bool m_touchDown1;
        private bool m_touchUp1;
        private bool m_touch1;

        private bool m_touchDown2;
        private bool m_touchUp2;
        private bool m_touch2;
        private const int EMPTY_TOUCH_ID = -1;
        private int m_touch1Id = EMPTY_TOUCH_ID;
        private int m_touch2Id = EMPTY_TOUCH_ID;

        public void SetCanvas(Canvas canvas)
        {
            m_canvas = canvas;
        }

        public void SetIgnoreLayers(int layers)
        {
            m_ignoreLayers = layers;
        }

        public void RegistDrag(System.Action startDrag,System.Action stopDrag,System.Action<Vector2> onDrag)
        {
            m_checkDrag1 = true;
            this.onStartDrag = startDrag;
            this.onStopDrag = stopDrag;
            this.onDragMove = onDrag;
        }

        public void RegistDrag2(System.Action startDrag, System.Action stopDrag, System.Action<Vector2> onDrag)
        {
            m_checkDrag2 = true;
            this.onStartDrag2 = startDrag;
            this.onStopDrag2 = stopDrag;
            this.onDragMove2 = onDrag;
        }

        public void RemoveDrag1()
        {
            m_checkDrag1 = false;
            this.onStartDrag = null;
            this.onStopDrag = null;
            this.onDragMove = null;
        }

        public void RemoveDrag2()
        {
            m_checkDrag2 = false;
            this.onStartDrag2 = null;
            this.onStopDrag2 = null;
            this.onDragMove2 = null;
        }


        private void Update()
        {
            if (!m_canvas)
                return;

            if (Application.isMobilePlatform)
            {
                var touchCount = Input.touchCount;

                if(m_touch1Id == EMPTY_TOUCH_ID && touchCount > 0)
                {
                    var touch1 = Input.GetTouch(0);
                    m_touch1Id = touch1.fingerId;
                    m_touchDown1 = true;
                }
                else
                {
                    m_touchDown1 = false;
                }

                if(m_touch1Id != EMPTY_TOUCH_ID && touchCount < 1)
                {
                    m_touch1Id = EMPTY_TOUCH_ID;
                    m_touchUp1 = true;
                }
                else
                {
                    m_touchUp1 = false;
                }

                if(m_touch2Id == EMPTY_TOUCH_ID && touchCount > 1)
                {
                    var touch2 = Input.GetTouch(1);
                    m_touch2Id = touch2.fingerId;
                    m_touchDown2 = true;
                }
                else
                {
                    m_touchDown2 = false;
                }

                if (m_touch2Id != EMPTY_TOUCH_ID && touchCount < 2)
                {
                    m_touch2Id = EMPTY_TOUCH_ID;
                    m_touchUp2 = true;

                    if (touchCount > 0)
                    {
                        var touch1 = Input.GetTouch(0);
                        m_touch1Id = touch1.fingerId;
                        OnStartDrag1();
                    }
                }
                else
                {
                    m_touchUp2 = false;
                }
                m_touch1 = m_touch1Id != EMPTY_TOUCH_ID;
            }
            else
            {
                m_touchDown1 = Input.GetMouseButtonDown(0);
                if (!m_touchDown1)
                    m_touchUp1 = Input.GetMouseButtonUp(0);

                m_touchDown2 = Input.GetMouseButtonDown(1);
                if (!m_touchDown2)
                    m_touchUp2 = Input.GetMouseButtonUp(1);
            }

            if(m_checkDrag1)
            {
                if (m_touchDown1)
                {
                    OnMouseButtonDown1();
                }
                else if (m_touchUp1)
                {
                    OnMouseButtonUp1();
                }
                else if (m_dragging)
                {
                    DragMove1();
                }
                else
                {
                    TryStartDrag1();
                }
            }

            if(m_checkDrag2)
            {
                if (m_touchDown2)
                {
                    OnMouseButtonDown2();
                }
                else if (m_touchUp2)
                {
                    OnMouseButtonUp2();
                }
                else if (m_dragging2)
                {
                    DragMove2();
                }
                else
                {
                    TryStartDrag2();
                }
            }
        }

        public void OnMouseButtonDown1()
        {
            if (!joyBG || !joyHandle)
            {
                Debug.LogError("!joyBG || !joyHandle");
                return;
            }
            m_offset = GetTouchPos1();
        }

        private void TryStartDrag1()
        {
            if (Application.isMobilePlatform)
            {
                m_touch1 = m_touch1Id != EMPTY_TOUCH_ID;
            }
            else
            {
                m_touch1 = Input.GetMouseButton(0);
            }

            if (m_touch1 && Vector2.Distance(GetTouchPos1(), m_offset) > activeOffset)
            {
                OnStartDrag1();
            }
        }

        public void OnMouseButtonUp1()
        {
            if (!joyBG || !joyHandle)
            {
                Debug.LogError("!joyBG || !joyHandle");
                return;
            }
            if (m_dragging)
            {
                onStopDrag?.Invoke();
                if (joyHandle)
                    joyHandle.anchoredPosition = Vector2.zero;
            }
            m_dragging = false;
            if (autoHide)
                joyBG.gameObject.SetActive(false);
        }

        protected void DragMove1()
        {
            Vector2 position = joyBG.anchoredPosition;
            Vector2 radius = joyBG.sizeDelta / 2;
            m_offset = (GetTouchPos1() / m_canvas.scaleFactor - position);
            var clampRange = radius.magnitude * handleRange;
            m_offset = m_offset.normalized * Mathf.Clamp(m_offset.magnitude, 0, clampRange);
            joyHandle.anchoredPosition = m_offset;
            onDragMove?.Invoke(m_offset.normalized);
        }

        public void OnMouseButtonDown2()
        {
            m_offset2 = GetTouchPos2();
            m_lastPosition2 = m_offset2;
        }

        private void TryStartDrag2()
        {
            if (Application.isMobilePlatform)
            {
                m_touch2 = m_touch2Id != EMPTY_TOUCH_ID;
            }
            else
            {
                m_touch2 = Input.GetMouseButton(1);
            }

            if (m_touch2 && Vector2.Distance(GetTouchPos2(), m_offset2) > activeOffset)
            {
                OnStartDrag2();
            }
        }

        private void OnStartDrag2()
        {
            onStartDrag2?.Invoke();
            m_dragging2 = true;
            m_lastPosition2 = GetTouchPos2();
        }

        public void OnMouseButtonUp2()
        {
            onStopDrag2?.Invoke();
            m_dragging2 = false;
        }

        protected void DragMove2()
        {
            var currentPos = GetTouchPos2();
            m_offset2 = ((currentPos - m_lastPosition2) / m_canvas.scaleFactor);
            m_lastPosition2 = currentPos;
            m_offset2 = 2 * m_offset2 / joyBG.sizeDelta;
            onDragMove2?.Invoke(m_offset2);
        }

        private Vector2 GetTouchPos1()
        {
            Vector3 pos = Vector3.zero;
            if(Application.isMobilePlatform)
            {
                if(Input.touchCount > 0)
                {
                    var touch = Input.GetTouch(0);
                    if(touch.fingerId == m_touch1Id || m_touch1Id == 0)
                    {
                        m_touch1Id = touch.fingerId;
                        pos = touch.position;
                    }
                }
            }
            else
            {
                pos = Input.mousePosition;
            }
            return pos;
        }

        private Vector2 GetTouchPos2()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount > 1)
                {
                    var touch = Input.GetTouch(1);
                    return touch.position;
                }
                return m_lastPosition2;
            }
            else
            {
                return Input.mousePosition;
            }
        }

        private void OnStartDrag1()
        {
            if (onCheckActiveForbid != null && onCheckActiveForbid.Invoke())
                return;

            if (m_canvas)
                joyBG.anchoredPosition = GetTouchPos1() / m_canvas.scaleFactor;
            else
                joyBG.anchoredPosition = GetTouchPos1();
            joyHandle.anchoredPosition = Vector2.zero;
            joyBG.gameObject.SetActive(true);
            onStartDrag?.Invoke();
            m_dragging = true;
        }

        /// <summary>
        /// 检测点击到UI
        /// </summary>
        /// <returns></returns>
        public bool IsTouchUI()
        {
            if (joyBG.gameObject.activeSelf)
                return false;

            if (Application.isMobilePlatform)
            {
                int fingerId = Input.GetTouch(0).fingerId;
                if (EventSystem.current.IsPointerOverGameObject(fingerId))
                {
                    return true;
                }
            }
            else
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 检测点击到场景里面的UI
        /// </summary>
        /// <returns></returns>
        public bool IsTouchWorldUI()
        {
            if (!Camera.main && m_ignoreLayers != 0)
                return false;
            Ray ray = Camera.main.ScreenPointToRay(GetTouchPos1());
            if (Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, m_ignoreLayers))
            {
                return true;
            }
            return false;
        }
    }
}