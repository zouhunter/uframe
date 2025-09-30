using UnityEngine;
using UnityEditor;
using UINodePanel;

[CustomPropertyDrawer(typeof(NodePanelPair)), CanEditMultipleObjects]
public class NodePanelPairDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0 ;
        var nodePanelProp = property.FindPropertyRelative("nodePanel");
        if (nodePanelProp == null||nodePanelProp.objectReferenceValue == null){
            height += EditorGUIUtility.singleLineHeight;
        }
        else 
        {
            height += 2 * EditorGUIUtility.singleLineHeight;
            var obj = new SerializedObject(nodePanelProp.objectReferenceValue);
            var typeProp = obj.FindProperty("openType");

            OpenType type = (OpenType)typeProp.intValue;

            if ((type & OpenType.ByToggle) == OpenType.ByToggle)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            if ((type & OpenType.ByButton) == OpenType.ByButton)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
        }
        
        return height;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;

        var hideSelfProp = property.FindPropertyRelative("hideSelf");
        var openBtnProp = property.FindPropertyRelative("openBtn");
        var openTogProp = property.FindPropertyRelative("openTog");
        var nodePanelProp = property.FindPropertyRelative("nodePanel");

        if (nodePanelProp == null || nodePanelProp.objectReferenceValue == null) {
            EditorGUI.PropertyField(rect, nodePanelProp);
            return;
        }
        else
        {

            EditorGUI.PropertyField(rect, hideSelfProp);

            var obj = new SerializedObject(nodePanelProp.objectReferenceValue);
            var typeProp = obj.FindProperty("openType");

            OpenType type = (OpenType)typeProp.intValue;

            if ((type & OpenType.ByToggle) == OpenType.ByToggle)
            {
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, openTogProp);
            }
            if ((type & OpenType.ByButton) == OpenType.ByButton)
            {
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, openBtnProp);
            }

            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, nodePanelProp);
        }
       
    }
}

[CustomEditor(typeof(NodePanel)), CanEditMultipleObjects]
public class NodePanelDrawer : Editor
{
    SerializedProperty script;

    SerializedProperty openTypeProp;
    SerializedProperty relatedPanelsProp;
    SerializedProperty closeBtnProp;
    private void OnEnable()
    {
        script = serializedObject.FindProperty("m_Script");
        openTypeProp = serializedObject.FindProperty("openType");
        closeBtnProp = serializedObject.FindProperty("closeBtn");
        relatedPanelsProp = serializedObject.FindProperty("relatedPanels");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(script);
        openTypeProp.intValue = EditorGUILayout.MaskField("打开方式",openTypeProp.intValue, openTypeProp.enumNames);
        OpenType type = (OpenType)openTypeProp.intValue;

        if ((type & OpenType.ByName) == OpenType.ByName || (type & OpenType.ByButton) == OpenType.ByButton){
            EditorGUILayout.PropertyField(closeBtnProp);
        }
        EditorGUILayout.PropertyField(relatedPanelsProp);
        serializedObject.ApplyModifiedProperties();
    }
}
