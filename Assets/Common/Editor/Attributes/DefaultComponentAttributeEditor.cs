//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-17 12:17:35
//* 描    述： 默认控件

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UFrame
{
    [CustomPropertyDrawer(typeof(DefaultComponentAttribute))]
    public class DefaultComponentAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObjects?.Length == 1)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label);
            }


            if (property.objectReferenceValue == null)
            {
                var targetObjs = property.serializedObject.targetObjects;
                for (int i = 0; i < targetObjs.Length; i++)
                {
                    var targetObj = targetObjs[i];
                    var serializeObj = new SerializedObject(targetObj);
                    var propertyInstance = serializeObj.FindProperty(property.propertyPath);
                    if (propertyInstance != null)
                    {
                        var targetBody = (targetObj as MonoBehaviour).gameObject;
                        if (typeof(Component).IsAssignableFrom(fieldInfo.FieldType))
                        {
                            var component = targetBody.GetComponent(fieldInfo.FieldType);
                            propertyInstance.objectReferenceValue = component;
                            propertyInstance.serializedObject.ApplyModifiedProperties();
                        }
                        else if (fieldInfo.FieldType == typeof(GameObject))
                        {
                            propertyInstance.objectReferenceValue = targetBody;
                            propertyInstance.serializedObject.ApplyModifiedProperties();
                        }
                        EditorUtility.SetDirty(targetBody);
                    }
                }
            }
            else if ((attribute as DefaultComponentAttribute).childOnly)
            {
                var targetObjs = property.serializedObject.targetObjects;
                for (int i = 0; i < targetObjs.Length; i++)
                {
                    var targetObj = targetObjs[i];
                    var serializeObj = new SerializedObject(targetObj);
                    var propertyInstance = serializeObj.FindProperty(property.propertyPath);
                    if (propertyInstance != null)
                    {
                        var targetBody = (targetObj as MonoBehaviour).gameObject;
                        if (typeof(Component).IsAssignableFrom(fieldInfo.FieldType))
                        {
                            var component = propertyInstance.objectReferenceValue as Component;
                            var childs = targetBody.GetComponentsInChildren(fieldInfo.FieldType);
                            if (!Array.Exists(childs, x => x == component))
                            {
                                propertyInstance.objectReferenceValue = null;
                                propertyInstance.serializedObject.ApplyModifiedProperties();
                                Debug.LogError("not child,fixed", targetBody);
                            }
                        }
                    }
                }
            }
        }
    }
}