using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UFrame.BridgeUI.Editors;
using UnityEditorInternal;

namespace UFrame.BridgeUI.Editors
{
    public class BindingWindow : EditorWindow
    {
        private GameObject prefab;
        private const int lableWidth = 60;
        private static int instenceID;
        private int selected;
        private string[] options = { "控件管理", "检视面板" };
        private MonoBehaviour panelCompnent;
        private Editor panelDrawer;
        private GenCodeRule rule;
        private List<ComponentItem> components = new List<ComponentItem>();
        private ReorderableList preComponentList;
        private ComponentItemDrawer itemDrawer = new ComponentItemDrawer();
        private Vector2 scrollPos;
        private bool bindingAble = true;
        private const int saveSecond = 5;
        private int currentSecond = 0;
        protected string m_matchText;
        protected List<ComponentItem> showComponents = new List<ComponentItem>();
        protected List<ReferenceItem> referenceItems = new List<ReferenceItem>();


        [MenuItem("GameObject/Foundation/BridgeUI/UI Binding", false, 10)]
        public static void AddUIBinding()
        {
            if (!Selection.activeGameObject)
                return;
            Selection.activeGameObject.AddComponent<UIBinding>();
        }

        [MenuItem("GameObject/Foundation/BridgeUI/UI Binding", true, 10)]
        public static bool AddUIBindinValid()
        {
            if (!Selection.activeGameObject)
                return false;
            return null == Selection.activeGameObject.GetComponent<IClassReference>();
        }


        [MenuItem("GameObject/Foundation/BridgeUI/Create Binding", false, 10)]
        public static void AddBindingRefrence()
        {
            string typeName = string.Format("{0}.{1}Reference", UISetting.defultNameSpace, Selection.activeGameObject.name);
            Type type = BridgeUI.Utility.FindTypeInAllAssemble(typeName);
            if (type != null)
            {
                Selection.activeGameObject.AddComponent(type);
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(Selection.activeGameObject))
            {
                Debug.LogError("请勿对预制体直接操作！");
                return;
            }
            bool confer = EditorUtility.DisplayDialog("创建BindingRefrence?", "如果需要创建控件引用，请按确认!", "确认");
            if (confer)
            {
                DoCreateScript();
            }
        }

        [MenuItem("GameObject/Foundation/BridgeUI/Create Binding", true, 10)]
        public static bool AddBindingRefrenceValid()
        {
            if (!Selection.activeGameObject)
                return false;

            return null == Selection.activeGameObject.GetComponent<IClassReference>();
        }

        [MenuItem("GameObject/Foundation/BridgeUI/Create View", false, 10)]
        protected static void CreateViewScript()
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(Selection.activeGameObject))
            {
                var prefabPath = UISetting.prefab_path + "/" + Selection.activeGameObject.name + ".prefab";
                if (PrefabUtility.SaveAsPrefabAssetAndConnect(Selection.activeGameObject, prefabPath, InteractionMode.UserAction))
                {
                    AssetDatabase.Refresh();
                }
            }

