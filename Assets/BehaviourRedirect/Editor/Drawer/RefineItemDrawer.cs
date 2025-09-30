using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace ScriptRefine
{
    [CustomPropertyDrawer(typeof(RefineItem))]
    public class RefineItemDrawer : PropertyDrawer
    {
        SerializedProperty typeProp;
        SerializedProperty nameProp;
        SerializedProperty argumentsProp;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            nameProp = property.FindPropertyRelative("name");
            typeProp = property.FindPropertyRelative("type");
            argumentsProp = property.FindPropertyRelative("arguments");
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            return (argumentsProp.arraySize + 1) * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(rect, string.Format("{0}___[{1}]", nameProp.stringValue, typeProp.stringValue), EditorStyles.toolbarDropDown))
            {
                property.isExpanded = !property.isExpanded;
            }
            if (property.isExpanded)
            {
                for (int i = 0; i < argumentsProp.arraySize; i++)
                {
                    rect.y += EditorGUIUtility.singleLineHeight;
                    var prop = argumentsProp.GetArrayElementAtIndex(i);
                    var pname = prop.FindPropertyRelative("name");
                    var ptype = prop.FindPropertyRelative("type");
                    var rect_left = rect;
                    rect_left.width *= 0.3f;

                    var rect_right = rect;
                    rect_right.width *= 0.7f;
                    rect_right.x += rect_left.width;

                    EditorGUI.SelectableLabel(rect_left, pname.stringValue, EditorStyles.miniBoldLabel);
                    EditorGUI.SelectableLabel(rect_right, "  [" + ptype.stringValue + "]", EditorStyles.miniLabel);
                }
            }
        }
    }

}