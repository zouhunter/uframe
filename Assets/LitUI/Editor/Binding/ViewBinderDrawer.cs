using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

namespace UFrame.LitUI
{
    public class ViewBinderDrawer
    {
        private GameObject target;
        private const int lableWidth = 60;
        private BindingView panelCompnent;
        private List<ComponentBinding> components = new List<ComponentBinding>();
        private ReorderableList preComponentList;
        private ComponentItemDrawer itemDrawer = new ComponentItemDrawer();
        private Vector2 scrollPos;
        private bool bindingAble = true;
        private const int saveSecond = 5;
        private int currentSecond = 0;
        protected string m_matchText;
        protected List<ComponentBinding> showComponents = new List<ComponentBinding>();

        public ComponentBinding[] bindings => components.ToArray();
        private UICodeGen uiCodeGen;
        public bool changed { get; set; }

        public void OpenWith(BindingView behaiver, UICodeGen codeGen)
        {
            this.target = behaiver.gameObject;
            this.uiCodeGen = codeGen;
            InitPanelNode();
            SwitchComponent(behaiver);
            changed = false;
        }

        public void OnGUI()
        {
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
                preComponentList = new ReorderableList(showComponents, typeof(ComponentBinding));
                preComponentList.drawHeaderCallback = DrawComponetHeader;
                preComponentList.showDefaultBackground = true;

                preComponentList.elementHeightCallback = (index) =>
                {
                    if (index >= showComponents.Count)
                        return 0;
                    var prop = showComponents[index];
                    return itemDrawer.GetItemHeight(prop);
                };
                preComponentList.drawElementCallback = (rect, index, isFocused, isActive) =>
                {
                    if (index >= showComponents.Count)
                        return;
                    itemDrawer.DrawItemOnRect(rect, index, showComponents[index], bindingAble);
                    if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete"), false, (x) =>
                        {

                            RemoveComponentItem(index);
                        }, null);
                        menu.ShowAsContext();
                    }
                };
                preComponentList.drawElementBackgroundCallback = (rect, index, isFocused, isActive) =>
                {
                    if (showComponents.Count > index && index >= 0)
                    {
                        itemDrawer.DrawBackground(rect, isFocused, showComponents[index]);
                    }
                };
                preComponentList.onAddCallback = (ReorderableList list) =>
                {
                    var item = new ComponentBinding();
                    components.Add(item);
                    showComponents.Add(item);
                };
                preComponentList.onRemoveCallback = (ReorderableList list) =>
                {
                    if (preComponentList.index >= 0 && showComponents.Count > preComponentList.index)
                    {
                        RemoveComponentItem(preComponentList.index);
                    }
                };
            }
        }

        private void RemoveComponentItem(int index)
        {
            var item = showComponents[index];
            if (item != null)
            {
                components.Remove(item);
                showComponents.Remove(item);
            }
            changed = true;
        }

        public static TypeInfo[] SortComponent(UnityEngine.Object target, params Type[] types)
        {
            var supportedlist = new List<TypeInfo>();
            supportedlist.Add(new TypeInfo(typeof(GameObject)));
            if (target && (target is GameObject || target is Component))
            {
                if (!(target is GameObject go))
                    go = (target as Component).gameObject;

                var innercomponentsTypes = new List<TypeInfo>();
                var innercomponents = go.GetComponents<Component>();
                for (int i = 0; i < innercomponents.Length; i++)//按指定的顺序添加控件{
                {
                    if (!innercomponents[i])
                    {
                        Debug.LogError(go.name + " componentLost:" + i);
                        continue;
                    }
                    innercomponentsTypes.Add(new TypeInfo(innercomponents[i].GetType()));
                }
                supportedlist.AddRange(innercomponentsTypes);
            }

            if (types != null)
            {
                foreach (var subType in types)
                {
                    if (supportedlist.Find(x => x.type == subType).type != null)
                    {
                        continue;
                    }
                    supportedlist.Add(new TypeInfo(subType));
                }
            }
            return supportedlist.ToArray();
        }

        private void SwitchComponent(BindingView component)
        {
            panelCompnent = component;
            if (component.binding != null && component.binding.refs != null)
                components = component.binding.refs.Where(x => x != null).Select(x => new ComponentBinding() { name = x.name, target = x.obj }).ToList();
            uiCodeGen.AnalysisBinding(component.binder, components.ToArray());
            itemDrawer.Bind(component);
            RefreshShowingComponentList();
        }

        private void DrawComponetHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "[控件列表]");
            var searchRect = new Rect(rect.x + rect.width - 105, rect.y + 1f, 80, EditorGUIUtility.singleLineHeight - 2f);
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
                AutoCollect();
            }
            AcceptDrag(rect);
        }

        private void AcceptDrag(Rect addRect)
        {
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
                                var obj = UIBindingDrawer.SelectBestObject(item as GameObject);
                                var c_item = components.Find(x => x.target == obj);
                                if (c_item == null)
                                {
                                    c_item = new ComponentBinding(obj);
                                    CreateDefaultViewAndEventOfComponent(c_item, obj);
                                    components.Add(c_item);
                                    showComponents.Add(c_item);
                                }
                                else
                                {
                                    Debug.LogError("duplicate!", obj);
                                }
                            }

                            else if (item is ScriptableObject)
                            {
                                var c_item = new ComponentBinding(item as ScriptableObject);
                                components.Add(c_item);
                                showComponents.Add(c_item);
                            }
                        }
                        DragAndDrop.AcceptDrag();
                    }

                }
            }
        }

        private void AutoCollect()
        {
            var obj = Selection.activeGameObject;
            if (obj)
            {
                AutoCollectDeepth(obj.transform);
            }
        }

        private void AutoCollectDeepth(Transform parent)
        {
            var autoTypeDic = UISetting.Instance.GetAutoCollectTypes();

            foreach (Transform item in parent)
            {
                if (item == parent)
                    continue;

                var subView = item.GetComponent<UIView>();
                if (subView)
                {
                    var comp = components.Find(x => x.target == subView);
                    if (comp != null)
                        continue;

                    var c_item = new ComponentBinding(subView);
                    components.Add(c_item);
                    showComponents.Add(c_item);
                    continue;
                }

                AutoCollectDeepth(item);

                if (!Regex.IsMatch(item.name, @"^[\w]+$"))
                    continue;

                foreach (var pair in autoTypeDic)
                {
                    if (item.name.EndsWith(pair.Key) || item.name.StartsWith(pair.Key))
                    {
                        if (pair.Value == typeof(GameObject))
                        {
                            var comp = components.Find(x => x.target == item.gameObject);
                            if (comp != null)
                                continue;

                            var c_item = new ComponentBinding(item.gameObject);
                            CreateDefaultViewAndEventOfComponent(c_item, item.gameObject);
                            components.Add(c_item);
                            showComponents.Add(c_item);
                            break;
                        }
                        else
                        {
                            var component = item.GetComponent(pair.Value);
                            if (component)
                            {
                                var comp = components.Find(x => x.target == component);
                                if (comp != null)
                                    continue;

                                var c_item = new ComponentBinding(component);
                                CreateDefaultViewAndEventOfComponent(c_item, component);
                                components.Add(c_item);
                                showComponents.Add(c_item);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建控件默认绑定
        /// </summary>
        /// <param name="compBinding"></param>
        /// <param name="target"></param>
        private void CreateDefaultViewAndEventOfComponent(ComponentBinding compBinding, Object target)
        {
            if (target is Selectable)
            {
                CreateDefaultEventView(compBinding, target);
                CreateDefaultMember(compBinding, target, true);
            }
            else if (target is Graphic || target is GameObject)
            {
                CreateDefaultMember(compBinding, target, false);
            }

        }

        private void CreateDefaultEventView(ComponentBinding compBinding, Object target)
        {
            var typeList = new List<Type>();
            var nameList = new List<string>();
            ComponentItemDrawer.CreateEventTypeInfos(target.GetType(), typeList, nameList);
            if (typeList.Count == 0)
                return;

            var index = 0;
            var prop = UISetting.Instance.FindDefaultEvent(target.GetType());
            if (!string.IsNullOrEmpty(prop))
            {
                index = nameList.IndexOf(prop);
                if (index < 0)
                    index = 0;
            }
            var defaultEvent = new BindingEvent();
            defaultEvent.bindingTarget = nameList[index];
            defaultEvent.bindingSource = ComponentItemDrawer.GetDefaultBiningEventSource(target.name, defaultEvent.bindingTarget);
            defaultEvent.bindingTargetType = new TypeInfo(typeList[index]);
            compBinding.eventItems.Add(defaultEvent);
        }

        private void CreateDefaultMember(ComponentBinding compBinding, Object target, bool defaultOnly)
        {
            var typeList = new List<Type>();
            var nameList = new List<string>();
            var methods = new List<string>();
            ComponentItemDrawer.CreateViewTypeInfos(target.GetType(), typeList, nameList, methods);
            if (nameList.Count == 0)
                return;

            var defaultShow = new BindingShow();
            var bindingTarget = ComponentItemDrawer.GetComponentDefaultViewName(target.GetType());
            if (!nameList.Contains(bindingTarget))
            {
                if (defaultOnly)
                    return;
                bindingTarget = nameList[0];
            }
            var index = nameList.IndexOf(bindingTarget);
            defaultShow.bindingTarget = bindingTarget;
            defaultShow.bindingSource = ComponentItemDrawer.GetDefaultViewSource(target.name, bindingTarget);
            defaultShow.isMethod = methods.Contains(bindingTarget);
            defaultShow.bindingTargetType = new TypeInfo(typeList[index]);
            compBinding.viewItems.Add(defaultShow);
        }

        private void RefreshShowingComponentList()
        {
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
                        if (!showComponents.Contains(item))
                            showComponents.Add(item);
                    }
                }
            }
        }

        private void SwitchDrawOption()
        {
            if (target == null) return;
            if (preComponentList == null)
            {
                InitPanelNode();
            }
            DrawPreComponents();

            if (System.DateTime.Now.Second - currentSecond > saveSecond)
            {
                currentSecond = System.DateTime.Now.Second;
                EditorUtility.SetDirty(panelCompnent);
            }
        }

        private void DrawPreComponents()
        {
            preComponentList.DoLayoutList();
        }

        internal void ClearBindings()
        {
            showComponents.Clear();
            components.Clear();
        }
    }
    public class MemberViewer
    {
        public Dictionary<System.Type, Type[]> memberTypeDic = new Dictionary<System.Type, Type[]>();
        public Dictionary<System.Type, string[]> memberNameDic = new Dictionary<System.Type, string[]>();
        public Dictionary<System.Type, string[]> memberViewNameDic = new Dictionary<System.Type, string[]>();
        public Dictionary<System.Type, List<string>> memberMethodDic = new Dictionary<Type, List<string>>();
        public Dictionary<System.Type, Dictionary<Type, string[]>> valueChangeNameDic = new Dictionary<Type, Dictionary<Type, string[]>>();
        public Dictionary<System.Type, SortedDictionary<string, Type>> valueChangeTypeDic = new Dictionary<Type, SortedDictionary<string, Type>>();
        public Type[] currentTypes;
        public string[] currentNames;
        public string[] currentViewNames;
        public List<string> currentMethods;
    }
    public class ComponentItemDrawer
    {
        Color fieldColor = new Color(.2f, 1f, .5f, 1f);
        Color activeColor = new Color(.2f, .5f, 1f, 1f);
        Dictionary<ComponentBinding, ReorderableList> viewDic = new Dictionary<ComponentBinding, ReorderableList>();
        Dictionary<ComponentBinding, ReorderableList> eventDic = new Dictionary<ComponentBinding, ReorderableList>();
        Dictionary<ComponentBinding, bool> openDict = new Dictionary<ComponentBinding, bool>();
        ReorderableList viewList;
        ReorderableList eventList;
        const float padding = 5;
        public float singleLineHeight = EditorGUIUtility.singleLineHeight + padding * 2;
        private float viewHeight;
        private float eventHeight;
        protected MonoBehaviour componentBehaviour { get; set; }
        private MemberViewer viewMemberViewer = new MemberViewer();
        private MemberViewer eventMemberViewer = new MemberViewer();
        private static Dictionary<Type, Texture> _previewIcons;
        public static Dictionary<Type, Texture> previewIcons
        {
            get
            {
                if (_previewIcons == null)
                {
                    _previewIcons = new Dictionary<Type, Texture>();
#if UNITY_5_3
                    _previewIcons.Add(typeof(GameObject), EditorGUIUtility.FindTexture("GameObject Icon"));
#elif UNITY_5_6
                    _previewIcons.Add(typeof(GameObject), EditorGUIUtility.IconContent("GameObject Icon").image);
#else
                    _previewIcons.Add(typeof(GameObject), EditorGUIUtility.IconContent("GameObject Icon").image);
#endif
                    _previewIcons.Add(typeof(Image), EditorGUIUtility.IconContent("Image Icon").image);
                    _previewIcons.Add(typeof(Text), EditorGUIUtility.IconContent("Text Icon").image);
                    _previewIcons.Add(typeof(Button), EditorGUIUtility.IconContent("Button Icon").image);
                    _previewIcons.Add(typeof(Toggle), EditorGUIUtility.IconContent("Toggle Icon").image);
                    _previewIcons.Add(typeof(Slider), EditorGUIUtility.IconContent("Slider Icon").image);
                    _previewIcons.Add(typeof(Scrollbar), EditorGUIUtility.IconContent("Scrollbar Icon").image);
                    _previewIcons.Add(typeof(Dropdown), EditorGUIUtility.IconContent("Dropdown Icon").image);
                    _previewIcons.Add(typeof(Canvas), EditorGUIUtility.IconContent("Canvas Icon").image);
                    _previewIcons.Add(typeof(RawImage), EditorGUIUtility.IconContent("RawImage Icon").image);
                    _previewIcons.Add(typeof(InputField), EditorGUIUtility.IconContent("InputField Icon").image);
                    _previewIcons.Add(typeof(ScrollRect), EditorGUIUtility.IconContent("ScrollRect Icon").image);
                    _previewIcons.Add(typeof(GridLayoutGroup), EditorGUIUtility.IconContent("GridLayoutGroup Icon").image);
                    _previewIcons.Add(typeof(ScriptableObject), EditorGUIUtility.IconContent("ScriptableObject Icon").image);
                }
                return _previewIcons;
            }
        }

        public float GetItemHeight(ComponentBinding item)
        {
            if (item.target is ScriptableObject)
            {
                return singleLineHeight;
            }
            else
            {
                UpdateHeights(item);
                openDict.TryGetValue(item, out var open);
                var height = singleLineHeight + (open ? viewHeight + eventHeight : 0f);
                return height;
            }
        }

        public void DrawItemOnRect(Rect rect, int index, ComponentBinding item, bool bindingAble)
        {
            rect.height = GetItemHeight(item);
            DrawInfoHead(rect, item);
            DrawIndex(rect, index, item);
            openDict.TryGetValue(item, out var open);
            if (open && !(item.target is ScriptableObject))
            {
                DrawLists(rect, item);
            }
        }
        public void DrawBackground(Rect rect, bool active, ComponentBinding item)
        {
            rect.height = GetItemHeight(item);
            var innerRect1 = new Rect(rect.x + padding, rect.y + padding, rect.width - 2 * padding, rect.height - 2 * padding);
            var color = GUI.backgroundColor;
            GUI.backgroundColor = active ? activeColor : color;
            GUI.Box(innerRect1, "");
            GUI.backgroundColor = color;
        }

        private void DrawInfoHead(Rect rect, ComponentBinding item)
        {
            var innerRect = new Rect(rect.x + padding + 20, rect.y + padding, rect.width - 2 * padding - 20, EditorGUIUtility.singleLineHeight);
            var nameRect = new Rect(innerRect.x, innerRect.y, innerRect.width * 0.2f, innerRect.height);
            var targetRect = new Rect(innerRect.x + innerRect.width * 0.2f, innerRect.y, innerRect.width * 0.25f, innerRect.height);
            var typeRect = new Rect(innerRect.x + innerRect.width * 0.5f, innerRect.y, innerRect.width * 0.5f - 25, innerRect.height);
            var iconRect = new Rect(innerRect.x + innerRect.width * 0.2f + 2.5f, innerRect.y + 2.5f, 12, 12);
            var exporeRect = new Rect(innerRect.x + innerRect.width - 20, innerRect.y, 20, innerRect.height);
            item.name = EditorGUI.TextField(nameRect, item.name);
            item.expose = EditorGUI.Toggle(exporeRect, item.expose, EditorStyles.radioButton);
            if (!(item.target is ScriptableObject))
            {
                DrawTarget(targetRect, item);

                var types = ViewBinderDrawer.SortComponent(item.target);
                var componentStr = types.Select(x => x.typeName).ToArray();
                int componentID = Array.IndexOf(componentStr, item.targetType.FullName);
                if (componentID < 0)
                {
                    Debug.LogError("component type lost:" + item.name);
                    componentID = 0;
                }

                componentID = EditorGUI.Popup(typeRect, componentID, componentStr);
                var type = types[componentID].type;
                item.target = ChangeObjectRef(item.target, type);
                if (previewIcons.ContainsKey(item.targetType))
                {
                    var icon = previewIcons[item.targetType];
                    EditorGUI.DrawTextureTransparent(iconRect, icon, ScaleMode.ScaleAndCrop);
                }
            }
            else
            {
                DrawScriptTarget(targetRect, item);

                EditorGUI.LabelField(typeRect, item.targetType.FullName);

                var icon = previewIcons[typeof(ScriptableObject)];
                EditorGUI.DrawTextureTransparent(iconRect, icon, ScaleMode.ScaleAndCrop);
            }

        }
        private UnityEngine.Object ChangeObjectRef(UnityEngine.Object target, Type type)
        {
            if (!target)
                return target;
            if (target.GetType() == type)
                return target;
            if (target is GameObject go)
            {
                var comp = go.GetComponent(type);
                return comp;
            }
            else if (target is Component comObj)
            {
                if (type == typeof(GameObject))
                {
                    target = comObj.gameObject;
                }
                else
                {
                    var comp = comObj.GetComponent(type);
                    target = comp;
                }
            }
            return target;
        }
        public void Bind(MonoBehaviour component)
        {
            componentBehaviour = component;
        }

        private void DrawScriptTarget(Rect targetRect, ComponentBinding item)
        {
            EditorGUI.BeginChangeCheck();
            var newTarget = EditorGUI.ObjectField(targetRect, item.target, item.targetType, true);
            if (newTarget != item.target)
            {
                var prefabTarget = PrefabUtility.GetCorrespondingObjectFromSource(newTarget);
                Debug.Log(prefabTarget);
                if (prefabTarget != null)
                {
                    newTarget = prefabTarget;
                }
                item.target = newTarget;
            }

            if (EditorGUI.EndChangeCheck() && item.target)
            {
                if (string.IsNullOrEmpty(item.name))
                {
                    item.name = item.target.name;
                }
            }

        }

        private void DrawTarget(Rect targetRect, ComponentBinding item)
        {
            EditorGUI.BeginChangeCheck();
            item.target = EditorGUI.ObjectField(targetRect, item.target, item.targetType, true);
            if (EditorGUI.EndChangeCheck() && item.target)
            {
                if (string.IsNullOrEmpty(item.name))
                {
                    item.name = item.target.name;
                }
            }
        }

        private void DrawIndex(Rect rect, int index, ComponentBinding item)
        {
            var indexRect = new Rect(rect.x, rect.y, 20, singleLineHeight);
            openDict.TryGetValue(item, out var open);
            openDict[item] = EditorGUI.Toggle(indexRect, open, EditorStyles.foldout);
        }

        private void DrawLists(Rect rect, ComponentBinding item)
        {
            UpdateHeights(item);
            var innerRect1 = new Rect(rect.x + padding, rect.y, rect.width - 30, rect.height - 2 * padding);

            viewList = GetViewList(item);
            var viewRect = new Rect(innerRect1.x, innerRect1.y + singleLineHeight, innerRect1.width, viewHeight);
            viewList.DoList(viewRect);

            eventList = GetEventList(item);
            var eventRect = new Rect(innerRect1.x, innerRect1.y + viewHeight + singleLineHeight, innerRect1.width, eventHeight);
            eventList.DoList(eventRect);
        }

        private void UpdateHeights(ComponentBinding item)
        {
            viewHeight = (EditorGUIUtility.singleLineHeight + padding) * (item.viewItems.Count >= 1 ? item.viewItems.Count + 2 : 3);
            eventHeight = (EditorGUIUtility.singleLineHeight + padding) * (item.eventItems.Count >= 1 ? item.eventItems.Count + 2 : 3);
        }


        private ReorderableList GetViewList(ComponentBinding item)
        {
            if (!viewDic.ContainsKey(item) || viewDic[item] == null)
            {
                var list = viewDic[item] = new ReorderableList(item.viewItems, typeof(BindingShow));
                list.drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "Members");
                };
                list.drawElementCallback = (rect, index, a, f) =>
                {
                    UpdateMemberByType(item.targetType);

                    var viewItem = item.viewItems[index];
                    rect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);
                    var targetRect = new Rect(rect.x + rect.width * 0.1f, rect.y, rect.width * 0.5f, rect.height);
                    var sourceRect = new Rect(rect.x + rect.width * 0.65f, rect.y, rect.width * 0.32f, EditorGUIUtility.singleLineHeight);
                    var valueChangeRect = Rect.zero; new Rect(rect.x + rect.width * 0.92f, rect.y, rect.width * 0.1f, rect.height);
                    var valueChageOptions = GetValueChangeOptions(item.targetType, viewItem.bindingTargetType.type);
                    if (valueChageOptions != null)
                    {
                        targetRect = new Rect(rect.x + rect.width * 0.1f, rect.y, rect.width * 0.3f, rect.height);
                        sourceRect = new Rect(rect.x + rect.width * 0.42f, rect.y, rect.width * 0.3f, rect.height);
                        valueChangeRect = new Rect(rect.x + rect.width * 0.74f, rect.y, rect.width * 0.25f, rect.height);
                    }

                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.1f, rect.height), new GUIContent("[t]", "Target"));
                    if (string.IsNullOrEmpty(viewItem.bindingSource))
                    {
                        EditorGUI.LabelField(sourceRect, "Source");
                    }
                    bool fristSelect = false;
                    if (string.IsNullOrEmpty(viewItem.bindingTarget) && viewMemberViewer.currentNames.Length > 0)
                    {
                        viewItem.bindingTarget = GetComponentDefaultViewName(item.targetType);
                        if (string.IsNullOrEmpty(viewItem.bindingTarget) || Array.IndexOf(viewMemberViewer.currentNames, viewItem.bindingTarget) < 0)
                        {
                            viewItem.bindingTarget = viewMemberViewer.currentNames[0];
                        }
                        fristSelect = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    var viewNameIndex = Array.IndexOf(viewMemberViewer.currentNames, viewItem.bindingTarget);
                    viewNameIndex = EditorGUI.Popup(targetRect, viewNameIndex, viewMemberViewer.currentViewNames);
                    var changed = EditorGUI.EndChangeCheck();
                    if (changed || fristSelect)
                    {
                        viewItem.bindingTargetType = new TypeInfo(viewMemberViewer.currentTypes[viewNameIndex]);
                        viewItem.bindingTarget = viewMemberViewer.currentNames[viewNameIndex];
                        viewItem.isMethod = viewMemberViewer.currentMethods.Contains(viewItem.bindingTarget);
                    }
                    viewItem.bindingSource = EditorGUI.TextField(sourceRect, viewItem.bindingSource);
                    if (changed || string.IsNullOrEmpty(viewItem.bindingSource))
                    {
                        viewItem.bindingSource = GetDefaultViewSource(item.name, viewMemberViewer.currentNames[viewNameIndex]);
                    }

                    if (valueChageOptions != null && valueChageOptions.Length > 0)
                    {
                        var changeIndex = Array.IndexOf(valueChageOptions, viewItem.changeEvent);
                        if (changeIndex < 0)
                            changeIndex = 0;
                        changeIndex = EditorGUI.Popup(valueChangeRect, changeIndex, valueChageOptions);
                        viewItem.changeEvent = valueChageOptions[changeIndex];
                    }
                    else
                    {
                        viewItem.changeEvent = null;
                    }
                };
            }
            return viewDic[item];
        }

        private string[] GetValueChangeOptions(Type type, Type targetType)
        {
            if (!eventMemberViewer.memberTypeDic.ContainsKey(type))
                UpdateEventByType(type);

            var eventTypes = eventMemberViewer.memberTypeDic[type];
            var eventNames = eventMemberViewer.memberNameDic[type];

            if (!viewMemberViewer.valueChangeTypeDic.TryGetValue(type, out var detailDict))
            {
                detailDict = null;
                for (int i = 0; i < eventTypes.Length; i++)
                {
                    var currentType = eventTypes[i];
                    while (currentType != null)
                    {
                        if (!currentType.Name.Contains("UnityEvent`"))
                        {
                            currentType = currentType.BaseType;
                            continue;
                        }

                        var genericArgs = currentType.GetGenericArguments();
                        if (genericArgs != null && genericArgs.Length == 1)
                        {
                            detailDict = detailDict ?? new SortedDictionary<string, Type>();
                            detailDict[eventNames[i]] = genericArgs[0];
                        }
                        break;
                    }
                }
                viewMemberViewer.valueChangeTypeDic[type] = detailDict;
            }
            if (!viewMemberViewer.valueChangeNameDic.TryGetValue(type, out var nameDetail))
            {
                nameDetail = new Dictionary<Type, string[]>();
                viewMemberViewer.valueChangeNameDic[type] = nameDetail;
            }

            if (targetType == null)
                return null;
            if (nameDetail.TryGetValue(targetType, out var detailOptions))
                return detailOptions;

            List<string> optionList = null;
            if (detailDict != null)
            {
                foreach (var pair in detailDict)
                {
                    if (pair.Value == targetType)
                    {
                        optionList = optionList ?? new List<string>();
                        optionList.Add(pair.Key);
                    }
                }

                if (optionList != null && optionList.Count > 1)
                {
                    string defaultEvent = UISetting.Instance.FindDefaultEvent(type);
                    if (optionList.Remove(defaultEvent))
                    {
                        optionList.Insert(0, defaultEvent);
                    }
                }
            }
            detailOptions = optionList?.ToArray();
            nameDetail[targetType] = detailOptions;
            return detailOptions;
        }

        private ReorderableList GetEventList(ComponentBinding item)
        {
            if (!eventDic.ContainsKey(item) || eventDic[item] == null)
            {
                var list = eventDic[item] = new ReorderableList(item.eventItems, typeof(BindingEvent));
                list.drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "Events");
                };

                list.drawElementCallback = (rect, index, a, f) =>
                {
                    var eventItem = item.eventItems[index];
                    UpdateEventByType(item.targetType);
                    var targetRect = new Rect(rect.x + rect.width * 0.1f, rect.y, rect.width * 0.35f, rect.height);
                    var sourceRect = new Rect(rect.x + rect.width * 0.475f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                    var enableRect = new Rect(rect.x + rect.width * 0.8f, rect.y, rect.width * 0.2f, rect.height);
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.1f, rect.height), new GUIContent("[t]", "Target"));

                    if (string.IsNullOrEmpty(eventItem.bindingSource))
                    {
                        EditorGUI.LabelField(sourceRect, "Source");
                    }
                    bool firstLoad = false;
                    if (string.IsNullOrEmpty(eventItem.bindingTarget) && eventMemberViewer.currentNames.Length > 0)
                    {
                        firstLoad = true;
                        eventItem.bindingTarget = eventMemberViewer.currentNames[0];
                    }
                    EditorGUI.BeginChangeCheck();
                    var viewNameIndex = Array.IndexOf(eventMemberViewer.currentNames, eventItem.bindingTarget);
                    viewNameIndex = EditorGUI.Popup(targetRect, viewNameIndex, eventMemberViewer.currentViewNames);
                    var changed = EditorGUI.EndChangeCheck();
                    if (changed || firstLoad)
                    {
                        eventItem.bindingTargetType = new TypeInfo(eventMemberViewer.currentTypes[viewNameIndex]);
                        eventItem.bindingTarget = eventMemberViewer.currentNames[viewNameIndex];
                    }
                    eventItem.bindingSource = EditorGUI.TextField(sourceRect, eventItem.bindingSource);
                    if (changed || (string.IsNullOrEmpty(eventItem.bindingSource) && viewNameIndex >= 0 && viewNameIndex < eventMemberViewer.currentViewNames.Length))
                    {
                        eventItem.bindingSource = GetDefaultBiningEventSource(item.name, eventMemberViewer.currentViewNames[viewNameIndex]);
                    }
                    eventItem.type = (BindingType)EditorGUI.EnumPopup(enableRect, eventItem.type);
                };
            }
            return eventDic[item];
        }

        public static string GetComponentDefaultViewName(Type type)
        {
            var prop = UISetting.Instance.FindDefaultProp(type);
            if (!string.IsNullOrEmpty(prop))
                return prop;
            return null;
        }

        public static string GetDefaultViewSource(string target, string viewName)
        {
            if (viewName.StartsWith("Set") && viewName.Length > 3)
            {
                viewName = viewName.Substring(3);
                viewName = char.ToUpper(viewName[0]) + viewName.Substring(1);
            }
            var fullName = string.Format("{0}_{1}", target, viewName);
            var quad = fullName.IndexOf('(');
            if (quad > 0)
            {
                fullName = fullName.Substring(0, quad).Trim();
            }
            return fullName;
        }

        public static string GetDefaultBiningEventSource(string target, string viewName)
        {
            var lowerTypeName = viewName.ToLower();
            if (lowerTypeName.StartsWith("on"))
                lowerTypeName = lowerTypeName.Substring(2);
            var fullName = target + char.ToUpper(lowerTypeName[0]) + lowerTypeName.Substring(1);
            var quad = fullName.IndexOf('(');
            if (quad > 0)
            {
                fullName = fullName.Substring(0, quad).Trim();
            }
            return fullName;
        }

        public static void CreateViewTypeInfos(Type type, List<Type> typeList, List<string> nameList, List<string> methods)
        {
            var members = type.GetMembers(
             System.Reflection.BindingFlags.Public |
             System.Reflection.BindingFlags.Instance |
             System.Reflection.BindingFlags.SetField);
            foreach (var member in members)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        var field = member as FieldInfo;
                        if (!field.IsInitOnly) continue;
                        if (field.IsLiteral) continue;
                        if (IsMemberSupported(field.FieldType))
                        {
                            typeList.Add(field.FieldType);
                            nameList.Add(field.Name);
                        }
                        break;
                    case MemberTypes.Property:
                        var prop = member as PropertyInfo;
                        if (prop.GetSetMethod() == null) continue;
                        if (IsMemberSupported(prop.PropertyType))
                        {
                            typeList.Add(prop.PropertyType);
                            nameList.Add(prop.Name);
                        }
                        break;
                    case MemberTypes.Method:
                        var methodInfo = member as MethodInfo;
                        var parmeters = methodInfo.GetParameters();
                        if (parmeters.Count() == 1 &&
                            methodInfo.IsPublic &&
                            methodInfo.ReturnType == typeof(void) &&
                            !methodInfo.Name.StartsWith("set_"))
                        {
                            var parmeter = parmeters[0];
                            if (IsMemberSupported(parmeter.ParameterType))
                            {
                                typeList.Add(parmeter.ParameterType);
                                nameList.Add(methodInfo.Name);
                                methods.Add(methodInfo.Name);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public static void CreateEventTypeInfos(Type type, List<Type> typeList, List<string> nameList)
        {
            var props = type.GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.GetProperty);

            var goodProps = props.Where(x => typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(x.PropertyType));
            typeList.AddRange(goodProps.Select(x => x.PropertyType).ToArray());
            nameList.AddRange(goodProps.Select(x => x.Name).ToArray());


            var fields = type.GetFields(
              System.Reflection.BindingFlags.Public |
              System.Reflection.BindingFlags.Instance |
              System.Reflection.BindingFlags.GetField);

            var goodFields = fields.Where(x => typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(x.FieldType));
            typeList.AddRange(goodFields.Select(x => x.FieldType).ToArray());
            nameList.AddRange(goodFields.Select(x => x.Name).ToArray());


        }
        private void UpdateMemberByType(System.Type type)
        {
            if (!viewMemberViewer.memberTypeDic.TryGetValue(type, out viewMemberViewer.currentTypes))
            {
                var typeList = new List<Type>();
                var nameList = new List<string>();
                var methods = new List<string>();
                CreateViewTypeInfos(type, typeList, nameList, methods);
                viewMemberViewer.memberTypeDic[type] = typeList.ToArray();
                viewMemberViewer.memberNameDic[type] = nameList.ToArray();
                viewMemberViewer.memberMethodDic[type] = methods;
                var viewNameList = new List<string>();
                for (int i = 0; i < typeList.Count; i++)
                {
                    viewNameList.Add(string.Format("{1} ({0})", typeList[i].Name, nameList[i]));
                }
                viewMemberViewer.memberViewNameDic[type] = viewNameList.ToArray();
            }

            viewMemberViewer.currentNames = viewMemberViewer.memberNameDic[type];
            viewMemberViewer.currentTypes = viewMemberViewer.memberTypeDic[type];
            viewMemberViewer.currentViewNames = viewMemberViewer.memberViewNameDic[type];
            viewMemberViewer.currentMethods = viewMemberViewer.memberMethodDic[type];
        }

        private static bool IsMemberSupported(System.Type type)
        {
            var attributes = type.GetCustomAttributes(true);
            if (attributes.Where(x => x is System.ObsoleteAttribute).Count() > 0)
            {
                return false;
            }
            return true;
        }

        private void UpdateEventByType(System.Type type)
        {
            if (!eventMemberViewer.memberTypeDic.TryGetValue(type, out eventMemberViewer.currentTypes))
            {
                var typeList = new List<Type>();
                var nameList = new List<string>();
                CreateEventTypeInfos(type, typeList, nameList);
                eventMemberViewer.memberTypeDic[type] = typeList.ToArray();
                eventMemberViewer.memberNameDic[type] = nameList.ToArray();
                var viewNameList = new List<string>();
                for (int i = 0; i < typeList.Count; i++)
                {
                    viewNameList.Add(string.Format("{1} ({0})", typeList[i].Name, nameList[i]));
                }
                eventMemberViewer.memberViewNameDic[type] = viewNameList.ToArray();
            }

            eventMemberViewer.currentNames = eventMemberViewer.memberNameDic[type];
            eventMemberViewer.currentTypes = eventMemberViewer.memberTypeDic[type];
            eventMemberViewer.currentViewNames = eventMemberViewer.memberViewNameDic[type];
        }
    }
    public class BindingEvent
    {
        public BindingType type;
        public string bindingSource;
        public string bindingTarget;
        public TypeInfo bindingTargetType;
    }
    public enum BindingType
    {
        Simple = 0,
        Full = 1
    }
    public class BindingShow
    {
        public bool isMethod;
        public string bindingSource;
        public string bindingTarget;
        public TypeInfo bindingTargetType;
        public string changeEvent;
    }
    [System.Serializable]
    public class ComponentBinding
    {
        public string name;
        public Object target;
        public bool expose;
        public List<BindingShow> viewItems = new List<BindingShow>();
        public List<BindingEvent> eventItems = new List<BindingEvent>();
        public Type targetType
        {
            get
            {
                if (target)
                    return target.GetType();
                return typeof(GameObject);
            }
        }
        public ComponentBinding() { }
        public ComponentBinding(Object target)
        {
            this.name = target.name;
            this.target = target;
        }
    }

    [System.Serializable]
    public struct TypeInfo
    {
        public System.Reflection.Assembly assemble
        {
            get
            {
                if (string.IsNullOrEmpty(assembleName))
                {
                    return null;
                }
                return System.Reflection.Assembly.Load(assembleName);
            }
        }
        public System.Type type
        {
            get
            {
                if (string.IsNullOrEmpty(typeName) || assemble == null)
                {
                    return null;
                }
                return assemble.GetType(typeName);
            }
        }

        public string assembleName;
        public string typeName;
        public TypeInfo(System.Type type)
        {
            this.assembleName = type.Assembly.ToString();
            this.typeName = type.FullName;
        }
        public void Update(System.Type type)
        {
            if (type == null)
            {
                UnityEngine.Debug.LogError("update typeinfo empty type!");
                return;
            }
            this.assembleName = type.Assembly.ToString();
            this.typeName = type.FullName;
        }
    }
}
