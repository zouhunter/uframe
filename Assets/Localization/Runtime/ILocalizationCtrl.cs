/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 文本控制器                                                                      *
*//************************************************************************************/

namespace UFrame.Texts
{
    public interface ILocalizationCtrl
    {
        void SwitchLanguage(int language,bool asDefault=false);
        void SetText(int id, string text, int language);
        void MakeKeyIndex(string key, int id);
        string GetText(int id, int language);
        string GetText(int id);
        string GetTextByKey(string key);
        string GetTextByDefault(string defaultText);
    }
}