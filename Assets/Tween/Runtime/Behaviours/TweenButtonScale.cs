using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UFrame.Tween
{
    [AddComponentMenu("UFrame/Tween/TweeButton Scale")]
    public class TweenButtonScale : MonoBehaviour, IPointerEnterHandler,
        IPointerDownHandler,
        IPointerClickHandler,
        IPointerUpHandler,
        IPointerExitHandler
    {

        public RectTransform tweenTarget;
        public Vector3 enter = new Vector3(1.1f, 1.1f, 1.1f);
        public Vector3 down = new Vector3(1.05f, 1.05f, 1.05f);
        public float duration = 1f;
        protected TweenScale m_tween;
        protected Vector3 mScale;

        // Use this for initialization
        void Start()
        {
            if (tweenTarget == null)
                tweenTarget = GetComponent<RectTransform>();
            mScale = tweenTarget.localScale;
            m_tween = new TweenScale(tweenTarget, tweenTarget.localScale, Vector3.one, duration);
        }

        void Update()
        {
            m_tween.Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Scale(enter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Scale(mScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Scale(down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Scale(mScale);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        void Scale(Vector3 to)
        {
            m_tween.to = to;
            m_tween.PlayForward();
        }
    }
}
