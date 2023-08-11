//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-19
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UFrame.DressAB.Editors
{
    [CustomEditor(typeof(AddressDefineObject))]
    public class AddressDefineObjectEditor : Editor
    {
        protected override void OnHeaderGUI()
        {
            var isDefault = AddressDefineObjectSetting.Instance.activeAddressDefineObject == target;
            if (isDefault)
            {
                GUILayout.Label("(Global)");
            }
            else
            {
                if (GUILayout.Button("Set Global"))
                {
                    AddressDefineObjectSetting.Instance.activeAddressDefineObject = target as AddressDefineObject;
                    AddressDefineObjectSetting.Save();
                }
            }
            base.OnHeaderGUI();
        }

        [MenuItem("CONTEXT/AddressDefineObject/SetDefault")]
        public static void SetDefault(MenuCommand cmd)
        {
            AddressDefineObjectSetting.Instance.activeAddressDefineObject = cmd.context as AddressDefineObject;
            AddressDefineObjectSetting.Save();
        }

        [MenuItem("CONTEXT/AddressDefineObject/SetDefault", true)]
        public static bool SetDefaultValid(MenuCommand cmd)
        {
            return AddressDefineObjectSetting.Instance.activeAddressDefineObject != cmd.context;
        }

        private ReorderableList m_reorderableList;
        private ReorderableList m_subReorderableList;
        private SerializedProperty m_scriptProp;
        public AddressDefineObject m_defineObj;
        private Dictionary<string, Object> m_guidMap;
        private List<AddressGatherInfo> m_gatherInfos;
        private Vector2 m_scrollPos;
        private bool m_error;
        private string m_matchStr;
        private ushort m_matchFlag;
        private GUIContent m_searchContent;
        private GUIContent m_sliceContent;
        private bool m_drawSubList = false;
        private List<AddressInfo> m_addressList;
        private List<AssetRefBundle> m_subBundleList;
        private HashSet<string> m_uniqueNameSet;
        private void OnEnable()
        {
            m_defineObj = target as AddressDefineObject;
            m_scriptProp = serializedObject.FindProperty("m_Script");
            m_addressList = new List<AddressInfo>();
            m_subBundleList = new List<AssetRefBundle>();
            m_gatherInfos = new List<AddressGatherInfo>();
            m_guidMap = new Dictionary<string, Object>();
            m_searchContent = EditorGUIUtility.IconContent("d_SearchDatabase Icon", "search");
            m_sliceContent = EditorGUIUtility.IconContent("d_SpriteAtlas On Icon", "auto slice asset bundle");
            m_uniqueNameSet = new HashSet<string>();
            RefreshReorderList();
            RefreshSubBundleList();
            Undo.undoRedoPerformed += OnUndo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        private void OnUndo()
        {
            RefreshReorderList();
            RefreshSubBundleList();
        }

        private void RefreshReorderList()
        {
            m_addressList.Clear();
            foreach (var info in m_defineObj.addressList)
            {
                if (m_matchFlag != 0 && (info.flags & m_matchFlag) == 0)
                    continue;

                if (!string.IsNullOrEmpty(m_matchStr) && !string.IsNullOrEmpty(info.address) && !info.address.ToLower().Contains(m_matchStr.ToLower()))
                    continue;

                m_addressList.Add(info);
            }
            m_reorderableList = new ReorderableList(m_addressList, typeof(AddressInfo));
            m_reorderableList.drawHeaderCallback = DrawListHeader;
            m_reorderableList.drawElementCallback = DrawListElement;
            m_reorderableList.elementHeightCallback = DrawElementHight;
            m_reorderableList.displayAdd = true;
            m_reorderableList.displayRemove = true;
            m_reorderableList.onAddCallback = (x) =>
            {
                OnInsertAddressItem(x.count);
            };
            m_reorderableList.onRemoveCallback = (x) =>
            {
                OnDeleteAddressItem(x.index);
            };
        }

        private void RefreshSubBundleList()
        {
            m_subBundleList.Clear();
            foreach (var info in m_defineObj.refBundleList)
            {
                if (!string.IsNullOrEmpty(m_matchStr) && !string.IsNullOrEmpty(info.address) && !info.address.ToLower().Contains(m_matchStr.ToLower()))
                    continue;
                m_subBundleList.Add(info);
            }
            m_subReorderableList = new ReorderableList(m_subBundleList, typeof(AssetRefBundle));
            m_subReorderableList.drawHeaderCallback = DrawSubListHeader;
            m_subReorderableList.drawElementCallback = DrawSubListElement;
            m_subReorderableList.elementHeightCallback = DrawSubElementHight;
            m_subReorderableList.onAddCallback = (x) =>
            {
                OnInsertSubAssetBundleItem(x.count);
            };
            m_subReorderableList.onRemoveCallback = (x) =>
            {
                OnDeleteSubAssetBundleItem(x.index);
            };
        }

        private void DrawSubListHeader(Rect rect)
        {
            var lastColor = GUI.color;
            GUI.color = m_error ? Color.red : Color.green;
            var title = "[SubBundleList]";
            if (AddressDefineObjectSetting.Instance.activeAddressDefineObject == m_defineObj)
            {
                title += " (Default)";
            }
            var labelRect = new Rect(rect.x, rect.y, 160, rect.height);
            GUI.Label(labelRect, title, EditorStyles.label);
            GUI.color = lastColor;
            var matchRect = new Rect(rect.width - 130, rect.y, 100, rect.height);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                m_matchStr = EditorGUI.TextField(matchRect, m_matchStr);
                if (changeScope.changed && string.IsNullOrEmpty(m_matchStr))
                {
                    RefreshSubBundleList();
                }
            }
            var searchRect = new Rect(rect.width - 30, rect.y, 30, rect.height);
            if (GUI.Button(searchRect, m_searchContent))
            {
                RefreshSubBundleList();
            }
            m_error = false;
        }

        private void DrawSubListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= m_subBundleList.Count)
                return;

            var info = m_subBundleList[index];
            var guidTargets = new Object[info.guids.Count];
            bool error = false;
            for (int i = 0; i < info.guids.Count; i++)
            {
                var guid = info.guids[i];
                Object guidTarget = null;
                if (!string.IsNullOrEmpty(guid) && !m_guidMap.TryGetValue(guid, out guidTarget))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        guidTarget = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        if (guidTarget)
                        {
                            m_guidMap[guid] = guidTarget;
                        }
                    }
                }

                guidTargets[i] = guidTarget;
                error = !guidTarget;
            }
            var lastColor = GUI.color;
            if (error)
            {
                GUI.color = Color.red;
                m_error = true;
            }

            var idRect = new Rect(rect.x - 20, rect.y, 40, rect.height);
            GUI.Label(idRect, (index + 1).ToString("000"), EditorStyles.miniBoldLabel);
            GUI.color = lastColor;

            if (idRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("delete"), false, OnDeleteSubAssetBundleItem, index);
                menu.AddItem(new GUIContent("insert"), false, OnInsertSubAssetBundleItem, index);
                menu.ShowAsContext();
            }

            bool changed = false;
            var addressRect = new Rect(rect.x + 10, rect.y + 2, rect.width * 0.5f - 10, rect.height - 4);
            var address = EditorGUI.TextField(addressRect, info.address);
            if (address != info.address)
            {
                Undo.RecordObject(target, "text changed");
                info.address = address;
                changed = true;
            }

            var nameRectX = rect.width * 0.5f + 30;
            for (int i = 0; i < guidTargets.Length; i++)
            {
                var nameRect = new Rect(nameRectX + 20 * i, rect.y + 2, 20, rect.height - 4);

                var guidTarget = guidTargets[i];
                var guid = info.guids[i];

                if (nameRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("delete"), false, OnDeleteSubAssetBundleGuid, guid);
                    menu.ShowAsContext();
                    Event.current.Use();
                }

                if (guidTarget)
                {
                    var path = AssetDatabase.GetAssetPath(guidTarget);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (GUI.Button(nameRect, new GUIContent((i + 1).ToString(), path)))
                        {
                            EditorGUIUtility.PingObject(guidTarget);
                        }
                    }

                    if (AcceptRegion(nameRect))
                    {
                        var gatherInfo = m_gatherInfos[0];
                        if (gatherInfo.addressInfo != null)
                            gatherInfo.addressInfo.active = false;
                        var id = m_defineObj.refBundleList.FindIndex(x => x.guids.Contains(gatherInfo.guid));
                        if (id < 0)
                        {
                            info.guids[i] = gatherInfo.guid;
                            changed = true;
                        }
                        else
                        {
                            m_subReorderableList.Select(id);
                        }
                    }
                }
                else
                {
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        guidTarget = EditorGUI.ObjectField(nameRect, guidTarget, typeof(Object), false);
                        if (changeScope.changed)
                        {
                            changed = true;

                            if (guidTarget)
                            {
                                var path = AssetDatabase.GetAssetPath(guidTarget);
                                info.guids[i] = AssetDatabase.GUIDFromAssetPath(path).ToString();
                            }
                            else
                            {
                                info.guids.RemoveAt(i);
                                break;
                            }
                        }
                    }

                }
            }

            if (nameRectX < rect.width)
            {
                var addRect = new Rect(nameRectX, rect.y + 2, rect.width - nameRectX, rect.height - 4);
                if (AcceptRegion(addRect))
                {
                    foreach (var m_gatherInfo in m_gatherInfos)
                    {
                        var oldRefBundle = m_defineObj.refBundleList.Find(x => x.guids.Contains(m_gatherInfo.guid));
                        if (oldRefBundle == null)
                        {
                            info.guids.Add(m_gatherInfo.guid);
                            changed = true;
                        }
                        else if (oldRefBundle != m_gatherInfo.assetRefBundle && !info.guids.Contains(m_gatherInfo.guid))
                        {
                            oldRefBundle.guids.Remove(m_gatherInfo.guid);
                            if (oldRefBundle.guids.Count == 0)
                                m_defineObj.refBundleList.Remove(oldRefBundle);

                            info.guids.Add(m_gatherInfo.guid);
                            changed = true;
                        }

                        if (m_gatherInfo.addressInfo != null)
                        {
                            m_defineObj.addressList.Remove(m_gatherInfo.addressInfo);
                            changed = true;
                        }
                    }

                }
            }

            if (changed)
                EditorUtility.SetDirty(m_defineObj);
        }

        private float DrawSubElementHight(int index)
        {
            if (index >= m_defineObj.refBundleList.Count)
                return 0;
            return EditorGUIUtility.singleLineHeight + 4;
        }

        private float DrawElementHight(int index)
        {
            if (index >= m_defineObj.addressList.Count)
                return 0;

            return EditorGUIUtility.singleLineHeight + 4;
        }

        public override void OnInspectorGUI()
        {
            using (var disableScope = new EditorGUI.DisabledScope(!m_defineObj.editable))
            {

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    using (var hor = new GUILayout.HorizontalScope())
                    {
                        using (var ver = new GUILayout.VerticalScope())
                        {
                            using (var dis = new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.PropertyField(m_scriptProp);
                            }
                            using (var hor1 = new GUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField("name:", GUILayout.Width(40));
                                m_defineObj.abNameLengthMax = EditorGUILayout.IntField(m_defineObj.abNameLengthMax, GUILayout.Width(30));
                                EditorGUILayout.LabelField("hash:", GUILayout.Width(40));
                                m_defineObj.hashLengthMax = EditorGUILayout.IntField(m_defineObj.hashLengthMax, GUILayout.Width(30));
                                EditorGUILayout.LabelField("reslice:", GUILayout.Width(50));
                                m_defineObj.autoSliceBundleOnBuild = EditorGUILayout.Toggle(m_defineObj.autoSliceBundleOnBuild, GUILayout.Width(30));
                                GUILayout.FlexibleSpace();
                                EditorGUILayout.LabelField("sub:", GUILayout.Width(30));
                                m_drawSubList = EditorGUILayout.Toggle(m_drawSubList, GUILayout.Width(20));
                                if (!m_drawSubList)
                                {
                                    if (GUILayout.Button("import"))
                                    {
                                        var filePath = EditorUtility.OpenFilePanelWithFilters("open asset list file", Application.dataPath, new string[] { "txt", "txt", "csv", "csv" });
                                        if (!string.IsNullOrEmpty(filePath))
                                        {
                                            AddressABBuilder.ImportAssetList(filePath, m_defineObj);
                                        }
                                    }
                                    else if (GUILayout.Button("deport"))
                                    {
                                        var filePath = EditorUtility.OpenFilePanelWithFilters("open asset list file", Application.dataPath, new string[] { "txt", "txt", "csv", "csv" });
                                        if (!string.IsNullOrEmpty(filePath))
                                        {
                                            AddressABBuilder.RemoveAssetList(filePath, m_defineObj);
                                        }
                                    }
                                }
                            }
                        }
                        if (GUILayout.Button(m_sliceContent, GUILayout.Width(40), GUILayout.Height(40)))
                        {
                            var confer = EditorUtility.DisplayDialog("slice confer", "auto generate assetbundle, to all muti used sub assets?", "ok");
                            if (confer)
                            {
                                var collector = new BundleBuildCollector(m_defineObj);
                                collector.AutoSliceBundles(m_defineObj);
                            }
                        }
                    }

                    using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPos))
                    {
                        m_scrollPos = scroll.scrollPosition;
                        if (m_drawSubList)
                        {
                            m_subReorderableList.DoLayoutList();
                        }
                        else
                        {
                            m_reorderableList.DoLayoutList();
                        }
                    }

                    if (changeScope.changed)
                    {
                        EditorUtility.SetDirty(m_defineObj);
                    }
                }
            }
        }
        private void DrawListHeader(Rect rect)
        {
            var lastColor = GUI.color;
            GUI.color = m_error ? Color.red : Color.green;
            var title = "[AddressList]";
            if (AddressDefineObjectSetting.Instance.activeAddressDefineObject == m_defineObj)
            {
                title += " (Default)";
            }

            var labelRect = new Rect(rect.x, rect.y, 160, rect.height);

            if (labelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("sort by name"), false, OnSortAddressInfoByName);
                menu.AddItem(new GUIContent("sort by flag"), false, OnSortAddressInfoByFlag);
                menu.ShowAsContext();
                Event.current.Use();
            }

            GUI.Label(labelRect, title, EditorStyles.label);
            GUI.color = lastColor;
            if (AcceptRegion(rect))
            {
                foreach (var gatherInfo in m_gatherInfos)
                {
                    var id = m_defineObj.addressList.FindIndex(x => x.guid == gatherInfo.guid);
                    if (id > -1)
                    {
                        m_reorderableList.Select(id);
                    }
                    else
                    {
                        AddressInfo info = new AddressInfo();
                        info.guid = gatherInfo.guid;
                        info.address = AssetAddressGUI.GetPreviewAddress(gatherInfo.path, m_defineObj);
                        info.flags = m_matchFlag;
                        m_defineObj.addressList.Add(info);
                        EditorUtility.SetDirty(m_defineObj);
                        RefreshReorderList();
                    }
                    if(gatherInfo.assetRefBundle != null)
                    {
                        gatherInfo.assetRefBundle.guids.Remove(gatherInfo.guid);
                        if(gatherInfo.assetRefBundle.guids.Count == 0)
                        {
                            m_defineObj.refBundleList.Remove(gatherInfo.assetRefBundle);
                        }
                    }
                }
            }

            var filterRect = new Rect(rect.width - 400, rect.y, 100, rect.height);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                m_matchFlag = (ushort)EditorGUI.MaskField(filterRect, m_matchFlag, m_defineObj.flags);
                if (changeScope.changed)
                {
                    RefreshReorderList();
                }
            }

            var matchRect = new Rect(rect.width - 300, rect.y, 100, rect.height);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                m_matchStr = EditorGUI.TextField(matchRect, m_matchStr);
                if (changeScope.changed && string.IsNullOrEmpty(m_matchStr))
                {
                    RefreshReorderList();
                }
            }

            var searchRect = new Rect(rect.width - 200, rect.y, 30, rect.height);
            if (GUI.Button(searchRect, m_searchContent))
            {
                RefreshReorderList();
            }

            var optionRect = new Rect(rect.width - 170, rect.y, 120, rect.height);
            m_defineObj.options = (BuildAssetBundleOptions)EditorGUI.EnumFlagsField(optionRect, m_defineObj.options);

            var buildRect = new Rect(rect.width - 50, rect.y, 60, rect.height);
            if (GUI.Button(buildRect, "Build", EditorStyles.miniButtonRight))
            {
                AddressABBuilder.AutoBuildAddressDefine(ScriptableObject.Instantiate(m_defineObj));
            }
            m_uniqueNameSet.Clear();
            m_error = false;
        }

        private void OnSortAddressInfoByFlag()
        {
            m_defineObj.addressList.Sort((x, y) => x.flags - y.flags);
            RefreshReorderList();
        }

        private void OnSortAddressInfoByName()
        {
            m_defineObj.addressList.Sort((x, y) => string.Compare(x.address, y.address));
            RefreshReorderList();
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= m_addressList.Count)
                return;

            var info = m_addressList[index];
            Object guidTarget = null;
            if (!string.IsNullOrEmpty(info.guid))
            {
                m_guidMap.TryGetValue(info.guid, out guidTarget);
            }

            if (!guidTarget && !string.IsNullOrEmpty(info.guid))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(info.guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    guidTarget = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    m_guidMap[info.guid] = guidTarget;
                }
            }
            var error = false;
            var alive = guidTarget != null;
            if (!alive)
            {
                error = true;
            }

            if (!info.split)
            {
                var uniqueName = info.address + info.flags;
                if (!m_uniqueNameSet.Contains(uniqueName))
                {
                    m_uniqueNameSet.Add(uniqueName);
                }
                else
                {
                    error = true;
                }
            }
            var lastColor = GUI.color;
            if (error)
            {
                m_error = true;
                GUI.color = Color.red;
            }

            var idRect = new Rect(rect.x - 20, rect.y, 40, rect.height);
            GUI.Label(idRect, (index + 1).ToString("000"), EditorStyles.miniBoldLabel);
            GUI.color = lastColor;

            if (idRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("delete"), false, OnDeleteAddressItem, index);
                menu.AddItem(new GUIContent("insert"), false, OnInsertAddressItem, index);

                var assetPath = AssetDatabase.GUIDToAssetPath(info.guid);
                if (System.IO.Directory.Exists(assetPath))
                {
                    if (info.split)
                    {
                        menu.AddItem(new GUIContent("park"), false, (x) =>
                        {
                            info.split = false;
                            EditorUtility.SetDirty(target);
                        }, index);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("split"), false, (x) =>
                        {
                            info.split = true;
                            EditorUtility.SetDirty(target);
                        }, index);
                    }
                }
                menu.ShowAsContext();
            }

            using (var disableGroup = new EditorGUI.DisabledGroupScope(!info.active))
            {
                var addressRect = new Rect(rect.x + 30, rect.y + 2, rect.width * 0.5f - 30, rect.height - 4);
                var address = EditorGUI.TextField(addressRect, info.address);
                if (info.address != address)
                {
                    info.address = address;
                    Undo.RecordObject(target, "text changed");
                }

                var flagsRect = new Rect(rect.x + rect.width * 0.5f, rect.y + 2, rect.width * 0.2f, rect.height - 4);
                var flags = (ushort)EditorGUI.MaskField(flagsRect, info.flags, m_defineObj.flags);
                if (flags != info.flags)
                {
                    Undo.RecordObject(target, "flag changed");
                    info.flags = flags;
                }
            }

            var activeRect = new Rect(rect.x + 10, rect.y + 2, 20, rect.height - 4);
            info.active = EditorGUI.Toggle(activeRect, info.active);
            var nameRect = new Rect(rect.x + rect.width * 0.7f, rect.y + 2, rect.width * 0.3f, rect.height - 4);
            if (!alive)
            {
                GUI.color = Color.red;
                EditorGUI.LabelField(nameRect, "(missing)");
                GUI.color = lastColor;
            }
            else
            {
                string targetName = guidTarget.name;
                if (guidTarget is DefaultAsset)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(info.guid);
                    if (System.IO.Directory.Exists(assetPath))
                    {
                        var rootDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(assetPath));
                        targetName = assetPath.Substring(rootDir.Length + 1);
                    }
                    if (info.split)
                    {
                        targetName = $"[{targetName}]`s";
                    }
                    else
                    {
                        targetName = $"[{targetName}]";
                    }
                }
                if (GUI.Button(nameRect, targetName, EditorStyles.linkLabel))
                {
                    EditorGUIUtility.PingObject(guidTarget);
                }
            }
            if (AcceptRegion(rect))
            {
                var gatherInfo = m_gatherInfos[0];
                var id = m_defineObj.addressList.FindIndex(x => x.guid == gatherInfo.guid);
                if (id < 0)
                {
                    info.guid = gatherInfo.guid;
                    info.address = AssetAddressGUI.GetPreviewAddress(gatherInfo.path, m_defineObj);
                    EditorUtility.SetDirty(m_defineObj);
                }
                else
                {
                    m_reorderableList.Select(id);
                }
            }
        }

        private void OnDeleteAddressItem(object arg)
        {
            var id = (int)arg;
            if (m_addressList.Count > id)
            {
                Undo.RecordObject(m_defineObj, "delete");
                var info = m_addressList[id];
                m_defineObj.addressList.Remove(info);
                EditorUtility.SetDirty(m_defineObj);
                RefreshReorderList();
            }
        }
        private void OnInsertAddressItem(object arg)
        {
            var info = new AddressInfo();
            info.active = true;
            info.flags = m_matchFlag;
            var id = (int)arg;
            if (m_addressList.Count > id)
            {
                var lastInfo = m_addressList[id];
                var lastIndex = m_defineObj.addressList.IndexOf(lastInfo);
                m_defineObj.addressList.Insert(lastIndex, info);
            }
            else
            {
                m_defineObj.addressList.Add(info);
            }
            EditorUtility.SetDirty(m_defineObj);
            RefreshReorderList();
        }

        private void OnDeleteSubAssetBundleItem(object arg)
        {
            var id = (int)arg;
            if (m_subBundleList.Count > id)
            {
                Undo.RecordObject(m_defineObj, "delete");
                var info = m_subBundleList[id];
                m_defineObj.refBundleList.Remove(info);
                EditorUtility.SetDirty(m_defineObj);
                RefreshSubBundleList();
            }
        }

        private void OnInsertSubAssetBundleItem(object arg)
        {
            var id = (int)arg;
            var info = new AssetRefBundle();
            if (m_subBundleList.Count > id)
            {
                var lastInfo = m_subBundleList[id];
                var lastIndex = m_defineObj.refBundleList.IndexOf(lastInfo);
                m_defineObj.refBundleList.Insert(lastIndex, info);
            }
            else
            {
                m_defineObj.refBundleList.Add(info);
            }
            EditorUtility.SetDirty(m_defineObj);
            RefreshSubBundleList();
        }

        private void OnDeleteSubAssetBundleGuid(object arg)
        {
            var guid = (string)arg;
            var refBundle = m_defineObj.refBundleList.Find(x => x.guids.Contains(guid));
            if (refBundle != null)
            {
                refBundle.guids.Remove(guid);
                Undo.RecordObject(m_defineObj, "delete");
                if (refBundle.guids.Count == 0)
                    m_defineObj.refBundleList.Remove(refBundle);
                EditorUtility.SetDirty(m_defineObj);
                RefreshSubBundleList();
            }
        }
        private bool AcceptRegion(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    m_gatherInfos.Clear();
                    foreach (var item in DragAndDrop.objectReferences)
                    {
                        var ok = AssetAddressGUI.TryGetPathAndGUIDFromTarget(item, out var path, out var guid);
                        if (ok)
                        {
                            var gatherInfo = new AddressGatherInfo();
                            gatherInfo.path = path;
                            gatherInfo.guid = guid;
                            gatherInfo.target = DragAndDrop.objectReferences[0];
                            m_gatherInfos.Add(gatherInfo);
                        }
                    }
                    if(m_gatherInfos.Count > 0)
                    {
                        Undo.RecordObject(m_defineObj, "dragPerform");
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
    }
}

