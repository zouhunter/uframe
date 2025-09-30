//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:26:14
//* 描    述：

//* ************************************************************************************
using UnityEditor;

namespace UFrame.LitUI
{
    [CustomEditor(typeof(UIGroupBehaviour))]
    public class UIGroupBehaviourDrawer : Editor
    {
        private SerializedProperty _groupProp;
        private SerializedProperty _scriptProp;
        private Editor _groupDrawer;

        private void OnEnable()
        {
            if (!target)
                return;
            _groupProp = serializedObject.FindProperty("group");
            _scriptProp = serializedObject.FindProperty("m_Script");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (var disable = new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_scriptProp);
            EditorGUILayout.PropertyField(_groupProp);
            if(_groupProp.objectReferenceValue)
            {
                if(_groupDrawer == null || _groupProp.objectReferenceValue != _groupDrawer.target)
                    _groupDrawer = Editor.CreateEditor(_groupProp.objectReferenceValue);
                _groupDrawer.OnInspectorGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
