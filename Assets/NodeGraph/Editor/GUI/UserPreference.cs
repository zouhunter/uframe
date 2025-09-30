/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 设置窗口                                                                        *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UFrame.NodeGraph
{
    public static class UserPreference
    {
        static readonly string kKEY_USERPREF_GRID = "NodeGraph.UserPref.GridSize";

        private static bool s_prefsLoaded = false;

        private static float s_editorWindowGridSize;

        public static float EditorWindowGridSize
        {
            get
            {
                LoadAllPreferenceValues();
                return s_editorWindowGridSize;
            }
            set
            {
                s_editorWindowGridSize = value;
                SaveAllPreferenceValues();
            }
        }

        private static void LoadAllPreferenceValues()
        {
            if (!s_prefsLoaded)
            {
                s_editorWindowGridSize = EditorPrefs.GetFloat(kKEY_USERPREF_GRID, 12f);

                s_prefsLoaded = true;
            }
        }

        private static void SaveAllPreferenceValues()
        {
            EditorPrefs.SetFloat(kKEY_USERPREF_GRID, s_editorWindowGridSize);
        }

        [SettingsProvider]
        public static SettingsProvider CreateProjectMenu()
        {
            var provider = new SettingsProvider("Project/UFrame/Node Graph", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Node Graph",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = DrawConfig,

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Node", "Graph" })
            };

            return provider;
        }

        public static void DrawConfig(string searchText)
        {
            LoadAllPreferenceValues();

            s_editorWindowGridSize = EditorGUILayout.FloatField("背景格子尺寸", s_editorWindowGridSize);

            if (GUI.changed)
            {
                SaveAllPreferenceValues();
            }
        }
    }
}