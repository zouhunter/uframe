/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-29
 * Version: 1.0.0
 * Description: 节点属性绘制
 *_*/

using NUnit.Framework;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using System.Reflection;
using System.Linq;

namespace UFrame.BehaviourTree
{
    public class ObjectPropDrawer
    {
        public bool expand;
        protected List<SerializedProperty> properties;
        private SerializedObject serializedObject;
        public ObjectPropDrawer(Object node)
        {
            properties = new List<SerializedProperty>();
            serializedObject = new SerializedObject(node);
            var props = node.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var prop in props)
            {
                var serializeProp = serializedObject.FindProperty(prop.Name);
                if (serializeProp != null)
                {
                    properties.Add(serializeProp);
                }
            }
        }

        /// <summary>
        /// 获取属性高度
        /// </summary>
        /// <returns></returns>
        public float GetHeight()
        {
            float height = 8;
            if (expand)
            {
                foreach (var item in properties)
                {
                    height += EditorGUI.GetPropertyHeight(item, true);
                }
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        /// <summary>
        /// 绘制属性
        /// </summary>
        /// <param name="rect"></param>
        public void OnGUI(Rect rect)
        {
            serializedObject.Update();
            GUI.Box(rect, GUIContent.none);
            rect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
            float y = rect.y;
            foreach (var item in properties)
            {
                var height = EditorGUI.GetPropertyHeight(item, true);
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, height), item, true);
                y += height;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
