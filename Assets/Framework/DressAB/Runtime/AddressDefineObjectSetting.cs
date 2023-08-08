//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-19
//* 描    述：

//* ************************************************************************************
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UFrame.DressAssetBundle {

    [FilePath("ProjectSettings/AddressDefineObjectSetting.asset",FilePathAttribute.Location.ProjectFolder)]
    public class AddressDefineObjectSetting : ScriptableObject
    {
        public AddressDefineObject activeAddressDefineObject;

        private static AddressDefineObjectSetting s_instance;
        public static AddressDefineObjectSetting Instance
        {
            get
            {
                if (s_instance)
                    return s_instance;
                var path = GetPath();
                if (!string.IsNullOrEmpty(path))
                {
                    UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                    if (objs != null && objs.Length > 0)
                        s_instance = objs[0] as AddressDefineObjectSetting;
                }
                if (!s_instance)
                    s_instance = CreateInstance<AddressDefineObjectSetting>();

                return s_instance;
            }

        }

        public static void Save()
        {
            var path = GetPath();
            if (!string.IsNullOrEmpty(path) && s_instance)
            {
                EditorUtility.SetDirty(s_instance);
                InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { s_instance }, path, true);
            }
        }

        private static string GetPath()
        {
            var attrs = typeof(AddressDefineObjectSetting).GetCustomAttributes(inherit: true);
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