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
            var provider = new SettingsProvider("Project/UFrame", SettingsScope.Project)
            {
                label = "UFrame",
                guiHandler = (searchContext) =>
                {
                    GUILayout.Space(10);
                    GUILayout.FlexibleSpace();
                },
                keywords = new HashSet<string>(new[] { "UFrame" })
            };
            return provider;
        }
    }
}