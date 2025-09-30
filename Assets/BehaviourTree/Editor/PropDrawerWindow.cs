using UnityEngine;
using UnityEditor;

namespace UFrame.BehaviourTree
{
    public class PropDrawerWindow : EditorWindow
    {
        private SerializedProperty prop;
        private SerializedProperty _scriptPorp;

        public static void Show(SerializedProperty prop)
        {
            var window = GetWindow<PropDrawerWindow>("节点详细数据");
            prop.isExpanded = true;
            window.prop = prop;
            window.ShowPopup();
        }
        private void OnGUI()
        {
            if (prop != null && prop.serializedObject.targetObject)
            {
                prop.serializedObject.Update();
                var nameProp = prop.FindPropertyRelative("name");
                EditorGUILayout.PropertyField(prop, new GUIContent(nameProp.stringValue), true);
                prop.serializedObject.ApplyModifiedProperties();
                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel(prop.managedReferenceFullTypename);
            }
        }
        void OnDisable()
        {
            if (prop != null && prop.serializedObject.targetObject)
                prop.isExpanded = false;
        }
    }
}
