using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace UFrame.Editors
{
    public class ObjViewToolMenu
    {
        [MenuItem("Assets/UFrame/ViewPrefab")]
        public static void ViewAllPrefab()
        {
            var selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(selectionPath))
            {
                var ok = EditorUtility.DisplayDialog("confer", "need show all assets:" + selectionPath, "ok");
                if (!ok)
                    return;

                var prefabPaths = System.IO.Directory.GetFiles(selectionPath, "*.prefab", System.IO.SearchOption.AllDirectories);
                if (prefabPaths != null && prefabPaths.Length > 0)
                {
                    int viewLineNum = (int)Mathf.Sqrt(prefabPaths.Length);
                    int viewlineCount = 0;
                    int viewLineRow = 0;
                    int span = 2;
                    Transform parent = new GameObject("[ViewPrefab]").transform;
                    for (int i = 0; i < prefabPaths.Length; i++)
                    {
                        var path = prefabPaths[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
                        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (viewlineCount > viewLineNum)
                        {
                            viewlineCount = 0;
                            viewLineRow++;
                        }
                        viewlineCount++;
                        var pos = new Vector3(span * viewLineRow, span * viewlineCount);
                        var instance = PrefabUtility.InstantiatePrefab(obj, parent) as GameObject;
                        instance.transform.position = pos;
                    }
                }
            }
        }

        [MenuItem("Assets/UFrame/ViewPrefab", validate = true)]
        public static bool CheckCanViewAllPrefab()
        {
            if (Selection.activeObject is DefaultAsset)
            {
                var selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (selectionPath == "Assets")
                    return false;
                return true;
            }
            return false;
        }
    }
}