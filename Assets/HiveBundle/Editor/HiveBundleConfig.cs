using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UFrame.HiveBundle
{
    public class HiveBundleConfig
    {
        private static HiveBundleConfig instance = new HiveBundleConfig();
        private Editor m_defaultEditor;

        [SettingsProvider]
        public static SettingsProvider VersionSetting()
        {
            var provider = new SettingsProvider($"Project/{typeof(AssetBundleSetting).Namespace.Split(".")[0]}/HiveBundle", SettingsScope.Project);
            provider.label = "HiveBundle";
            provider.guiHandler = instance.OnGUILayout;
            provider.deactivateHandler = instance.OnDeactive;
            provider.keywords = new string[] { "bundle", "assetbundle" };
            return provider;
        }


        private void OnDeactive()
        {
            EditorUtility.SetDirty(AssetBundleSetting.Instance);
            AssetBundleSetting.Save();
        }

        private void OnGUILayout(string obj)
        {
            GUILayout.Space(10);
            Editor.DrawFoldoutInspector(AssetBundleSetting.Instance, ref m_defaultEditor);
            GUILayout.FlexibleSpace();
        }
    }
}
