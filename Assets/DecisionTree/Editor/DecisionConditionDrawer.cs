//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-06-04
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System;
using UFrame.BehaviourTree;
using System.Collections.Generic;

namespace UFrame.Decision
{

    public class DecisionConditionDrawer
    {
        public bool changed;
        public void OnGUI(Rect conditionRect, DecisionTreeNode node)
        {
            using (var checkChangeScope = new EditorGUI.ChangeCheckScope())
            {
                conditionRect = new Rect(conditionRect.x, conditionRect.y, conditionRect.width, conditionRect.height);
                float padding = 4f;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float x = conditionRect.x + padding;
                float width = conditionRect.width - 2 * padding;
                float iconWidth = 20f;
                GUIContent iconContent = EditorGUIUtility.IconContent("console.infoicon");
                var iconRect = new Rect(x, conditionRect.y, iconWidth, lineHeight);
                if (GUI.Button(iconRect, iconContent))
                {
                    CreateNewCondition(node);
                }
                if (node.condition == null)
                {
                    x += iconWidth + padding;
                    if (GUI.Button(new Rect(x, conditionRect.y, width - iconWidth - padding, lineHeight), "Null", EditorStyles.textField))
                    {
                        CreateNewCondition(node);
                    }
                    return;
                }
                var condition = node.condition;
                // key
                float keyWidth = width * 0.35f - iconWidth;
                float compireWidth = 20f;
                float valueWidth = 40;
                if (condition is DecisionBoolCondition)
                {
                    valueWidth = 20f;
                }
                x += iconWidth + padding;
                // key输入框
                var keyRect = new Rect(x, conditionRect.y, keyWidth, lineHeight);
                condition.key = EditorGUI.TextField(keyRect, condition.key);
                x += keyWidth + padding;
                // checkCompire
                var compireRect = new Rect(x, conditionRect.y, compireWidth, lineHeight);
                string[] compireSymbols = new string[] { "=", ">", "<", ">=", "!=", "<=" };
                int compireIndex = (int)condition.checkCompire;
                int newCompireIndex = EditorGUI.Popup(compireRect, compireIndex, compireSymbols, EditorStyles.largeLabel);
                condition.checkCompire = (CompireType)newCompireIndex;
                x += compireWidth + padding;
                // Value字段（反射）
                var valueRect = new Rect(x, conditionRect.y, valueWidth, lineHeight);
                var valueField = condition.GetType().GetField("value");
                if (valueField != null)
                {
                    object value = valueField.GetValue(condition);
                    Type valueType = valueField.FieldType;
                    if (valueType == typeof(int))
                    {
                        int newValue = EditorGUI.IntField(valueRect, value != null ? (int)value : 0);
                        valueField.SetValue(condition, newValue);
                    }
                    else if (valueType == typeof(float))
                    {
                        float newValue = EditorGUI.FloatField(valueRect, value != null ? (float)value : 0f);
                        valueField.SetValue(condition, newValue);
                    }
                    else if (valueType == typeof(bool))
                    {
                        bool newValue = EditorGUI.Toggle(valueRect, value != null ? (bool)value : false);
                        valueField.SetValue(condition, newValue);
                    }
                    else if (valueType == typeof(string))
                    {
                        string newValue = EditorGUI.TextField(valueRect, value != null ? (string)value : "");
                        valueField.SetValue(condition, newValue);
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                    {
                        UnityEngine.Object newValue = EditorGUI.ObjectField(valueRect, value as UnityEngine.Object, valueType, true);
                        valueField.SetValue(condition, newValue);
                    }
                    else
                    {
                        EditorGUI.LabelField(valueRect, value != null ? value.ToString() : "null");
                    }
                }
                else
                {
                    EditorGUI.LabelField(valueRect, "Any!");
                }
                x += valueWidth + padding;
                // desc描述信息
                float descWidth = width - (keyWidth + compireWidth + valueWidth + padding * 4);
                var descRect = new Rect(x + 10, conditionRect.y, descWidth - 20, lineHeight);
                var leftParenRect = new Rect(x, conditionRect.y, 10, lineHeight);
                var rightParenRect = new Rect(x + descWidth - 10, conditionRect.y, 10, lineHeight);
                EditorGUI.LabelField(leftParenRect, "(", EditorStyles.miniLabel);
                condition.desc = EditorGUI.TextField(descRect, condition.desc, EditorStyles.textField);
                EditorGUI.LabelField(rightParenRect, ")", EditorStyles.miniLabel);
                x += descWidth + padding;
                changed = checkChangeScope.changed;
            }
        }

        private void CreateNewCondition(DecisionTreeNode node)
        {
            ShowConditionTypeMenu(type =>
            {
                var newCondition = Activator.CreateInstance(type) as DecisionCondition;
                if (node.condition != null)
                {
                    newCondition.desc = node.condition.desc;
                    newCondition.key = node.condition.key;
                }
                node.condition = newCondition;
                changed = true;
            });
        }

        // 抽取菜单弹出逻辑
        private void ShowConditionTypeMenu(Action<Type> onSelect)
        {
            var baseType = typeof(DecisionCondition);
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (baseType.IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
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

