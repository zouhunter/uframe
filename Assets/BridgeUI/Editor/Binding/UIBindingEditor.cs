//*************************************************************************************
//* 作    者： 
//* 创建时间： 2022-01-24 09:55:53
//*  描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using static UFrame.BridgeUI.UIBinding;
using System.Linq;
using UFrame.BridgeUI.Editors;

namespace UFrame.BridgeUI
{
    [DisallowMultipleComponent]
    [CustomEditor(typeof(UIBinding),true)]
    public class UIBindingEditor : Editor
    {
        protected SerializedProperty scriptProp;
        protected SerializedProperty typeFullNameProp;
        protected SerializedProperty logicScriptTypeProp;
        public ReorderableList m_reorderList;
        protected  SerializedProperty bindingInfoProp;
        protected UIBinding m_uiBinding;
        protected List<System.Type> supportedTypes;
        protected SerializedProperty currentProp;
        protected const float span = 10;
        protected const float half_span = 5;
        protected const float quad_span = 0.25f;
        protected ReferenceItem currentItem;
        protected bool m_isMvvm;
        protected ReorderableList m_refList;
        protected ReorderableList m_dataList;
        protected List<Object> m_refPool = new List<Object>();
        protected List<string> m_dataPool = new List<string>();
        protected bool m_changed;
        protected static Texture2D hierarchyEventIcon;
        protected static Texture2D hierarchyEventParentIcon;
        protected static HashSet<int> referencedGameObjects = new HashSet<int>();
        protected static HashSet<int> childInstanceIds = new HashSet<int>();
        protected static Texture2D HierarchyEventIcon
        {
            get
            {
                if (hierarchyEventIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("dded146e9a6d3a648ad7afa40e6bfdec");//ref_mark
                    hierarchyEventIcon = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return hierarchyEventIcon;
            }
        }
        protected static Texture2D HierarchyEventParentIcon
        {
            get
            {
                if (hierarchyEventParentIcon == null)
                {
                    var path = AssetDatabase.GUIDToAssetPath("dddce5da2dd6c5a43ba264fb665b3400");//ref_mark parent
                    hierarchyEventParentIcon = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                return hierarchyEventParentIcon;
            }
        }
        static UIBindingEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
        }

        protected virtual void OnEnable()
        {
            m_uiBinding = target as UIBinding;

            if (!m_uiBinding)
                return;

            if (m_uiBinding.infos == null)
                m_uiBinding.infos = new List<ReferenceItem>();
            scriptProp = serializedObject.FindProperty("m_Script");
            bindingInfoProp = serializedObject.FindProperty("infos");
            CreateReorderList();
            CreateRefList();
            CreateDataList();
            UpdateRefScriptFullName();
            supportedTypes = Utility.InitSupportTypes();

            InitChildItems();
            CheckIsMvvm();
            RefreshRefDraw();
        }

        protected virtual void InitChildItems()
        {
            var childs = m_uiBinding.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < childs.Length; i++)
            {
                childInstanceIds.Add(childs[i].gameObject.GetInstanceID());
            }
            childInstanceIds.Remove(m_uiBinding.gameObject.GetInstanceID());
        }

        protected virtual void CheckIsMvvm()
        {
            if (!string.IsNullOrEmpty(typeFullNameProp.stringValue))
            {
                var typeFull = Utility.FindTypeInAllAssemble(typeFullNameProp.stringValue);
                if (typeFull != null && typeof(ViewModel).IsAssignableFrom(typeFull))
                    m_isMvvm = true;
                else if (typeFull != null && typeof(BindingViewBase).IsAssignableFrom(typeFull))
                    m_isMvvm = true;
                else
                    m_isMvvm = false;
            }
        }

        protected virtual void CreateRefList()
        {
            m_refList = new ReorderableList(m_refPool, typeof(Object));
            m_refList.drawHeaderCallback = DrawRefHead;
            m_refList.elementHeight = EditorGUIUtility.singleLineHeight + 10;
            m_refList.drawElementCallback = DrawRefElement;
        }

