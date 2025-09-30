//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-04 05:08:35
//* 描    述： 

//* ************************************************************************************
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UFrame.Localization
{
    public class LocalizationBinding : MonoBehaviour
    {
        [SerializeField]
        protected UnityEvent<string> m_onLoadText;
        [SerializeField]
        protected string m_key;
        [SerializeField]
        protected string m_format;

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
            if (m_onLoadText == null)
                return;

            var textValue = LocalizationAgent.Instance.GetText(m_key);
            SetText(textValue);
        }

        private void SetText(string textValue)
        {
            if (string.IsNullOrEmpty(m_format))
            {
                m_onLoadText?.Invoke(textValue);
            }
            else
            {
                m_onLoadText?.Invoke(m_format.Replace("{0}", textValue));
            }
        }

        public static void Binding(Text target, string keyCode, string format = null)
        {
            if (!target)
                return;
            var localizationText = target.gameObject.GetComponent<LocalizationBinding>();
            if (!localizationText)
                localizationText = target.gameObject.AddComponent<LocalizationBinding>();
            localizationText.m_key = keyCode;
            localizationText.m_format = format;
            if (localizationText.m_onLoadText == null)
                localizationText.m_onLoadText = new UnityEvent<string>();
            localizationText.m_onLoadText.AddListener((x)=> {
                if(target) target.text = x;
            });
        }
    }
}