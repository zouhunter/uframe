using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.Editors
{
    public class ProjEditor
    {
        [SettingsProvider]
        public static SettingsProvider CreateProjectMenu()
        {
            var provider = new SettingsProvider("Project/Jagat", SettingsScope.Project)
            {
                label = "Jagat",
                guiHandler = (searchContext) =>
                {
                    GUILayout.Space(10);
                    GUILayout.FlexibleSpace();
                },
                keywords = new HashSet<string>(new[] { "Jagat" })
            };
            return provider;
        }
    }
}