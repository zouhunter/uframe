//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using UnityEditor;
using UnityEngine;

namespace UFrame
{
    [CustomPropertyDrawer(typeof(DisableInInspectorAttribute))]
    public class DisablePropertyDrawer : PropertyDrawer
    {
        private SerializedProperty property;

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, prop, label);
            EditorGUI.EndDisabledGroup();
        }
    }
}