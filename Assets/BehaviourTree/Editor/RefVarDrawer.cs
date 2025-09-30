/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: 变量绘制
 *_*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;

namespace UFrame.BehaviourTree
{
    [CustomPropertyDrawer(typeof(Ref<>))]
    public class RefVarDrawer : PropertyDrawer
    {
        private SerializedProperty keyProp;
        private SerializedProperty autoCreateProp;
        private SerializedProperty defaultProp;
        private int _index = 0;
        private System.Type _valueType;
        private void FindProperties(SerializedProperty property)
        {
            keyProp = property.FindPropertyRelative("_key");
            autoCreateProp = property.FindPropertyRelative("_autoCreate");
            defaultProp = property.FindPropertyRelative("_default");
        }

        private void SelectIndex(SerializedProperty property)
        {
            var contentObj = GetContentObject(property.serializedObject.targetObject, property.propertyPath);
            var subProps = contentObj.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
            var index = 0;
            foreach (var item in subProps)
            {
                if (!typeof(IRef).IsAssignableFrom(item.FieldType))
                    continue;

                _valueType = item.FieldType.GetGenericArguments()[0];

                index++;
                if (property.propertyPath.EndsWith(item.Name))
                {
                    this._index = index;
                    break;
                }
            }
        }

        private object GetContentObject(UnityEngine.Object target, string propertyPath)
        {
            Type type = target.GetType();
            string[] pathParts = propertyPath.Split('.');
            FieldInfo field = null;
            object content = target;

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                if (pathParts[i] == "Array")
                {
                    if (i + 1 < pathParts.Length && pathParts[i + 1].StartsWith("data["))
                    {
                        i++;
                        if (field != null)
                        {
                            if (content != null)
                            {
                                var id = int.Parse(pathParts[i].Replace("data[", "").Replace("]", ""));
                                if (content is IList)
                                {
                                    MethodInfo getItemMethod = content.GetType().GetProperty("Item").GetGetMethod();
                                    content = getItemMethod.Invoke(content, new object[] { id });
                                    type = content.GetType();
                                }
                                else if (content is Array)
                                {
                                    content = (content as Array).GetValue(id);
                                    type = content.GetType();
                                }
                            }
                            else
                            {
                                type = field.FieldType.GetElementType();
                            }
                        }
                    }
                }
                else
                {
                    field = type?.GetField(pathParts[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                    if (field != null)
                    {
                        content = field.GetValue(content);

                        type = field.FieldType;
                        if (type.IsArray)
                        {
                            type = type.GetElementType();
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                    }
                    else
                    {
                        return content;
                    }
                }
            }

            return content;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindProperties(property);
            var height = EditorGUIUtility.singleLineHeight + 12;
            if (property.isExpanded && defaultProp != null)
            {
                height += EditorGUI.GetPropertyHeight(defaultProp);
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindProperties(property);
            SelectIndex(property);

            // 计算区域
            float bgHeight = EditorGUIUtility.singleLineHeight + (property.isExpanded && defaultProp != null ? EditorGUI.GetPropertyHeight(defaultProp) + 16 : 12);
            var bgRect = new Rect(position.x, position.y, position.width, bgHeight);

            // 1. 先画背景（不要用 GUI.Box，避免遮挡Foldout）
            //EditorGUI.DrawRect(bgRect, property.isExpanded ? new Color(0.18f, 0.22f, 0.32f, 0.95f) : new Color(0.13f, 0.15f, 0.18f, 0.85f));
            //鼠标悬停高亮
            if (bgRect.Contains(Event.current.mousePosition))
                EditorGUI.DrawRect(bgRect, new Color(0.25f, 0.35f, 0.5f, 0.08f));

            // 2. Foldout区域
            var foldRect = new Rect(position.x - 4, position.y + 3, 120, 20);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);

            // 3. 其它内容
            var nameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = property.isExpanded ? new Color(0.9f, 0.95f, 1f) : new Color(0.7f, 0.8f, 1f) }
            };
            var typeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.5f, 0.8f, 0.9f) }
            };

            // 变量名、ID、Key输入框
            float leftStart = position.x + 22;
            float nameWidth = 90;
            float idWidth = 40;
            float keyWidth = position.width - 280;
            float spacing = -16;

            var nameRect = new Rect(leftStart, position.y + 4, nameWidth, EditorGUIUtility.singleLineHeight);
            GUI.Label(nameRect, $"{_index}. {label.text}", nameStyle);

            var idRect = new Rect(nameRect.xMax, nameRect.y, idWidth, nameRect.height);
            EditorGUI.LabelField(idRect, "ID:", EditorStyles.miniBoldLabel);

            var keyRect = new Rect(idRect.xMax + spacing, idRect.y, keyWidth, idRect.height);
            keyProp.stringValue = EditorGUI.TextField(keyRect, keyProp.stringValue);

            // 类型名紧跟在keyProp后面，间隔20像素
            string typeName = defaultProp != null ? defaultProp.type : (_valueType != null ? _valueType.Name : "");
            float typeWidth = 110;
            var typeRect = new Rect(position.x + position.width - typeWidth - 80, keyRect.y, typeWidth, keyRect.height);
            GUI.Label(typeRect, $"({typeName})", typeStyle);

            // Ensure/Toggle区域
            float rightPadding = -10;
            float toggleWidth = 20;
            float ensureLabelWidth = 48;

            float toggleX = position.x + position.width + rightPadding - 30;
            float ensureLabelX = toggleX - ensureLabelWidth;

            var autoRect = new Rect(ensureLabelX, idRect.y, 60, idRect.height);

            using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(keyProp.stringValue)))
            {
                var autoCreateRect = new Rect(ensureLabelX, autoRect.y, ensureLabelWidth + toggleWidth, autoRect.height);
                autoCreateProp.boolValue = EditorGUI.ToggleLeft(autoCreateRect, new GUIContent("Ensure:", "create if not exists!"), autoCreateProp.boolValue, EditorStyles.miniLabel);
            }

            // 展开内容
            if (property.isExpanded && defaultProp != null)
            {
                var propRect = new Rect(position.x + 16, position.y + EditorGUIUtility.singleLineHeight + 8, position.width - 32, EditorGUI.GetPropertyHeight(defaultProp));
                using (new EditorGUI.DisabledGroupScope(!string.IsNullOrEmpty(keyProp.stringValue)))
                {
                    float originalLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUI.PropertyField(propRect, defaultProp, true);
                    EditorGUIUtility.labelWidth = originalLabelWidth;
                }
            }
        }
    }
}

