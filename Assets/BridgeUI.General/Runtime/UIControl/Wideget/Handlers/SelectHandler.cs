using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace UFrame.BridgeUI
{
    public class SelectHandler : MonoBehaviour ,IUpdateSelectedHandler,ISelectHandler,IDeselectHandler
    {
        [SerializeField]
        protected BaseSystemEvent _onUpdateSelected;
        [SerializeField]
        protected BaseSystemEvent _onSelect;
        [SerializeField]
        protected BaseSystemEvent _onDeselect;

        public BaseSystemEvent onUpdateSelected { get { return _onUpdateSelected; } }
        public BaseSystemEvent onSelect { get { return _onSelect; } }
        public BaseSystemEvent onDeselect { get { return _onDeselect; } }

        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if(onUpdateSelected != null)
            {
                onUpdateSelected.Invoke(eventData);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null)
            {
                onSelect.Invoke(eventData);
            }
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (onDeselect != null)
            {
                onDeselect.Invoke(eventData);
            }
        }

    }
}