/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   bool缓存。                                                                        *
*//************************************************************************************/

using UnityEditor;

namespace UFrame.Setting.Prefer
{
    public class EditorPrefsBool : PreferValue<bool>
    {
        public EditorPrefsBool(string key) : base(key) { }

        protected override bool GetPreferValue()
        {
            if (EditorPrefs.HasKey(key))
            {
                return EditorPrefs.GetBool(key);
            }
            return false;
        }

        protected override void SetPreferValue(bool value)
        {
            EditorPrefs.SetBool(key, value);
        }
        protected override bool Equals(bool a, bool b)
        {
            return a == b;
        }
    }

}