            string viewTypeName = string.Format("{0}.{1}", UISetting.defultNameSpace, Selection.activeGameObject.name);
            Type viewType = BridgeUI.Utility.FindTypeInAllAssemble(viewTypeName);
            if (viewType == null)
            {
                GenCodeUtil.CreateDefaultViewCode(Selection.activeGameObject.name);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("view script already exists:" + viewTypeName);
            }
        }

        [MenuItem("GameObject/Foundation/BridgeUI/Create View", true, 10)]
        public static bool CreateViewScriptCheck()
        {
            if (!Selection.activeGameObject)
                return false;

            string viewTypeName = string.Format("{0}.{1}", UISetting.defultNameSpace, Selection.activeGameObject.name);
            Type viewType = BridgeUI.Utility.FindTypeInAllAssemble(viewTypeName);
            if (viewType != null)
                return false;
           
            bool valid = false;
            var refScript = Selection.activeGameObject.GetComponent<BindingReference>();
            if (!refScript)
                valid = true;

            if(!valid)
            {
                var panel = Selection.activeGameObject.GetComponent<ViewBaseComponent>();
                if (!panel)
                    valid = true;

            }
            return valid;
        }

        protected static void DoCreateScript()
        {
            var behaiver = Selection.activeGameObject.AddComponent<ScriptCreateCatchBehaiver>();
            string typeName = string.Format("{0}.{1}Reference", UISetting.defultNameSpace, Selection.activeGameObject.name);
            behaiver.typeCatch = typeName;
            behaiver.assembleCatch = BridgeUI.Utility.GameAssembly.FullName;
            GenCodeRule rule = new GenCodeRule(UISetting.defultNameSpace);
            rule.baseTypeIndex = 0;
            GenCodeUtil.UpdateBindingScripts(Selection.activeGameObject, new List<ComponentItem>(), new List<ReferenceItem>(), rule);
        }

        public void OpenWith(MonoBehaviour behaiver)
        {
            this.prefab = behaiver.gameObject;
            rule = new GenCodeRule(UISetting.defultNameSpace);
            InitPanelNode();
            SwitchComponent(behaiver);
        }

        private void OnEnable()
        {
            if (panelCompnent != null)
            {
                SwitchComponent(panelCompnent);
            }
        }

        private void UpdateBindingAble()
        {
            var baseTypeStr = GenCodeUtil.supportBaseTypes[rule.baseTypeIndex];
            var baseType = BridgeUI.Utility.FindTypeInAllAssemble(baseTypeStr);
            bindingAble = typeof(BindingViewBase).IsAssignableFrom(baseType);
        }

        private void OnGUI()
        {
            DrawObjectField();
            DrawHeadInfos();
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scroll.scrollPosition;
                SwitchDrawOption();
            }
        }

        protected virtual void InitPanelNode()
        {
            if (preComponentList == null)
            {
                UpdateBindingAble();

                preComponentList = new ReorderableList(showComponents, typeof(ComponentItem));
                preComponentList.drawHeaderCallback = DrawComponetHeader;
                preComponentList.showDefaultBackground = true;

                preComponentList.elementHeightCallback = (index) =>
                {
                    if (index >= showComponents.Count)
                        return 0;
                    var prop = showComponents[index];
                    return itemDrawer.GetItemHeight(prop, bindingAble);
                };
                preComponentList.drawElementCallback = (rect, index, isFocused, isActive) =>
                {
                    if (index >= showComponents.Count)
                        return;
                    itemDrawer.DrawItemOnRect(rect, index, showComponents[index], bindingAble);
                };
                preComponentList.drawElementBackgroundCallback = (rect, index, isFocused, isActive) =>
                {
                    if (showComponents.Count > index && index >= 0)
                    {
                        itemDrawer.DrawBackground(rect, isFocused, showComponents[index], bindingAble);
                    }
                };
                preComponentList.onAddCallback = (ReorderableList list) =>
                {
                    var item = new ComponentItem();
                    components.Add(item);
                    showComponents.Add(item);
                };
                preComponentList.onRemoveCallback = (ReorderableList list) =>
                {
                    if (preComponentList.index >= 0 && showComponents.Count > preComponentList.index)
                    {
                        var item = showComponents[preComponentList.index];
                        if (item != null)
                        {
                            components.Remove(item);
                            showComponents.Remove(item);
                        }
                    }
                };
            }

            if (components != null)
            {
                components.ForEach((x) =>
                {
                    if (x.target != null)
                    {
                        x.components = GenCodeUtil.SortComponent(x.target);
                    }
                });
            }

        }

        private void SwitchComponent(MonoBehaviour component)
        {
            panelCompnent = component;
            if (panelCompnent != null)
            {
                panelDrawer = UnityEditor.Editor.CreateEditor(panelCompnent);
            }
            referenceItems.Clear();
            GenCodeUtil.AnalysisComponent(panelCompnent, components, referenceItems, rule);
            itemDrawer.Bind(component);
            RefreshShowingComponentList();
        }

        //private void InitPanelBase(MonoBehaviour v)
        //{
        //    if (v is BindingViewBaseComponent)
        //    {
        //        var type = v.GetType();
        //        var find = false;
        //        while (!find && type.BaseType != typeof(object))
        //        {
        //            type = type.BaseType;
        //            for (int i = 0; i < GenCodeUtil.supportBaseTypes.Length; i++)
        //            {
        //                if (type.FullName == GenCodeUtil.supportBaseTypes[i])
        //                {
        //                    rule.baseTypeIndex = i;
        //                    find = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}
        private void DrawComponetHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "[控件列表]");
            var searchRect = new Rect(rect.x + 60, rect.y + 1f, 80, EditorGUIUtility.singleLineHeight - 2f);
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                m_matchText = EditorGUI.TextField(searchRect, m_matchText);
                if (change.changed)
                {
                    RefreshShowingComponentList();
                }
            }
            var btnRect = new Rect(rect.x + rect.width - 20, rect.y + 1, 20, EditorGUIUtility.singleLineHeight - 2f);
            if (GUI.Button(btnRect, new GUIContent("←", "快速解析"), EditorStyles.toolbarButton))
            {
                GenCodeUtil.ChoiseAnReferenceMonobehiver(prefab, component =>
                {
                    if (component == null)
                    {
                        EditorApplication.Beep();
                    }
                    else
                    {
                        //从旧的脚本解析出
                        referenceItems.Clear();
                        GenCodeUtil.AnalysisComponent(component, components, referenceItems, rule);
                        RefreshShowingComponentList();
                    }
                });

            }
        }

        private void RefreshShowingComponentList()
        {
            if (components.Count > 0)
                components.AddRange(showComponents);

            showComponents.Clear();
            if (string.IsNullOrEmpty(m_matchText))
            {
                showComponents.AddRange(components);
            }
            else
            {
                foreach (var item in components)
                {
                    if (item.name.ToLower().Contains(m_matchText.ToLower()))
                    {
                        showComponents.Add(item);
                    }
                }
            }
        }

        private void SwitchDrawOption()
        {
            selected = GUILayout.Toolbar(selected, options);
            if (prefab == null) return;

            if (selected == 0)
            {
                if (preComponentList == null)
                {
                    InitPanelNode();
                }
                DrawPreComponents();
            }
            else if (selected == 1)
            {
                DrawPanelComponent();
            }

            if (System.DateTime.Now.Second - currentSecond > saveSecond)
            {
                currentSecond = System.DateTime.Now.Second;
                EditorUtility.SetDirty(panelDrawer.target);
            }
        }

        private void DrawObjectField()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("预制体:", EditorStyles.toolbarButton, GUILayout.Width(lableWidth)))
                {
                    ToggleOpen();
                }
                EditorGUI.BeginChangeCheck();
                prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), false) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    GenCodeUtil.ChoiseAnReferenceMonobehiver(prefab, v =>
                    {
                        OpenWith(v);
                    });
                }
            }
        }

        /// <summary>
        /// 切换面板的打开状态
        /// </summary>
        private void ToggleOpen()
        {
            if (instenceID == 0)
            {
                Transform parent = null;
                var group = FindObjectOfType<PanelGroup>();
                if (group != null)
                {
                    parent = group.GetComponent<Transform>();
                }
                else
                {
                    var canvas = FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        parent = canvas.GetComponent<Transform>();
                    }
                }
                if (parent != null)
                {
                    var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    obj.transform.SetParent(parent, false);
                    instenceID = obj.GetInstanceID();
                }
            }
            else
            {
                var obj = EditorUtility.InstanceIDToObject(instenceID);
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
                instenceID = 0;
            }

        }
        private void DrawPanelComponent()
        {
            if (!panelCompnent)
                return;

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("→", "快速绑定"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GenCodeUtil.BindingUIComponents(panelCompnent, components);
                }
            }

            if (panelDrawer != null)
            {
                panelDrawer.OnInspectorGUI();
            }
        }

        private void DrawHeadInfos()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("命名空间", GUILayout.Width(60));
                rule.nameSpace = GUILayout.TextField(rule.nameSpace);
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("BaseType:", GUILayout.Width(lableWidth));
                EditorGUI.BeginChangeCheck();
                rule.baseTypeIndex = EditorGUILayout.Popup(rule.baseTypeIndex, GenCodeUtil.supportBaseTypes);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateBindingAble();
                }
                if (GUILayout.Button("update", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    var go = prefab;
                    rule.bindingAble = bindingAble;
                    var bindingRef = go.GetComponent<BindingReference>();
                    if (bindingRef && !(bindingRef is UIBinding))
                    {
                        var referenceBehaiver = go.AddComponent<ReferenceCatchBehaiver>();
                        var tempRefItems = UFrame.BridgeUI.Editors.GenCodeUtil.WorpReferenceItems(components);
                        referenceBehaiver.SetReferenceItems(tempRefItems);
                    }
                    EditorUtility.SetDirty(go);
                    GenCodeUtil.UpdateBindingScripts(go, components, referenceItems, rule);
                }
            }
        }

        private void DrawPreComponents()
        {
            preComponentList.DoLayoutList();

            var addRect = GUILayoutUtility.GetRect(BridgeUI.Editors.BridgeEditorUtility.currentViewWidth, EditorGUIUtility.singleLineHeight * 3);

            if (addRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        foreach (var item in DragAndDrop.objectReferences)
                        {
                            if (item is GameObject || item is ScriptableObject)
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                            }
                        }
                    }
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    if (DragAndDrop.objectReferences.Length > 0)
                    {
                        foreach (var item in DragAndDrop.objectReferences)
                        {
                            if (item is GameObject)
                            {
                                var obj = item as GameObject;
                                var parent = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                                if (parent)
                                {
                                    obj = parent as GameObject;
                                }
                                var c_item = components.Find(x => x.target == obj);
                                if (c_item == null)
                                {
                                    c_item = new ComponentItem(obj);
                                    c_item.components = GenCodeUtil.SortComponent(obj);
                                    components.Add(c_item);
                                    showComponents.Add(c_item);
                                }
                                else
                                {
                                    Debug.LogError("duplicate!", obj);
                                }
                                if (c_item != null)
                                {
                                    GenCodeUtil.BindingUIComponent(panelCompnent, c_item);
                                }
                            }

                            else if (item is ScriptableObject)
                            {
                                var c_item = new ComponentItem(item as ScriptableObject);
                                components.Add(c_item);
                                showComponents.Add(c_item);
                            }
                        }
                        DragAndDrop.AcceptDrag();
                    }

                }
            }
        }
    }
}