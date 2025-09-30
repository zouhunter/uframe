//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UFrame
{
    using UnityEditor;
    [CustomPropertyDrawer(typeof(InspactorBtnAttribute))]
    public class InspactorBtnAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            var btnRect = new Rect(position.width - 40, position.y, 40, position.height);
            position.width = position.width - 60;

            EditorGUI.PropertyField(position, property, label);

            var btnAttribute = (InspactorBtnAttribute)attribute;
            if (GUI.Button(btnRect, btnAttribute.btnName,EditorStyles.miniButtonRight))
            {
                var method = property.serializedObject.targetObject.GetType().GetMethod(btnAttribute.funName,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.DeclaredOnly |
                    System.Reflection.BindingFlags.InvokeMethod |
                    System.Reflection.BindingFlags.NonPublic|
                    System.Reflection.BindingFlags.Public);
                if (method != null)
                {
                    method.Invoke(property.serializedObject.targetObject, null);
                }
            }
        }
    }
}