using UnityEngine;
using UnityEditor;

namespace UFrame.PathFind
{
    [CustomPropertyDrawer(typeof(MapLayer))]
    public class MapLayerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProp = property.FindPropertyRelative("name");
            var valueProp = property.FindPropertyRelative("value");
            var colorProp = property.FindPropertyRelative("color");
            EditorGUI.LabelField(new Rect(position.x, position.y, 40,position.height),"layer:");
            var nameRect = new Rect(position.x + 40,position.y,position.width * 0.3f,position.height);
            EditorGUI.TextField(nameRect, nameProp.stringValue);

            var valueRect = new Rect(position.x + 40 + position.width * 0.3f, position.y,position.width * 0.3f, position.height);
            valueProp.intValue = EditorGUI.IntField(valueRect, valueProp.intValue);

            var colorRect = new Rect(position.x + 40 + position.width * 0.6f, position.y, position.width * 0.3f, position.height);
            colorProp.colorValue = EditorGUI.ColorField(colorRect, colorProp.colorValue);
        }
    }
}