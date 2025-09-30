//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace UFrame
{
	[CustomPropertyDrawer(typeof(SortingLayerAttribute))]
	public class SortingLayerDrawer : PropertyDrawer
	{

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.String)
			{
				Type t = typeof(UnityEditorInternal.InternalEditorUtility);
				PropertyInfo sortingLayersProperty = t.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
				string[] sortingLayers = (string[])sortingLayersProperty.GetValue(null, new object[0]);

				int currentSelection = 0;
				GUIContent[] guiContent = new GUIContent[sortingLayers.Length];
				for (int i = 0; i < sortingLayers.Length; i++)
				{
					string s = sortingLayers[i];
					guiContent[i] = new GUIContent(s);
					if (s == property.stringValue) currentSelection = i;
				}

				int newSelection = EditorGUI.Popup(rect, label, currentSelection, guiContent);
				property.stringValue = sortingLayers[newSelection];
			}
		}
	}
}