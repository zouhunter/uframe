/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 文本控制器                                                                      *
*//************************************************************************************/
using System.Collections.Generic;

namespace UFrame.Localization
{
    public class LocalizationCtrl<LanguageId> : ILocalizationCtrl<LanguageId>
    {
        internal LanguageId m_currentLanguage;
        internal Dictionary<LanguageId, Dictionary<string, string>> m_textMap = new Dictionary<LanguageId, Dictionary<string, string>>();
        internal Dictionary<string, string> m_currentMap = null;
        internal Dictionary<string, string> m_linkMap = null;
        public event System.Action onLanguageChanged;
        public LanguageId Language { get { return m_currentLanguage; } }

        public virtual string GetText(string key, LanguageId language)
        {
            if (m_linkMap != null && m_linkMap.TryGetValue(key, out var targetKey))
                key = targetKey;

            string text = null;
            if (language.Equals(m_currentLanguage))
            {
                if (m_currentMap == null)
                {
                    m_textMap.TryGetValue(language, out m_currentMap);
                }
                if (m_currentMap != null)
                {
                    m_currentMap.TryGetValue(key, out text);
                }
            }
            else
            {
                if (m_textMap.TryGetValue(language, out var map) && map != null)
                {
                    map.TryGetValue(key, out text);
                }
            }
            return text;
        }

        public virtual void SetText(string key, string text, LanguageId language)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key))
                return;

            if (!m_textMap.TryGetValue(language, out var map) || map == null)
            {
                map = m_textMap[language] = new Dictionary<string, string>();
            }
            map[key] = text;
        }

        public virtual void SetText(string key, string text)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key))
                return;
            if (m_currentMap == null)
                return;
            m_currentMap[key] = text;
        }

        public virtual bool ExistLanguage(LanguageId language)
        {
            return m_textMap.ContainsKey(language);
        }

        public virtual void SwitchLanguage(LanguageId language,bool notice = true)
        {
            if (language.Equals(m_currentLanguage) && m_currentMap != null)
                return;

            m_currentLanguage = language;
            if (!m_textMap.TryGetValue(language, out m_currentMap) || m_currentMap == null)
            {
                m_currentMap = m_textMap[m_currentLanguage] = new Dictionary<string, string>();
            }

            if(notice)
            {
                try
                {
                    onLanguageChanged?.Invoke();
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
           
        }

        public virtual string GetText(string key)
        {
            if (m_linkMap != null && m_linkMap.TryGetValue(key, out var targetKey))
                key = targetKey;

            if (m_currentMap != null && m_currentMap.TryGetValue(key, out var text))
            {
                return text;
            }
            return key.ToString();
        }

        public void LinkKey(string sourceKey, string targetKey)
        {
            if (m_linkMap == null)
                m_linkMap = new Dictionary<string, string>();
            m_linkMap[sourceKey] = targetKey;
        }
    }
}
