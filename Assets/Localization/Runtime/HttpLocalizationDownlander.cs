//*************************************************************************************
//* 作    者： zht
//* 创建时间： 2022-08-20 13:48:07
//* 描    述： 多语言包下载器

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


namespace UFrame.Localization
{
    public delegate Dictionary<string, string> JsonDecodeFunc(string json);
    public delegate void DownloadFunc(string url,System.Action<string> onload);
    public delegate Dictionary<string, string> LanguageVersionCallback(string versionMap);
    public delegate void LanguageDownloadCallback(string language, string languageData);

    internal class HttpLocalizationDownlander
    {
        public DownloadFunc donwlandFunc { get; set; }//下载器
        private event LanguageDownloadCallback onLanguageLoad;
        private event LanguageVersionCallback versionCallback;
        private string m_languageRootUrl;
        private Dictionary<string, string> m_versionMap;

        public void SetLanguageRoot(string languageRootUrl)
        {
            m_languageRootUrl = languageRootUrl;
        }

        public void StartLoadVersion(string versionUrl,LanguageVersionCallback callback)
        {
            versionCallback = callback;
            if (donwlandFunc != null)
            {
               donwlandFunc?.Invoke(versionUrl,(versionJson) =>{
                   m_versionMap = null;
                   if (string.IsNullOrEmpty(versionJson))
                   {
                       callback?.Invoke(null);
                       return;
                   }
                   m_versionMap = callback?.Invoke(versionJson);
               });
            }
            else
            {
                Debug.LogError("downlandFunc not reg!");
                callback?.Invoke(null);
            }
        }

        public void DownloadLanguage(string language, LanguageDownloadCallback callback)
        {
            onLanguageLoad = callback;
            if(m_versionMap != null && m_versionMap.TryGetValue(language, out var versionId))
            {
                if(!TryLoadLocalLanguage(language,versionId,out var languageData) || string.IsNullOrEmpty(languageData))
                {
                    var url = $"{m_languageRootUrl}/{language}_{versionId}.bundle";
                    donwlandFunc?.Invoke(url, (languageData) =>
                    {
                        callback?.Invoke(language, languageData);
                        CacheLanguage(language, versionId, languageData);
                    });
                }
                else
                {
                    callback?.Invoke(language, languageData);
                }
            }
            else
            {
                Debug.LogError("failed find version of:" + language);
                callback?.Invoke(language, null);
            }
        }

        private bool TryLoadLocalLanguage(string language,string versionId,out string data)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                data = null;
                return false;
            }
            var cacheFolder = Application.persistentDataPath + "/Language";
            var filePath = $"{cacheFolder}/{language}_{versionId}.data"; 
            if(System.IO.File.Exists(filePath))
            {
                data = System.IO.File.ReadAllText(filePath);
                return true;
            }
            data = null;
            return false;
        }

        private void CacheLanguage(string language, string versionId, string data)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return;

            var cacheFolder = Application.persistentDataPath + "/Language";
            var filePath = $"{cacheFolder}/{language}_{versionId}.data";
            if (!System.IO.Directory.Exists(cacheFolder))
            {
                System.IO.Directory.CreateDirectory(cacheFolder);
            }
            System.IO.File.WriteAllText(filePath, data);
        }
    }
}