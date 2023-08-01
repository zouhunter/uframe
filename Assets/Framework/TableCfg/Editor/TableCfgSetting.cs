using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using UnityEngine.UIElements;

namespace Jagat.TableCfg
{
    public class TableCfgSetting : ScriptableObject
    {
        public bool genCsvReader;
        public bool genBinReader;
        public bool genBinWriter;
        public bool forceMString;

        [SettingsProvider]
        public static SettingsProvider VersionSetting()
        {
            var provider = new SettingsProvider("Project/Jagat/Table Config", SettingsScope.Project);
            provider.label = "Table Config";
            provider.guiHandler = Instance.OnGUILayout;
            provider.deactivateHandler = Instance.OnDeactive;
            provider.keywords = new string[] { "table", "config" };
            return provider;
        }

        public static string settingPath = "ProjectSettings/JagatSettings.asset";
        private static TableCfgSetting m_instance;
        public static TableCfgSetting Instance
        {
            get
            {
                if (m_instance)
                    return m_instance;

                if(System.IO.File.Exists(settingPath))
                {
                   var items = InternalEditorUtility.LoadSerializedFileAndForget(settingPath);
                    var tableCfgItem = Array.Find(items, x => x.GetType() == typeof(TableCfgSetting));
                    if(tableCfgItem != null)
                    {
                        m_instance = tableCfgItem as TableCfgSetting;
                    }
                }
                if(!m_instance)
                {
                    m_instance = ScriptableObject.CreateInstance<TableCfgSetting>();
                }
                return m_instance;
            }
        }
        private Editor m_defaultEditor;

        private void OnDeactive()
        {
            EditorUtility.SetDirty(this);
            if (System.IO.File.Exists(settingPath))
            {
                var items = InternalEditorUtility.LoadSerializedFileAndForget(settingPath);
                var tableCfgItemIndex = Array.FindIndex(items, x => x.GetType() == typeof(TableCfgSetting));
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