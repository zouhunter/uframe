//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:18:46
//* 描    述： 

//* ************************************************************************************
using UnityEngine;

namespace UFrame.LitUI
{
    public abstract class UIView : MonoBehaviour
    {
        public UIBinding binding;
        public event UICloseEvent onClose;
        protected bool m_closed;
        public virtual bool Alive => !m_closed;
        public bool Showing => Alive && isActiveAndEnabled;
        protected virtual void Awake() { m_closed = false; }
        protected virtual void OnDestroy() { if (!m_closed) OnClose(); }
        public virtual void OnOpen(object arg = null) { }
        public virtual void SetActive(bool active) {
            if(Alive)
                gameObject.SetActive(active);
        }
        public T GetRef<T>(string name) where T:Object
        {
            return binding?.GetRef<T>(name);
        }
        public virtual void Close() {
            if (!m_closed)
            {
                m_closed = true;
                SetActive(false);
                OnClose();
                Destroy(gameObject);
            }
        }
        protected virtual void OnClose() {
            onClose?.Invoke(this);
            m_closed = true;
        }
    }
}
