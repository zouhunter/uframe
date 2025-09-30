/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   整型缓存。                                                                        *
*//************************************************************************************/

using UnityEditor;

namespace UFrame.Setting.Prefer
{
    public class EditorPrefsInt : PreferValue<int>
    {
        public EditorPrefsInt(string key) : base(key) { }

        protected override int GetPreferValue()
        {
            if (EditorPrefs.HasKey(key))
            {
                return EditorPrefs.GetInt(key);
            }
            return 0;
        }

        protected override void SetPreferValue(int value)
        {
            EditorPrefs.SetInt(key, value);
        }
        protected override bool Equals(int a, int b)
        {
            return a == b;
        }
    }

}