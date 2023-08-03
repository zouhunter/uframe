using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame
{
    public class WidgetTool
    {
        public static string prefabPath = "Assets/ArtRes/Widgets";
        public static string scriptPath = "Assets/Editor/UIWidgets.cs";

        public static void InstanceWidget(string widgetName)
        {
            GameObject target = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/ArtRes/Widgets/{widgetName}.prefab");
            if (!target)
            {
                Debug.LogError("failed create:" + widgetName);
                return;
            }
            var instance = GameObject.Instantiate((GameObject)target);
            instance.name = target.name;
            if (Selection.activeTransform)
            {
                instance.transform.SetParent(Selection.activeTransform, false);
            }
        }
        [MenuItem("GameObject/UFrame/RebuildWidgetMenu", priority = -100)]
        private static void ReBuildMenuScript()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("#region GENERATE_MENUS");
            var prefabPaths = System.IO.Directory.GetFiles(prefabPath, "*.prefab", System.IO.SearchOption.AllDirectories);
            foreach (var item in prefabPaths)
            {
                var reletivePath = System.IO.Path.GetRelativePath(prefabPath, item).Replace('\\', '/').Replace(".prefab", "");
                var menuName = $"GameObject/UFrame/{reletivePath}";
                var funcName = $"Create{reletivePath.Replace("\\", "/").Replace("/", "_")}";
                sb.AppendLine($"\t\t[MenuItem(\"{menuName}\")]");
                sb.AppendLine($"\t\tpublic static void {funcName}()");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\tJagatEditorTools.InstanceWidget(\"{reletivePath}\");");
                sb.AppendLine("\t\t}");
            }
            sb.Append("\t\t#endregion GENERATE_MENUS");
            InsertCodeToScript(scriptPath, sb.ToString());
        }

        private static void InsertCodeToScript(string scriptPath, string code)
        {
            Debug.Log(code);
            var fileText = System.IO.File.ReadAllText(scriptPath);
            var key1 = "#region GENERATE_MENUS";
            var key2 = "#endregion GENERATE_MENUS";
            var startIndex = fileText.IndexOf(key1);
            var endIndex = fileText.IndexOf(key2);
            if (startIndex > 0 && endIndex > 0)
            {
                fileText = fileText.Substring(0, startIndex) + code + fileText.Substring(endIndex + key2.Length);
                System.IO.File.WriteAllText(scriptPath, fileText);
            }
            AssetDatabase.Refresh();
        }
    }
}