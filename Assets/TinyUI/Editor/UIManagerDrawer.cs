//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-04-06 09:52:33
//* 描    述：

//* ************************************************************************************
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.TinyUI
{
    [CustomEditor(typeof(UIManager))]
    public class UIManagerDrawer : Editor
    {
        private ReorderableList panelItemsDrawer;
        private ReorderableList ruleListDrawer;

        private List<Rule> ruleList;
        private List<PanelItem> panelItemList;
        private UIManager manager;
        private bool isEditRule;
        private SerializedProperty scriptProp;

        private void OnEnable()
        {
            manager = target as UIManager;
            scriptProp = serializedObject.FindProperty("m_Script");
            ruleList = new List<Rule>(GetRules());
            panelItemList = new List<PanelItem>(GetPanelItems());

            panelItemsDrawer = new ReorderableList(panelItemList, typeof(PanelItem));
            panelItemsDrawer.drawHeaderCallback = DrawPanelItemsHead;
            panelItemsDrawer.elementHeight = EditorGUIUtility.singleLineHeight + 10;
            panelItemsDrawer.drawElementCallback = DrawPanelItemDetail;

            ruleListDrawer = new ReorderableList(ruleList, typeof(Rule));
            ruleListDrawer.drawHeaderCallback = DrawRuleListHead;
            ruleListDrawer.elementHeight = EditorGUIUtility.singleLineHeight + 10;
            ruleListDrawer.drawElementCallback = DrawRuleItemDetail;
        }


        #region GUI
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(scriptProp);
            EditorGUI.EndDisabledGroup();
            serializedObject.Update();
            if (isEditRule)
            {
                ruleListDrawer.DoLayoutList();
            }
            else
            {
                panelItemsDrawer.DoLayoutList();
            }
            SaveToBehaiver();
            serializedObject.ApplyModifiedProperties();
        }
        private void DrawPanelItemsHead(Rect rect)
        {
            var btn0Rect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
            var btn1Rect = new Rect(btn0Rect.x - 70, rect.y, 60, rect.height);

            if (GUI.Button(btn0Rect, "检测"))
            {
                CheckPrefabs();
            }

            if (GUI.Button(btn1Rect, "关联"))
            {
                isEditRule = true;
            }

            EditorGUI.LabelField(rect, "预制体列表");
        }

        private void DrawRuleListHead(Rect rect)
        {
            var btn0Rect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
            if (GUI.Button(btn0Rect, "完成"))
            {
                if (CheckRules())
                {
                    isEditRule = false;
                }
            }
            EditorGUI.LabelField(rect, "面板关联列表");
        }


        private void DrawPanelItemDetail(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = panelItemList[index];
            var boxRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10);
            GUI.Box(boxRect, "");

            var idRect = new Rect(rect.x - 10, rect.y + 4, 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(idRect, index.ToString("00"));

            var fieldWidth = (boxRect.width - 40);
            var nameRect = new Rect(boxRect.x, boxRect.y, fieldWidth * 0.4f, boxRect.height);
            item.panelName = EditorGUI.TextField(nameRect, item.panelName);
            var objectRect = new Rect(boxRect.x + nameRect.width + 5, boxRect.y, fieldWidth * 0.5f, boxRect.height);
            item.prefab = EditorGUI.ObjectField(objectRect, item.prefab, typeof(GameObject), true) as GameObject;
            var btnRect = new Rect(boxRect.x + boxRect.width - 40, boxRect.y, 40, boxRect.height);

            if (item.prefab != null)
            {
                if (GUI.Button(btnRect, "o", EditorStyles.miniButtonRight))
                {
                    var instence = PrefabUtility.InstantiatePrefab(item.prefab) as GameObject;
                    instence.transform.SetParent(manager.transform, false);
                }
            }
        }

        private void DrawRuleItemDetail(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = ruleList[index];
            var boxRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10);
            GUI.Box(boxRect, "");

            var idRect = new Rect(rect.x - 10, rect.y + 4, 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(idRect, index.ToString("00"));

            var fieldWidth = (boxRect.width - 200);
            var name1Rect = new Rect(boxRect.x, boxRect.y, fieldWidth * 0.4f, boxRect.height);
            item.from_panel = EditorGUI.TextField(name1Rect, item.from_panel);
            var toRect = new Rect(name1Rect.x + name1Rect.width, name1Rect.y, 20, name1Rect.height);
            EditorGUI.LabelField(toRect, "→");
            var name2Rect = new Rect(boxRect.x + name1Rect.width + 20, boxRect.y, fieldWidth * 0.4f, boxRect.height);
            item.to_panel = EditorGUI.TextField(name2Rect, item.to_panel);

            var propRectA = new Rect(boxRect.x + fieldWidth, boxRect.y, 60, boxRect.height);
            var propRectB = new Rect(propRectA.x + 65, propRectA.y, propRectA.width, propRectA.height);
            var propRectC = new Rect(propRectB.x + 65, propRectA.y, propRectA.width, propRectA.height);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(item.from_panel));
            item.auto = EditorGUI.ToggleLeft(propRectA, new GUIContent("Auto", "子面板自动打开"), item.auto);
            EditorGUI.EndDisabledGroup();

            item.hide = EditorGUI.ToggleLeft(propRectB, new GUIContent("Hide", "父级面板隐藏"), item.hide);
            item.mutix = EditorGUI.ToggleLeft(propRectC, new GUIContent("Mutix", "兄弟面板互斥"), item.mutix);
        }

        #endregion
        private void SaveToBehaiver()
        {
            manager.GetType().GetField("bridges", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 .SetValue(manager, ruleList.ToArray());
            manager.GetType().GetField("panelItems", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 .SetValue(manager, panelItemList.ToArray());
            EditorUtility.SetDirty(manager);
        }
        private Rule[] GetRules()
        {
            var array = manager.GetType().GetField("bridges", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 .GetValue(manager) as Rule[];
            if (array == null) array = new Rule[0];
            return array;
        }
        private PanelItem[] GetPanelItems()
        {
            var array = manager.GetType().GetField("panelItems", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 .GetValue(manager) as PanelItem[];
            if (array == null) array = new PanelItem[0];
            return array;
        }
        private bool CheckRules()
        {
            var ok = true;
            var keyset = new HashSet<string>();
            for (int i = 0; i < ruleList.Count; i++)
            {
                var rule = ruleList[i];
                string key = rule.to_panel;
                if (!string.IsNullOrEmpty(rule.from_panel))
                {
                    key = rule.from_panel + "." + rule.to_panel;
                }

                if (keyset.Contains(key))
                {
                    ok = false;
                    Debug.LogErrorFormat("关键字重复：{0} from:{1},to:{2}", i, rule.from_panel, rule.to_panel);
                }

                keyset.Add(key);
            }
            return ok;
        }
        private void CheckPrefabs()
        {
            var keyset = new HashSet<string>();
            for (int i = 0; i < panelItemList.Count; i++)
            {
                var item = panelItemList[i];
                if (keyset.Contains(item.panelName))
                {
                    Debug.LogErrorFormat("名称重复:{0}.{1}", i, item.panelName);
                }
                keyset.Add(item.panelName);

                if (item.prefab == null)
                {
                    Debug.LogErrorFormat("预制体为空:{0}.{1}", i, item.panelName);
                }
            }
        }
    }
}