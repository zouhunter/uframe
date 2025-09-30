using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.BehaviourTree
{
    [CustomEditor(typeof(TreeBehaviourRunner), true)]
    public class TreeBehaviourRunnerDrawer : Editor
    {
        private SerializedProperty _treeProp;
        private Editor _treeDrawer;
        private bool _dynimic;

        private void OnEnable()
        {
            _treeProp = serializedObject.FindProperty("_btInstance");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (_treeDrawer == null && _treeProp != null && _treeProp.objectReferenceValue != null)
            {
                _treeDrawer = Editor.CreateEditor(_treeProp.objectReferenceValue);
                _dynimic = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_treeDrawer.target));
            }

            if (_treeDrawer && _treeDrawer.target)
            {
                _treeDrawer.OnInspectorGUI();
            }
        }
    }
}
