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
	[CustomPropertyDrawer(typeof(ResourcePathAttribute))]
	public class ResourcePathDrawer : PropertyDrawer
	{

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.String)
			{
				Type objType = ((ResourcePathAttribute)attribute).resourceType;
				UnityEngine.Object obj = null;
				if (property.stringValue != "")
				{
					obj = Resources.Load(property.stringValue, objType);
				}

				obj = EditorGUI.ObjectField(rect, label, obj, objType, false);
				string assetPath = AssetDatabase.GetAssetPath(obj);
				if (assetPath != "")
				{
					string[] split = assetPath.Split(new string[] { "Resources/" }, StringSplitOptions.RemoveEmptyEntries);
					string ext = System.IO.Path.GetExtension(assetPath);
					property.stringValue = split[split.Length - 1].Replace(ext, "");
				}
			}
		}
	}
}