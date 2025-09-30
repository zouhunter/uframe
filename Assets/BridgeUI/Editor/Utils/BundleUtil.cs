using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEditor;
using UFrame.BridgeUI;
using System.Linq;

namespace UFrame.BridgeUI.Editors
{
    public static class BundleUtil
    {
        private static List<Type> subRules;
        private static GUIContent[] options;

        public static void CreateNewBundleCreateRule(UnityAction<ScriptableObject> onCreate)
        {
            InitEnviroments();
            EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.zero), options, -1, OnSelect, onCreate);
        }

        private static void OnSelect(object userData, string[] options, int selected)
        {
            if (selected >= 0 && options.Length > 0)
            {
                var type = subRules[selected];
                BridgeUI.Editors.BridgeEditorUtility.CreateAssets(type, userData as UnityAction<ScriptableObject>);
                
            }
        }

        private static void InitEnviroments()
        {
            if (subRules == null)
            {
                subRules = BridgeUI.Editors.BridgeEditorUtility.GetSubInstenceTypes(typeof(UILoader));
            }

            if (options == null)
            {
                options = (from model in subRules
                           select new GUIContent(model.FullName)).ToArray();
            }

        }

    }
}