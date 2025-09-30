using UnityEditor;
using UnityEngine;

namespace UFrame.BridgeUI.Editors
{
    [CustomPropertyDrawer(typeof(ResourceUIInfo))]
    public class ResourceUIInfoDrawer : UIInfoBaseDrawer
    {
        protected SerializedProperty goodProp;
        protected SerializedProperty guidProp;
        protected SerializedProperty resourcePathProp;
        private const int labelWidth = 60;

        protected override void InitPropertys(SerializedProperty property)
        {
            base.InitPropertys(property);
            goodProp = property.FindPropertyRelative("good");
            guidProp = property.FindPropertyRelative("guid");
            resourcePathProp = property.FindPropertyRelative("resourcePath");
        }
        protected override float GetInfoItemHeight()
        {
            return 0;// EditorGUIUtility.singleLineHeight * 1.2f;
        }

        protected override void DragAndDrapAction(Rect acceptRect)
        {
            base.DragAndDrapAction(acceptRect);
            if (Event.current.type == EventType.Repaint)
            {
                var path0 = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
                var obj0 = AssetDatabase.LoadAssetAtPath<GameObject>(path0);
                goodProp.boolValue = obj0 != null;
            }
        }

        protected override void DrawExpanded(Rect opendRect)
        {
            var rect = new Rect(opendRect.x, opendRect.y + 5f, opendRect.width, singleHeight);
            var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            var bodyRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);
            using(var disableGroup = new EditorGUI.DisabledScope(/*!PanelGroupBaseDrawer.Current || !PanelGroupBaseDrawer.Current.editAble*/))
            {
                EditorGUI.LabelField(labelRect, "[资源路径]");
                using (var disableGroup0 = new EditorGUI.DisabledScope())
                    resourcePathProp.stringValue = EditorGUI.TextField(bodyRect, resourcePathProp.stringValue);
                rect.y += singleHeight;
                EditorGUI.PropertyField(rect, typeProp);
            }
        }

        protected override void DrawObjectField(Rect acceptRect)
        {
            if (goodProp.boolValue)
            {
                if (GUI.Button(acceptRect, "", EditorStyles.objectFieldMiniThumb))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    EditorGUIUtility.PingObject(obj);
                }
            }
            else
            {
                var obj = EditorGUI.ObjectField(acceptRect, null, typeof(GameObject), false);
                if (obj != null)
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                }
            }

        }

        protected override void WorningIfNotRight(Rect rect)
        {
            if (goodProp != null && !goodProp.boolValue)
            {
                Worning(rect, panelNameProp.stringValue + " Changed！!!");
            }
        }

        protected override void OnDragPerformGameObject(GameObject go)
        {
            var path = AssetDatabase.GetAssetPath(go);
            if (!string.IsNullOrEmpty(path))
            {
                guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
                panelNameProp.stringValue = go.name;
                resourcePathProp.stringValue = BridgeUI.Editors.BridgeEditorUtility.GetResourcesPath(path);
                Debug.Log(resourcePathProp.stringValue);
            }
        }

        protected override void InstantiatePrefab(GameObject gopfb, Transform parent)
        {
            base.InstantiatePrefab(gopfb, parent);
            var path = AssetDatabase.GetAssetPath(gopfb);
            guidProp.stringValue = AssetDatabase.AssetPathToGUID(path);
        }

        protected override GameObject GetPrefabItem()
        {
            return Resources.Load<GameObject>(resourcePathProp.stringValue);
        }
    }
}