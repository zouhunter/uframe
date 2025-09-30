//UI按钮放缩
//按钮缩放动画组件
using UnityEngine;
using UnityEngine.EventSystems;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/ButtonScaleCurve")]
    public class UIButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        #region PrivateMember
        [SerializeField]
        private Transform mTweenTarget;
        [SerializeField]
        private bool mDisableHover = true;//悬停开关
        [SerializeField]
        private Vector3 mHover = new Vector3(1.0f, 1.0f, 1.0f);//悬停缩放参数
        [SerializeField]
        private Vector3 mPressed = new Vector3(0.95f, 0.95f, 0.95f);//按下缩放参数
        [SerializeField]
        private float mDuration = 0.2f;//动画执行时间
        private Vector3 m_scale;
        private bool m_started = false;

        #endregion

        #region Property
        public bool DisableHover
        {
            get { return mDisableHover; }
            set { mDisableHover = value; }
        }
        public Vector3 Hover
        {
            get { return mHover; }
            set { mHover = value; }
        }
        public Vector3 Pressed
        {
            get { return mPressed; }
            set { mPressed = value; }
        }
        public float Duration
        {
            get { return mDuration; }
            set { mDuration = value; }
        }

        #endregion

        #region PrivateFun
        void Start()
        {
            if (!m_started)
            {
                m_started = true;
                if (mTweenTarget == null)
                    mTweenTarget = this.transform;
                m_scale = mTweenTarget.localScale;
            }
        }

        void OnDisable()
        {
            if (m_started && mTweenTarget != null)
            {
                mTweenTarget.localScale = m_scale;
            }
        }
        public void Click()
        {
            mTweenTarget.localScale = mPressed;
        }
        void EditorDebug(string message)
        {
#if UNITY_EDITOR
            //Debug.Log(message);
#endif
        }

        #endregion

        #region PublicFun

        #region InterfaceClickFun
        /// <summary>
        /// 按下
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (this.enabled)
            {

                EditorDebug("按下");
                if (!m_started)
                    Start();

                mTweenTarget.localScale = mDisableHover ? mPressed : m_scale;
                //iTween.ScaleTo(mTweenTarget.gameObject, mDisableHover ? mPressed : m_scale, mDuration);

            }

        }

        /// <summary>
        /// 抬起
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (this.enabled)
            {
                EditorDebug("抬起");
                mTweenTarget.localScale = mDisableHover ? m_scale : mHover;
                //iTween.ScaleTo(mTweenTarget.gameObject, mDisableHover ? m_scale : mHover, mDuration);
            }

        }

        /// <summary>
        /// 离开
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (this.enabled)
            {
                EditorDebug("离开");
                mTweenTarget.localScale = m_scale;
                //iTween.ScaleTo(mTweenTarget.gameObject, m_scale, mDuration);
            }

        }
        /// <summary>
        /// 悬停
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enabled && !mDisableHover)
            {
                EditorDebug("悬停");
                if (!m_started) Start();
                mTweenTarget.localScale = mHover;
                //iTween.ScaleTo(mTweenTarget.gameObject, mHover, mDuration);
            }
        }
        #endregion

        /// <summary>
        /// 按钮加缩放组件
        /// </summary>
        public static UIButtonScale Binding(GameObject obj, Vector3 hover, Vector3 pressed, bool disableHover = true)
        {
            UIButtonScale btnScale = obj.GetComponent<UIButtonScale>();
            if (btnScale == null)
                btnScale = obj.AddComponent<UIButtonScale>();

            btnScale.Hover = hover;
            btnScale.Pressed = pressed;
            btnScale.DisableHover = disableHover;
            return btnScale;
        }
        #endregion

    }
}
