using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace UFrame.BridgeUI
{
    public class UIEventListener : BridgeUIControl, IScrollHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IEventSystemHandler
    {
        public GameObject target { get; private set; }

        private PressHandler _pressHandler;
        private HoverHandler _hoverHandler;
        private DragHandler _dragHandler;
        private SelectHandler _selectHandler;

        private PressHandler pressHandler
        {
            get
            {
                if(_pressHandler == null)
                {
                    _pressHandler = FindOrCreateComponent<PressHandler>(target);
                }
                return _pressHandler;
            }
        }
        private HoverHandler hoverHandler
        {
            get
            {
                if(_hoverHandler == null)
                {
                    _hoverHandler = FindOrCreateComponent<HoverHandler>(target);
                }
                return _hoverHandler;
            }
        }
        private DragHandler dragHandler
        {
            get
            {
                if(_dragHandler  == null)
                {
                    _dragHandler = FindOrCreateComponent<DragHandler>(target);
                }
                return _dragHandler;
            }
        }
        private SelectHandler selectHandler
        {
            get
            {
                if(_selectHandler == null)
                {
                    _selectHandler = FindOrCreateComponent<SelectHandler>(target);
                }
                return _selectHandler;
            }
        }

        public PointerSystemEvent onPointDown { get { return pressHandler.onPointDown; } }
        public PointerSystemEvent onPointUp { get { return pressHandler.onPointUp; } }
        public PointerSystemEvent onPointClick { get { return pressHandler.onPointClick; } }
        public PointerSystemEvent onDoubleClick { get { return pressHandler.onDoubleClick; } }
        public PointerSystemEvent onLongPointDown { get { return pressHandler.onLongPointDown; } }
        public UnityEngine.UI.Toggle.ToggleEvent onPress { get { return pressHandler.onPress; } }
        public PointerSystemEvent onPointerEnter { get { return hoverHandler.onPointerEnter; } }
        public PointerSystemEvent onPointerExit { get { return hoverHandler.onPointerExit; } }
        public PointerSystemEvent onDrag { get { return dragHandler.onDrag; } }
        public PointerSystemEvent onDrop { get { return dragHandler.onDrop; } }
        public PointerSystemEvent onBeginDrag { get { return dragHandler.onBeginDrag; } }
        public PointerSystemEvent onEndDrag { get { return dragHandler.onEndDrag; } }
        public PointerSystemEvent onInitializePotentialDrag { get { return dragHandler.onInitializePotentialDrag; } }
        public BaseSystemEvent onUpdateSelected { get { return selectHandler.onUpdateSelected; } }
        public BaseSystemEvent onSelect { get { return selectHandler.onSelect; } }
        public BaseSystemEvent onDeselect { get { return selectHandler.onDeselect; } }


        public virtual void OnScroll(PointerEventData eventData)
        {
            Debug.Log("OnScroll:" + eventData.scrollDelta);
        }

        public virtual void OnMove(AxisEventData eventData)
        {
            Debug.Log("OnMove:" + eventData.moveDir);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Debug.Log("OnSubmit:" + eventData.selectedObject);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            Debug.Log("OnCancel:" + eventData.selectedObject);
        }


        public static UIEventListener Get(GameObject target)
        {
            if (target == null) return null;
            var uiListener = FindOrCreateComponent<UIEventListener>(target);
            uiListener.target = target;
            return uiListener;
        }

        private static T FindOrCreateComponent<T>(GameObject target) where T : MonoBehaviour
        {
            T component = default(T);
            component = target.GetComponent<T>();
            if (component == null){
                component = target.AddComponent<T>();
            }
            return component;
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnRelease()
        {
        }
    }
}