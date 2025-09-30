//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2022-02-10 11:45:16
//* 描    述：

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace UFrame.BridgeUI.Editors
{
    [CustomEditor(typeof(UIPanelGroup))]
    public class TextPanelGroupDrawer : Editor
    {
        protected SerializedProperty script;
        protected SerializedProperty bundleCreateRuleProp;
        private string query;
        protected const float widthBt = 20;
        protected UIInfoListDrawer bundleInfoList;
        protected UIInfoListDrawer bridgeList;
        public virtual bool editAble => false;
        protected string[] option = { "Node", "Bridge" };
        protected int selected;
        protected int selectedGraph = -1;
        protected bool drawGraph = true;
        private ReorderableList prop_panelConfigsList;
        private ReorderableList prop_connectionConfigsList;
        private UIPanelGroup m_panelGroup;
        private SortedDictionary<string, UIInfoBase> m_abNodes = new SortedDictionary<string, UIInfoBase>();
        private List<BundleUIInfo> m_bdInfos = new List<BundleUIInfo>();
        private Dictionary<BridgeInfo, string> m_bridgePaths = new Dictionary<BridgeInfo, string>();
        protected virtual void OnEnable()
        {
            m_panelGroup = target as UIPanelGroup;
            script = serializedObject.FindProperty("m_Script");
            bundleCreateRuleProp = serializedObject.FindProperty("bundleCreateRule");
            bundleInfoList = new UIInfoListDrawer("Nodes", this);
            bundleInfoList.InitReorderList(serializedObject.FindProperty("b_nodes"));
            bridgeList = new UIInfoListDrawer("Bridges", this);
            bridgeList.InitReorderList(serializedObject.FindProperty("bridges"));
            UIInfoBaseDrawer.uiTypeChangeAction = SaveBackUITypeChange;
            UIInfoBaseDrawer.selectedParent = (PanelGroupBase)target;
            BridgeInfoDrawer.bridgeChangeAction = SaveBackBridge;
            InitPanelConfigs();
            InitBridgeConfigs();
            UpdateMarchList();
            UpdateRuntimeInfo();
            UpdateMarchList();
        }

        private void InitPanelConfigs()
        {
            var prop = serializedObject.FindProperty("panelConfigs");
            prop_panelConfigsList = new ReorderableList(serializedObject, prop);
            prop_panelConfigsList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "PanelConfigs");
            prop_panelConfigsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var obj = prop.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, obj);
            };
            prop_panelConfigsList.onAddCallback = (list) => {
                ProjectWindowUtil.CreateAssetWithContent("new_ui_nodes.csv", "desc,path,layer,index,fixed,destroy,mask,hide,alpha");
            };
        }

        private void InitBridgeConfigs()
        {
            var prop = serializedObject.FindProperty("connectionConfigs");
            prop_connectionConfigsList = new ReorderableList(serializedObject, prop);
            prop_connectionConfigsList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "BridgeConfigs");
            prop_connectionConfigsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var obj = prop.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, obj);
            };
            prop_connectionConfigsList.onAddCallback = (list) => {
                ProjectWindowUtil.CreateAssetWithContent("new_ui_bridge.csv", "in,out,auto,single,index,mutex,base");
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawScript();
            prop_panelConfigsList.DoLayoutList();
            prop_connectionConfigsList.DoLayoutList();
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                DrawOption();
                DrawToolButtons();
                DrawMatchField();
            }
            DrawRuntimeItems();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnGraphListChanged()
        {
            UpdateRuntimeInfo();
            UpdateMarchList();
        }

        private void DrawOption()
        {
            EditorGUI.BeginChangeCheck();
            selected = GUILayout.Toolbar(selected, option, EditorStyles.toolbarButton, GUILayout.Width(200));// GUILayout.Toolbar(defultTypeProp.enumValueIndex, option, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMarchList();
            }
        }

        private void DrawMatchField()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refesh", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    OnGraphListChanged();
                }

                EditorGUI.BeginChangeCheck();
                query = EditorGUILayout.TextField(query);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateMarchList();
                }
            }
        }

        protected virtual void DrawRuntimeItems()
        {
            if (selected == 1)
            {
                bridgeList.DoLayoutList();
            }
            else if (selected == 0)
            {
                if (bundleCreateRuleProp != null)
                {
                    var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
                    DrawBundleCreateRule(rect, bundleCreateRuleProp);
                }
                bundleInfoList.DoLayoutList();
            }
        }


        private void DrawBundleCreateRule(Rect position, SerializedProperty property)
        {
            int labelWidth = 100;
            int btnWidth = 60;
            var rect0 = new Rect(position.x, position.y, labelWidth, position.height);
            var rect1 = new Rect(position.x + labelWidth, position.y, position.width - btnWidth - labelWidth, position.height);
            var rect2 = new Rect(position.x + position.width - btnWidth, position.y, btnWidth, position.height);
            EditorGUI.LabelField(rect0, "资源包加载规则");
            property.objectReferenceValue = EditorGUI.ObjectField(rect1, property.objectReferenceValue, typeof(UILoader), false);
            var path = property.propertyPath;
            var obj = property.serializedObject.targetObject;
            if (GUI.Button(rect2, "new", EditorStyles.miniButtonRight))
            {
                BundleUtil.CreateNewBundleCreateRule(x =>
                {
                    property = new SerializedObject(obj).FindProperty(path);
                    property.objectReferenceValue = x;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
        }


        protected void UpdateMarchList()
        {
            if (selected == 0)
            {
                var bundles = GetBundleUIInfos(query);
                m_panelGroup.b_nodes.Clear();
                m_panelGroup.b_nodes.AddRange(bundles);
                EditorUtility.SetDirty(m_panelGroup);
            }
            else
            {
                //var resources = GetResourceUIInfos(query);
                //m_panelGroup.r_nodes.Clear();
                //m_panelGroup.r_nodes.AddRange(resources);
                //EditorUtility.SetDirty(m_panelGroup);
            }
        }

        private void DrawToolButtons()
        {
            var btnStyle = EditorStyles.miniButton;
            var widthSytle = GUILayout.Width(20);
            if (selected == 0)
            {
                using (var hor = new EditorGUILayout.HorizontalScope(widthSytle))
                {
                    if (GUILayout.Button(new GUIContent("o", "批量加载"), btnStyle))
                    {
                        GroupLoadItems((m_panelGroup.b_nodes).ToArray());
                    }
                    if (GUILayout.Button(new GUIContent("c", "批量关闭"), btnStyle))
                    {
                        CloseAllCreated((m_panelGroup.b_nodes).ToArray());
                    }
                }
            }
        }


        private void GroupLoadItems(UIInfoBase[] infoList)
        {
            Dictionary<int, Transform> rootDic = new Dictionary<int, Transform>();
            for (int i = 0; i < infoList.Length; i++)
            {
                UIInfoBase item = infoList[i];
                GameObject prefab = null;
                if (item is PrefabUIInfo)
                {
                    prefab = (item as PrefabUIInfo).prefab;
                }
                else if (item is BundleUIInfo)
                {
                    var bundle = item as BundleUIInfo;
                    if (string.IsNullOrEmpty(bundle.guid))
                        prefab = GetCustomPrefab(bundle.bundleName, bundle.panelName);
                    else
                    {
                        var guid = (item as BundleUIInfo).guid;
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                }
                else if (item is ResourceUIInfo)
                {
                    var guid = (item as ResourceUIInfo).guid;
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }

                if (item.instanceID != 0 && UnityEditor.EditorUtility.InstanceIDToObject(item.instanceID))
                    continue;

                if (prefab == null)
                {
                    UnityEditor.EditorUtility.DisplayDialog("空对象", "找不到预制体" + item.panelName, "确认");
                }
                else
                {
                    GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    if (target is PanelGroupBase)
                    {
                        var parentRoot = (target as PanelGroupBase).transform;
                        Utility.SetTranform(go.transform, item.type.layer, item.type.layerIndex, parentRoot, rootDic, null);
                    }
                    else
                    {
                        if (go.GetComponent<Transform>() is RectTransform)
                        {
                            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                            go.transform.SetParent(canvas.transform, false);
                        }
                        else
                        {
                            go.transform.SetParent(null);
                        }
                    }

                    item.instanceID = go.GetInstanceID();
                }

            }
        }

        private void CloseAllCreated(UIInfoBase[] infoList)
        {
            TrySaveAllPrefabs(infoList);
            for (int i = 0; i < infoList.Length; i++)
            {
                var item = infoList[i];
                BridgeEditorUtility.SavePrefab(ref item.instanceID);
            }
        }

        private void TrySaveAllPrefabs(UIInfoBase[] proprety)
        {
            for (int i = 0; i < proprety.Length; i++)
            {
                var item = proprety[i];
                var obj = EditorUtility.InstanceIDToObject(item.instanceID);
                if (obj == null) continue;
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefab != null)
                {
                    var root = PrefabUtility.GetOutermostPrefabInstanceRoot((GameObject)prefab);
                    if (root != null)
                    {
                        var prefabPath = AssetDatabase.GetAssetPath(root);
                        bool success;
                        PrefabUtility.SaveAsPrefabAsset(obj as GameObject, prefabPath, out success);
                    }
                }
            }
        }

        protected void DrawScript()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();
        }

        protected List<BundleUIInfo> GetBundleUIInfos(string fliter)
        {
            var nodes = new List<BundleUIInfo>();

            if (!m_panelGroup.UseAssetBundle)
                return nodes;

            nodes.AddRange(m_abNodes.Values.Select(x => x as BundleUIInfo));

            if (string.IsNullOrEmpty(fliter))
            {
                return nodes;
            }
            else
            {
                return nodes.FindAll(x => x.panelName.ToLower().Contains(fliter.ToLower()));
            }
        }

        protected List<PrefabUIInfo> GetPrefabUIInfos(string fliter)
        {
            return new List<PrefabUIInfo>();
        }


        public GameObject GetCustomPrefab(string bundleName, string panelName)
        {
#if ADDRESSABLE
            var addressableSetting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            foreach (var group in addressableSetting.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.address == bundleName)
                    {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.AssetPath);
                        if (prefab != null && prefab.name == panelName)
                            return prefab;
                    }
                }
            }
