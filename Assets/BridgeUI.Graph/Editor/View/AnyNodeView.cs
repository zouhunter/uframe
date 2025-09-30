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
using UFrame.NodeGraph.DataModel;

namespace UFrame.BridgeUI.Editors
{
    [CustomNodeViewAttribute(typeof(Graph.AnyNode))]
    public class AnyNodeView : BaseSkinNodeView
    {
        public override float SuperHeight
        {
            get
            {
                return -EditorGUIUtility.singleLineHeight * 0.5f;
            }
        }

        public override void OnInspectorGUI(NodeGUI gui)
        {
            base.OnInspectorGUI(gui);
            if (target != null)
            {
                gui.Name = "AnyState";
            }
        }
    }

}