//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UFrame
{
	[CustomPropertyDrawer(typeof(LayerAttribute))]
	public class LayerDrawer : PropertyDrawer
	{

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.Integer)
			{
				List<int> layers = new List<int>();
				List<GUIContent> layerNames = new List<GUIContent>();
				int currentSelection = 0;
				for (int i = 0; i < 32; i++)
				{
					string name = LayerMask.LayerToName(i);
					if (name != null)
					{
						if (i == property.intValue) currentSelection = i;
						layerNames.Add(new GUIContent(name));
						layers.Add(i);
					}
				}

				int newSelection = EditorGUI.Popup(rect, label, currentSelection, layerNames.ToArray());
				property.intValue = layers[newSelection];
			}
		}
	}
}