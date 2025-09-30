//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace UFrame
{
    [CustomPropertyDrawer(typeof(EnumLabelAttribute))]
    public class EnumLabelDrawer : PropertyDrawer
    {
        private Dictionary<string, string> customEnumNames = new Dictionary<string, string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetUpCustomEnumNames(property, property.enumNames);

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginChangeCheck();
                string[] displayedOptions = property.enumNames
                        .Where(enumName => customEnumNames.ContainsKey(enumName))
                        .Select<string, string>(enumName => customEnumNames[enumName])
                        .ToArray();

                int[] indexArray = GetIndexArray(enumLabelAttribute.orders);
                if (indexArray.Length != displayedOptions.Length)
                {
                    indexArray = new int[displayedOptions.Length];
                    for (int i = 0; i < indexArray.Length; i++)
                    {
                        indexArray[i] = i;
                    }
                }
                string[] items = new string[displayedOptions.Length];
                items[0] = displayedOptions[0];
                for (int i = 0; i < displayedOptions.Length; i++)
                {
                    items[i] = displayedOptions[indexArray[i]];
                }
                int index = -1;
                for (int i = 0; i < indexArray.Length; i++)
                {
                    if (indexArray[i] == property.enumValueIndex)
                    {
                        index = i;
                        break;
                    }
                }
                if ((index == -1) && (property.enumValueIndex != -1)) { SortingError(position, property, label); return; }
                index = EditorGUI.Popup(position, enumLabelAttribute.label, index, items);
                if (EditorGUI.EndChangeCheck())
                {
                    if (index >= 0)
                        property.enumValueIndex = indexArray[index];
                }
            }
        }

        private EnumLabelAttribute enumLabelAttribute
        {
            get
            {
                return (EnumLabelAttribute)attribute;
            }
        }

        public void SetUpCustomEnumNames(SerializedProperty property, string[] enumNames)
        {


            object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumLabelAttribute), false);
            foreach (EnumLabelAttribute customAttribute in customAttributes)
            {
                Type enumType = fieldInfo.FieldType;

                foreach (string enumName in enumNames)
                {
                    FieldInfo field = enumType.GetField(enumName);
                    if (field == null) continue;
                    EnumLabelAttribute[] attrs = (EnumLabelAttribute[])field.GetCustomAttributes(customAttribute.GetType(), false);

                    if (!customEnumNames.ContainsKey(enumName))
                    {
                        foreach (EnumLabelAttribute labelAttribute in attrs)
                        {
                            customEnumNames.Add(enumName, labelAttribute.label);
                        }
                    }
                }
            }
        }


        int[] GetIndexArray(int[] order)
        {
            int[] indexArray = new int[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                int index = 0;
                for (int j = 0; j < order.Length; j++)
                {
                    if (order[i] > order[j])
                    {
                        index++;
                    }
                }
                indexArray[i] = index;
            }
            return (indexArray);
        }

        void SortingError(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (sorting error)"));
            EditorGUI.EndProperty();
        }
    }
    public class EnumLabelEditorTool
    {
        static public object GetEnum(Type type, SerializedObject serializedObject, string path)
        {
            SerializedProperty property = GetPropety(serializedObject, path);
            return System.Enum.GetValues(type).GetValue(property.enumValueIndex);
        }
        static public object DrawEnum(Type type, SerializedObject serializedObject, string path)
        {
            return DrawEnum(type, serializedObject, GetPropety(serializedObject, path));
        }
        static public object DrawEnum(Type type, SerializedObject serializedObject, SerializedProperty property)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(property);
            serializedObject.ApplyModifiedProperties();
            return System.Enum.GetValues(type).GetValue(property.enumValueIndex);
        }
        static public SerializedProperty GetPropety(SerializedObject serializedObject, string path)
        {
            string[] contents = path.Split('/');
            SerializedProperty property = serializedObject.FindProperty(contents[0]);
            for (int i = 1; i < contents.Length; i++)
            {
                property = property.FindPropertyRelative(contents[i]);
            }
            return property;
        }
    }
}