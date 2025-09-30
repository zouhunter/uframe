//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using UnityEngine;
using UnityEditor;
using System;

namespace UFrame
{
	[CustomPropertyDrawer(typeof(MaterialIndexAttribute))]
	public class MaterialIndexDrawer : PropertyDrawer
	{

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Integer)
			{
				UnityEngine.Object obj = property.serializedObject.targetObject;
				GameObject go = null;
				if (obj is GameObject) go = obj as GameObject;
				else if (obj is Component) go = (obj as Component).gameObject;

				GUIContent[] materialNames = Array.ConvertAll(
					go.GetComponent<Renderer>().sharedMaterials,
					m => new GUIContent((m == null) ? "null" : m.name)
				);
				property.intValue = EditorGUI.Popup(rect, label, property.intValue, materialNames);
			}
		}
	}
}