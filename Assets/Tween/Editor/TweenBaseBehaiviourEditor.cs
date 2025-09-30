using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace UFrame.Tween
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TweenBehaviour),true)]
    public class TweenBaseBehaiviourEditor : Editor
    {
        protected SerializedProperty m_scriptPorp;
        protected List<SerializedProperty> m_tweenProps;

        private void OnEnable()
        {
            m_scriptPorp = serializedObject.FindProperty("m_Script");
            m_tweenProps = new List<SerializedProperty>();

            var tweenProp = serializedObject.FindProperty("m_tween");
            var field = target.GetType().GetField("m_tween",BindingFlags.FlattenHierarchy|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.GetField);
            var subFields = field.FieldType.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy|BindingFlags.Instance|BindingFlags.GetField);
            foreach (var item in subFields)
            {
               var subProp = tweenProp.FindPropertyRelative(item.Name); 
                if(subProp != null)
                {
                    m_tweenProps.Add(subProp);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            using (var disableScope = new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_scriptPorp);
            }
            serializedObject.Update();
            foreach (var subProp in m_tweenProps)
            {
                EditorGUILayout.PropertyField(subProp);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}