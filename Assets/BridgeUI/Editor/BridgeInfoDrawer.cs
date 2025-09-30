using System;
using UnityEditor;
using UnityEngine;

namespace UFrame.BridgeUI.Editors
{
    [CustomPropertyDrawer(typeof(BridgeInfo))]
    public class BridgeInfoDrawer : PropertyDrawer
    {
        protected SerializedProperty inNodeProp;
        protected SerializedProperty outNodeProp;
        protected SerializedProperty indexProp;

        protected SerializedProperty autoProp;
        protected SerializedProperty singleProp;
        protected SerializedProperty mutexProp;
        protected SerializedProperty baseShowProp;
        private const int labelWidth = 60;
        public static System.Action bridgeChangeAction { get; set; }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitPropertys(property);
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
                height *= 8;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitPropertys(property);

            Rect btnRect = new Rect(position.xMin, position.yMin + 2f, position.width * 0.9f, EditorGUIUtility.singleLineHeight);

            GUI.contentColor = Color.green;

            var inNode = !string.IsNullOrEmpty(inNodeProp.stringValue) ? inNodeProp.stringValue : "(any)";
            var showName = $"{inNode}->{outNodeProp.stringValue}";

            if (GUI.Button(btnRect, showName, EditorStyles.toolbarDropDown))
            {
                property.isExpanded = !property.isExpanded;
            }

            GUI.contentColor = Color.white;

            var autoRect = new Rect(btnRect.x + btnRect.width * 0.5f, btnRect.y, 30, btnRect.height);

            if (autoProp.boolValue)
            {
                GUI.contentColor = Color.blue;
                EditorGUI.LabelField(autoRect, "[A]");
            }

            if (singleProp.boolValue)
            {
                var singRect = autoRect;
                singRect.x += autoRect.width;
                GUI.contentColor = Color.yellow;
                EditorGUI.LabelField(singRect, "[S]");
            }

            if (mutexProp.enumValueIndex != 0)
            {
                var mutexRect = autoRect;
                mutexRect.x += autoRect.width * 2;
                GUI.contentColor = Color.red;
                EditorGUI.LabelField(mutexRect, "[M]" + mutexProp.enumValueIndex);
            }

            if (baseShowProp.enumValueIndex != 0)
            {
                var baseRect = autoRect;
                baseRect.x += autoRect.width * 3;
                GUI.contentColor = Color.cyan;
                EditorGUI.LabelField(baseRect, "[P]" + baseShowProp.enumValueIndex);
            }

            GUI.contentColor = Color.white;

            Rect acceptRect = new Rect(position.max.x - position.width * 0.05f, position.yMin + 2f, position.width * 0.1f, EditorGUIUtility.singleLineHeight);


            EditorGUI.LabelField(acceptRect, indexProp.intValue.ToString());

            if (property.isExpanded)
            {
                Rect opendRect = new Rect(position.xMin, position.yMin + EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight);
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    DrawExpanded(opendRect);
                    if (changeScope.changed)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        bridgeChangeAction?.Invoke();
                    }
                }
            }
        }

        protected void InitPropertys(SerializedProperty property)
        {
            inNodeProp = property.FindPropertyRelative("inNode");
            outNodeProp = property.FindPropertyRelative("outNode");
            indexProp = property.FindPropertyRelative("index");
            autoProp = property.FindPropertyRelative("showModel.auto");
            singleProp = property.FindPropertyRelative("showModel.single");
            mutexProp = property.FindPropertyRelative("showModel.mutex");
            baseShowProp = property.FindPropertyRelative("showModel.baseShow");
        }

        protected void DrawExpanded(Rect opendRect)
        {
            var rect = new Rect(opendRect.x, opendRect.y + 5f, opendRect.width, EditorGUIUtility.singleLineHeight);
            var propRect = new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight);
            var labelRect = new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight);
            using (var disable = new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUI.LabelField(labelRect, "触发界面:");
                inNodeProp.stringValue = EditorGUI.TextField(propRect, inNodeProp.stringValue);

                labelRect.y += EditorGUIUtility.singleLineHeight;
                propRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(labelRect, "目标界面:");
                outNodeProp.stringValue = EditorGUI.TextField(propRect, outNodeProp.stringValue);
            }

            labelRect.y += EditorGUIUtility.singleLineHeight;
            propRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, "端口:");
            indexProp.intValue = EditorGUI.IntField(propRect, indexProp.intValue);

            labelRect.y += EditorGUIUtility.singleLineHeight;
            propRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, "自动打开:");
            autoProp.boolValue = EditorGUI.Toggle(propRect, autoProp.boolValue);

            labelRect.y += EditorGUIUtility.singleLineHeight;
            propRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, "单独显示:");
            singleProp.boolValue = EditorGUI.Toggle(propRect, singleProp.boolValue);

            labelRect.y += EditorGUIUtility.singleLineHeight;
            propRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, "互斥:");
            mutexProp.enumValueIndex = (int)(MutexRule)EditorGUI.EnumPopup(propRect, (MutexRule)mutexProp.enumValueIndex);

            labelRect.y += EditorGUIUtility.singleLineHeight;
            propRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, "父级控制:");
            baseShowProp.enumValueIndex = (int)(ParentShow)EditorGUI.EnumPopup(propRect, (ParentShow)baseShowProp.enumValueIndex);
        }
    }
}