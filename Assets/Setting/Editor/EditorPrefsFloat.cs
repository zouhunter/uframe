/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   符点缓存。                                                                        *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;

namespace UFrame.Setting.Prefer
{
    public class EditorPrefsFloat : PreferValue<float>
    {
        public EditorPrefsFloat(string key) : base(key) { }

        protected override float GetPreferValue()
        {
            if (EditorPrefs.HasKey(key))
            {
                return EditorPrefs.GetFloat(key);
            }
            return 0;
        }

        protected override void SetPreferValue(float value)
        {
            EditorPrefs.SetFloat(key, value);
        }
        protected override bool Equals(float a, float b)
        {
            return Mathf.Abs(a - b) < 0.01f;
        }
    }

}