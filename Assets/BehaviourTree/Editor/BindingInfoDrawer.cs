/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-13
 * Version: 1.0.0
 * Description: 
 *_*/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    [CustomPropertyDrawer(typeof(BindingInfo))]
    public class BindingInfoDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string valuelabel = $"variable:";
            if (property.propertyPath.EndsWith(']'))
            {
                var lastIndex = property.propertyPath.LastIndexOf('[');
                var indexStr = property.propertyPath.Substring(lastIndex + 1, property.propertyPath.Length - lastIndex - 2);
                valuelabel = $"var [{indexStr}]:";
            }
            EditorGUI.BeginProperty(position, label, property);
            var key = property.FindPropertyRelative("name");
            var value = property.FindPropertyRelative("target");
            var keyLableRect = new Rect(position.x - 5, position.y, 60, position.height);
            var keyRect = new Rect(keyLableRect.max.x, position.y, position.width * 0.6f - keyLableRect.width - 10, position.height);
            var valueRect = new Rect(keyRect.max.x + 10, position.y, position.width * 0.4f - 10, position.height);
            EditorGUI.LabelField(keyLableRect, valuelabel);
            EditorGUI.PropertyField(keyRect, key, GUIContent.none);
            EditorGUI.PropertyField(valueRect, value, GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
}

