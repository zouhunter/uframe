using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.EasyBT
{
    [CustomPropertyDrawer(typeof(SharedVariable),true)]
    public class SharedVariableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //EditorGUI.BeginProperty(position, label, property);
            var nameProperty = property.FindPropertyRelative("Value");
            if(nameProperty != null)
                EditorGUI.PropertyField(position, nameProperty, label);
            else
                EditorGUI.PropertyField(position, property, label);
            //EditorGUI.EndProperty();
        }
    }
}