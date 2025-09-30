/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 文本控制器                                                                      *
*//************************************************************************************/

namespace UFrame.Localization
{
    public interface ILocalizationCtrl<LanguageId>
    {
        void SwitchLanguage(LanguageId language,bool notice = false);
        void SetText(string key, string text, LanguageId language);
        string GetText(string key, LanguageId language);
        string GetText(string key);
    }
}