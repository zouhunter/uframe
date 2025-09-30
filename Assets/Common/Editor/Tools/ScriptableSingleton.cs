//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-19
//* 描    述： editor环境下的scritpableobject单例

//* ************************************************************************************
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UFrame.Editors
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T s_instance;
        public static T Instance
        {
            get
            {
                if (s_instance)
                    return s_instance;
                ReloadInstance();
                return s_instance;
            }

        }

        public static void ReloadInstance()
        {
            var path = GetPath();
            if (!string.IsNullOrEmpty(path))
            {
                UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                if (objs != null && objs.Length > 0)
                {
                    foreach (var obj in objs)
                    {
                        if (obj && obj is T)
                        {
                            s_instance = obj as T;
                            break;
                        }
                    }
                }
            }
            if (!s_instance)
                s_instance = CreateInstance<T>();
        }

        public static void Save()
        {
            var path = GetPath();
            if (!string.IsNullOrEmpty(path) && s_instance)
            {
                EditorUtility.SetDirty(s_instance);
                UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                if (objs != null && objs.Length > 0)
                {
                    int i = 0;
                    for (; i < objs.Length; i++)
                    {
                        var obj = objs[i];
                        if (obj && obj is T)
                        {
                            objs[i] = s_instance;
                            break;
                        }
                    }

                    if (i == objs.Length)
                    {
                        for (i = 0; i < objs.Length; i++)
                        {
                            var obj = objs[i];
                            if (!obj)
                            {
                                objs[i] = s_instance;
                                break;
                            }
                        }
                    }

                    if (i == objs.Length)
                    {
                        System.Array.Resize(ref objs, objs.Length + 1);
                        objs[i] = s_instance;
                    }
                }
                InternalEditorUtility.SaveToSerializedFileAndForget(objs, path, true);
            }
        }
        private static string GetPath()
        {
            var attrs = typeof(T).GetCustomAttributes(inherit: true);
            foreach (var item in attrs)
            {
                if (item is FilePathAttribute)
                {
                    var propAttr = typeof(FilePathAttribute).GetProperty("filepath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);
                    return propAttr.GetValue(item).ToString();
                }
            }
            return null;
        }
    }
}
#endif