using UnityEditor;
using UnityEngine;

namespace UFrame.BridgeUI.Editors
{
    [CustomPropertyDrawer(typeof(PrefabUIInfo))]
    public class PrefabUIInfoDrawer : UIInfoBaseDrawer
    {
        SerializedProperty prefabProp;//prefab
        protected const int ht = 0;

        protected override void InitPropertys(SerializedProperty property)
        {
            base.InitPropertys(property);
            prefabProp = property.FindPropertyRelative("prefab");
        }

        protected override float GetInfoItemHeight()
        {
            return ht * (EditorGUIUtility.singleLineHeight);
        }

        protected override void DrawExpanded(Rect opendRect)
        {
            using (var disableGroup = new EditorGUI.DisabledScope(true)/*!PanelGroupBaseDrawer.Current || !PanelGroupBaseDrawer.Current.editAble)*/)
            {
                var rect = opendRect;// GetPaddingRect(new Rect(opendRect.x, opendRect.y, opendRect.width, singleHeight),5);
                EditorGUI.PropertyField(rect, typeProp, new GUIContent("[type]"));
            }
        }

        protected override void DrawObjectField(Rect rect)
        {
            if (prefabProp.objectReferenceValue != null)
            {
                if (GUI.Button(rect, " ", EditorStyles.objectFieldMiniThumb))
                {
                    EditorGUIUtility.PingObject(prefabProp.objectReferenceValue);
                }
            }
            else
            {
                prefabProp.objectReferenceValue = EditorGUI.ObjectField(rect, null, typeof(GameObject), false);
            }
        }


        protected override void OnDragPerformGameObject(GameObject go)
        {
            prefabProp.objectReferenceValue = go;
            panelNameProp.stringValue = go.name;
        }

        protected override void ResetBuildInfoOnOpen()
        {
            base.ResetBuildInfoOnOpen();

            if (prefabProp.objectReferenceValue != null)
            {
                panelNameProp.stringValue = prefabProp.objectReferenceValue.name;
            }
        }

        protected override GameObject GetPrefabItem()
        {
            GameObject gopfb = prefabProp.objectReferenceValue as GameObject;
            return gopfb;
        }

        protected override void WorningIfNotRight(Rect rect)
        {
            if (prefabProp.objectReferenceValue == null)
            {
                Worning(rect, "prefab:" + panelNameProp.stringValue + "  is empty!");
            }
        }
    }
}