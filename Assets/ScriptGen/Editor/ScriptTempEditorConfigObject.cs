using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UFrame.ScriptGen
{
    public class ScriptTempEditorConfigObject : ScriptableObject
    {
        public static string path = "Library/ScriptTempEditorConfigObject.asset";
        public static ScriptTempEditorConfigObject m_instance;
        public static ScriptTempEditorConfigObject Instance
        {
            get
            {
                if (m_instance)
                    return m_instance;

                UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                if (objs != null && objs.Length > 0)
                {
                    m_instance = objs[0] as ScriptTempEditorConfigObject;
                }
                if (m_instance == null)
                {
                    m_instance = CreateInstance<ScriptTempEditorConfigObject>();
                }
                return m_instance;
            }
        }

        public static void Save()
        {
            if (m_instance != null)
            {
                EditorUtility.SetDirty(m_instance);
                var objs = new UnityEngine.Object[1] { m_instance };
                InternalEditorUtility.SaveToSerializedFileAndForget(objs, path, true);
            }
        }
        public string activeClassTemplate;
        public string activeStructTemplate;
        public string activeEnumTemplate;
        public string activeInterfaceTemplate;
        public WaitAddTemplate waitAddTemplate = new WaitAddTemplate();
        public string author;
        public string workingNameSpace;
        public string workingEditorNameSpace;

        internal bool IsDefault(string name,TemplateType type)
        {
            return GetDefault(type) == name;
        }

        internal void SetDefault(string name, TemplateType type)
        {
            switch (type)
            {
                case TemplateType.Enum:
                    activeEnumTemplate = name;
                    break;
                case TemplateType.Struct:
                    activeStructTemplate = name;
                    break;
                case TemplateType.Class:
                    activeClassTemplate = name;
                    break;
                case TemplateType.Interface:
                    activeInterfaceTemplate = name;
                    break;
                default:
                    break;
            }
        }
        internal string GetDefault(TemplateType type)
        {
            string defaultName = null;
            switch (type)
            {
                case TemplateType.Enum:
                    defaultName = Instance.activeEnumTemplate;
                    break;
                case TemplateType.Struct:
                    defaultName = Instance.activeStructTemplate;
                    break;
                case TemplateType.Class:
                    defaultName = Instance.activeClassTemplate;
                    break;
                case TemplateType.Interface:
                    defaultName = Instance.activeInterfaceTemplate;
                    break;
                default:
                    break;
            }
            return defaultName;
        }
    }
}