#endif
            string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName.ToLower(), panelName);
            if (paths != null && paths.Length > 0)
            {
                GameObject gopfb = AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
                return gopfb;
            }
            var guids = AssetDatabase.FindAssets("t:GameObject " + panelName);
            GameObject target = null;
            if (guids != null && guids.Length > 0)
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    target = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (path.Replace('\\', '/').ToLower().Contains(bundleName.ToLower()))
                        break;
                }
            }
            return target;
        }


        protected void UpdateRuntimeInfo()
        {
            m_panelGroup = target as UIPanelGroup;
            Dictionary<string, UIInfoBase> m_abNodes0 = new Dictionary<string, UIInfoBase>();
            DecodeNodes(true, m_abNodes0);
            m_abNodes.Clear();
            foreach (var pair in m_abNodes0)
            {
                var bundleInfo = pair.Value as BundleUIInfo;
                if (string.IsNullOrEmpty(bundleInfo.guid))
                {
                    var prefab = GetCustomPrefab(bundleInfo.bundleName, bundleInfo.panelName);
                    if (prefab)
                        bundleInfo.guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                }
                bundleInfo.good = !string.IsNullOrEmpty(bundleInfo.guid);
                m_abNodes.Add(pair.Key, pair.Value);
            }
            DecodeBridges(m_panelGroup.bridges);
        }

        public void SaveBackUITypeChange(string panelName, UIType uiType)
        {
            for (int i = 0; i < prop_panelConfigsList.count; i++)
            {
                var configElement = prop_panelConfigsList.serializedProperty.GetArrayElementAtIndex(i);
                if (configElement.objectReferenceValue != null && configElement.objectReferenceValue is TextAsset)
                {
                    var textAsset = configElement.objectReferenceValue as TextAsset;
                    var path = AssetDatabase.GetAssetPath(textAsset);
                    ApplyChangeToText(path, textAsset.text, panelName, uiType);
                }
            }
        }

        private void SaveBackBridge()
        {
            Dictionary<string, System.Text.StringBuilder> sbMap = new Dictionary<string, System.Text.StringBuilder>();
            foreach (var pair in m_bridgePaths)
            {
                var bridgeInfo = pair.Key;
                var path = pair.Value;

                if(!sbMap.TryGetValue(path,out var sb))
                {
                    sb = new System.Text.StringBuilder();
                    sbMap[path] = sb;
                }
                var infos = new string[7];
                infos[0] = bridgeInfo.inNode;
                infos[1] = bridgeInfo.outNode;
                infos[2] = bridgeInfo.index.ToString();
                infos[3] = bridgeInfo.showModel.auto ?"1":"0";
                infos[4] = bridgeInfo.showModel.single ? "1" : "0";
                infos[5] = ((int)(bridgeInfo.showModel.mutex)).ToString();
                infos[6] = ((int)(bridgeInfo.showModel.baseShow)).ToString();
                sb.AppendLine(string.Join(",", infos));
            }

            foreach (var pair in sbMap)
            {
                var lines = System.IO.File.ReadAllLines(pair.Key);
                pair.Value.Insert(0,lines[0] + "\n");
                System.IO.File.WriteAllText(pair.Key, pair.Value.ToString());
                AssetDatabase.ImportAsset(pair.Key);
                AssetDatabase.Refresh();
            }
        }

        private void ApplyChangeToText(string textPath, string text, string panelName, UIType uiType)
        {
            var sb = new System.Text.StringBuilder();

            using (var textReader = new System.IO.StringReader(text))
            {
                var line = textReader.ReadLine();
                sb.AppendLine(line);
                while (textReader.Peek() > 0)
                {
                    line = textReader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        sb.AppendLine(line);
                        continue;
                    }

                    var infos = line.Split(',');
                    var desc = infos[0];
                    var path = infos[1];

                    if (!path.EndsWith(panelName))
                    {
                        sb.AppendLine(line);
                        continue;
                    }

                    if (infos.Length < 9)
                    {
                        infos = new string[9];
                        infos[0] = desc;
                        infos[1] = path;
                    }
                    infos[2] = uiType.layer.ToString();
                    infos[3] = uiType.layerIndex.ToString();
                    infos[4] = ((int)uiType.form).ToString();
                    infos[5] = ((int)uiType.closeRule).ToString();
                    infos[6] = ((int)uiType.cover).ToString();
                    infos[7] = ((int)uiType.hideRule).ToString();
                    infos[8] = uiType.hideAlaph.ToString("0.00");
                    sb.AppendLine(string.Join(",", infos));
                }
            }
            System.IO.File.WriteAllText(textPath, sb.ToString());
            AssetDatabase.ImportAsset(textPath);
            AssetDatabase.Refresh();
        }

        //name,path,layer_type,layer_id,fixed,destroy_type,mask
        public void DecodeNodes(bool bundle, Dictionary<string, UIInfoBase> nodes)
        {
            nodes.Clear();
            if (m_panelGroup.panelConfigs == null)
                return;
            foreach (var m_panelConfig in m_panelGroup.panelConfigs)
            {
                if (m_panelConfig && !string.IsNullOrEmpty(m_panelConfig.text))
                {
                    var text = System.Text.Encoding.UTF8.GetString(m_panelConfig.bytes).Replace(" ", "");
                    using (var textReader = new System.IO.StringReader(text))
                    {
                        var line = textReader.ReadLine();
                        while (textReader.Peek() > 0)
                        {
                            line = textReader.ReadLine();
                            if (string.IsNullOrEmpty(line))
                                continue;

                            var infos = line.Split(',');
                            UIInfoBase node = null;
                            if (bundle)
                            {
                                BundleUIInfo b_info = new BundleUIInfo();
                                node = b_info;
                                if (infos.Length > 1) b_info.bundleName = infos[1];
                            }
                            else
                            {
                                var r_info = new ResourceUIInfo();
                                node = r_info;
                                if (infos.Length > 1) r_info.resourcePath = infos[1];
                            }
                            if (infos.Length > 1)
                            {
                                var panelName = infos[1];
                                if (panelName.Contains("/"))
                                {
                                    panelName = panelName.Substring(panelName.LastIndexOf('/') + 1);
                                }
                                node.panelName = panelName;
                            }

                            if (!string.IsNullOrEmpty(node.panelName))
                                nodes[node.panelName] = node;
                            else
                                continue;

                            if (infos.Length > 0)
                            {
                                node.discription = infos[0];
                                if (node.panelName == node.discription)
                                {
                                    node.discription = "";
                                }
                            }

                            var uiType = new UIType();
                            if (infos.Length > 2) uiType.layer = (UILayerType)System.Enum.Parse(typeof(UILayerType), infos[2]);
                            if (infos.Length > 3) uiType.layerIndex = int.Parse(infos[3]);
                            if (infos.Length > 4) uiType.form = (UIFormType)int.Parse(infos[4]);
                            if (infos.Length > 5) uiType.closeRule = (CloseRule)int.Parse(infos[5]);
                            if (infos.Length > 6) uiType.cover = (UIMask)int.Parse(infos[6]);
                            if (infos.Length > 7) uiType.hideRule = (HideRule)int.Parse(infos[7]);
                            if (infos.Length > 8) uiType.hideAlaph = float.Parse(infos[8]);
                            node.type = uiType;
                        }
                    }
                }
            }
        }

        //in,out,auto,single,index,mutex,parent
        public void DecodeBridges(List<BridgeInfo> bridges)
        {
            bridges.Clear();
            m_bridgePaths.Clear();
            foreach (var m_connectionConfig in m_panelGroup.connectionConfigs)
            {
                if (m_connectionConfig && !string.IsNullOrEmpty(m_connectionConfig.text))
                {
                    var path = AssetDatabase.GetAssetPath(m_connectionConfig);
                    var text = System.Text.Encoding.UTF8.GetString(m_connectionConfig.bytes).Replace(" ", "");
                    using (var textReader = new System.IO.StringReader(text))
                    {
                        var line = textReader.ReadLine();
                        while (textReader.Peek() > 0)
                        {
                            line = textReader.ReadLine();
                            if (string.IsNullOrEmpty(line))
                                continue;
                            var infos = line.Split(',');
                            BridgeInfo info = new BridgeInfo();
                            var showMode = new ShowMode();
                            if (infos.Length > 0) info.inNode = infos[0];
                            if (infos.Length > 1) info.outNode = infos[1];
                            if (string.IsNullOrEmpty(info.outNode))
                                continue;
                            if (infos.Length > 2 && !string.IsNullOrEmpty(infos[2])) showMode.auto = int.Parse(infos[2]) != 0;
                            if (infos.Length > 3 && !string.IsNullOrEmpty(infos[3])) showMode.single = int.Parse(infos[3]) != 0;
                            if (infos.Length > 4 && !string.IsNullOrEmpty(infos[4])) info.index = short.Parse(infos[4]);
                            if (infos.Length > 5 && !string.IsNullOrEmpty(infos[5])) showMode.mutex = (MutexRule)int.Parse(infos[5]);
                            if (infos.Length > 6 && !string.IsNullOrEmpty(infos[6])) showMode.baseShow = (ParentShow)int.Parse(infos[6]);
                            info.showModel = showMode;
                            bridges.Add(info);
                            m_bridgePaths[info] = path;
                        }
                    }
                }
            }
        }
    }
}