        protected virtual void CreateDataList()
        {
            m_dataList = new ReorderableList(m_dataPool, typeof(string));
            m_dataList.drawHeaderCallback = DrawRefHead;
            m_dataList.elementHeight = EditorGUIUtility.singleLineHeight + 10;
            m_dataList.drawElementCallback = DrawDataElement;
        }

        protected virtual void DrawRefElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            if (currentItem != null && m_refPool != null && m_refPool.Count > index)
            {
                var innerRect = new Rect(rect.x + span, rect.y + half_span, rect.width - span, rect.height - span);
                m_refPool[index] = EditorGUI.ObjectField(innerRect, GUIContent.none, m_refPool[index], currentItem.type, true);
            }
            if (EditorGUI.EndChangeCheck())
                m_changed = true;
        }

        protected virtual void DrawDataElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            if (currentItem != null && m_dataPool != null && m_dataPool.Count > index)
            {
                var innerRect = new Rect(rect.x + span, rect.y + half_span, rect.width - span, rect.height - span);
                m_dataPool[index] = DrawElement(innerRect, currentItem.type, m_dataPool[index]);
            }
            if (EditorGUI.EndChangeCheck())
                m_changed = true;
        }

        protected virtual void DrawRefHead(Rect rect)
        {
            if (currentItem != null)
            {
                EditorGUI.LabelField(rect, currentItem.name);
                if (currentItem.type != null && typeof(Object).IsAssignableFrom(currentItem.type))
                {
                    DrawDragContent(rect, currentItem);
                }
            }
        }

        protected virtual void DrawDragContent(Rect rect, ReferenceItem refrenceItem)
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        Object obj = DragAndDrop.objectReferences[i];
                        if (obj.GetType() == refrenceItem.type)
                        {
                            m_refPool.Add(obj);
                            m_changed = true;
                        }
                        else if (obj is GameObject && typeof(Component).IsAssignableFrom(refrenceItem.type))
                        {
                            var componentItem = (obj as GameObject).GetComponent(refrenceItem.type);
                            m_refPool.Add(componentItem);
                            m_changed = true;
                        }
                        else if (obj is Component && typeof(GameObject) == refrenceItem.type)
                        {
                            var go = (obj as Component).gameObject;
                            m_refPool.Add(go);
                            m_changed = true;
                        }
                    }
                }
            }
        }

        protected virtual void CreateReorderList()
        {
            m_reorderList = new ReorderableList(m_uiBinding.infos, typeof(ReferenceItem));
            m_reorderList.displayAdd = true;
            m_reorderList.displayRemove = true;
            m_reorderList.elementHeight = EditorGUIUtility.singleLineHeight + span;
            m_reorderList.drawHeaderCallback = DrawListHead;
            m_reorderList.drawElementCallback = DrawElement;
            m_reorderList.onChangedCallback = (list) => RefreshRefDraw();
        }

        protected virtual void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var referenceItems = m_uiBinding.infos;
            if (referenceItems.Count > index && index >= 0)
            {
                var boxRect = new Rect(rect.x + half_span, rect.y + quad_span, rect.width, rect.height - quad_span);
                GUI.Box(boxRect, "");
                var innerRect = new Rect(rect.x + span, rect.y + half_span, rect.width - span, rect.height - span);
                var idRect = new Rect(rect.x - 15, rect.y + half_span, 40, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(idRect, index.ToString("00"));
                var referenceItem = referenceItems[index];
                var nameRect = new Rect(innerRect.x, innerRect.y, innerRect.width * 0.2f, innerRect.height);
                EditorGUI.BeginChangeCheck();
                referenceItem.name = EditorGUI.TextField(nameRect, referenceItem.name);

                if (referenceItem.type == null && !string.IsNullOrEmpty(referenceItem.typeFullName))
                {
                    referenceItem.type = BridgeUI.Utility.FindTypeInAllAssemble(referenceItem.typeFullName);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (referenceItem.type == typeof(GameObject))
                    {
                        referenceItem.referenceTarget.name = referenceItem.name;
                    }
                }

                var isArrayRect = new Rect(innerRect.x + innerRect.width - 20, innerRect.y, 10, innerRect.height);
                DrawIsArrayField(isArrayRect, referenceItem);

                var typeRect = new Rect(innerRect.x + innerRect.width * 0.25f, innerRect.y, innerRect.width * 0.3f, innerRect.height);
                GUI.Box(typeRect, "");
                DrawTypeField(typeRect, referenceItem);

                var contentRect = new Rect(innerRect.x + innerRect.width * 0.6f, innerRect.y, innerRect.width * 0.3f, innerRect.height);
                if (referenceItem.type != null)
                {
                    var refType = GetReferenceType(referenceItem.type, referenceItem.isArray);
                    switch (refType)
                    {
                        case ReferenceItemType.Reference:
                            referenceItem.referenceTarget = EditorGUI.ObjectField(contentRect, GUIContent.none, referenceItem.referenceTarget, referenceItem.type, true);
                            break;
                        case ReferenceItemType.ConventAble:
                        case ReferenceItemType.Struct:
                            referenceItem.value = DrawElement(contentRect, referenceItem.type, referenceItem.value);
                            break;
                        case ReferenceItemType.ReferenceArray:
                            if (referenceItem.referenceTargets == null)
                                referenceItem.referenceTargets = new List<Object>();
                            EditorGUI.LabelField(contentRect, "(引用数组)" + referenceItem.referenceTargets.Count);
                            break;
                        case ReferenceItemType.ConventAbleArray:
                            if (referenceItem.values == null)
                                referenceItem.values = new List<string>();
                            EditorGUI.LabelField(contentRect, "(简单数组)" + referenceItem.values.Count);
                            break;
                        case ReferenceItemType.StructArray:
                            if (referenceItem.values == null)
                                referenceItem.values = new List<string>();
                            EditorGUI.LabelField(contentRect, "(结构数组)" + referenceItem.values.Count);
                            break;
                        default:
                            break;
                    }
                }

                if (isActive)
                {
                    DrawActiveReferenceItem(referenceItem);
                    referenceItem.desc = GUILayout.TextField(referenceItem.desc, EditorStyles.toolbarButton);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_changed = true;
            }
        }

        protected virtual void DrawActiveReferenceItem(ReferenceItem referenceItem)
        {
            if (!referenceItem.isArray)
            {
                return;
            }
            else
            {
                currentItem = referenceItem;
                if (typeof(Object).IsAssignableFrom(currentItem.type))
                {
                    m_refPool.Clear();
                    if (currentItem.referenceTargets != null)
                        m_refPool.AddRange(currentItem.referenceTargets);
                    m_refList.DoLayoutList();
                    if (m_refPool.Count > 0)
                    {
                        if (currentItem.referenceTargets == null)
                            currentItem.referenceTargets = new List<Object>();
                        currentItem.referenceTargets.Clear();
                        currentItem.referenceTargets.AddRange(m_refPool);
                    }
                }
                else
                {
                    m_dataPool.Clear();
                    if (currentItem.values != null && currentItem.values.Count > 0)
                        m_dataPool.AddRange(currentItem.values);
                    m_dataList.DoLayoutList();
                    if (m_dataPool.Count > 0)
                    {
                        if (currentItem.values == null)
                            currentItem.values = new List<string>();
                        currentItem.values.Clear();
                        currentItem.values.AddRange(m_dataPool);
                    }
                }
            }
        }

        protected virtual string DrawElement(Rect contentRect, System.Type type, string value)
        {
            if (typeof(System.IConvertible).IsAssignableFrom(type))
            {
                value = EditorGUI.TextField(contentRect, value);
                try
                {
                    if (type != typeof(string))
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            System.Convert.ChangeType(value, type);
                        }
                        else
                        {
                            value = System.Activator.CreateInstance(type).ToString();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log("值转换失败：" + type.FullName + " -> detail:" + e);
                }
            }
            else if (type == typeof(Color))
            {
                var vectorValue = Utility.ChangeType<Color>(value);
                vectorValue = EditorGUI.ColorField(contentRect, "", vectorValue);
                value = Utility.ChangeType(vectorValue);
            }
            else if (type == typeof(Vector2))
            {
                var vectorValue = Utility.ChangeType<Vector2>(value);
                vectorValue = EditorGUI.Vector2Field(contentRect, "", vectorValue);
                value = Utility.ChangeType(vectorValue);
            }
            else if (type == typeof(Vector3))
            {
                var vectorValue = Utility.ChangeType<Vector3>(value);
                vectorValue = EditorGUI.Vector3Field(contentRect, "", vectorValue);
                value = Utility.ChangeType(vectorValue);
            }
            else if (type == typeof(Vector4))
            {
                var vectorValue = Utility.ChangeType<Vector4>(value);
                vectorValue = EditorGUI.Vector4Field(contentRect, "", vectorValue);
                value = Utility.ChangeType(vectorValue);
            }
            else if (type == typeof(Vector2Int))
            {
                var vectorValue = Utility.ChangeType<Vector2Int>(value);
                vectorValue = EditorGUI.Vector2IntField(contentRect, "", vectorValue);
                value = Utility.ChangeType(vectorValue);
            }
            else if (type == typeof(Vector3Int))
            {
                var vectorValue = Utility.ChangeType<Vector3Int>(value);
                vectorValue = EditorGUI.Vector3IntField(contentRect, "", vectorValue);
                value = Utility.ChangeType(vectorValue);
            }
            else
            {
                EditorGUI.LabelField(contentRect, "(结构)");
            }
            return value;
        }

        protected virtual void DrawIsArrayField(Rect isArrayRect, ReferenceItem referenceItem)
        {
            EditorGUI.BeginChangeCheck();
            referenceItem.isArray = EditorGUI.Toggle(isArrayRect, referenceItem.isArray, EditorStyles.radioButton);
            if (EditorGUI.EndChangeCheck())
            {
                //将单个类型自动添加到数组
            }
        }

        protected virtual void DrawTypeField(Rect rect, ReferenceItem item)
        {
            if (item.type != null)
            {
                EditorGUI.LabelField(rect, item.type.Name);
            }
            else
            {
                EditorGUI.LabelField(rect, "未指定类型");
            }

            if (Event.current.type == EventType.ContextClick)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    currentItem = item;
                    var changeTypeMenu = new GenericMenu();
                    if (currentItem.referenceTarget != null)
                    {
                        TryAddMenuFromTarget(currentItem.referenceTarget, item.type, changeTypeMenu);
                    }
                    if (currentItem.referenceTargets != null && currentItem.referenceTargets.Count > 0)
                    {
                        TryAddMenuFromTarget(currentItem.referenceTargets[0], item.type, changeTypeMenu);
                    }
                    for (int i = 0; i < supportedTypes.Count; i++)
                    {
                        var type = supportedTypes[i];
                        if (!CheckExists(currentItem, type))
                            continue;
                        changeTypeMenu.AddItem(new GUIContent(type.FullName), type == item.type, OnChangedType, type);
                    }
                    changeTypeMenu.ShowAsContext();
                }
            }
        }

        protected virtual bool CheckExists(ReferenceItem target, System.Type type)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return true;
            }

            if (target.isArray)
            {
                return true;
            }
            else
            {
                if (target.referenceTarget == null)
                    return true;

                if (!typeof(Component).IsAssignableFrom(type))
                    return true;

                if (target.referenceTarget is GameObject)
                {
                    return (target.referenceTarget as GameObject).GetComponent(type);
                }
                else if (target.referenceTarget is Component)
                {
                    return (target.referenceTarget as Component).GetComponent(type);
                }
                return false;
            }
        }

        protected virtual void TryAddMenuFromTarget(Object target, System.Type currentType, GenericMenu menu)
        {
            var uiControl = TryGetControl(target);

            if (uiControl != null)
            {
                var type = uiControl.GetType();
                var select = currentType == type;
                menu.AddItem(new GUIContent(type.FullName), select, OnChangedType, type);
            }
            else
            {
                MonoBehaviour[] components = null;
                if (target is GameObject)
                {
                    components = (target as GameObject).GetComponents<MonoBehaviour>();
                }
                else if (target is Component)
                {
                    components = (target as Component).GetComponents<MonoBehaviour>();
                }
                if (components != null && components.Length > 0)
                {
                    foreach (var comp in components)
                    {
                        var type = comp.GetType();
                        var select = currentType == type;
                        menu.AddItem(new GUIContent(type.FullName), select, OnChangedType, type);
                    }
                }
            }
        }

        protected virtual void OnChangedType(object data)
        {
            if (currentItem != null)
            {
                var type = (System.Type)data;
                currentItem.type = type;
                currentItem.typeFullName = type.FullName;

                var refType = GetReferenceType(currentItem.type, currentItem.isArray);
                switch (refType)
                {
                    case ReferenceItemType.Reference:
                        {
                            //类型不匹配的问题
                            if (currentItem.referenceTarget != null && currentItem.referenceTarget.GetType() != currentItem.type)
                            {
                                currentItem.referenceTarget = WorpObject(currentItem.referenceTarget, currentItem.type);
                            }

                            //默认值
                            if (currentItem.referenceTarget == null && currentItem.referenceTargets != null && currentItem.referenceTargets.Count > 0)
                            {
                                var arrayItem = currentItem.referenceTargets[0];
                                if (arrayItem.GetType() == currentItem.type)
                                {
                                    currentItem.referenceTarget = arrayItem;
                                }
                                else
                                {
                                    currentItem.referenceTarget = WorpObject(arrayItem, currentItem.type);
                                }
                            }
                        }

                        break;
                    case ReferenceItemType.ConventAble:
                        break;
                    case ReferenceItemType.Struct:
                        break;
                    case ReferenceItemType.ReferenceArray:
                        {
                            if (currentItem.referenceTargets == null)
                                currentItem.referenceTargets = new List<Object>();

                            if (currentItem.referenceTargets.Count > 0)
                            {
                                for (int i = 0; i < currentItem.referenceTargets.Count; i++)
                                {
                                    var item = currentItem.referenceTargets[i];
                                    if (item != null)
                                    {
                                        if (item.GetType() != currentItem.type)
                                        {
                                            currentItem.referenceTargets[i] = WorpObject(item, currentItem.type);
                                        }
                                    }
                                }
                            }
                            else if (currentItem.referenceTarget != null)
                            {
                                currentItem.referenceTargets.Add(WorpObject(currentItem.referenceTarget, currentItem.type));
                            }
                        }
                        break;
                    case ReferenceItemType.ConventAbleArray:
                        break;
                    case ReferenceItemType.StructArray:
                        break;
                    default:
                        break;
                }
            }
        }

        protected virtual Object WorpObject(Object currentItemreferenceTarget, System.Type currentItemtype)
        {
            if (currentItemreferenceTarget == null) return null;

            if (currentItemreferenceTarget.GetType() == currentItemtype)
            {
                return currentItemreferenceTarget;
            }

            if (currentItemreferenceTarget is GameObject)
            {
                if (typeof(Component).IsAssignableFrom(currentItemtype))
                {
                    currentItemreferenceTarget = (currentItemreferenceTarget as GameObject).GetComponent(currentItemtype);
                }
                else
                {
                    currentItemreferenceTarget = null;
                }
            }
            else
            {
                if (typeof(Component).IsAssignableFrom(currentItemtype))
                {
                    currentItemreferenceTarget = (currentItemreferenceTarget as Component).GetComponent(currentItemtype);
                }
                else if (currentItemtype == typeof(GameObject))
                {
                    currentItemreferenceTarget = (currentItemreferenceTarget as Component).gameObject;
                }
                else
                {
                    currentItemreferenceTarget = null;
                }
            }
            return currentItemreferenceTarget;
        }

        protected virtual IUIControl TryGetControl(Object target)
        {
            IUIControl uiControl = null;
            if (target is GameObject)
            {
                uiControl = (target as GameObject).GetComponent<IUIControl>();

            }
            if (target is Component)
            {
                uiControl = (target as Component).GetComponent<IUIControl>();
            }

            return uiControl;
        }

        protected virtual ReferenceItemType GetReferenceType(System.Type type, bool isArray)
        {
            if (type.IsArray)
            {
                var arryType = type.GetElementType();
                if (typeof(UnityEngine.Object).IsAssignableFrom(arryType))
                {
                    return ReferenceItemType.ReferenceArray;
                }
                else if (typeof(System.IConvertible).IsAssignableFrom(arryType))
                {
                    return ReferenceItemType.ConventAbleArray;
                }
                else
                {
                    return ReferenceItemType.StructArray;
                }
            }
            else if (isArray)
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                {
                    return ReferenceItemType.ReferenceArray;
                }
                else if (typeof(System.IConvertible).IsAssignableFrom(type))
                {
                    return ReferenceItemType.ConventAbleArray;
                }
                else
                {
                    return ReferenceItemType.StructArray;
                }
            }
            else
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                {
                    return ReferenceItemType.Reference;
                }
                else if (typeof(System.IConvertible).IsAssignableFrom(type))
                {
                    return ReferenceItemType.ConventAble;
                }
                else
                {
                    return ReferenceItemType.Struct;
                }
            }
        }

        protected virtual void DrawListHead(Rect rect)
        {
            var buttonRect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
            if (GUI.Button(buttonRect, "apply"))
            {
                EditorUtility.SetDirty(m_uiBinding);
                ApplyChangesToScript();
            }
            EditorGUI.LabelField(rect, "数据及引用信息列表");
            if (Event.current.type == EventType.DragUpdated)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        GameObject target = null;
                        Object obj = DragAndDrop.objectReferences[i];

                        if (CheckNameExists(obj.name))
                            continue;

                        if (obj is GameObject)
                        {
                            target = obj as GameObject;
                            obj = SortObjectType(target);
                        }
                        else if (obj is Component)
                        {
                            target = (obj as Component).gameObject;
                        }
                        ReferenceItem refItem = new ReferenceItem();
                        m_uiBinding.infos.Add(refItem);
                        refItem.name = obj.name;
                        refItem.referenceTarget = obj;
                        refItem.typeFullName = obj.GetType().FullName;
                        refItem.type = obj.GetType();
                        EditorUtility.SetDirty(m_uiBinding);
                    }
                }
            }
        }

        protected virtual bool CheckNameExists(string name)
        {
            for (int i = 0; i < bindingInfoProp.arraySize; i++)
            {
                var element = bindingInfoProp.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("name").stringValue == name)
                    return true;
            }
            return false;
        }

        protected virtual Object SortObjectType(GameObject go)
        {
            var uiControl = go.GetComponent<IUIControl>();
            supportedTypes = Utility.InitSupportTypes();

            if (uiControl != null)
            {
                return uiControl as Component;
            }
            else
            {
                var behaviours = go.GetComponents<MonoBehaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (!supportedTypes.Contains(behaviour.GetType()))
                        return behaviour;
                }
            }
            for (int i = 0; i < supportedTypes.Count; i++)
            {
                var type = supportedTypes[i];
                if (typeof(Component).IsAssignableFrom(type))
                {
                    var obj = go.GetComponent(type);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            return go;
        }

        protected virtual void ApplyChangesToScript()
        {
            var components = new List<ComponentItem>();
            var referenceItems = new List<ReferenceItem>();
            var rule = new GenCodeRule(UISetting.defultNameSpace);
            GenCodeUtil.AnalysisComponent(m_uiBinding, components, referenceItems, rule);
            GenCodeUtil.UpdateBindingScripts(m_uiBinding.gameObject, components, referenceItems, rule);
        }

        protected virtual void UpdateRefScriptFullName()
        {
            if (target)
            {
                typeFullNameProp = serializedObject.FindProperty("m_viewTypeName");
                if (typeFullNameProp != null && string.IsNullOrEmpty(typeFullNameProp.stringValue))
                {
                    if (string.IsNullOrEmpty(UISetting.defultNameSpace))
                    {
                        typeFullNameProp.stringValue = target.name;
                    }
                    else
                    {
                        typeFullNameProp.stringValue = string.Format("{0}.{1}", UISetting.defultNameSpace, target.name);
                    }
                }

                CheckIsMvvm();

                logicScriptTypeProp = serializedObject.FindProperty("m_logicTypeName");
                if (string.IsNullOrEmpty(logicScriptTypeProp.stringValue))
                {
                    logicScriptTypeProp.stringValue = UISetting.defultNameSpace + "." + m_uiBinding.gameObject.name + "Logic";
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            using (var disableScope = new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProp);
            }
            serializedObject.Update();
            DrawScriptTypeName();
            if (m_isMvvm)
            {
                EditorGUILayout.PropertyField(logicScriptTypeProp);
            }
            m_reorderList.DoLayoutList();
            if (m_changed)
            {
                m_changed = false;
                EditorUtility.SetDirty(m_uiBinding);
                Debug.Log("apply ui binding changed!");
            }
            var propChanged = serializedObject.ApplyModifiedProperties();
            if (propChanged)
                RefreshRefDraw();
        }

        protected virtual void DrawScriptTypeName()
        {
            EditorGUILayout.PropertyField(typeFullNameProp);
        }

        protected virtual void RefreshRefDraw()
        {
            referencedGameObjects.RemoveWhere(x => childInstanceIds.Contains(x));

            var referenceTargets = new List<Object>();

            foreach (var info in m_uiBinding.infos)
            {
                if (info.referenceTarget)
                {
                    referenceTargets.Add(info.referenceTarget);
                }
                if (info.referenceTargets != null)
                {
                    foreach (var subInfo in info.referenceTargets)
                    {
                        if (subInfo)
                            referenceTargets.Add(subInfo);
                    }
                }
            }

            for (int i = 0; i < referenceTargets.Count; i++)
            {
                var referenceTarget = referenceTargets[i];

                if (referenceTarget)
                {
                    if (referenceTarget is Component)
                    {
                        referencedGameObjects.Add((referenceTarget as Component).gameObject.GetInstanceID());
                    }
                    else if (referenceTarget is GameObject)
                    {
                        referencedGameObjects.Add((referenceTarget as GameObject).GetInstanceID());
                    }
                }
            }

        }
        protected static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
        {
            if (!childInstanceIds.Contains(instanceID))
                return;

            if (referencedGameObjects.Contains(instanceID))
            {
                Rect rect = new Rect(selectionRect.x + selectionRect.width - 16f, selectionRect.y + 4f, 16f, 8f);
                GUI.DrawTexture(rect, HierarchyEventIcon);
            }
            else
            {
                var target = EditorUtility.InstanceIDToObject(instanceID);
                if (target && target is GameObject)
                {
                    var childs = (target as GameObject).GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < childs.Length; i++)
                    {
                        if (referencedGameObjects.Contains(childs[i].gameObject.GetInstanceID()))
                        {
                            Rect rect = new Rect(selectionRect.x + selectionRect.width - 16, selectionRect.y + 4f, 16f, 8f);
                            GUI.DrawTexture(rect, HierarchyEventParentIcon);
                            break;
                        }
                    }
                }
            }
        }
    }
}