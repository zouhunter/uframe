using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.EasyBT
{
    [InitializeOnLoad]
    public class NodeHierarchyDrawer
    {
        static NodeHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        }

        static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            if (!Application.isPlaying)
                return;

            var objItem = EditorUtility.InstanceIDToObject(instanceID);
            if(objItem is GameObject objGo)
            {
                var bhTask = objGo.GetComponent<BaseNode>();
                var groupTask = objGo.GetComponent<ConditionGroupNode>();
                if (groupTask)
                    bhTask = groupTask;
                if(bhTask)
                {
                    var backgroundColor = GUI.backgroundColor;
                    switch (bhTask.Status)
                    {
                        case NodeStatus.Inactive:
                            GUI.backgroundColor = Color.gray;
                            GUI.Box(selectionRect, "");
                            break;
                        case NodeStatus.Running:
                            GUI.backgroundColor = Color.green;
                            selectionRect.width *= 0.5f;
                            GUI.Box(selectionRect,"");
                            break;
                        case NodeStatus.Failure:
                            GUI.backgroundColor = Color.red;
                            GUI.Box(selectionRect, "");
                            break;
                        case NodeStatus.Success:
                            GUI.backgroundColor = Color.yellow;
                            GUI.Box(selectionRect, "");
                            break;
                        default:
                            break;
                    }
                    GUI.backgroundColor = backgroundColor;
                }
            }
        }
    }
}
