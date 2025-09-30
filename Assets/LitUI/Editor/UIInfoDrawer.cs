//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:26:14
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UFrame.LitUI
{
    [CustomPropertyDrawer(typeof(UIInfo), true)]
    public class UIInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 设置固定宽度
            var labelWidth = 10;
            var variableWidth = 20;
            var popWidth = 80;

            position.width -= variableWidth;

            // 开始一个新的属性绘制块
            EditorGUI.BeginProperty(position, label, property);

            // 标识每个字段的名前缀
            string[] fieldNames = { "name", "guid", "layer","modify", "priority", "mutex" };

            // 计算每个区域的初始位置和尺寸
            var singleFieldHeight = position.height;
            var currentX = position.x;
            var forceMutex = false;

            foreach (var fieldName in fieldNames)
            {
                // 绘制空白标签，鼠标悬停时显示字段名称
                Rect labelRect = new Rect(currentX, position.y, labelWidth, singleFieldHeight);
                EditorGUI.LabelField(labelRect, new GUIContent("|", fieldName),EditorStyles.miniBoldLabel);
                currentX += labelWidth;  // 移动画布位置
                float width = variableWidth;
                if(fieldName == "layer" || fieldName == "modify")
                {
                    width = popWidth;

                }
                else if (fieldName == "name" || fieldName == "guid")
                {
                    width = (position.width - variableWidth * 2 - popWidth *2 - labelWidth * 6) / 2;
                }
                SerializedProperty fieldProp = property.FindPropertyRelative(fieldName);
                // 绘制实际字段
                Rect fieldRect = new Rect(currentX, position.y, width, singleFieldHeight);
                if (fieldName == "guid")
                {
                    var path = AssetDatabase.GUIDToAssetPath(fieldProp.stringValue);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var target = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        using (var changeCheck = new EditorGUI.ChangeCheckScope())
                        {
                            target = EditorGUI.ObjectField(fieldRect, target, typeof(GameObject), false) as GameObject;
                            if (changeCheck.changed)
                            {
                                path = AssetDatabase.GetAssetPath(target);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    var guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
                                    if (!string.IsNullOrEmpty(guid))
                                        fieldProp.stringValue = guid;
                                }
                            }
                        }
                    }
                }
                else if(fieldName == "layer")
                {
                    var layerId = fieldProp.intValue;
                    int newLayerId = 1;
                    while(UISetting.Instance.layers.Count <= layerId)
                        UISetting.Instance.layers.Add("NewLayer" + (newLayerId++));
                    fieldProp.intValue = EditorGUI.Popup(fieldRect,layerId, UISetting.Instance.layers.ToArray());
                    if(fieldProp.intValue == 0 && UISetting.Instance.forceMutexFirstLayer)
                    {
                        forceMutex = true;
                    }
                }
                else if (fieldName == "modify")
                {
                    var layerId = fieldProp.intValue;
                    CheckModify(layerId);
                    if(UISetting.Instance.modifys.Count > 0)
                        fieldProp.intValue = EditorGUI.MaskField(fieldRect, layerId, UISetting.Instance.modifys.Select(x=>x.name).ToArray());
                }
                else if (fieldName == "mutex" && forceMutex)
                {
                    fieldProp.boolValue = true;
                    using(var disable = new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.PropertyField(fieldRect, fieldProp, GUIContent.none);
                    }
                }
                else
                {
                    EditorGUI.PropertyField(fieldRect, fieldProp, GUIContent.none);
                }
                currentX += width + 4;  // 加上间距
            }
            //drag
            DrawAcceptDrag(position, property);
            // 结束属性绘制块
            EditorGUI.EndProperty();

            var labelPos = new Rect(position.x - 20, position.y, 40, position.height);
            if (labelPos.Contains(Event.current.mousePosition) && Event.current.button == 1 && Event.current.type == EventType.MouseUp)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Show"), false, (x) =>
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(property.FindPropertyRelative("guid").stringValue));
                    if (prefab)
                    {
                        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                        if (Selection.activeTransform)
                        {
                            instance.transform.SetParent(Selection.activeTransform, false);
                        }
                    }
                }, 0);
                menu.ShowAsContext();
            }
        }

        /// <summary>
        /// 校验modify
        /// </summary>
        /// <param name="layerId"></param>
        private void CheckModify(int layerId)
        {
            if (layerId == -1 || layerId == 0)
                return;

            for (int i = 0; i < 32; i++) // 对于一个int类型总共有32位
            {
                // 检查第i位是否被设置：(layerId & (1 << i)) != 0
                if ((layerId & (1 << i)) != 0)
                {
                    if(UISetting.Instance.modifys.Count <= i)
                    {
                        UISetting.Instance.modifys.Add( new UISetting.Modify() { name= "Modify" + i });
                    }
                }
            }
        }

        private void DrawAcceptDrag(Rect rect, SerializedProperty property)
        {
            if (!rect.Contains(Event.current.mousePosition))
                return;

            if (Event.current.type == EventType.DragUpdated)
            {
                if (DragAndDrop.objectReferences != null)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.objectReferences.Length < 1)
                    return;

                var go = DragAndDrop.objectReferences[0];
                if (go)
                {
                    property.FindPropertyRelative("name").stringValue = go.name;
                    if (PrefabUtility.IsPartOfPrefabInstance(go))
                        go = PrefabUtility.GetCorrespondingObjectFromSource(go);
                    property.FindPropertyRelative("guid").stringValue = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(go)).ToString();
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
