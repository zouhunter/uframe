//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-06-04
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace UFrame.Decision
{
    [CustomEditor(typeof(DecisionTree))]
    public class DecisionTreeEditor : Editor
    {
        protected SerializedProperty _scriptProp;
        protected DecisionTreeNode _rootNode;
        public bool drawScript { get; set; } = true;
        private DecisionTreeNodeDrawer _treeInfoDrawer;
        private DecisionTree _tree;
        protected virtual void OnEnable()
        {
            _tree = target as DecisionTree;
            if (!_tree)
                return;
            _tree.rootNode = _tree.rootNode ?? new DecisionRootNode();
            _scriptProp = serializedObject.FindProperty("m_Script");
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            RebuildView();
        }
        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            if (EditorApplication.isPlaying)
                return;

            if (_tree)
            {
                Undo.RecordObject(_tree, "tree disabled!");
                EditorUtility.SetDirty(_tree);
            }
        }

        public override void OnInspectorGUI()
        {
            if (_rootNode != _tree.rootNode && _tree.rootNode != null)
            {
                RebuildView();
            }
            if (drawScript)
            {
                if (_scriptProp == null || _scriptProp.objectReferenceValue == null)
                    return;

                using (var disable = new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(_scriptProp);
                }
            }
            serializedObject.Update();
            DrawNTreeHeader();
            DrawTreeView();
            GUILayout.Space(8);
            serializedObject.ApplyModifiedProperties();
        }


        public void RebuildView()
        {
            _rootNode = _tree.rootNode;
            _treeInfoDrawer = new DecisionTreeNodeDrawer(_tree, null, (target as DecisionTree).rootNode, OnChangeRootNode);
        }

        private void OnChangeRootNode(DecisionTreeNode node)
        {
            _tree.rootNode = node;
            RebuildView();
        }

        private void DrawNTreeHeader()
        {
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredStyle.alignment = TextAnchor.MiddleLeft;
            centeredStyle.fontSize = 14; // 设置字体大小
            centeredStyle.richText = true;
            GUIContent content = new GUIContent($"Unity-DesisionTree <size=12><b><color=black>v1.0</color></b></size> <size=12><b>({target.name})</b></size>");

            var lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(lastRect, "");
            var readMeRect = new Rect(lastRect.x + lastRect.width - 60, lastRect.max.y - 20 + EditorGUIUtility.singleLineHeight, 60, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.LinkButton(readMeRect, "README"))
                Application.OpenURL("https://web-alidocs.dingtalk.com/i/nodes/QOG9lyrgJP3BQREofr345nLxVzN67Mw4?doc_type=wiki_doc");

            if (GUILayout.Button(content, centeredStyle))
            {
                DecisionGraphWindow.OpenWindow(target as DecisionTree);
            }

            GUILayout.Box("", GUILayout.Height(3), GUILayout.ExpandWidth(true));
        }

        private void DrawTreeView()
        {
            if (_treeInfoDrawer == null)
                return;
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                var rect = GUILayoutUtility.GetRect(0, _treeInfoDrawer.GetHeight());
                _treeInfoDrawer?.OnInspectorGUI(rect, "");
                if (changeCheck.changed && _treeInfoDrawer.changeTrigger)
                {
                    EditorUtility.SetDirty(_tree);
                }
            }
        }

        private void OnUndoRedoPerformed()
        {
            RebuildView();
        }

        internal void RecordUndo(string flag)
        {
            if (EditorApplication.isPlaying)
                return;

            if (_tree)
            {
                Undo.RecordObject(_tree, flag);
                EditorUtility.SetDirty(_tree);
                EditorApplication.delayCall += RebuildView;
            }
        }
    }

    [CustomPropertyDrawer(typeof(DecisionTree))]
    public class DecisionTreePropDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.width -= 20;
            EditorGUI.ObjectField(position, property, label);
            var btRect = new Rect(position.x + position.width, position.y, 20, position.height);
            //绘制创建按钮
            if (GUI.Button(btRect, EditorGUIUtility.IconContent("d_Toolbar Plus"), EditorStyles.label))
            {
                var tree = ScriptableObject.CreateInstance<DecisionTree>();
                ProjectWindowUtil.CreateAsset(tree, "NewDecisionTree.asset");
                property.objectReferenceValue = Selection.activeObject;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}