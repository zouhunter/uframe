/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   字符串缓存。                                                                      *
*//************************************************************************************/

using UnityEditor;

namespace UFrame.Setting.Prefer
{
    public class EditorPrefsString : PreferValue<string>
    {
        public EditorPrefsString(string key) : base(key) { }

        protected override string GetPreferValue()
        {
            if (EditorPrefs.HasKey(key))
            {
                return EditorPrefs.GetString(key);
            }
            return "";
        }

        protected override void SetPreferValue(string value)
        {
            EditorPrefs.SetString(key, value);
        }
        protected override bool Equals(string a, string b)
        {
            return a == b;
        }
    }

}