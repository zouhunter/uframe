/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 字符串用户数据                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Setting.Prefer
{
    public class PlayerPrefsString : PreferValue<string>
    {
        public PlayerPrefsString(string key) : base(key) { }

        protected override string GetPreferValue()
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }
            return "";
        }

        protected override void SetPreferValue(string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        protected override bool Equals(string a, string b)
        {
            return a == b;
        }
    }

}