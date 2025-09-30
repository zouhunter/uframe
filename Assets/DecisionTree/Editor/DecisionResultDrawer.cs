//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-06-04
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace UFrame.Decision
{
    public class DecisionResultDrawer
    {
        public bool changed;
        public DecisionLeafNode node;
        public DecisionResultDrawer(DecisionLeafNode node)
        {
            this.node = node;
        }
        public float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight + 8;
        }

        public void OnGUI(Rect rect)
        {
            using (var checkChangeScope = new EditorGUI.ChangeCheckScope())
            {
                float padding = 4f;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float x = rect.x + padding;
                float width = rect.width - 2 * padding;
                float iconWidth = 20f;
                GUIContent iconContent = EditorGUIUtility.IconContent("d_FilterSelectedOnly"); // 可换为其他内置图标
                                                                                               // 图标
                var iconRect = new Rect(x, rect.y, iconWidth, lineHeight);
                if (GUI.Button(iconRect, iconContent))
                {
                    CreateNewResult();
                }
                x += iconWidth + padding;
                if (node.result == null)
                {
                    if (GUI.Button(new Rect(x, rect.y, width - iconWidth - padding, lineHeight), "Null", EditorStyles.textField))
                    {
                        CreateNewResult();
                    }
                    return;
                }
                var result = node.result;
                // key
                float keyWidth = width * 0.35f - iconWidth;
                float valueWidth = 40;
                if (result is DecisionBoolResult)
                {
                    valueWidth = 20;
                }
                var keyRect = new Rect(x, rect.y, keyWidth, lineHeight);
                result.key = EditorGUI.TextField(keyRect, result.key);
                x += keyWidth + padding;
                // key和value之间的->符号
                float arrowWidth = 20f;
                var arrowRect = new Rect(x, rect.y, arrowWidth, lineHeight);
                EditorGUI.LabelField(arrowRect, "->", EditorStyles.miniLabel);
                x += arrowWidth + padding;
                // Value字段（反射）
                var valueRect = new Rect(x, rect.y, valueWidth, lineHeight);
                var valueField = result.GetType().GetField("value");
                if (valueField != null)
                {
                    object value = valueField.GetValue(result);
                    Type valueType = valueField.FieldType;
                    if (valueType == typeof(int))
                    {
                        int newValue = EditorGUI.IntField(valueRect, value != null ? (int)value : 0);
                        valueField.SetValue(result, newValue);
                    }
                    else if (valueType == typeof(float))
                    {
                        float newValue = EditorGUI.FloatField(valueRect, value != null ? (float)value : 0f);
                        valueField.SetValue(result, newValue);
                    }
                    else if (valueType == typeof(bool))
                    {
                        bool newValue = EditorGUI.Toggle(valueRect, value != null ? (bool)value : false);
                        valueField.SetValue(result, newValue);
                    }
                    else if (valueType == typeof(string))
                    {
                        string newValue = EditorGUI.TextField(valueRect, value != null ? (string)value : "");
                        valueField.SetValue(result, newValue);
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                    {
                        UnityEngine.Object newValue = EditorGUI.ObjectField(valueRect, value as UnityEngine.Object, valueType, true);
                        valueField.SetValue(result, newValue);
                    }
                    else
                    {
                        EditorGUI.LabelField(valueRect, value != null ? value.ToString() : "null");
                    }
                }
                else
                {
                    EditorGUI.LabelField(valueRect, "无value属性");
                }
                x += valueWidth + padding;
                // desc描述信息
                float descWidth = width - (iconWidth + keyWidth + valueWidth + padding * 4);
                var descRect = new Rect(x + 10, rect.y, descWidth - 20, lineHeight);
                var leftParenRect = new Rect(x, rect.y, 10, lineHeight);
                var rightParenRect = new Rect(x + descWidth - 10, rect.y, 10, lineHeight);
                EditorGUI.LabelField(leftParenRect, "(", EditorStyles.miniLabel);
                result.desc = EditorGUI.TextField(descRect, result.desc, EditorStyles.textField);
                EditorGUI.LabelField(rightParenRect, ")", EditorStyles.miniLabel);
                x += descWidth + padding;

                changed = checkChangeScope.changed;
            }
        }

        private void CreateNewResult()
        {
            ShowResultTypeMenu(type =>
            {
                var newResult = Activator.CreateInstance(type) as DecisionResult;
                if (node.result != null)
                {
                    newResult.key = node.condition.key;
                    newResult.desc = node.condition.desc;
                }
                node.result = newResult;
                changed = true;
            });
        }

        // 抽取菜单弹出逻辑
        private void ShowResultTypeMenu(Action<Type> onSelect)
        {
            var baseType = typeof(DecisionResult);
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (baseType.IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null && type != baseType)
                    {
                        types.Add(type);
                    }
                }
            }
            var menu = new GenericMenu();
            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => onSelect(type));
            }
            menu.ShowAsContext();
        }
    }
}

