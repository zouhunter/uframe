using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using UFrame;

using UnityEditorInternal;

namespace UFrame.Refrence
{
    [CustomEditor(typeof(ObjectRefView), true)]
    public class ObjectRefEditor : Editor
    {
        protected SerializedProperty prop_description;
        protected SerializedProperty prop_script;
        protected ReorderableList m_list;
        protected int m_rightClickIndex;
        protected List<System.Type> m_subTypes;
        protected string m_matchText;
        protected ObjectRefView m_refView;
        protected virtual void OnEnable()
        {
            try
            {
                prop_description = serializedObject.FindProperty("description");
                m_refView = target as ObjectRefView;
                RebuildListView();
                InitSubTypes();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                DestroyImmediate(this);
            }
        }

        private void RebuildListView()
        {
            prop_script = serializedObject.FindProperty("m_Script");
            //prop_objRefs = serializedObject.FindProperty("objRefs");
            m_list = new ReorderableList(m_refView.objRefs, typeof(ObjectRef));
            m_list.drawHeaderCallback = DrawHeadRect;
            m_list.elementHeightCallback = DrawElementHeight;// EditorGUIUtility.singleLineHeight + 4;
            m_list.drawElementCallback = DrawElement;
            m_list.onSelectCallback = OnSelectItem;
        }

        private float DrawElementHeight(int index)
        {
            if (m_refView.objRefs.Count > index && !string.IsNullOrEmpty(m_matchText))
            {
                var propItem = m_refView.objRefs[index];
                var contentString = propItem.name + propItem.desc;
                if (propItem.target)
                {
                    contentString += propItem.target.name;
                }
                if (!contentString.ToLower().Contains(m_matchText.ToLower()))
                {
                    return EditorGUIUtility.singleLineHeight * 0.2f;
                }
            }
            return EditorGUIUtility.singleLineHeight + 4;
        }

