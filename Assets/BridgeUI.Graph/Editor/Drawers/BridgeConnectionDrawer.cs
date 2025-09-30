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
using UnityEditor;
using UFrame.BridgeUI;
using System;
namespace UFrame.BridgeUI.Editors
{
    [CustomEditor(typeof(Graph.BridgeConnection))]
    public class BridgeConnectionDrawer : Editor
    {
        private Graph.BridgeConnection connecton;
        protected const float buttonWidth = 20;
        private void OnEnable()
        {
            if(target == null)
            {
                DestroyImmediate(this);
                return;
            }
            target.name = target.GetType().FullName;
        }
        protected override void OnHeaderGUI() { }

        public override void OnInspectorGUI()
        {
            connecton = target as Graph.BridgeConnection;
            DrawTitle(connecton.name);
            DrawHead("通道堵塞");
            connecton.blocking = DrawToggle(connecton.blocking, "执行时跳过这个路径");
            DrawHead("自动打开");
            connecton.show.auto = DrawToggle(connecton.show.auto, "更随上级同步打开");
            DrawHead("独立显示");
            connecton.show.single = DrawToggle(connecton.show.single, "只显示当前界面（关闭其他）");
            DrawHead("界面互斥");
            DrawMutexRules();
            DrawHead("父级变化");
            DrawBaseShow();
            //DrawHead("数据模型");
            //DrawViewModel();
        }

        private void DrawTitle(string title)
        {
            var position = GUILayoutUtility.GetRect(BridgeUI.Editors.BridgeEditorUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 1.5f);
            GUI.color = Color.green;
            GUI.Box(position, "", EditorStyles.miniButton);
            GUI.color = Color.white;
            EditorGUI.LabelField(position, string.Format("【{0}】", title), EditorStyles.largeLabel);
        }

        private void DrawHead(string label)
        {
            EditorGUILayout.LabelField("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - ");
            var rect = GUILayoutUtility.GetRect(BridgeUI.Editors.BridgeEditorUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 1.1f);
            GUI.color = Color.gray;
            GUI.Box(rect, "", EditorStyles.miniButton);
            GUI.color = Color.white;
            EditorGUI.LabelField(rect, string.Format("【{0}】", label), EditorStyles.largeLabel);
        }
     

        string[] mutexRules;
        string[] mutexRulesNotice = { "不排斥", "排斥同父级中的同层级", "排斥同层级" };
        int mutexRulesSelected;
        private void DrawMutexRules()
        {
            if (mutexRules == null)
            {
                mutexRules = System.Enum.GetNames(typeof(MutexRule));
            }

            mutexRulesSelected = System.Array.IndexOf(mutexRules, connecton.show.mutex.ToString());

            for (int i = 0; i < mutexRules.Length; i++)
            {
                var isOn = EditorGUILayout.ToggleLeft(string.Format("{0}--{1}", mutexRules[i], mutexRulesNotice[i]), mutexRulesSelected == i);
                if (isOn)
                {
                    connecton.show.mutex = (MutexRule)i;
                    mutexRulesSelected = i;
                }
            }
        }


        string[] baseShows;
        string[] baseShowsNotice = { "不改变父级状态", "隐藏父级(在本面板关闭时打开)", "销毁父级(接管因为父级面关闭的面板)" };
        int baseShowsSelected;
        private void DrawBaseShow()
        {
            if (baseShows == null)
            {
                baseShows = System.Enum.GetNames(typeof(ParentShow));
            }

            baseShowsSelected = System.Array.IndexOf(baseShows, connecton.show.baseShow.ToString());

            for (int i = 0; i < mutexRules.Length; i++)
            {
                var isOn = EditorGUILayout.ToggleLeft(string.Format("{0} --{1}", baseShows[i], baseShowsNotice[i]), baseShowsSelected == i);
                if (isOn)
                {
                    connecton.show.baseShow = (ParentShow)i;
                    baseShowsSelected = i;
                }
            }
        }

        private bool DrawToggle(bool on, string tip)
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                on = EditorGUILayout.ToggleLeft(" --" + tip, on);
            }
            return on;
        }

        //private void DrawViewModel()
        //{
        //    using (var hor = new EditorGUILayout.HorizontalScope())
        //    {
        //        var model = EditorGUILayout.ObjectField(new GUIContent("IViewModel"), connecton.viewModel, typeof(ScriptableObject), false) ;

        //        if(model != connecton.viewModel && model is Binding.IViewModel)
        //        {
        //            connecton.viewModel = model as ScriptableObject;
        //        }

        //        if(model == null)
        //        {
        //            connecton.viewModel = null;
        //        }

        //        if (GUILayout.Button("new", EditorStyles.miniButtonRight, GUILayout.Width(60)))
        //        {
        //            MvvmUtil.CreateNewViewModel((viewModel) =>
        //            {
        //                connecton.viewModel = viewModel;
        //            });

        //        }
        //    }
        //}
    }

}