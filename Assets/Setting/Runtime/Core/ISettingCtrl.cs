using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Setting
{
    public interface ISettingCtrl
    {
        System.Action onSave { get; set; }
        int GetInt(int settingID, int defaultValue = 0);
        float GetFloat(int settingID, float defaultValue = 0);
        string GetString(int settingID, string defaultValue = "");
        void SetInt(int settingID, int value);
        void SetFloat(int settingID, float value);
        void SetString(int settingID, string value);
        bool TryGetInt(int settingID, out int value);
        bool TryGetFloat(int settingID, out float value);
        bool TryGetString(int settingID, out string value);
        void RegistIntChanged(int settingID, System.Action<int> action);
        void RegistFloatChanged(int settingID, System.Action<float> action);
        void RegistStringChanged(int settingID, System.Action<string> action);
        void UnRegistIntChanged(int settingID, System.Action<int> action);
        void UnRegistFloatChanged(int settingID, System.Action<float> action);
        void UnRegistStringChanged(int settingID, System.Action<string> action);
        void ClearAll();
        void FlashChange();
        void ParseFromJson(string json);
        string SerializeToJson();
    }
}