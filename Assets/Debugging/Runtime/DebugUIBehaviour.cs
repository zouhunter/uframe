using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UFrame
{
    public class DebugUIBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected Text m_showText;
        [SerializeField]
        protected Button m_clearBtn;
        [SerializeField]
        protected int m_maxLine = 100;
        protected List<string> m_showList = new List<string>();

        private void Awake()
        {
            if (!m_showText)
                m_showText = transform.GetComponent<Text>();
            m_clearBtn.onClick.AddListener(OnClearClick);
            RefreshShow();
        }

        private void OnClearClick()
        {
            m_showList.Clear();
            RefreshShow();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += OnLogCallBack;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogCallBack;
        }

        private void OnLogCallBack(string condition, string stackTrace, LogType type)
        {
            if (m_showList.Count > m_maxLine)
                m_showList.RemoveAt(0);
            m_showList.Add(condition);
            RefreshShow();
        }

        private void RefreshShow()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_showList.Count; i++)
            {
                sb.Append(m_showList[i]);
                sb.Append("\n");
            }
            m_showText.text = sb.ToString();
        }
    }
}