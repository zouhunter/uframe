//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:26:14
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UFrame.LitUI
{
    [CustomEditor(typeof(UIGroupObject))]
    public class UIGroupDrawer : Editor
    {
        private SerializedProperty _infosProp;
        private SerializedProperty _scriptProp;
        private ReorderableList _reorderList;
        private string _matchText;

        private void OnEnable()
        {
            if (!target)
                return;
            _infosProp = serializedObject.FindProperty("infos");
            _scriptProp = serializedObject.FindProperty("m_Script");
            ReloadReorderList();
        }

        private void ReloadReorderList()
        {
            _reorderList = new ReorderableList(serializedObject, _infosProp, true, true, true, true);
            _reorderList.drawHeaderCallback = OnDrawInfoHead;
            _reorderList.drawElementCallback = OnDrawElement;
            _reorderList.elementHeightCallback = OnElementHeight;
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if(rect.height <= 0.1f)
                return;
            GUI.Box(rect, "");
            var innerRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            var prop = _infosProp.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(innerRect, prop, true);
        }

        private void OnDrawInfoHead(Rect rect)
        {
            if (rect.width < 0)
                return;
            var infoRect = new Rect(rect.x, rect.y, 60, rect.height);
            EditorGUI.LabelField(infoRect, "UI列表");
            DrawAccept(infoRect);

            var searchRect = new Rect(infoRect.xMax + 5, infoRect.y, 120, infoRect.height);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                _matchText = EditorGUI.TextField(searchRect, _matchText, EditorStyles.toolbarSearchField);
                if (changeScope.changed)
                {
                    ReloadReorderList();
                }
            }

            var labelWidth = 10;
            var variableWidth = 20;
            var popWidth = 80;
            var layerRect = new Rect(rect.x + rect.width - labelWidth * 4 - variableWidth * 2 - popWidth * 2, rect.y, popWidth, rect.y);
            EditorGUI.LabelField(layerRect, "Layer", EditorStyles.miniBoldLabel);

            var modifyRect = new Rect(layerRect.xMax + labelWidth, layerRect.y, layerRect.width, layerRect.height);
            EditorGUI.LabelField(modifyRect, "Modify", EditorStyles.miniBoldLabel);

            var prioRect = new Rect(modifyRect.xMax - labelWidth, modifyRect.y, 40, modifyRect.height);
            EditorGUI.LabelField(prioRect, "Priority", EditorStyles.miniBoldLabel);

            var mutRect = new Rect(prioRect.xMax + labelWidth, prioRect.y, 40, prioRect.height);
            EditorGUI.LabelField(mutRect, "Repel", EditorStyles.miniBoldLabel);
        }

        private float OnElementHeight(int index)
        {
            var nampProp = _infosProp.GetArrayElementAtIndex(index).FindPropertyRelative("name");
            if (string.IsNullOrEmpty(_matchText) || nampProp.stringValue.ToLower().Contains(_matchText.ToLower()))
            {
                return EditorGUIUtility.singleLineHeight + 4;
            }
            return 0;
        }

        private void DrawAccept(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    if (DragAndDrop.objectReferences != null)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
                if (Event.current.type == EventType.DragPerform)
                {
                    if (DragAndDrop.objectReferences != null)
                    {
                        serializedObject.Update();
                        foreach (var item in DragAndDrop.objectReferences)
                        {
                            _infosProp.InsertArrayElementAtIndex(_infosProp.arraySize);
                            var element = _infosProp.GetArrayElementAtIndex(_infosProp.arraySize - 1);
                            element.FindPropertyRelative("name").stringValue = item.name;
                            var prefab = item;
                            if (PrefabUtility.IsPartOfPrefabInstance(item))
                                prefab = PrefabUtility.GetCorrespondingObjectFromSource(item);
                            element.FindPropertyRelative("guid").stringValue = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(prefab)).ToString();
                            element.FindPropertyRelative("modify").intValue = UISetting.Instance.GetDefaultModify();
                        }
                        EditorUtility.SetDirty(target);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (Selection.activeObject == target)
                using (var disable = new EditorGUI.DisabledScope(true))
                    EditorGUILayout.PropertyField(_scriptProp);
            _reorderList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
