//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-04 05:08:35
//* 描    述： 

//* ************************************************************************************
using UnityEngine;
using UnityEngine.UI;

namespace UFrame.Localization
{
    public class LocalizationText : Text
    {
        [SerializeField]
        protected string m_key;
        [SerializeField]
        protected string m_format;

        protected override void OnEnable()
        {
            base.OnEnable();
            LocalizationAgent.Instance.onLanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            LocalizationAgent.Instance.onLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            var textValue = LocalizationAgent.Instance.GetText(m_key);
            SetText(textValue);
        }

        private void SetText(string textValue)
        {
            if (string.IsNullOrEmpty(m_format))
            {
                text = textValue;
            }
            else
            {
                text = m_format.Replace("{0}", textValue);
            }
        }
    }
}