/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 用户数据 设置器                                                                 *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Setting
{
    public class PreferSettingCtrl : BaseSettingCtrl
    {
        private string m_perfer_key_ = "prefer_setting_";
        private string m_perfer_ints = "perfer_ints";
        private string m_perfer_floats = "perfer_floats";
        private string m_perfer_strs = "perfer_strs";
        private HashSet<int> m_perferInts = new HashSet<int>();
        private HashSet<int> m_perferFloats = new HashSet<int>();
        private HashSet<int> m_perferStrs = new HashSet<int>();

        public void Initialize(string preferKey = null)
        {
            if (!string.IsNullOrEmpty(preferKey))
            {
                m_perfer_key_ = preferKey;
                m_perfer_ints = preferKey + "ints";
                m_perfer_floats = preferKey + "floats";
                m_perfer_strs = preferKey + "strs";

                ParseKeys(m_perfer_ints, m_perferInts);
                ParseKeys(m_perfer_floats, m_perferFloats);
                ParseKeys(m_perfer_strs, m_perferStrs);
            }
        }

        private void ParseKeys(string key,HashSet<int> keys)
        {
            var keyStr = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(keyStr))
            {
                var strKeys = keyStr.Split(':');
                foreach (var strId in strKeys)
                {
                    if (string.IsNullOrEmpty(strId))
                        continue;
                    if (int.TryParse(strId, out var id))
                        keys.Add(id);
                }
            }
        }

        protected override bool SetFloatOnly(int id, float value)
        {
            var key = m_perfer_key_ + id;
            UnityEngine.PlayerPrefs.SetFloat(key, value);
            m_perferFloats.Add(id);
            return true;
        }

        protected override bool SetIntOnly(int id, int value)
        {
            var key = m_perfer_key_ + id;
            UnityEngine.PlayerPrefs.SetInt(key, value);
            m_perferInts.Add(id);
            return true;
        }

        protected override bool SetStringOnly(int id, string value)
        {
            var key = m_perfer_key_ + id;
            UnityEngine.PlayerPrefs.SetString(key, value);
            m_perferStrs.Add(id);
            return true;
        }

        protected override void DoSave()
        {
            SavePerfers(m_perfer_ints, m_perferInts);
            SavePerfers(m_perfer_strs, m_perferStrs);
            SavePerfers(m_perfer_floats, m_perferFloats);
            PlayerPrefs.Save();
            onSave?.Invoke();
        }

        private void SavePerfers(string key,HashSet<int> list)
        {
            var perferKeyStr = new System.Text.StringBuilder();
            foreach (var keyId in list)
            {
                perferKeyStr.Append(":");
                perferKeyStr.Append(keyId);
            }
            PlayerPrefs.SetString(key, perferKeyStr.ToString());
        }

        public override bool TryGetInt(int settingID, out int value)
        {
            var key = m_perfer_key_ + settingID;
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetInt(key);
                return true;
            }
            value = 0;
            return false;
        }

        public override bool TryGetFloat(int settingID, out float value)
        {
            var key = m_perfer_key_ + settingID;
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetFloat(key);
                return true;
            }
            value = 0;
            return false;
        }

        public override bool TryGetString(int settingID, out string value)
        {
            var key = m_perfer_key_ + settingID;
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetString(key);
                return true;
            }
            value = null;
            return false;
        }

        public override void ClearAll()
        {
            base.ClearAll();
            RemovePerfer(m_perfer_ints, m_perferInts);
            RemovePerfer(m_perfer_floats, m_perferFloats);
            RemovePerfer(m_perfer_strs, m_perferStrs);
        }

        private void RemovePerfer(string key,HashSet<int> list)
        {
            foreach (var id in list)
            {
                UnityEngine.PlayerPrefs.DeleteKey(m_perfer_key_ + id);
            }
            UnityEngine.PlayerPrefs.DeleteKey(key);
            list.Clear();
        }

        public override string SerializeToJson()
        {
            SettingSerialize serialize = new SettingSerialize();
            foreach (var key in m_perferInts)
            {
                var value = PlayerPrefs.GetInt(m_perfer_key_ + key);
                serialize.i.Add(new SettingSerialize.SettingNode(key, value.ToString()));
            }
            foreach (var key in m_perferFloats)
            {
                var value = PlayerPrefs.GetFloat(m_perfer_key_ + key);
                serialize.f.Add(new SettingSerialize.SettingNode(key, value.ToString("0.00")));
            }
            foreach (var key in m_perferStrs)
            {
                var value = PlayerPrefs.GetString(m_perfer_key_ + key);
                serialize.s.Add(new SettingSerialize.SettingNode(key, value));
            }
            return JsonUtility.ToJson(serialize);
        }

        public override void ParseFromJson(string json)
        {
            var jsonSerialize = JsonUtility.FromJson<SettingSerialize>(json);
            if (jsonSerialize != null)
            {
                foreach (var pair in jsonSerialize.i)
                {
                    if (int.TryParse(pair.v, out var v))
                        PlayerPrefs.SetInt(m_perfer_key_ + pair.k, v);
                }
                foreach (var pair in jsonSerialize.f)
                {
                    if (float.TryParse(pair.v, out var v))
                        PlayerPrefs.SetFloat(m_perfer_key_ + pair.k, v);
                }
                foreach (var pair in jsonSerialize.s)
                {
                    PlayerPrefs.SetString(m_perfer_key_ + pair.k, pair.v);
                }
            }
        }
    }
}