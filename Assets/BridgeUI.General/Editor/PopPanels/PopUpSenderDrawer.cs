using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UFrame.BridgeUI
{
    [CustomEditor(typeof(PopupSender))]
    public class PopupSenderDrawer : Editor
    {
        private SerializedProperty popEnumProp;
        private SerializedProperty scriptProp;
        private SerializedProperty selectedProp;
        private SerializedProperty enumTypeProp;

        private string[] options;
        private int selected;
        MonoScript m_popEnum;

        void OnEnable()
        {
            scriptProp = serializedObject.FindProperty("m_Script");
            popEnumProp = serializedObject.FindProperty("popEnumGuid");
            selectedProp = serializedObject.FindProperty("selected");
            enumTypeProp = serializedObject.FindProperty("enumType");

            if (!string.IsNullOrEmpty(popEnumProp.stringValue))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(popEnumProp.stringValue);
                m_popEnum = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            }
            UpdateOptions();
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(scriptProp);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            m_popEnum = EditorGUILayout.ObjectField(m_popEnum, typeof(MonoScript), false) as MonoScript;
            if (EditorGUI.EndChangeCheck())
            {
                UpdateOptions();
                if (m_popEnum)
                {
                    var assetPath = AssetDatabase.GetAssetPath(m_popEnum);
                    popEnumProp.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    popEnumProp.stringValue = "";
                    EditorUtility.SetDirty(target);
                }
            }

            EditorGUI.BeginDisabledGroup(!m_popEnum);
            if (options != null)
            {
                EditorGUI.BeginChangeCheck();
                selected = EditorGUILayout.Popup(selected, options);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateTargetInfo();
                }
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateTargetInfo()
        {
            if (options != null && options.Length > selected && selected >= 0)
            {
                selectedProp.stringValue = options[selected];
            }
        }

        private void UpdateOptions()
        {
            if (m_popEnum)
            {
                var classes = m_popEnum.GetClass();
                if (!classes.IsEnum)
                {
                    Debug.LogError("请放入枚举类型！");
                    popEnumProp.stringValue = "";
                }
                else
                {
                    enumTypeProp.stringValue = classes.FullName;
                    options = System.Enum.GetNames(classes);
                }
            }
        }
    }
}