        protected virtual void InitSubTypes()
        {
            if (m_subTypes == null)
            {
                m_subTypes = new List<System.Type>();
                var types = typeof(ObjectRefView).Assembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].IsSubclassOf(typeof(ObjectRefView)) && !types[i].IsAbstract)
                    {
                        m_subTypes.Add(types[i]);
                    }
                }
            }
        }

        private bool ItemRightClick(Rect contextRect, int index)
        {
            contextRect = contextRect.MoveX(-20);
            if (Event.current.type == EventType.ContextClick)
            {
                var mousePos = Event.current.mousePosition;

                if (contextRect.Contains(mousePos))
                {
                    // Now create the menu, add items and show it
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("拷贝"), false, GenericMenuCallback, 1);
                    if (ObjectRef.copyInstance != null)
                    {
                        menu.AddItem(new GUIContent("粘贴"), false, GenericMenuCallback, 2);
                    }
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("删除"), false, GenericMenuCallback, 3);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("插入"), false, GenericMenuCallback, 4);
                    menu.ShowAsContext();
                    Event.current.Use();
                    return true;
                }
            }
            return false;
        }

        private void GenericMenuCallback(object state)
        {
            var intState = (int)state;
            var rightClickProp = m_refView.objRefs[m_rightClickIndex];
            if (intState == 1 && rightClickProp != null)
            {
                ObjectRef.copyInstance = new ObjectRef();
                ObjectRef.copyInstance.name = rightClickProp.name;
                ObjectRef.copyInstance.desc = rightClickProp.desc;
                ObjectRef.copyInstance.target = rightClickProp.target;
            }
            else if (intState == 2 && ObjectRef.copyInstance != null && rightClickProp != null)
            {
                rightClickProp.name = ObjectRef.copyInstance.name;
                rightClickProp.desc = ObjectRef.copyInstance.desc;
                rightClickProp.target = ObjectRef.copyInstance.target;
                ObjectRef.copyInstance = null;
            }
            else if (intState == 3)
            {
                m_refView.objRefs.RemoveAt(m_rightClickIndex);
            }
            else if (intState == 4)
            {
                var objRef = new ObjectRef();
                m_refView.objRefs.Insert(m_rightClickIndex, objRef);
            }
            else
            {
                Debug.LogError("unhandled!");
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectItem(ReorderableList list)
        {
            if (m_refView.objRefs.Count <= list.index)
                return;

            var refItemProp = m_refView.objRefs[list.index];
            if (refItemProp.target)
            {
                Selection.activeObject = refItemProp.target;
            }
        }

        protected virtual void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_refView.objRefs.Count <= index)
                return;

            if (isActive)
            {
                EditorGUI.DrawRect(rect, Color.gray);
            }

            bool rightClicked = ItemRightClick(rect, index);

            var indexRect = rect.Move(-20, 2).ReSize(30, rect.height * 0.9f);
            var objRect = rect.Move(10, 2).ReSize(rect.width * 0.25f, rect.height * 0.9f);
            var nameRect = objRect.MoveX(objRect.width + 10).ReSizeW((rect.width - objRect.width - 10) * 0.3f);
            var descRect = nameRect.MoveX(nameRect.width + 10).ReSizeW((rect.width - 80 - nameRect.width - 20));

            EditorGUI.LabelField(indexRect, (index + 1).ToString(), EditorStyles.miniButtonRight);
            var refItemProp = m_refView.objRefs[index];
            if (refItemProp != null)
            {
                refItemProp.name = EditorGUI.TextField(nameRect, refItemProp.name);
                var newObjectRefValue = EditorGUI.ObjectField(objRect, refItemProp.target, typeof(UnityEngine.Object), true);
                if (newObjectRefValue != refItemProp.target)
                {
                    var ok = CheckIsValid(newObjectRefValue);
                    if (ok)
                    {
                        refItemProp.target = newObjectRefValue;
                    }
                }
                refItemProp.desc = EditorGUI.TextField(descRect, refItemProp.desc);

                if (rightClicked)
                {
                    m_rightClickIndex = index;
                }
            }
            if(refItemProp != null && refItemProp.target != null && PrefabUtility.IsPartOfPrefabInstance(refItemProp.target))
            {
                refItemProp.target = PrefabUtility.GetCorrespondingObjectFromSource(refItemProp.target);
            }
        }

        protected virtual void DrawHeadRect(Rect rect)
        {
            var labelRect = rect.ReSizeW(100);

            EditorGUI.LabelField(labelRect, "引用列表");

            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("拷贝"), false, (state)=> {
                    ObjectRef.copytedRefs = new List<ObjectRef>();
                    var refView = target as ObjectRefView;
                    ObjectRef.copytedRefs.AddRange(refView.objRefs);
                }, 1);

                if (ObjectRef.copytedRefs != null && ObjectRef.copytedRefs.Count > 0)
                {
                    menu.AddItem(new GUIContent("粘贴"), false, (state) =>
                    {
                        var refView = target as ObjectRefView;
                        for (int i = 0; i < ObjectRef.copytedRefs.Count; i++)
                        {
                            if (refView.objRefs.Find(x => x.target == ObjectRef.copytedRefs[i].target) != null)
                                continue;
                            refView.objRefs.Add(ObjectRef.copytedRefs[i]);
                        }
                    }, 2);
                }
                menu.ShowAsContext();
                Event.current.Use();
            }

            if (GUI.Button(labelRect, "", EditorStyles.label))
            {
                ProjectWindowUtil.ShowCreatedAsset(target);
            }

            var searchRect = rect.Move(110, 1).ReSize(90, EditorGUIUtility.singleLineHeight - 2);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                m_matchText = EditorGUI.TextField(searchRect, m_matchText);
                if (changeScope.changed)
                {
                    RebuildListView();
                }
            }

            var sortARect = searchRect.MoveX(searchRect.width + 10).ReSizeW(40);
            if (GUI.Button(sortARect, "升序", EditorStyles.miniButtonLeft))
            {
                var refView = target as ObjectRefView;
                refView.objRefs.Sort((x, y) => string.Compare(x.name, y.name));
                EditorUtility.SetDirty(target);
            }
            sortARect = sortARect.MoveX(sortARect.width + 5);
            if (GUI.Button(sortARect, "降序", EditorStyles.miniButtonRight))
            {
                var refView = target as ObjectRefView;
                refView.objRefs.Sort((x, y) => -string.Compare(x.name, y.name));
                EditorUtility.SetDirty(target);
            }
            var checkRect = rect.Move(rect.width - 60, 2).ReSize(30, EditorGUIUtility.singleLineHeight - 4);
            if (GUI.Button(checkRect, "导入", EditorStyles.miniButtonLeft))
            {
                var filepath = EditorUtility.OpenFilePanel("加载配制文件", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(filepath))
                {
                    var lines = System.IO.File.ReadAllLines(filepath,System.Text.Encoding.GetEncoding("gb2312"));
                    Dictionary<string, string> infoDic = new Dictionary<string, string>();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var str = lines[i].Trim();
                        var spiterIndex = str.IndexOf(",");
                        if (spiterIndex < 0 || spiterIndex > str.Length - 1)
                            continue;

                        infoDic[str.Substring(0, spiterIndex)] = str.Substring(spiterIndex + 1);
                    }
                    var refView = target as ObjectRefView;
                    for (int i = 0; i < refView.objRefs.Count; i++)
                    {
                        var refItem = refView.objRefs[i];
                        if (infoDic.TryGetValue(refItem.name, out string value))
                        {
                            refItem.desc = value;
                        }
                    }
                }

            }
            checkRect = checkRect.MoveX(30);
            if (GUI.Button(checkRect, "导出", EditorStyles.miniButtonLeft))
            {
                var filepath = EditorUtility.SaveFilePanel("保存配制文件", Application.dataPath, "ref_info", "csv");
                if (!string.IsNullOrEmpty(filepath))
                {
                    var refView = target as ObjectRefView;
                    System.Text.StringBuilder textBuilder = new System.Text.StringBuilder();
                    for (int i = 0; i < refView.objRefs.Count; i++)
                    {
                        var refItem = refView.objRefs[i];
                        textBuilder.AppendLine(string.Format("{0},{1}", refItem.name, refItem.desc));
                    }
                    using (var file = new System.IO.FileStream(filepath,System.IO.FileMode.OpenOrCreate))
                    {
                        file.Seek(0, System.IO.SeekOrigin.End);
                        var bytes = System.Text.Encoding.GetEncoding("gb2312").GetBytes(textBuilder.ToString());
                        file.Write(bytes,0, bytes.Length);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            var boxRect = new Rect(EditorGUIUtility.currentViewWidth - 10, 5, 10, EditorGUIUtility.singleLineHeight);

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (changeCheck.changed)
                {
                    target.name = prop_description.stringValue;
                }
            }
            ScriptTypeRightClick(boxRect);
            DrawListView();
            EditorUtility.SetDirty(target);
        }
        private void ScriptTypeRightClick(Rect contextRect)
        {
            if (Event.current.type == EventType.ContextClick)
            {
                var mousePos = Event.current.mousePosition;

                if (contextRect.Contains(mousePos))
                {
                    // Now create the menu, add items and show it
                    GenericMenu menu = new GenericMenu();
                    for (int i = 0; i < m_subTypes.Count; i++)
                    {
                        menu.AddItem(new GUIContent(m_subTypes[i].Name), false, OnChangeScriptCallBack, i);
                    }
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
        }

        private void OnChangeScriptCallBack(object index)
        {
            var id = (int)index;
            if (m_subTypes.Count > id)
            {
                var type = m_subTypes[id];
                var obj = ScriptableObject.CreateInstance(type);
                prop_script.objectReferenceValue = MonoScript.FromScriptableObject(obj);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void DrawListView()
        {
            serializedObject.Update();
            if (string.IsNullOrEmpty(prop_description.stringValue))
            {
                prop_description.stringValue = target.name;
            }
            m_list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            var lastRect = GUILayoutUtility.GetLastRect();
            var rect = GUILayoutUtility.GetRect(lastRect.width, EditorGUIUtility.singleLineHeight * 3);
            DragRect(rect);
        }


        protected virtual void DragRect(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    if (CheckObjectPlaceAble(DragAndDrop.objectReferences))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        var obj = DragAndDrop.objectReferences[i];
                        TryPlaceObject(obj);
                    }
                    DragAndDrop.AcceptDrag();
                    EditorUtility.SetDirty(target);
                }
            }
        }

        protected virtual void TryPlaceObject(Object obj)
        {
            var refView = target as ObjectRefView;
            var objs = FindSubObjects(obj);
            if (objs != null && objs.Count > 0)
            {
                foreach (var subObj in objs)
                {
                    var prefab = subObj;
                    if (PrefabUtility.IsPartOfPrefabInstance(subObj))
                        prefab = PrefabUtility.GetCorrespondingObjectFromSource(subObj);

                    if (refView && refView.objRefs.Find(x => x.target == prefab) == null && refView.objRefs.Find(x => x.name == prefab.name) == null)
                    {
                        var refItem = new ObjectRef();
                        refItem.target = prefab;
                        refItem.name = prefab.name;
                        refView.objRefs.Add(refItem);
                    }
                    else
                    {
                        Debug.Log("duplicate, ignore:" + subObj.name);
                    }
                }
            }
        }

        protected virtual List<Object> FindSubObjects(Object obj)
        {
            var list = new List<Object>();
            if (obj is DefaultAsset)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                FindSubObjectByDirectory(path, list);
            }
            else
            {
                FindSubObjectsBySingle(obj, list);
            }
            return list;
        }

        protected virtual void FindSubObjectsBySingle(Object obj, List<Object> list)
        {
            if (CheckIsValid(obj))
            {
                list.Add(obj);
            }
        }

        protected virtual void FindSubObjectByDirectory(string path, List<Object> list)
        {
            var subFolders = AssetDatabase.GetSubFolders(path);
            foreach (var subFolder in subFolders)
            {
                FindSubObjectByDirectory(subFolder, list);
            }
            var files = System.IO.Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (!file.ToLower().EndsWith(".meta"))
                {
                    var assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                    var assetItem = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    FindSubObjectsBySingle(assetItem, list);
                }
            }
        }

        protected bool CheckObjectPlaceAble(Object[] objects)
        {
            if (objects == null || objects.Length == 0)
                return false;
            var placeAble = false;
            foreach (var item in objects)
            {
                placeAble |= CheckPlaceAble(item);
            }
            return placeAble;
        }

        protected virtual bool CheckPlaceAble(Object obj)
        {
            return true;
        }

        protected virtual bool CheckIsValid(Object obj)
        {
            return true;
        }
    }

    [CustomEditor(typeof(ObjectRefView<>), true)]
    public class ObjectRefGenericEditor : ObjectRefEditor
    {
        protected System.Type m_contentType;
        protected override void OnEnable()
        {
            base.OnEnable();
            if (target)
            {
                var baseType = target.GetType().BaseType;
                if (baseType.IsGenericType)
                {
                    m_contentType = baseType.GenericTypeArguments[0];
                }
                else
                {
                    Debug.LogError(baseType);
                }
            }
        }
        protected override bool CheckIsValid(Object obj)
        {
            if (m_contentType != null)
            {
                if (obj.GetType().Equals(m_contentType))
                {
                    return true;
                }
                else if (obj is GameObject && m_contentType.IsSubclassOf(typeof(Component)))
                {
                    return true;
                }
                return false;
            }
            return base.CheckIsValid(obj);
        }

        protected override void FindSubObjectsBySingle(Object obj, List<Object> list)
        {
            if (CheckIsValid(obj))
            {
                if (obj.GetType().Equals(m_contentType))
                {
                    list.Add(obj);
                }
                else if (obj is GameObject && m_contentType.IsSubclassOf(typeof(Component)))
                {
                    list.Add((obj as GameObject).AddComponent(m_contentType));
                }
            }
        }

        protected override bool CheckPlaceAble(Object obj)
        {
            if (obj is DefaultAsset)
            {
                return true;
            }
            if (m_contentType != null)
            {
                return obj.GetType().Equals(m_contentType);
            }
            return base.CheckPlaceAble(obj);
        }
    }
}