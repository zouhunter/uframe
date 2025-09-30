using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;

namespace UFrame.ObjDrawer
{
    public static class EditorGUILayout_object
    {
        public static bool Serialized_objectField(Serialized_object s_obj, GUIContent label = null)
        {
            Serialized_property iterator = s_obj.GetIterator();
            bool enterChildren = true;
            var changed = false;
            while ((iterator = iterator.NextVisible(enterChildren)) != null)
            {
                changed |= PropertyField(iterator, new GUILayoutOption[0]);
                enterChildren = false;
            }
            return changed;
        }
        public static bool PropertyField(Serialized_property property, params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();

            if (property.propertyType == Serialized_propertyType.Class)
            {
                DrawClassObject(property);
            }
            if(property.propertyType == Serialized_propertyType.ArraySize)
            {

            }
            else
            {
                DrawField(property);
            }
            return EditorGUI.EndChangeCheck();
        }

        public static void DrawClassObject(Serialized_property property)
        {
            if (!property.hasChildren) return;

            EditorGUI.indentLevel++;

            if (GUILayout.Button(property.name, EditorStyles.boldLabel))
            {
                property.isExpanded = !property.isExpanded;
            }

            if (property.isExpanded)
            {
                var iterator = property;
                var enterChildren = true;
                while ((iterator = iterator.NextVisible(enterChildren)) != null)
                {
                    PropertyField(iterator, new GUILayoutOption[0]);
                    enterChildren = false;
                }
            }
        }

        public static void DrawField(Serialized_property property)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(property.name, GUILayout.Width(100));
            if (property.value is int)
            {
                property.value = EditorGUILayout.IntField(Convert.ToInt32(property.value));
            }
            else if (property.value is bool)
            {
                property.value = EditorGUILayout.Toggle(Convert.ToBoolean(property.value));
            }
            else if (property.value is float || property.value is double)
            {
                property.value = EditorGUILayout.FloatField(float.Parse(property.value.ToString()));
            }
            else if (property.value is string)
            {
                property.value = EditorGUILayout.TextField(property.value.ToString());
            }
            else if (property.value is Color)
            {
                property.value = EditorGUILayout.ColorField((Color)property.value);
            }
            else if (property.value is Enum)
            {
                property.value = EditorGUILayout.EnumPopup((Enum)property.value);
            }
            else if (property.value is Vector2)
            {
                property.value = EditorGUILayout.Vector2Field("", (Vector2)property.value);
            }
            else if (property.value is Vector3)
            {
                property.value = EditorGUILayout.Vector3Field("", (Vector3)property.value);
            }
            else if (property.value is Vector4)
            {
                property.value = EditorGUILayout.Vector4Field("", (Vector4)property.value);
            }
            else if (property.value is Rect)
            {
                property.value = EditorGUILayout.RectField("", (Rect)property.value);
            }
            EditorGUILayout.EndHorizontal();
        }
        

    }
}