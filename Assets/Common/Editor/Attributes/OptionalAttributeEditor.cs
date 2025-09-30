//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-25 09:30:58
//* 描    述：  

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEditor;

namespace UFrame
{
    [CustomPropertyDrawer(typeof(OptionalAttribute))]
    public class OptionalAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propertyAttribute = attribute as OptionalAttribute;
            var basicPropPath = propertyAttribute.refPropPath;
            var basicProp = property.serializedObject.FindProperty(basicPropPath);
            bool disable = false;
            switch (basicProp.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    disable = basicProp.intValue == 0;
                    break;
                case SerializedPropertyType.Boolean:
                    disable = basicProp.boolValue == false;
                    break;
                case SerializedPropertyType.Float:
                    disable = basicProp.floatValue == 0;
                    break;
                case SerializedPropertyType.String:
                    disable = string.IsNullOrEmpty(basicProp.stringValue);
                    break;
                case SerializedPropertyType.Color:
                    break;
                case SerializedPropertyType.ObjectReference:
                    disable = basicProp.objectReferenceValue == null;
                    break;
                case SerializedPropertyType.LayerMask:
                    break;
                case SerializedPropertyType.Enum:
                    break;
                case SerializedPropertyType.Vector2:
                    break;
                case SerializedPropertyType.Vector3:
                    break;
                case SerializedPropertyType.Vector4:
                    break;
                case SerializedPropertyType.Rect:
                    break;
                case SerializedPropertyType.ArraySize:
                    break;
                case SerializedPropertyType.Character:
                    break;
                case SerializedPropertyType.AnimationCurve:
                    break;
                case SerializedPropertyType.Bounds:
                    break;
                case SerializedPropertyType.Gradient:
                    break;
                case SerializedPropertyType.Quaternion:
                    break;
                case SerializedPropertyType.ExposedReference:
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    break;
                case SerializedPropertyType.Vector2Int:
                    break;
                case SerializedPropertyType.Vector3Int:
                    break;
                case SerializedPropertyType.RectInt:
                    break;
                case SerializedPropertyType.BoundsInt:
                    break;
                case SerializedPropertyType.ManagedReference:
                    break;
                default:
                    break;
            }
            using (var disableScope = new EditorGUI.DisabledGroupScope(disable))
            {
                EditorGUI.PropertyField(position, property,label, true);
                //base.OnGUI(position, property, label);
            }
        }
    }
}