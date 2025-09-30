using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace UFrame.BridgeUI {
    public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected PointerSystemEvent _onPointerEnter;
        [SerializeField]
        protected PointerSystemEvent _onPointerExit;

        public PointerSystemEvent onPointerEnter { get { return _onPointerEnter; } }
        public PointerSystemEvent onPointerExit { get { return _onPointerExit; } }
        /// <summary>
        /// 鼠标输入
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (onPointerEnter != null)
            {
                onPointerEnter.Invoke(eventData);
            }
        }
        /// <summary>
        /// 鼠标移出
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (onPointerExit != null)
            {
                onPointerExit.Invoke(eventData);
            }
        }
    }
}
