using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

namespace UFrame.ScriptGen
{
    public class ScriptTempConfigObject : ScriptableObject
    {
        public static string path = "ProjectSettings/ScriptTempConfigObject.asset";
        public static ScriptTempConfigObject m_instance;
        public static ScriptTempConfigObject Instance
        {
            get
            {
                if (m_instance)
                    return m_instance;

                UnityEngine.Object[] objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                if (objs != null && objs.Length > 0)
                {
                    m_instance = objs[0] as ScriptTempConfigObject;
                }
                if (m_instance == null)
                {
                    m_instance = CreateInstance<ScriptTempConfigObject>();
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

        public string header = @"//*************************************************************************************
//* 作    者： $author
//* 创建时间： $time
//* 描    述：
$desc
//* ************************************************************************************";
        public List<ScriptTemplate> templates = new List<ScriptTemplate>();

    }
}