/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 符点用户数据                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Setting.Prefer
{
    public class PlayerPrefsFloat : PreferValue<float>
    {
        public PlayerPrefsFloat(string key) : base(key) { }

        protected override float GetPreferValue()
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetFloat(key);
            }
            return 0;
        }

        protected override void SetPreferValue(float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
        protected override bool Equals(float a, float b)
        {
            return Mathf.Abs(a - b) < 0.01f;
        }
    }

}