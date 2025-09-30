/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 节点gui                                                                         *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace UFrame.NodeGraph
{
    [CustomEditor(typeof(NodeGUIInspectorHelper))]
    public class NodeGUIEditor : Editor
    {
        protected override void OnHeaderGUI()
        {
        }
        public override bool RequiresConstantRepaint()
        {
            return true;
        }
        public override void OnInspectorGUI()
        {
            var currentTarget = (NodeGUIInspectorHelper)target;
            var node = currentTarget.Node;

            if (node == null)
            {
                return;
            }

            UnityEngine.Assertions.Assert.IsNotNull(node);

            node.DrawNodeGUI(this);

            var errors = currentTarget.Errors;
            if (errors != null && errors.Any())
            {
                foreach (var error in errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }
        public string DrawFolderSelector(string label,
            string dialogTitle,
            string currentDirPath,
            string directoryOpenPath,
            Func<string, string> onValidFolderSelected = null)
        {
            string newDirPath;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (string.IsNullOrEmpty(label))
                {
                    newDirPath = EditorGUILayout.TextField(currentDirPath);
                }
                else
                {
                    newDirPath = EditorGUILayout.TextField(label, currentDirPath);
                }

                if (GUILayout.Button("Select", GUILayout.Width(50f)))
                {
                    var folderSelected =
                        EditorUtility.OpenFolderPanel(dialogTitle, directoryOpenPath, "");
                    if (!string.IsNullOrEmpty(folderSelected))
                    {
                        if (onValidFolderSelected != null)
                        {
                            newDirPath = onValidFolderSelected(folderSelected);
                        }
                        else
                        {
                            newDirPath = folderSelected;
                        }
                    }
                }
            }
            return newDirPath;
        }
    }
}