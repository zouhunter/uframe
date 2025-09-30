/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - UGUI摇杆                                                                        *
*//************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/UIJoystickBehaviour")]
    public class UIJoystickBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        public UnityEvent onDragBegin;
        [SerializeField]
        public UnityEvent<Vector2> onDrag;
        [SerializeField]
        public UnityEvent onDragEnd;

        [SerializeField]
        protected Transform target;
        [SerializeField]
        protected float radius = 50f;
        [SerializeField]
        protected Vector2 position;

        private bool isDragging = false;

        private RectTransform thumb;

        protected virtual void Start()
        {
            thumb = target.GetComponent<RectTransform>();
        }

        public void OnBeginDrag(PointerEventData data)
        {
            isDragging = true;
            if (onDragBegin != null)
                onDragBegin.Invoke();
        }

        public void OnDrag(PointerEventData data)
        {
            RectTransform draggingPlane = transform as RectTransform;
            Vector3 mousePos;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out mousePos))
            {
                thumb.position = mousePos;
            }
            float length = target.localPosition.magnitude;

            if (length > radius)
            {
                target.localPosition = Vector3.ClampMagnitude(target.localPosition, radius);
            }
            position = target.localPosition;
            position = position / radius * Mathf.InverseLerp(radius, 2, 1);
        }

        protected virtual void Update()
        {
            if (isDragging && onDrag != null)
                onDrag.Invoke(position);
        }

        public virtual void OnEndDrag(PointerEventData data)
        {
            position = Vector2.zero;
            target.position = transform.position;

            isDragging = false;
            if (onDragEnd != null)
                onDragEnd.Invoke();
        }
    }

}