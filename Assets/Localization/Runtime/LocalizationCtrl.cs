/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 文本控制器                                                                      *
*//************************************************************************************/

using System.Collections.Generic;

namespace UFrame.Texts
{
    public class LocalizationCtrl : ILocalizationCtrl
    {
        internal int m_currentLanguage;
        internal int m_defaultLanguage;
        internal Dictionary<int, Dictionary<int, string>> m_languageMap = new Dictionary<int, Dictionary<int, string>>();
        internal Dictionary<string, int> m_defaultIndexMap = new Dictionary<string, int>();
        internal Dictionary<string, int> m_keyIndexMap = new Dictionary<string, int>();
        internal Dictionary<int, string> m_currentMap = null;

        public event System.Action onLanguageChanged;
        public int Language { get { return m_currentLanguage; } }

        public virtual string GetText(int id, int language)
        {
            string text = null;
            if (language == m_currentLanguage)
            {
                if (m_currentMap == null)
                {
                    m_languageMap.TryGetValue(language, out m_currentMap);
                }
                if (m_currentMap != null)
                {
                    m_currentMap.TryGetValue(id, out text);
                }
            }
            else
            {
                if (m_languageMap.TryGetValue(language, out var map) && map != null)
                {
                    map.TryGetValue(id, out text);
                }
            }
            return text;
        }
        
        public virtual void MakeKeyIndex(string text, int id)
        {
            m_keyIndexMap[text] = id;
        }

        public virtual void SetText(int id, string text, int language)
        {
            if (!m_languageMap.TryGetValue(language, out var map) || map == null)
            {
                map = m_languageMap[language] = new Dictionary<int, string>();
            }
            map[id] = text;
        }

        public virtual bool ExistLanguage(int language)
        {
            return m_languageMap.ContainsKey(language);
        }

        public virtual void SwitchLanguage(int language, bool markDefault = false)
        {
            if (m_currentLanguage == language && m_currentMap != null)
                return;

            m_currentLanguage = language;
            if(!m_languageMap.TryGetValue(language, out m_currentMap) || m_currentMap == null)
            {
                UnityEngine.Debug.LogError("language not exists:" + language);
                return;
            }

            if (markDefault && m_currentMap != null)
            {
                m_defaultLanguage = language;
                BuildDefaultIndexMap(m_currentMap);
            }
            try
            {
                onLanguageChanged?.Invoke();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public virtual string GetText(int key)
        {
            if (m_currentMap != null && m_currentMap.TryGetValue(key, out var text))
            {
                return text;
            }
            return key.ToString();
        }

        public virtual string GetTextByKey(string key)
        {
            if(m_keyIndexMap.TryGetValue(key,out var id))
            {
                return GetText(id);
            }
            return key;
        }
        public virtual string GetTextByKey(string key,int language)
        {
            if (m_keyIndexMap.TryGetValue(key, out var id))
            {
                return GetText(id, language);
            }
            return key;
        }
        public virtual string GetTextByDefault(string defaultText)
        {
            if (m_currentLanguage == m_defaultLanguage)
            {
                return defaultText;
            }
            if (m_defaultIndexMap.TryGetValue(defaultText, out var key))
            {
                return GetText(key);
            }
            return defaultText;
        }
        protected void BuildDefaultIndexMap(Dictionary<int,string> textMap)
        {
            m_defaultIndexMap.Clear();
            foreach (var item in textMap)
            {
                m_defaultIndexMap[item.Value] = item.Key;
            }
        }
    }
}
