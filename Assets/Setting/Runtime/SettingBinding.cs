/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 设置绑定行为                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Setting
{
    public abstract class SettingBining : MonoBehaviour
    {
        protected abstract int PreferID { get; }
        protected abstract ISettingCtrl SettingCtrl { get; }
    }
    public abstract class IntSettingBinding : SettingBining
    {
        protected virtual void Start()
        {
            if (SettingCtrl != null)
            {
                OnSettingValueChanged(SettingCtrl.GetInt(PreferID));

                SettingCtrl.RegistIntChanged(PreferID, OnSettingValueChanged);
            }
        }

        protected abstract void OnSettingValueChanged(int intValue);
    }
    public abstract class StringSettingBinding : SettingBining
    {
        protected virtual void Start()
        {
            if (SettingCtrl != null)
            {
                OnSettingValueChanged(SettingCtrl.GetString(PreferID));

                SettingCtrl.RegistStringChanged(PreferID, OnSettingValueChanged);
            }
        }

        protected abstract void OnSettingValueChanged(string intValue);
    }
    public abstract class FloatSettingBinding : SettingBining
    {
        protected virtual void Start()
        {
            if (SettingCtrl != null)
            {
                OnSettingValueChanged(SettingCtrl.GetFloat(PreferID));
                SettingCtrl.RegistFloatChanged(PreferID, OnSettingValueChanged);
            }
        }
        protected abstract void OnSettingValueChanged(float floatValue);
    }
}
