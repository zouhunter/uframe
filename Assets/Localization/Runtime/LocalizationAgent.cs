//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2021-09-23 08:11:33
//* 描    述： 本地化

//* ************************************************************************************
using UFrame.Localization;
using LanguageId = System.String;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LocalizationAgent : LocalizationCtrl<LanguageId>
{
    private static LocalizationAgent _instance;
    public static LocalizationAgent Instance
    {
        get
        {
            if (_instance == null)
                _instance = new LocalizationAgent();
            return _instance;
        }
    }
    private HttpLocalizationDownlander httpLocaliztionDownlander;
    private bool m_httpAlive;
    private string m_versionJson;
    private Dictionary<LanguageId, string> m_httpDownloadMap;
    private JsonDecodeFunc m_decodeFunc;
    private bool m_inited;
    public void Initialize(string languageRootUrl, DownloadFunc downloadFunc, JsonDecodeFunc decodeFunc)
    {
        httpLocaliztionDownlander = new HttpLocalizationDownlander();
        httpLocaliztionDownlander.donwlandFunc = downloadFunc;
        httpLocaliztionDownlander.SetLanguageRoot(languageRootUrl);
        m_httpDownloadMap = new Dictionary<LanguageId, string>();
        m_decodeFunc = decodeFunc;
        m_httpAlive = false;
        m_inited = true;
    }

    public void LoadVersionMenu(string versionUrl, LanguageVersionCallback callback)
    {
        if (!m_inited)
        {
            Debug.LogError("localization not initialized !");
            callback?.Invoke(null);
            return;
        }
        if (m_versionJson != null)
        {
            callback?.Invoke(m_versionJson);
            return;
        }
        httpLocaliztionDownlander.StartLoadVersion(versionUrl, (vjson) =>
        {
            m_versionJson = vjson;
            m_httpAlive = true;
            return callback?.Invoke(vjson);
        });
    }

    public void SwitchLanguageByDownloader(LanguageId language, LanguageDownloadCallback callback)
    {
        if (m_httpDownloadMap.TryGetValue(language, out var languageData) && ExistLanguage(language))
        {
            SwitchLanguage(language);
            callback?.Invoke(language, languageData);
        }
        else if (m_httpAlive)
        {
            httpLocaliztionDownlander.DownloadLanguage(language, (language, languageData) =>
            {
                if (!string.IsNullOrEmpty(languageData))
                {
                    SwitchLanguage(language, false);
                    var languageMap = m_decodeFunc?.Invoke(languageData);
                    if (languageMap != null)
                    {
                        foreach (var pair in languageMap)
                        {
                            SetText(pair.Key, pair.Value);
                        }
                    }
                    m_httpDownloadMap[language] = languageData;
                }
                SwitchLanguage(language);
                callback?.Invoke(language, languageData);
            });
        }
        else
        {
            Debug.LogError($"Failed Switch Language to ; {language}");
        }
    }

    //@{0}
    public static string GetString(string key, params object[] args)
    {
        var textValue = Instance.GetText(key);
        if (args != null && args.Length > 0)
        {
            textValue = Regex.Replace(textValue, "@{", "{");
            try
            {
                textValue = string.Format(textValue, args);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        textValue = textValue.Replace(@"\n", "\n");
        textValue = textValue.Replace(@"\t", "\t");
        return textValue;
    }

}
