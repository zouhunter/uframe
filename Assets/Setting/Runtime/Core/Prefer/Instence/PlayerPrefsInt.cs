/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 整型用户数据                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Setting.Prefer
{
    public class PlayerPrefsInt : PreferValue<int>
    {
        public PlayerPrefsInt(string key) : base(key) { }

        protected override int GetPreferValue()
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetInt(key);
            }
            return 0;
        }

        protected override void SetPreferValue(int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        protected override bool Equals(int a, int b)
        {
            return a == b;
        }
    }

}