//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:18:46
//* 描    述： 

//* ************************************************************************************
using UnityEngine;

namespace UFrame.LitUI
{
    public class UIView : MonoBehaviour
    {
        public UIBinding binding;
        public event UICloseEvent onClose;
        public event UIActiveEvent onActive;
        protected bool m_closed;
        protected UIView m_parentView;
        public virtual bool Alive => !m_closed;
        public bool Showing => Alive && isActiveAndEnabled;
        protected virtual void Awake() {
            m_closed = false;
            var baseView = transform.parent?.GetComponentInParent<UIView>();
            if (baseView)
                SetParent(baseView);
        }
        protected virtual void OnDestroy() { if (!m_closed) OnClose(); }
        public virtual void OnOpen(object arg = null) { }
        public virtual void SetParent(UIView parentView)
        {
            this.m_parentView = parentView;
        }
        public virtual void SetActive(bool active) {
            if (Alive && gameObject)
                gameObject.SetActive(active);
            onActive?.Invoke(this, active);
        }
        public virtual T GetRef<T>(string name) where T:Object
        {
            var refItem = binding?.GetRef<T>(name);
            if (refItem == null && m_parentView)
                refItem = m_parentView.GetRef<T>(name);
            return refItem;
        }
        public virtual void Close() {
            if (m_parentView)
            {
                m_parentView.Close();
            }
            else
            {
                if (!m_closed)
                {
                    m_closed = true;
                    SetActive(false);
                    OnClose();
                    Destroy(gameObject);
                }
            }
        }
        protected virtual void OnClose() {
            m_closed = true;
            onClose?.Invoke(this);
        }
    }
}
