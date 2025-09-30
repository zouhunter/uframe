/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-01                                                                   *
*  版本: master_d8e549                                                                *
*  功能:                                                                              *
*   - FPS显示小助手                                                                   *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.Debugging
{
    [AddComponentMenu("UFrame/Debug/ShowFpsBehaviour")]
    public class ShowFpsBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected float m_updateInterval = 0.5F;

        [SerializeField]
        protected UnityEvent<string> m_onFpsChanged;

        [SerializeField]
        protected string m_fpsFormat = "FPS:{0}";

        [SerializeField]
        protected bool m_showOnGui;

        [SerializeField]
        protected Rect m_onGuiRect = new Rect(100, 0, 300, 200);

        [SerializeField]
        protected int m_fontSize = 30;

        protected float m_lastInterval;
        protected int m_frames = 0;
        protected float m_fps;

        protected virtual void Start()
        {
            m_lastInterval = Time.realtimeSinceStartup;
            m_frames = 0;
        }

        protected virtual void OnGUI()
        {
            if (m_showOnGui)
            {
                GUI.skin.label.fontSize = m_fontSize;
                GUI.Label(m_onGuiRect, string.Format(m_fpsFormat, m_fps.ToString("f2")));
            }
        }

        protected virtual void Update()
        {
            ++m_frames;

            if (Time.realtimeSinceStartup > m_lastInterval + m_updateInterval)
            {
                m_fps = m_frames / (Time.realtimeSinceStartup - m_lastInterval);

                m_frames = 0;

                m_lastInterval = Time.realtimeSinceStartup;

                m_onFpsChanged.Invoke(string.Format(m_fpsFormat, m_fps.ToString("f2")));
            }
        }
    }
}