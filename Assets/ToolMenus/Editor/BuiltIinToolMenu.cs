using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.Editors
{
    public class BuiltInToolMenu
    {
        protected static string GetBuiltInPath(string rePath)
        {
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var folder = System.IO.Path.GetDirectoryName(exePath);
            return System.IO.Path.Combine(folder, rePath);
        }

        [MenuItem("Assets/UFrame/BuiltIn/Binary2Text")]
        public static void Binary2Text()
        {
            if (Selection.activeObject)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(path))
                {
                    var bin2txtExePath = GetBuiltInPath("Data/Tools/binary2text.exe");
                    System.Threading.Thread thread = new System.Threading.Thread(() =>
                    {
                        var process = new System.Diagnostics.Process();
                        process.StartInfo = new System.Diagnostics.ProcessStartInfo();
                        process.StartInfo.FileName = bin2txtExePath;
                        process.StartInfo.Arguments = path;
                        process.Start();
                        EditorApplication.delayCall += () => { AssetDatabase.Refresh(); };
                    });
                    thread.Start();
                }
            }
        }

        [MenuItem("Assets/UFrame/BuiltIn/Binary2Text", validate = true)]
        static bool Binary2TextValid()
        {
            if (Selection.activeObject is DefaultAsset)
                return false;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path);
        }
    }
}