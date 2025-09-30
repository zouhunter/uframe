using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace UFrame.BridgeUI {
    public class DragHandler : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler, IInitializePotentialDragHandler
    {
        [SerializeField]
        protected PointerSystemEvent _onDrag;
        [SerializeField]
        protected PointerSystemEvent _onDrop;
        [SerializeField]
        protected PointerSystemEvent _onBeginDrag;
        [SerializeField]
        protected PointerSystemEvent _onEndDrag;
        [SerializeField]
        protected PointerSystemEvent _onInitializePotentialDrag;

        public PointerSystemEvent onDrag { get { return _onDrag; } }
        public PointerSystemEvent onDrop { get { return _onDrop; } }
        public PointerSystemEvent onBeginDrag { get { return _onBeginDrag; } }
        public PointerSystemEvent onEndDrag { get { return _onEndDrag; } }
        public PointerSystemEvent onInitializePotentialDrag { get { return _onInitializePotentialDrag; } }


        /// <summary>
        /// 初始化拖拽功能
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if(onInitializePotentialDrag!=null)
            {
                onInitializePotentialDrag.Invoke(eventData);
            }
        }

        /// <summary>
        /// 拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if(onDrag!=null)
            {
                onDrag.Invoke(eventData);
            }
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag!=null)
            {
                onBeginDrag.Invoke(eventData);
            }
        }

        /// <summary>
        /// 释放拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag!=null)
            {
                onEndDrag.Invoke(eventData);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnDrop(PointerEventData eventData)
        {
            if (onDrop!=null)
            {
                onDrop.Invoke(eventData);
            }
        }
    }
}