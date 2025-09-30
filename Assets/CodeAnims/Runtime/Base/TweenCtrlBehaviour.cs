using UnityEngine;

namespace UFrame.Tween
{
    public class TweenCtrlBehaviour : MonoBehaviour
    {
        public readonly TweenCtrl tweenCtrl = new TweenCtrl();
        private static TweenCtrlBehaviour m_instance;
        public static TweenCtrlBehaviour Instance
        {
            get
            {
                if(!m_instance)
                {
                    m_instance = FindObjectOfType<TweenCtrlBehaviour>();
                    if(!m_instance)
                    {
                        m_instance = new GameObject("TweenCtrlBehaviour", typeof(TweenCtrlBehaviour)).GetComponent<TweenCtrlBehaviour>();
                    }
                }
                return m_instance;
            }
        }
        public void Awake()
        {
            if (m_instance && m_instance != this)
                Destroy(this);
            else
                m_instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void Update()
        {
            tweenCtrl.Refresh(false);
        }
    }
}