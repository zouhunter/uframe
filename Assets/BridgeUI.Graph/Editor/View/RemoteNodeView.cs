
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UnityEditor;
using UFrame.NodeGraph.DefultSkin;

namespace UFrame.BridgeUI.Editors
{
    [CustomNodeViewAttribute(typeof(Graph.RemoteNode))]
    public class RemoteNodeView : BaseSkinNodeView
    {
        public override string Category
        {
            get
            {
                return "remote";
            }
        }
        public override void OnInspectorGUI(NodeGUI gui)
        {
            base.OnInspectorGUI(gui);
            gui.Name = EditorGUILayout.TextField("Name", gui.Name);
        }
    }

}