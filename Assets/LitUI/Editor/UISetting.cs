/*-*-* Copyright (c) uframe@zht
 * Author: 
 * Creation Date: 2024-12-23
 * Version: 1.0.0
 * Description: 
 *_*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEditorInternal;
using TMPro;

namespace UFrame.LitUI
{
    public class UISetting : ScriptableObject
    {
        public static string settingPath => $"ProjectSettings/{typeof(UIView).Namespace.Split(".")[0]}Settings.asset";
        private static UISetting m_instance;
        private Editor m_defaultEditor;
        public static UISetting Instance
        {
            get
            {
                if (m_instance)
                {
                    if (m_instance.autoCollects.Count == 0)
                        m_instance.LoadDefaultUISetting();
                    if (m_instance.layers.Count == 0)
                        m_instance.layers.Add("Base");
                    return m_instance;
                }

                if (System.IO.File.Exists(settingPath))
                {
                    var items = InternalEditorUtility.LoadSerializedFileAndForget(settingPath);
                    var tableCfgItem = Array.Find(items, x => x.GetType() == typeof(UISetting));
                    if (tableCfgItem != null)
                    {
                        m_instance = tableCfgItem as UISetting;
                    }
                }
                if (!m_instance)
                {
                    m_instance = ScriptableObject.CreateInstance<UISetting>();
                }
                return m_instance;
            }
        }
        public string defaultNamespace = "UFrame.LitUI";
        public string uiViewBase = "UIView";
        public string vmBinderBase = "VMBinder";
        public string vmBase = "ViewModel";
        public bool useBinderVM = true;
        public string uiViewReginFlag = "AUTO_UI_BINDING";
        public string codePath = "Game/Scripts/UI/View";
        public bool forceMutexFirstLayer;
        public List<AutoCollectInfo> autoCollects = new List<AutoCollectInfo>();
        public List<string> layers = new List<string>();
        public List<Modify> modifys = new List<Modify>();

        [System.Serializable]
        public class AutoCollectInfo
        {
            public string flag;
            public string assembly = "UnityEngine.UI";
            public string type;
            public string defalutProp;
            public string defalutEvent;
        }

        [Serializable]
        public class Modify
        {
            public string name;
            public bool defaultApply;
        }

        public void LoadDefaultUISetting()
        {
            RegistToAutoCollect("Img", typeof(Image), "sprite");
            RegistToAutoCollect("Txt", typeof(Text), "text");
            RegistToAutoCollect("Input", typeof(InputField), "text");
            RegistToAutoCollect("Tran", typeof(Transform), "position");
            RegistToAutoCollect("Raw", typeof(RawImage), "texture");
            RegistToAutoCollect("Obj", typeof(GameObject), "SetActive");
            RegistToAutoCollect("Drop", typeof(Dropdown), "value");
            RegistToAutoCollect("TogG", typeof(ToggleGroup), "allowSwitchOff");
            RegistToAutoCollect("Btn", typeof(Button), "interactable");
            RegistToAutoCollect("Slider", typeof(Slider), "value");
            RegistToAutoCollect("Bar", typeof(Scrollbar), "value");
            RegistToAutoCollect("Tog", typeof(Toggle), "isOn");
            RegistToAutoCollect("Scroll V", typeof(ScrollRect), "");
            RegistToAutoCollect("TxtM", typeof(TextMeshProUGUI), "text");
            RegistToAutoCollect("DropM", typeof(TMP_Dropdown), "value");
            RegistToAutoCollect("InputM", typeof(TMP_InputField), "text");
            OnDeactive();
        }

        private void RegistToAutoCollect(string flag, Type type, string defaultProp)
        {
            var info = autoCollects.Find(x => x.flag == flag);
            if (info == null)
            {
                autoCollects.Add(new AutoCollectInfo()
                {
                    flag = flag,
                    assembly = type.Assembly.GetName().Name,
                    type = type.FullName,
                    defalutProp = defaultProp
                });
            }
        }

        public int GetDefaultModify()
        {
            int modifyValue = 0;
            for (int i = 0; i < modifys.Count; i++)
            {
                var modify = modifys[i];
                if (modify.defaultApply)
                    modifyValue = 1 << i;
            }
            return modifyValue;
        }

        public string FindDefaultProp(Type componentType)
        {
            foreach (var item in autoCollects)
            {
                if (item.type == componentType.FullName)
                {
                    return item.defalutProp;
                }
            }
            return null;
        }


        public string FindDefaultEvent(Type componentType)
        {
            foreach (var item in autoCollects)
            {
                if (item.type == componentType.FullName)
                {
                    return item.defalutEvent;
                }
            }
            return null;
        }


        public Dictionary<string, Type> GetAutoCollectTypes()
        {
            var dict = new Dictionary<string, Type>();
            foreach (var item in autoCollects)
            {
                var flag = item.flag;
                try
                {
                    var type = System.Reflection.Assembly.Load(item.assembly)?.GetType(item.type);
                    if (type != null)
                    {
                        dict[flag] = type;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            return dict;
        }

        [SettingsProvider]
        public static SettingsProvider VersionSetting()
        {
            var provider = new SettingsProvider($"Project/{typeof(UIView).Namespace.Split(".")[0]}/UI Settings", SettingsScope.Project);
            provider.label = "UI Settings";
            provider.guiHandler = Instance.OnGUILayout;
            provider.deactivateHandler = Instance.OnDeactive;
            provider.keywords = new string[] { "ui", "setting" };
            return provider;
        }

        private void OnDeactive()
        {
            EditorUtility.SetDirty(this);
            if (System.IO.File.Exists(settingPath))
            {
                var items = InternalEditorUtility.LoadSerializedFileAndForget(settingPath);
                var tableCfgItemIndex = Array.FindIndex(items, x => x.GetType() == typeof(UISetting));
                if (tableCfgItemIndex >= 0)
                {
                    items[tableCfgItemIndex] = this;
                }
                else
                {
                    var newItems = new List<UnityEngine.Object>(items);
                    newItems.Add(this);
                    items = newItems.ToArray();
                }
                InternalEditorUtility.SaveToSerializedFileAndForget(items, settingPath, true);
            }
            else
            {
                InternalEditorUtility.SaveToSerializedFileAndForget(new UnityEngine.Object[] { this }, settingPath, true);
            }
        }

        private void OnGUILayout(string obj)
        {
            GUILayout.Space(10);
            Editor.DrawFoldoutInspector(this, ref m_defaultEditor);
            GUILayout.FlexibleSpace();
        }
    }
}
