//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-04 05:08:35
//* 描    述： 

//* ************************************************************************************
using UnityEngine;
using UnityEngine.UI;

namespace UFrame.Localization
{
    public class LocalizationTextBinding : MonoBehaviour
    {
        [SerializeField]
        protected string m_key;
        [SerializeField]
        protected string m_format;
        private Text m_text;

        private void OnEnable()
        {
            LocalizationAgent.Instance.onLanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            LocalizationAgent.Instance.onLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            if (!m_text)
                m_text = GetComponent<Text>();
            if (m_text)
            {
                var textValue = LocalizationAgent.Instance.GetText(m_key);
                SetText(textValue);
            }
        }

        private void SetText(string textValue)
        {
            if (string.IsNullOrEmpty(m_format))
            {
                m_text.text = textValue;
            }
            else
            {
                m_text.text = m_format.Replace("{0}", textValue);
            }
        }

        public static void Binding(Text target, string keyCode, string format = null)
        {
            if (!target)
                return;
            var localizationText = target.gameObject.GetComponent<LocalizationTextBinding>();
            if (!localizationText)
                localizationText = target.gameObject.AddComponent<LocalizationTextBinding>();
            localizationText.m_key = keyCode;
            localizationText.m_format = format;
        }
    }
}