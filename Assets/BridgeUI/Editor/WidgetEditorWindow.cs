/*【功能说明】
    -.将预制体信息作为模板，记录到配制文档。
    -.从配制文档加载信息，以列表方式显示到窗口中。
    -.从WidgetEditor窗口快速实现控件的创建。
 */
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditorInternal;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace UFrame.BridgeUI.Editors
{
    public class WidgetEditorWindow : EditorWindow
    {
        [MenuItem("Window/Foundation/UI-Editor/WidgetRecorder")]
        public static void OpenWidgeEditorWindow()
        {
            var window = GetWindow<WidgetEditorWindow>("通用控件定制");
            window.Show();
        }
        private string configPath;
        private WidgetEditorCtrl widgetEditorCtrl;
        private Dictionary<string, List<WidgetNode>> widgetNodeDic = new Dictionary<string, List<WidgetNode>>();
        private Dictionary<string, ReorderableList> widgetListDrawers = new Dictionary<string, ReorderableList>();
        private List<string> groupKeys;
        private string defultLayerName = "UI";
        private Vector2 scroll_1;
        private string matchValue;
        private WidgetNode activeNode;
        private ByteHelper byteHelper;
        private string activeGroupKey;
        private HeadInfo _headInfo;
        private string importPreviewPath { get { return headInfo.preview_importPath; } set { headInfo.preview_importPath = value; } }
        private string exportPreviewPath { get { return headInfo.preview_exportPath; } set { headInfo.preview_exportPath = value; } }
        private string importJsonPath { get { return headInfo.json_importPath; } set { headInfo.json_importPath = value; } }
        private string exportJsonPath { get { return headInfo.json_exportPath; } set { headInfo.json_exportPath = value; } }
        private HeadInfo headInfo
        {
            get
            {
                if (_headInfo == null)
                {
                    _headInfo = new HeadInfo();
                }
                return _headInfo;
            }
            set
            {
                if (value != null)
                {
                    _headInfo = value;
                }
            }
        }

        public WidgetEditorWindow()
        {
            widgetNodeDic = new Dictionary<string, List<WidgetNode>>();
            widgetListDrawers = new Dictionary<string, ReorderableList>();
            byteHelper = new ByteHelper();
            groupKeys = new List<string>();
        }

        private void OnEnable()
        {
            widgetEditorCtrl = new WidgetEditorCtrl();
            configPath = UISetting.script_path +"/widget_config.bin";
            importJsonPath = Application.dataPath;
            exportJsonPath = Application.dataPath;
            importPreviewPath = Application.dataPath;
            exportPreviewPath = Application.dataPath;
            LoadFromFile();
            UpdateDrawers();
        }

        private void OnGUI()
        {
            var headHeight = EditorGUIUtility.singleLineHeight * 3;
            var bodyheight = position.height - headHeight;
            var leftWidth = EditorGUIUtility.currentViewWidth * 0.6f;
            var leftBodyWidth = leftWidth * 0.98f;
            var rightWidth = EditorGUIUtility.currentViewWidth * 0.4f;
            var infoHeight = 5 * EditorGUIUtility.singleLineHeight;

            var boxRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, headHeight);
            GUI.Box(boxRect, "");

            boxRect = new Rect(0, headHeight, leftWidth, bodyheight);
            GUI.Box(boxRect, "");

            boxRect = new Rect(leftWidth, headHeight, rightWidth, bodyheight);
            GUI.Box(boxRect, "");

            boxRect = new Rect(leftWidth, headHeight + infoHeight, rightWidth, bodyheight - infoHeight);
            GUI.Box(boxRect, "");


            using (var vertical_1 = new EditorGUILayout.VerticalScope())
            {
                using (var vertical_4 = new EditorGUILayout.VerticalScope(GUILayout.Height(headHeight)))
                {
                    EditorGUILayout.LabelField("【工具栏】", GUILayout.Width(120));

                    using (var hori_1 = new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("批量加载"))
                        {
                            LoadAllGroup();
                        }
                        if (GUILayout.Button("保存配制"))
                        {
                            SaveRecordToFile();
                        }

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("导入Json"))
                        {
                            ImportJson();
                        }
                        if (GUILayout.Button("导出Json"))
                        {
                            ExportJson();
                        }

                        GUILayout.FlexibleSpace();
                    }

                }

                using (var hori_2 = new EditorGUILayout.HorizontalScope(GUILayout.Height(bodyheight)))
                {
                    GUILayoutUtility.GetRect(leftWidth - leftBodyWidth, bodyheight);
                    using (var vertical_2 = new EditorGUILayout.VerticalScope(GUILayout.Width(leftBodyWidth)))
                    {
                        var rect = GUILayoutUtility.GetRect(leftBodyWidth, EditorGUIUtility.singleLineHeight);
                        var strRect = new Rect(rect.x + 5, rect.y - 2, rect.width - 110, EditorGUIUtility.singleLineHeight);
                        matchValue = EditorGUI.TextField(strRect, matchValue);
                        var btnRect = new Rect(strRect.x + strRect.width, strRect.y, 100, strRect.height);
                        if (GUI.Button(btnRect, "添加组"))
                        {
                            AddGroup();
                        }
                        using (var scroll = new EditorGUILayout.ScrollViewScope(scroll_1, GUILayout.Width(leftBodyWidth)))
                        {
                            scroll_1 = scroll.scrollPosition;
                            DrawReorderLists();
                        }

                    }
                    using (var vertical_3 = new EditorGUILayout.VerticalScope(GUILayout.Width(rightWidth)))
                    {
                        if (activeNode != null)
                        {
                            EditorGUILayout.LabelField("名称：");
                            var nameRect = GUILayoutUtility.GetLastRect();
                            var tempRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);
                            tempRect.x += 20;
                            tempRect.width = rightWidth - 25;
                            activeNode.name = EditorGUI.TextField(tempRect, activeNode.name);
                            EditorGUILayout.LabelField("描述：");
                            tempRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);
                            tempRect.x += 20;
                            tempRect.width = rightWidth - 25;
                            activeNode.desc = EditorGUI.TextField(tempRect, activeNode.desc);
                            GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 0.5f);
                            EditorGUILayout.LabelField("预览：");
                            var previewRect = GUILayoutUtility.GetLastRect();
                            var previewBtnRect = new Rect(previewRect.x + 40, previewRect.y, 40, EditorGUIUtility.singleLineHeight);
                            var itemBtnRect = new Rect(nameRect.x + 40, nameRect.y, 40, EditorGUIUtility.singleLineHeight);
                            if (GUI.Button(itemBtnRect, "创建", EditorStyles.miniButtonLeft))
                            {
                                CreateWidgetInstence(activeNode);
                            }
                            itemBtnRect.x += 40;
                            if (GUI.Button(itemBtnRect, "替换", EditorStyles.miniButtonRight))
                            {
                                ReplaceTargetInstence();
                            }
                            itemBtnRect.x += 40;
                            if (GUI.Button(itemBtnRect, "记录", EditorStyles.miniButtonRight))
                            {
                                if (EditorUtility.DisplayDialog("注意", "些操作会将重写已有的信息", "继续"))
                                {
                                    UpdateRecordSelectedNode();
                                }
                            }

                            if (GUI.Button(previewBtnRect, "导入", EditorStyles.miniButtonLeft))
                            {
                                ImportTextureData();
                            }
                            previewBtnRect.x += 41;
                            if (GUI.Button(previewBtnRect, "导出", EditorStyles.miniButtonRight))
                            {
                                ExportTextureData();
                            }
                            previewBtnRect.x += 41;
                            if (GUI.Button(previewBtnRect, "删除", EditorStyles.miniButtonRight))
                            {
                                DeleteTextureData();
                            }

                            if (activeNode.preview != null)
                            {
                                var texture = activeNode.preview;
                                if (texture != null && texture.width > 0)
                                {
                                    var rect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 0.5f);
                                    var width = rightWidth - 20;
                                    var heigth = width * texture.height / texture.width;
                                    rect = new Rect(rect.x + 10, rect.y + EditorGUIUtility.singleLineHeight * 0.5f, width, heigth);
                                    EditorGUI.DrawPreviewTexture(rect, texture);
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("【信息详情区】");
                        }
                    }

                }
            }
        }

        private void ReplaceTargetInstence()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
            {
                var ok = EditorUtility.DisplayDialog("警告","此操作会将所有选中的物体，全部替换为指定的控件，并且无法恢复，是否继续？","继续");
                if(!ok){
                    return;
                }
                for (int sid = 0; sid < Selection.gameObjects.Length; sid++)
                {
                    var target = Selection.gameObjects[sid];
                    Undo.RecordObject(target, "替换目标" + sid);
                    var childs = new GameObject[target.transform.childCount];
                    for (int i = 0; i < childs.Length; i++)
                    {
                        childs[i] = target.transform.GetChild(i).gameObject;
                    }
                    for (int i = 0; i < childs.Length; i++)
                    {
                        UnityEngine.Object.DestroyImmediate(childs[i]);
                    }
                    //一般保留坐标
                    var oringalPos = target.transform.position;
                    var instenceDic = new Dictionary<string, GameObject>();
                    widgetEditorCtrl.MakeInsenceDicDeepth(target, activeNode, instenceDic);
                    widgetEditorCtrl.ChargeWidghtInfos(activeNode, instenceDic);
                    target.transform.position = oringalPos;
                    SetAllChildLayer(target, target.layer);
                }
            }
        }

        private void UpdateRecordSelectedNode()
        {
            if (Selection.activeGameObject != null)
            {
                var dic = new Dictionary<int, WidgetNode>();
                var widgetNode = widgetEditorCtrl.MakeWidgetDicDeepth(Selection.activeGameObject, dic);
                widgetEditorCtrl.AnalysisComponetInfos(dic);
                activeNode.active = widgetNode.active;
                activeNode.components = widgetNode.components;
                activeNode.childNodes.Clear();
                if (widgetNode.childNodes != null && widgetNode.childNodes.Count > 0)
                {
                    for (int i = 0; i < widgetNode.childNodes.Count; i++)
                    {
                        activeNode.AddChildNode(widgetNode.childNodes[i]);
                    }
                }
            }
            else
            {
                Debug.LogError("请先选择目标控件！");
            }
        }

        private void DrawReorderLists()
        {
            if (groupKeys == null)
                return;
            using (var enumerator = groupKeys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current;
                    if (string.IsNullOrEmpty(matchValue) || key.Contains(matchValue))
                    {
                        if (widgetListDrawers.ContainsKey(key))
                        {
                            widgetListDrawers[key].DoLayoutList();
                        }
                    }
                }
            }
        }

        #region ButtonEvents
        private void LoadAllGroup()
        {
            if(groupKeys.Contains(activeGroupKey))
            {
                var value = widgetNodeDic[activeGroupKey];
                for (int i = 0; i < value.Count; i++){
                    CreateWidgetInstence(value[i]);
                }
            }
            else
            {
                using (var enumerator = groupKeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var value = widgetNodeDic[enumerator.Current];
                        for (int i = 0; i < value.Count; i++)
                        {
                            CreateWidgetInstence(value[i]);
                        }
                    }
                }
            }
        }
        private void LoadFromFile()
        {
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                Debug.LogError("控件信息丢失：" + configPath);
                LoadFromWidgetGroup(new WidgetGroup());
            }
            else
            {
                var bytes = System.IO.File.ReadAllBytes(configPath);
                if (bytes != null && bytes.Length > 0)
                {
                    var widgetGroup = GroupInfoFromBytes(bytes);
                    LoadFromWidgetGroup(widgetGroup);
                }
                else
                {
                    Debug.LogError("安装信息为空");
                }
            }
        }

        private void LoadFromWidgetGroup(WidgetGroup widgetGroup)
        {
            if (widgetGroup != null)
            {
                widgetNodeDic.Clear();
                groupKeys.Clear();
                widgetListDrawers.Clear();

                headInfo = widgetGroup.headInfo;
                if (headInfo == null)
                {
                    headInfo = new HeadInfo();
                }
                //设置起始id
                WidgetNode.currentID = headInfo.currentID;
                Dictionary<string, WidgetNode> widgetDic = new Dictionary<string, WidgetNode>();
                var rootWidgets = widgetEditorCtrl.MakeRootWidgetNodes(widgetGroup.widgetNodes.ToArray());

                for (int i = 0; i < rootWidgets.Length; i++)
                {
                    var widget = rootWidgets[i];
                    widgetDic[widget.guid] = widget;
                }

                for (int i = 0; i < widgetGroup.groupInfo.Count; i++)
                {
                    var groupName = widgetGroup.groupInfo[i].name;
                    var widgets = widgetGroup.groupInfo[i].widgets;
                    if (widgets != null && widgets.Length > 0)
                    {
                        List<WidgetNode> widgetList = null;
                        if (!widgetNodeDic.TryGetValue(groupName, out widgetList))
                        {
                            widgetList = RegistGroup(groupName);
                        }

                        for (int j = 0; j < widgets.Length; j++)
                        {
                            var guid = widgets[j];
                            WidgetNode rootWidget = null;
                            if (widgetDic.TryGetValue(guid, out rootWidget))
                            {
                                widgetList.Add(rootWidget);
                            }
                        }
                    }
                }
            }
        }

        private List<WidgetNode> RegistGroup(string groupName)
        {
            var list = widgetNodeDic[groupName] = new List<WidgetNode>();
            groupKeys.Add(groupName);
            return list;
        }

        private void SaveRecordToFile()
        {
            if (!string.IsNullOrEmpty(configPath))
            {
                if (!EditorUtility.DisplayDialog("请确认", "将重写数据文件", "继续"))
                {
                    return;
                }
            }
            WidgetGroup group = new WidgetGroup();
            headInfo.currentID = WidgetNode.currentID;
            group.headInfo = headInfo;
            using (var enumerator = groupKeys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current;
                    var list = widgetNodeDic[key];
                    var groupInfo = new WidgeGroupInfo();
                    groupInfo.name = key;
                    groupInfo.widgets = list.Select(x => x.guid).ToArray();
                    group.groupInfo.Add(groupInfo);

                    widgetEditorCtrl.MakeSingleWidgetNodes(list.ToArray(), group.widgetNodes);
                }
            }
            var bytes = GroupInfoToBytes(group);
            if (bytes != null)
            {
                Debug.LogFormat("当前配制文件大小：{0}kb", bytes.Length / 1024);
                System.IO.File.WriteAllBytes(configPath, bytes);
            }
        }

        private void CreateWidgetInstence(WidgetNode node)
        {
            var instence = new GameObject(node.name);
            var instenceDic = new Dictionary<string, GameObject>();
            widgetEditorCtrl.MakeInsenceDicDeepth(instence, activeNode, instenceDic);
            widgetEditorCtrl.ChargeWidghtInfos(activeNode, instenceDic);

            SetAllChildLayer(instence, LayerMask.NameToLayer(defultLayerName));
            if (Selection.activeTransform != null)
            {
                instence.transform.SetParent(Selection.activeTransform, false);
            }
        }
        private void SetAllChildLayer(GameObject target, int layer)
        {
            target.layer = layer;
            if (target.transform.childCount == 0)
            {
                return;
            }
            for (int i = 0; i <= target.transform.childCount - 1; i++)
            {
                Transform ts = target.transform.GetChild(i);
                if (ts != null)
                {
                    SetAllChildLayer(ts.gameObject, layer);
                }
            }
        }

        private void OnGroupMenu_Delete()
        {
            if (string.IsNullOrEmpty(activeGroupKey)) return;

            if (EditorUtility.DisplayDialog("是否删除", "些操作无法恢复，请确认！", "确认"))
            {
                EditorApplication.delayCall += () =>
                {
                    var removed = groupKeys.Remove(activeGroupKey);

                    if (removed)
                    {
                        groupKeys.Remove(activeGroupKey);
                        widgetNodeDic.Remove(activeGroupKey);
                        UpdateDrawers();
                    }
                };
            }
        }

        private void OnGroupMenu_MoveDown()
        {
            if (string.IsNullOrEmpty(activeGroupKey)) return;
            EditorApplication.delayCall += () =>
            {
                if (widgetNodeDic.ContainsKey(activeGroupKey))
                {
                    var index = groupKeys.IndexOf(activeGroupKey);
                    if (index < groupKeys.Count - 1)
                    {
                        groupKeys.RemoveAt(index);
                        groupKeys.Insert(index + 1, activeGroupKey);
                    }
                }
            };
        }

        private void OnGroupMenu_MoveUp()
        {
            if (string.IsNullOrEmpty(activeGroupKey)) return;
            EditorApplication.delayCall += () =>
            {
                if (widgetNodeDic.ContainsKey(activeGroupKey))
                {
                    var index = groupKeys.IndexOf(activeGroupKey);
                    if (index > 0)
                    {
                        groupKeys.RemoveAt(index);
                        groupKeys.Insert(index - 1, activeGroupKey);
                    }
                }
            };
        }

        #endregion

        #region PrivateFunctions
        private void ImportTextureData()
        {
            if (activeNode != null)
            {
                var imgFile = EditorUtility.OpenFilePanel("选择图片文件", importPreviewPath, "png");
                if (!string.IsNullOrEmpty(imgFile))
                {
                    importPreviewPath = System.IO.Path.GetDirectoryName(imgFile);
                    var bytes = System.IO.File.ReadAllBytes(imgFile);
                    var scale = bytes.Length / 2048f;
                    byte[] recordBytes = null;
                    if (scale >= 1)
                    {
                        recordBytes = ScaleTextureToTargetLength(bytes, 80);
                    }
                    else
                    {
                        recordBytes = bytes;
                    }

                    if (recordBytes.Length < 4096)
                    {
                        activeNode.preview_bytes = recordBytes;
                        activeNode.ReadAndRecordTexture();
                        Debug.LogFormat("新增预览图：{0} 大小：{1}kb", System.IO.Path.GetFileNameWithoutExtension(imgFile), (recordBytes.Length / 1024f).ToString("0.0"));
                    }
                    else
                    {
                        Debug.LogFormat("图片缩放后依然超过4k达到了{0}k,请压缩后重新试！", (recordBytes.Length / 1024f).ToString("0.0"));
                    }
                }
            }
        }

        /// 对尺寸进行缩放以实现尺寸压缩
        private byte[] ScaleTextureToTargetLength(byte[] bytes, int targetWidth)
        {
            var source = new Texture2D(0, 0);
            source.LoadImage(bytes);
            var width = targetWidth;
            var height = Mathf.FloorToInt((source.height / (float)source.width) * width);
            var scaledTexture = ScaleTexture(source, width, height);
            return scaledTexture.EncodeToJPG();
        }

        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
            float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.filterMode = FilterMode.Point;
            result.mipMapBias = 1;
            result.Apply();
            return result;
        }

        private void ExportTextureData()
        {
            if (activeNode != null && activeNode.preview_bytes != null && activeNode.preview_bytes.Length > 0)
            {
                var imgFile = EditorUtility.SaveFilePanel("保存图片文件", exportPreviewPath, activeNode.name, "png");
                if (!string.IsNullOrEmpty(imgFile))
                {
                    exportPreviewPath = System.IO.Path.GetDirectoryName(imgFile);
                    System.IO.File.WriteAllBytes(imgFile, activeNode.preview_bytes);
                }
            }
            else
            {
                Debug.LogError("目标对象无预览图片！");
            }
        }

        private void DeleteTextureData()
        {
            if (activeNode != null && activeNode.preview_bytes != null && activeNode.preview_bytes.Length > 0)
            {
                activeNode.ClearTexture();
            }
            else
            {
                Debug.LogError("目标对象无预览图片！");
            }
        }

        private void RecordToGroup(string groupName)
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
            {
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    var go = Selection.gameObjects[i];
                    var dic = new Dictionary<int, WidgetNode>();
                    var widgetNode = widgetEditorCtrl.MakeWidgetDicDeepth(go, dic);
                    widgetEditorCtrl.AnalysisComponetInfos(dic);
                    List<WidgetNode> list = null;
                    if (!widgetNodeDic.TryGetValue(groupName, out list))
                    {
                        var widgetList = RegistGroup(groupName);
                        widgetList.Add(widgetNode);
                    }
                    else
                    {
                        widgetNodeDic[groupName].Add(widgetNode);
                    }
                }
                UpdateDrawers();
            }
        }

        private void ImportJson()
        {
            var filePath = EditorUtility.OpenFilePanel("导入json", importJsonPath, "json");
            if (!string.IsNullOrEmpty(filePath))
            {
                importJsonPath = System.IO.Path.GetDirectoryName(filePath);
                var text = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                if (!string.IsNullOrEmpty(text))
                {
                    var widgetGroup = JsonUtility.FromJson<WidgetGroup>(text);
                    LoadFromWidgetGroup(widgetGroup);
                    UpdateDrawers();
                }
            }
        }

        private void ExportJson()
        {
            var jsonValue = GroupInfoToJson();
            if (!string.IsNullOrEmpty(jsonValue))
            {
                var filePath = EditorUtility.SaveFilePanel("导出json", exportJsonPath, "控件配制", "json");
                if (!string.IsNullOrEmpty(filePath))
                {
                    exportJsonPath = System.IO.Path.GetDirectoryName(filePath);
                    System.IO.File.WriteAllText(filePath, jsonValue, System.Text.Encoding.UTF8);
                }
            }
        }

        private void AddGroup()
        {
            activeNode = null;

            if (!string.IsNullOrEmpty(matchValue))
            {
                if (!widgetNodeDic.ContainsKey(matchValue))
                {
                    RegistGroup(matchValue);
                }
                UpdateDrawers();
            }
        }

        private string GroupInfoToJson()
        {
            WidgetGroup group = new WidgetGroup();
            headInfo.currentID = WidgetNode.currentID;
            group.headInfo = headInfo;
            using (var enumerator = groupKeys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current;
                    var list = widgetNodeDic[key];
                    var groupInfo = new WidgeGroupInfo();
                    groupInfo.name = key;
                    groupInfo.widgets = list.Select(x => x.guid).ToArray();
                    group.groupInfo.Add(groupInfo);

                    widgetEditorCtrl.MakeSingleWidgetNodes(list.ToArray(), group.widgetNodes);
                }
            }
            var jsonValue = JsonUtility.ToJson(group);
            return jsonValue;
        }

        private WidgetGroup GroupInfoFromBytes(byte[] bytes)
        {
            if (bytes == null) return null;

            bytes = byteHelper.Decompress(bytes);

            WidgetGroup group = null;
            try
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    group = (WidgetGroup)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return group;
        }

        private byte[] GroupInfoToBytes(WidgetGroup group)
        {
            byte[] bytes = null;

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, group);
                bytes = stream.ToArray();
            }

            if (bytes != null)
            {
                bytes = byteHelper.Compress(bytes);
            }
            return bytes;
        }

        private void UpdateDrawers()
        {
            if (widgetNodeDic == null)
                return;

            using (var enumerator = widgetNodeDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;
                    var list = enumerator.Current.Value;
                    if (!widgetListDrawers.ContainsKey(key))
                    {
                        widgetListDrawers[key] = CreateReorederList(key, list);
                    }
                }
            }

        }
        private ReorderableList CreateReorederList(string key, List<WidgetNode> widgets)
        {
            var reorderList = new ReorderableList(widgets, typeof(WidgetNode));
            reorderList.drawHeaderCallback = (rect) =>
            {
                var keyIndex = groupKeys.IndexOf(key);
                //padding 2
                rect = new Rect(rect.x + 2, rect.y, rect.width, rect.height);

                var btnRect = new Rect(rect.x + rect.width - 40, rect.y, 40, rect.height);
                if (GUI.Button(btnRect, "记录", EditorStyles.miniButtonLeft))
                {
                    RecordToGroup(key);
                }
                EditorGUI.LabelField(rect, (keyIndex + 1) + "." + key);

                if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
                {
                    activeGroupKey = key;
                    var groupGenericMenu = new GenericMenu();
                    var index = groupKeys.IndexOf(key);
                    if (index > 0) groupGenericMenu.AddItem(new GUIContent("上移"), false, OnGroupMenu_MoveUp);
                    if (index < groupKeys.Count - 1) groupGenericMenu.AddItem(new GUIContent("下移"), false, OnGroupMenu_MoveDown);
                    groupGenericMenu.AddItem(new GUIContent("删除"), false, OnGroupMenu_Delete);
                    groupGenericMenu.ShowAsContext();
                }
            };
            reorderList.drawElementCallback = delegate (Rect rect, int index, bool isActive, bool isFocused)
            {
                GUI.Box(rect, "");
                //padding 2
                rect = new Rect(rect.x + 2, rect.y, rect.width, rect.height);

                WidgetNode node = widgets[index];

                if (isActive && isFocused)
                {
                    activeNode = node;
                }

                EditorGUI.LabelField(rect, new GUIContent((index + 1).ToString() + "." + node.name, node.desc));
                GUI.backgroundColor = Color.white;
            };

            reorderList.drawElementBackgroundCallback = delegate (Rect rect, int index, bool isActive, bool isFocused)
            {
                if (widgets.Count <= index || index < 0) return;

                WidgetNode node = widgets[index];

                if (activeNode == node)
                {
                    GUI.backgroundColor = Color.gray;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
            };

            reorderList.onRemoveCallback = (list) =>
            {
                if (EditorUtility.DisplayDialog("是否删除", "些操作无法恢复，请确认！", "确认"))
                {
                    if (list.index >= 0)
                        widgets.RemoveAt(list.index);
                };
            };
            reorderList.displayAdd = false;
            return reorderList;
        }

        #endregion
    }

    #region Controller
    //压缩数组
    public class ByteHelper
    {
        public const ushort COMPRESSION_FORMAT_LZNT1 = 2;
        public const ushort COMPRESSION_ENGINE_MAXIMUM = 0x100;

        [DllImport("ntdll.dll")]
        public static extern uint RtlGetCompressionWorkSpaceSize(ushort dCompressionFormat, out uint dNeededBufferSize, out uint dUnknown);

        [DllImport("ntdll.dll")]
        public static extern uint RtlCompressBuffer(ushort dCompressionFormat, byte[] dSourceBuffer, int dSourceBufferLength, byte[] dDestinationBuffer,
        int dDestinationBufferLength, uint dUnknown, out int dDestinationSize, IntPtr dWorkspaceBuffer);

        [DllImport("ntdll.dll")]
        public static extern uint RtlDecompressBuffer(ushort dCompressionFormat, byte[] dDestinationBuffer, int dDestinationBufferLength, byte[] dSourceBuffer, int dSourceBufferLength, out uint dDestinationSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LocalAlloc(int uFlags, IntPtr sizetdwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr hMem);


        public byte[] Decompress(byte[] buffer)
        {
            var outBuf = new byte[buffer.Length * 6];
            uint dwSize = 0, dwRet = 0;
            uint ret = RtlGetCompressionWorkSpaceSize(COMPRESSION_FORMAT_LZNT1, out dwSize, out dwRet);
            if (ret != 0) return null;

            ret = RtlDecompressBuffer(COMPRESSION_FORMAT_LZNT1, outBuf, outBuf.Length, buffer, buffer.Length, out dwRet);
            if (ret != 0) return null;

            Array.Resize(ref outBuf, (Int32)dwRet);
            return outBuf;
        }


        public byte[] Compress(byte[] buffer)
        {
            var outBuf = new byte[buffer.Length * 6];
            uint dwSize = 0, dwRet = 0;
            uint ret = RtlGetCompressionWorkSpaceSize(COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE_MAXIMUM, out dwSize, out dwRet);
            if (ret != 0) return null;

            int dstSize = 0;
            IntPtr hWork = LocalAlloc(0, new IntPtr(dwSize));
            ret = RtlCompressBuffer(COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE_MAXIMUM, buffer, buffer.Length, outBuf, outBuf.Length, 0, out dstSize, hWork);
            if (ret != 0) return null;

            LocalFree(hWork);

            Array.Resize(ref outBuf, dstSize);
            return outBuf;
        }
    }

    public class WidgetEditorCtrl
    {
        private const BindingFlags set_field_flags = BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        private const BindingFlags get_field_flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.FlattenHierarchy;
        private const BindingFlags get_prop_flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy;
        private const BindingFlags set_prop_flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.FlattenHierarchy;

        private ConventHelper conventHelper = new ConventHelper();
        private List<Type> ignorePropTypes = new List<Type>();

        public WidgetEditorCtrl()
        {
            ignorePropTypes.Add(typeof(CanvasRenderer));
        }
        /// <summary>
        /// 记录预制体关联信息到字典
        /// </summary>
        /// <param name="widgetItem"></param>
        /// <param name="widgetDic"></param>
        /// <returns></returns>
        public WidgetNode MakeWidgetDicDeepth(GameObject widgetItem, Dictionary<int, WidgetNode> widgetDic)
        {
            var node = WidgetNode.CreateOne();
            node.name = widgetItem.name;
            node.active = widgetItem.activeSelf;
            node.parent_guid = null;
            widgetDic.Add(widgetItem.GetInstanceID(), node);
            var childCount = widgetItem.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var childWidgetItem = widgetItem.transform.GetChild(i);
                var childNode = MakeWidgetDicDeepth(childWidgetItem.gameObject, widgetDic);
                node.AddChildNode(childNode);
            }
            return node;
        }

        // <summary>
        // 从预制体生成控件信息
        // </summary>
        // <param name="widgetItems"></param>
        // <param name="parent_guid"></param>
        // <returns></returns>
        public void AnalysisComponetInfos(Dictionary<int, WidgetNode> widgetDic)
        {
            if (widgetDic == null) return;
            using (var enumerator = widgetDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var instenceID = enumerator.Current.Key;
                    var widgetItem = EditorUtility.InstanceIDToObject(instenceID) as GameObject;
                    if (widgetItem)
                    {
                        enumerator.Current.Value.components = AnalysisComponentInfos(widgetItem, widgetDic);
                    }
                }
            }

        }

        /// <summary>
        /// 创建实例到字典
        /// </summary>
        /// <param name="root"></param>
        /// <param name="widgetNode"></param>
        /// <param name="instenceDic"></param>
        public void MakeInsenceDicDeepth(GameObject root, WidgetNode widgetNode, Dictionary<string, GameObject> instenceDic)
        {
            root.SetActive(widgetNode.active);
            instenceDic.Add(widgetNode.guid, root);
            for (int j = 0; j < widgetNode.components.Length; j++)
            {
                var componentInfo = widgetNode.components[j];
                var component = MustComponent(root, GetAssemble(componentInfo.assemble).GetType(componentInfo.type));

                if (component is Behaviour)
                {
                    (component as Behaviour).enabled = componentInfo.active;
                }
            }

            if (widgetNode.childNodes != null)
            {
                for (int i = 0; i < widgetNode.childNodes.Count; i++)
                {
                    var childNode = widgetNode.childNodes[i];
                    var childGo = new GameObject(childNode.name);
                   
                    MakeInsenceDicDeepth(childGo, childNode, instenceDic);
                    childGo.transform.SetParent(root.transform, false);
                }
            }
        }
        /// <summary>
        /// 深度填充信息
        /// </summary>
        /// <param name="widgetNode"></param>
        /// <param name="instenceDic"></param>
        public void ChargeWidghtInfos(WidgetNode widgetNode, Dictionary<string, GameObject> instenceDic)
        {
            var go = instenceDic[widgetNode.guid];

            if (widgetNode.components != null)
            {
                for (int i = 0; i < widgetNode.components.Length; i++)
                {
                    var componentInfo = widgetNode.components[i];

                    var component = MustComponent(go, GetAssemble(componentInfo.assemble).GetType(componentInfo.type));

                    if (component == null)
                    {
                        Debug.LogError("组件脚本丢失：" + componentInfo.type);
                        continue;
                    }

                    SetParmasInfos(componentInfo.paramsInfos, component, instenceDic);

                    if (componentInfo.classInfos != null)
                    {
                        for (int j = 0; j < componentInfo.classInfos.Length; j++)
                        {
                            var classInfo = componentInfo.classInfos[j];
                            object classInstence = null;
                            if (classInfo.isProperty)
                            {
                                var property = component.GetType().GetProperty(classInfo.fieldName, get_prop_flags);
                                if (property == null)
                                {
                                    Debug.LogError(componentInfo.type + " :属性丢失：" + classInfo.fieldName);
                                }
                                classInstence = property.GetValue(component, new object[] { });
                                if (classInstence == null)
                                {
                                    property.SetValue(component, System.Activator.CreateInstance(property.PropertyType), new object[] { });
                                    classInstence = property.GetValue(component, new object[] { });
                                }
                            }
                            else
                            {
                                var property = component.GetType().GetField(classInfo.fieldName, get_field_flags);
                                if (property == null)
                                {
                                    Debug.LogError(componentInfo.type + " :属性丢失：" + classInfo.fieldName);
                                }
                                classInstence = property.GetValue(component);
                                if (classInstence == null)
                                {
                                    property.SetValue(component, System.Activator.CreateInstance(property.FieldType));
                                    classInstence = property.GetValue(component);
                                }
                            }

                            if (classInstence != null)
                            {
                                SetParmasInfos(classInfo.parmasInfos, classInstence, instenceDic);
                                //Debug.Log("设置子类信息：" + classInfo.fieldName);
                            }
                            else
                            {
                                Debug.Log("类为空：" + classInfo.fieldName);
                            }
                        }
                    }
                }
            }

            if (widgetNode.childNodes != null)
            {
                for (int i = 0; i < widgetNode.childNodes.Count; i++)
                {
                    ChargeWidghtInfos(widgetNode.childNodes[i], instenceDic);
                }
            }
        }

        //忽略版本号的问题
        private Assembly GetAssemble(string assemble)
        {
            if (assemble.Contains(","))
            {
                assemble = assemble.Substring(0, assemble.IndexOf(","));
            }

            var assembles = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assembles.Length; i++)
            {
                var assembleItem = assembles[i];
                var assembleItem_subStr = assembleItem.FullName.Substring(0, assembleItem.FullName.IndexOf(","));
                if (assembleItem_subStr == assemble)
                {
                    return assembleItem;
                }
            }
            return Assembly.Load(assemble);
        }

        public void SetParmasInfos(Param[] paramsInfos, object instence, Dictionary<string, GameObject> instenceDic)
        {
            if (instence == null) return;

            if (paramsInfos != null && paramsInfos.Length > 0)
            {
                for (int j = 0; j < paramsInfos.Length; j++)
                {
                    var prop = paramsInfos[j];
                    if (prop.isProperty)
                    {
                        var property = instence.GetType().GetProperty(prop.fieldName, get_prop_flags);
                        if (property != null)
                        {
                            var fieldValue = conventHelper.StringToObject(property.PropertyType, prop.valueStr, instenceDic);
                            if (fieldValue != null)
                            {
                                property.SetValue(instence, fieldValue, new object[] { });
                            }
                            else
                            {
                                Debug.Log("can`t find " + prop.fieldName);
                            }
                        }
                        else
                        {
                            Debug.Log("can`t get prop: " + prop.fieldName);
                        }
                    }
                    else
                    {
                        var field = instence.GetType().GetField(prop.fieldName, get_field_flags);
                        if (field != null)
                        {
                            var fieldValue = conventHelper.StringToObject(field.FieldType, prop.valueStr, instenceDic);
                            if (fieldValue != null)
                            {
                                //私有字段赋值
                                instence.GetType().InvokeMember(prop.fieldName, set_field_flags, null, instence, new object[] { fieldValue });
                                //field.SetValue(instence, fieldValue);
                            }
                            else
                            {
                                Debug.Log("can`t find " + prop.fieldName);
                            }
                        }
                        else
                        {
                            Debug.Log("can`t get field: " + prop.fieldName);
                        }
                    }


                }
            }
        }

        /// <summary>
        /// 控件整合为树
        /// </summary>
        /// <param name="widgetNodes"></param>
        /// <returns></returns>
        public WidgetNode[] MakeRootWidgetNodes(WidgetNode[] widgetNodes)
        {
            if (widgetNodes == null) return null;
            List<WidgetNode> rootNodes = new List<WidgetNode>();
            List<WidgetNode> childNodes = new List<WidgetNode>();
            Dictionary<string, WidgetNode> nodeDic = new Dictionary<string, WidgetNode>();
            for (int i = 0; i < widgetNodes.Length; i++)
            {
                var widgetNode = widgetNodes[i];
                if (string.IsNullOrEmpty(widgetNode.parent_guid))
                {
                    rootNodes.Add(widgetNode);
                }
                else
                {
                    childNodes.Add(widgetNode);
                }

                if (!string.IsNullOrEmpty(widgetNode.guid))
                {
                    nodeDic[widgetNode.guid] = widgetNode;
                }
            }

            for (int i = 0; i < childNodes.Count; i++)
            {
                var childNode = childNodes[i];
                WidgetNode parentNode = null;

                if(childNode.parent_guid == "75")
                {
                    Debug.LogError(childNode.name);
                }

                if (nodeDic.TryGetValue(childNode.parent_guid, out parentNode))
                {
                    parentNode.AddChildNode(childNode);
                }
                else
                {
                    Debug.LogError("节点父级信息丢失:" + childNode.name + ":" + childNode.guid);
                }
            }

            return rootNodes.ToArray();
        }

        /// <summary>
        /// 节点树拆分单个
        /// </summary>
        /// <param name="widgetNode"></param>
        /// <returns></returns>
        public void MakeSingleWidgetNodes(WidgetNode[] widgetNodes, List<WidgetNode> listNodes)
        {
            if (widgetNodes == null || listNodes == null) return;

            for (int i = 0; i < widgetNodes.Length; i++)
            {
                var widgetNode = widgetNodes[i];
                if (widgetNode != null)
                {
                    listNodes.Add(widgetNode);
                }

                MakeSingleWidgetNodes(widgetNode.childNodes.ToArray(), listNodes);
            }

        }

        #region 控件信息转换
        public ComponentInfo[] AnalysisComponentInfos(GameObject target, Dictionary<int, WidgetNode> widgetDic)
        {
            var components = target.GetComponents<Component>();
            var infos = new List<ComponentInfo>();
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var info = ToCommonentInfo(component, widgetDic);
                infos.Add(info);
            }
            return infos.ToArray();
        }

        public ComponentInfo ToCommonentInfo(Component component, Dictionary<int, WidgetNode> widgetDic)
        {
            ComponentInfo info = new ComponentInfo();
            var componetType = component.GetType();
            info.type = componetType.FullName;
            info.assemble = componetType.Assembly.FullName.Substring(0, componetType.Assembly.FullName.IndexOf(","));

            if (component is Behaviour)
                info.active = (component as Behaviour).enabled;

            var classInfos = new List<ClassInfo>();
            info.paramsInfos = AnalysisParmasInfo(componetType, component, classInfos, widgetDic);
            info.classInfos = classInfos.ToArray();

            return info;
        }

        private Param[] AnalysisParmasInfo(Type type, object obj, List<ClassInfo> classInfos, Dictionary<int, WidgetNode> widgetDic)
        {
            var fields = GetAllSupportedFieldInfos(type);

            var paramsInfos = new List<Param>();
            for (int j = 0; j < fields.Length; j++)
            {
                var field = fields[j];
                var fieldValue = field.GetValue(obj);
                if (fieldValue != null)
                {
                    var value = conventHelper.GetStringValue(field.FieldType, fieldValue, widgetDic);
                    if (!string.IsNullOrEmpty(value))
                    {
                        //Debug.Log(obj + "-record value:" + field.Name);
                        paramsInfos.Add(new Param(field.Name, value, false));
                    }
                    else if (IsSerializableClassType(fieldValue.GetType()))
                    {
                        var classInfo = TryGenerateClassInfo(field.FieldType, field.Name, fieldValue, widgetDic);
                        if (classInfos != null && classInfo != null)
                        {
                            classInfo.isProperty = false;
                            classInfos.Add(classInfo);
                            //Debug.Log(obj + "-record class:" + field.Name);
                        }
                        else
                        {
                            //默认用json来存
                            value = JsonUtility.ToJson(fieldValue);
                            paramsInfos.Add(new Param(field.Name, value, false));
                            //Debug.Log(obj + "-record json:" + field.Name);
                        }
                    }
                    else
                    {
                        Debug.Log(obj + "-ignore:" + field.Name);
                    }
                }
                else
                {
                   Debug.Log(obj + "-empty:" + field.Name);
                }
            }

            var props = GetAllSupportedPropInfos(type);
            for (int j = 0; j < props.Length; j++)
            {
                var prop = props[j];
                try
                {
                    var fieldValue = prop.GetValue(obj, new object[] { });
                    if (fieldValue != null)
                    {
                        var value = conventHelper.GetStringValue(prop.PropertyType, fieldValue, widgetDic);
                        if (!string.IsNullOrEmpty(value))
                        {
                            paramsInfos.Add(new Param(prop.Name, value, true));
                        }
                        else if (IsSerializableClassType(fieldValue.GetType()))
                        {
                            var classInfo = TryGenerateClassInfo(prop.PropertyType, prop.Name, fieldValue, widgetDic);
                            if (classInfos != null && classInfo != null)
                            {
                                classInfo.isProperty = true;
                                classInfos.Add(classInfo);
                            }
                            else
                            {
                                //默认用json来存
                                value = JsonUtility.ToJson(fieldValue);
                                paramsInfos.Add(new Param(prop.Name, value, false));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

            }
            return paramsInfos.ToArray();
        }

        /// <summary>
        /// 可序列化的子类
        /// </summary>
        /// <returns></returns>
        private bool IsSerializableClassType(Type type)
        {
            if (!type.IsClass) return false;
            if (type.IsArray) return false;
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return false;
            return true;
        }

        private ClassInfo TryGenerateClassInfo(Type type, string fullName, object target, Dictionary<int, WidgetNode> widgetDic)
        {
            if (target == null) return null;
            if (!type.IsClass) return null;
            if (type == typeof(string)) return null;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return null;
            //null表示不分析类中的类？
            var subInfo = AnalysisParmasInfo(type, target, null, widgetDic);
            if (subInfo != null)
            {
                Debug.LogFormat("记录子类信息：name:{0} type:{1}", fullName, type);
                var classInfo = new ClassInfo();
                classInfo.fieldName = fullName;
                classInfo.parmasInfos = subInfo;
                return classInfo;
            }
            else
            {
                Debug.LogFormat("子类信息分析失败：name:{0} type:{1}", fullName, type);
            }
            return null;
        }

        #endregion
        private Component MustComponent(GameObject obj, Type scriptType)
        {
            if (!typeof(Component).IsAssignableFrom(scriptType))
            {
                return null;
            }
            var comp = obj.GetComponent(scriptType);
            if (comp == null)
            {
                comp = obj.AddComponent(scriptType);
            }
            return comp;
        }
        /// <summary>
        /// 找到Behaivers所有支持FieldInfo
        /// </summary>
        /// <param name="behaiver"></param>
        /// <returns></returns>
        private FieldInfo[] GetAllSupportedFieldInfos(Type type)
        {
            var fields = type.GetFields(get_field_flags);
            var supportedFields = new List<FieldInfo>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!field.IsPublic)
                {
                    var attributes = field.GetCustomAttributes(true);
                    var supported = false;
                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (attributes[j] is SerializeField)
                        {
                            supported = true;
                        }
                    }

                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                        continue;
                    if (typeof(UnityEngine.Events.UnityEvent).IsAssignableFrom(field.FieldType))
                        continue;
                    if (typeof(UnityEngine.Events.UnityEvent<>).IsAssignableFrom(field.FieldType))
                        continue;
                    if (!supported)
                        continue;
                }
                else
                {
                    var noserialize = field.GetCustomAttributes(true).Where(x => x.GetType() == typeof(System.NonSerializedAttribute)).Count() > 0;
                    if (noserialize) continue;
                }
                supportedFields.Add(field);
            }
            return supportedFields.ToArray();
        }

        /// <summary>
        /// 找到Behaivers所有支持属性
        /// 内置控件只记录有用的信息
        /// </summary>
        /// <param name="behaiver"></param>
        /// <returns></returns>
        private PropertyInfo[] GetAllSupportedPropInfos(Type type)
        {
            var supportedProps = new List<PropertyInfo>();

            if(ignorePropTypes.Contains(type))
            {
                //不记录任何属性
            }
            else if (type == typeof(RectTransform))
            {
                supportedProps.Add(type.GetProperty("anchoredPosition"));
                supportedProps.Add(type.GetProperty("anchoredPosition3D"));
                supportedProps.Add(type.GetProperty("anchorMax"));
                supportedProps.Add(type.GetProperty("anchorMin"));
                supportedProps.Add(type.GetProperty("offsetMax"));
                supportedProps.Add(type.GetProperty("offsetMin"));
            }
            else if (type == typeof(Text))
            {
                supportedProps.Add(type.GetProperty("color"));
            }
            else if (type == typeof(Transform))
            {
                supportedProps.Add(type.GetProperty("localPosition"));
                supportedProps.Add(type.GetProperty("localRotation"));
                supportedProps.Add(type.GetProperty("localScale"));
            }
            else if (type == typeof(BoxCollider))
            {
                supportedProps.Add(type.GetProperty("center"));
                supportedProps.Add(type.GetProperty("extents"));
                supportedProps.Add(type.GetProperty("size"));
            }
            else
            {
                //var propertys = type.GetProperties(get_prop_flags);
                //for (int i = 0; i < propertys.Length; i++)
                //{
                //    var prop = propertys[i];
                //    if(prop.GetSetMethod() != null)
                //    {
                //        supportedProps.Add(prop);
                //    }
                //}
            }
            return supportedProps.ToArray();
        }
    }

    public class ConventHelper
    {
        public char ele_seperator = ',';
        public string group_left = "(";
        public string group_right = ")";
        public string group_pattern = @"\((.*)\)";
        public string range_left = "[";
        public string range_right = "]";
        public string range_pattern = @"\[(.*)\]";
        public string innerasset_address = "inner-";
        public string innerasset_guid = "0000000000000000f000000000000000";
        public Dictionary<string, UnityEngine.Object> inner_objDic;

        public List<Type> specialTypes = new List<Type>
        {
            typeof(AnimationCurve)
        };

        public ConventHelper()
        {
            LoadInnerAssets();
        }

        private void LoadInnerAssets()
        {
            inner_objDic = new Dictionary<string, UnityEngine.Object>();
            var allAssets = AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");
            for (int i = 0; i < allAssets.Length; i++)
            {
                var obj = allAssets[i];
                inner_objDic[obj.name] = obj;
            }
        }

        #region String To Object
        /// <summary>
        /// 按类型解析值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object StringToObject(Type type, string value, Dictionary<string, GameObject> dic)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (specialTypes.Contains(type))
            {
                return LoadSpecialObject(type, value, dic);
            }
            //资源型引用类型,记录guid
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object obj = null;
                if (IsAssetsType(type))//资源类型
                {
                    if (TryLoadAssetFromGUID(type, value, out obj))
                    {
                        return obj;
                    }
                    else
                    {
                        Debug.LogWarningFormat("参数{0}解析失败:guid找不到!", value);
                    }
                }
                else
                {
                    if (type == typeof(GameObject))
                    {
                        if (TryLoadAssetFromGUID(type, value, out obj))
                        {
                            return obj;
                        }
                        else if (dic.ContainsKey(value))
                        {
                            return dic[value].gameObject;
                        }
                        else
                        {
                            Debug.LogWarningFormat("参数{0}解析失败,未找到GameOject:{1}", value, type);
                        }
                    }
                    else if (type.IsSubclassOf(typeof(Component)))
                    {
                        if (TryLoadAssetFromGUID(type, value, out obj))
                        {
                            return obj;
                        }
                        else if (dic.ContainsKey(value))
                        {
                            return dic[value].gameObject.GetComponent(type);
                        }
                        else
                        {
                            Debug.LogWarningFormat("参数{0}解析失败未找到组件:{1}", value, type);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("参数{0}解析失败类型未判断:{1}", value, type);
                    }
                }
            }
            else if (IsInnerStructure(type))
            {
                var objValue = InnerStructFromString(type, value);
                if (objValue == null)
                {
                    Debug.LogWarningFormat("参数{0}解析失败,类型:{1}", value, type);
                }
                return objValue;
            }
            else if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return IconventibleFromString(type, value);
            }
            else if (type.IsSerializable)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var argmentType = type.GetGenericArguments()[0];
                    var range = RangeToArray(value);
                    var listValue = Activator.CreateInstance(type);
                    if (range != null)
                    {
                        var addmethod = type.GetMethod("Add");
                        for (int i = 0; i < range.Length; i++)
                        {
                            var rangeObj = StringToObject(argmentType, range[i], dic);
                            addmethod.Invoke(listValue, new object[] { rangeObj });
                        }
                    }
                    return listValue;
                }
                else if (type.IsArray)
                {
                    var argmentType = type.GetElementType();
                    var listValue = Activator.CreateInstance(typeof(List<>).MakeGenericType(argmentType));
                    var range = RangeToArray(value);
                    if (range != null)
                    {
                        var addmethod = listValue.GetType().GetMethod("Add");
                        for (int i = 0; i < range.Length; i++)
                        {
                            var rangeObj = StringToObject(argmentType, range[i], dic);
                            addmethod.Invoke(listValue, new object[] { rangeObj });
                        }
                    }
                    return listValue.GetType().GetMethod("ToArray").Invoke(listValue, new object[] { });
                }
                else
                {
                    return JsonUtility.FromJson(value, type);
                }
            }

            Debug.LogWarningFormat("参数{0}解析失败,类型:{1}", value, type);
            return null;
        }

        /// <summary>
        /// 加载特殊对象信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private object LoadSpecialObject(Type type, string valueStr, Dictionary<string, GameObject> dic)
        {
            if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = new AnimationCurve();
                var array = GroupToArray(valueStr);

                for (int i = 0; i < array.Length; i += 2)
                {
                    float time = 0;
                    if (!float.TryParse(array[i], out time))
                    {
                        Debug.LogError(array[i]);
                    }
                    float value = 0;
                    if (array.Length > i + 1)
                    {
                        if (!float.TryParse(array[i + 1], out value))
                        {
                            Debug.LogError(array[i + 1]);
                        }
                    }
                    curve.AddKey(time, value);
                }
                return curve;
            }
            return null;
        }

        /// <summary>
        /// 尝试从guid加载资源
        /// </summary>
        /// <param name="type"></param>
        /// <param name="guid"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        private bool TryLoadAssetFromGUID(Type type, string guid, out UnityEngine.Object asset)
        {
            if (guid.StartsWith(innerasset_address))
            {
                var assetName = guid.Substring(innerasset_address.Length);
                inner_objDic.TryGetValue(assetName, out asset);
                return asset != null;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(assetPath))//资源路径
            {
                asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                return asset != null;
            }
            asset = null;
            return false;
        }

        public object InnerStructFromString(Type type, string value)
        {
            if(type == typeof(SpriteState))
            {
                return StringToSpriteState(value);
            }
            else if (type == typeof(ColorBlock))
            {
                return StringToColorBlock(value);
            }
            else if (type == typeof(Color))
            {
                return StringToColor(value);
            }
            else if (type == typeof(Vector2))
            {
                return StringToVector2(value);
            }
            else if (type == typeof(Vector3))
            {
                return StringToVector3(value);
            }
            else if (type == typeof(Vector4))
            {
                return StringToVector4(value);
            }
            else if (type == typeof(Rect))
            {
                return StringToRect(value);
            }
            else if (type == typeof(Quaternion))
            {
                return StringToQuaternion(value);
            }
            else if (type == typeof(Matrix4x4))
            {
                return StringToMatrix4x4(value);
            }
            Debug.LogWarningFormat("未成功解析类型为{0}的信息", type.FullName);
            return null;
        }
        public SpriteState StringToSpriteState(string value, string spriteFolderPath)
        {
            var array = GroupToArray(value);
            if (array == null) return default(SpriteState);
            SpriteState spriteState = new SpriteState();
            for (int i = 0; i < array.Length; i++)
            {
                var sprite = StringToObject<Sprite>(array[i], spriteFolderPath);
                if (sprite == null) continue;
                if (i == 0)
                {
                    spriteState.highlightedSprite = sprite;
                }
                else if (i == 1)
                {
                    spriteState.pressedSprite = sprite;
                }
                else if (i == 2)
                {
                    spriteState.disabledSprite = sprite;
                }
            }
            return spriteState;
        }
        public ColorBlock StringToColorBlock(string value)
        {
            var array = GroupToArray(value);
            if (array == null)
            {
                return default(ColorBlock);
            }

            ColorBlock colorBlock = new ColorBlock();
            colorBlock.colorMultiplier = 1;
            for (int i = 0; i < 4; i++)
            {
                Color color = Color.white;
                if (array.Length > i)
                {
                    color = StringToColor(array[i]);
                }
                if (i == 0)
                {
                    colorBlock.normalColor = color;
                }
                else if (i == 1)
                {
                    colorBlock.highlightedColor = color;
                }
                else if (i == 2)
                {
                    colorBlock.pressedColor = color;
                }
                else if (i == 3)
                {
                    colorBlock.disabledColor = color;
                }
            }
            return colorBlock;
        }

        public SpriteState StringToSpriteState(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.Log("图片组解析失败！");
                return default(SpriteState);
            }

            var array = GroupToArray(value);
            SpriteState spriteState = new SpriteState();
            for (int i = 0; i < 3; i++)
            {
                if (array.Length <= i) break;
                UnityEngine.Object sprite = null;
                if (TryLoadAssetFromGUID(typeof(Sprite), array[i], out sprite))
                {
                    if (i == 0) spriteState.highlightedSprite = (Sprite)sprite;
                    else if (i == 1) spriteState.pressedSprite = (Sprite)sprite;
                    else if (i == 2) spriteState.disabledSprite = (Sprite)sprite;
                }
            }
            return spriteState;
        }

        public Color StringToColor(string value)
        {
            if (!value.StartsWith("#"))
            {
                value = "#" + value;
            }
            var color = Color.white;
            ColorUtility.TryParseHtmlString(value, out color);
            return color;
        }

        public T StringToObject<T>(string path, string folderPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) return null;

            var fullPath = folderPath + "/" + path;
            fullPath = System.IO.Path.GetFullPath(fullPath);
            if (System.IO.File.Exists(fullPath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                if (typeof(T) == typeof(Texture))
                {
                    var texture = new Texture2D(2048, 2048);
                    texture.LoadImage(fileBytes, false);
                    return texture as T;
                }
                else if (typeof(T) == typeof(Sprite))
                {
                    var texture = new Texture2D(2048, 2048);
                    texture.LoadImage(fileBytes, false);
                    var rect = new Rect(0, 0, texture.width, texture.height);
                    return Sprite.Create(texture, rect, rect.size * 0.5f) as T;
                }
                else
                {
                    Debug.LogWarning("暂时无法运行时加载：" + typeof(T).FullName);
                }
            }
            else
            {
                Debug.LogWarning("未找到文件：" + fullPath);
            }
            return null;
        }

        public Rect StringToRect(string rectString)
        {
            var array = RangeToArray(rectString);
            if (array == null)
            {
                return new Rect();
            }
            var rect = new Rect();
            for (int i = 0; i < array.Length; i++)
            {
                float value = 0;
                float.TryParse(array[i], out value);
                if (i == 0)
                {
                    rect.x = value;
                }
                else if (i == 1)
                {
                    rect.y = value;
                }
                else if (i == 2)
                {
                    rect.width = value;
                }
                else if (i == 3)
                {
                    rect.height = value;
                }
            }
            return rect;
        }

        public Quaternion StringToQuaternion(string rectString)
        {
            var array = RangeToArray(rectString);
            if (array == null)
            {
                return new Quaternion();
            }
            var rect = new Quaternion();
            for (int i = 0; i < array.Length; i++)
            {
                float value = 0;
                float.TryParse(array[i], out value);
                if (i == 0)
                {
                    rect.x = value;
                }
                else if (i == 1)
                {
                    rect.y = value;
                }
                else if (i == 2)
                {
                    rect.z = value;
                }
                else if (i == 3)
                {
                    rect.w = value;
                }
            }
            return rect;
        }

        public Matrix4x4 StringToMatrix4x4(string rectString)
        {
            var array = RangeToArray(rectString);
            if (array == null)
            {
                return new Matrix4x4();
            }
            var rect = new Matrix4x4();
            for (int i = 0; i < array.Length; i++)
            {
                float value = 0;
                float.TryParse(array[i], out value);
                if (i < 16)
                {
                    rect[i] = value;
                }
            }
            return rect;
        }

        public Vector2 StringToVector2(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector2.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 2)
                    {
                        vector[i] = array[i];
                    }
                }
            }
            return vector;
        }
        public Vector3 StringToVector3(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector3.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 3)
                    {
                        vector[i] = array[i];
                    }
                }
            }
            return vector;
        }
        public Vector4 StringToVector4(string param)
        {
            var array = GroupToFloatArray(param);
            var vector = Vector4.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i < 4)
                    {
                        vector[i] = array[i];
                    }
                }
            }
            return vector;
        }
        public float[] GroupToFloatArray(string groupStr)
        {
            var array = GroupToArray(groupStr);
            if (array == null)
            {
                return null;
            }
            var fArray = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                float.TryParse(array[i], out fArray[i]);
            }
            return fArray;
        }

        public string[] RangeToArray(string rangeStr)
        {
            if (string.IsNullOrEmpty(rangeStr)) return null;

            var match = Regex.Match(rangeStr, range_pattern);
            if (match.Success)
            {
                var text = match.Groups[1];
                var array = text.Value.Split(ele_seperator);
                return array;
            }
            return null;
        }

        public string[] GroupToArray(string groupStr)
        {
            var match = Regex.Match(groupStr, group_pattern);
            if (match.Success)
            {
                var text = match.Groups[1];
                if (!text.Value.Contains(ele_seperator.ToString()))
                {
                    return new string[0];
                }
                else
                {
                    var array = text.Value.Split(ele_seperator);
                    return array;
                }

            }
            return null;
        }

        public object IconventibleFromString(Type type, string value)
        {
            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                if (type.IsSubclassOf(typeof(System.Enum)))
                {
                    return Enum.Parse(type, value);
                }
                else
                {
                    try
                    {
                        var objValue = Convert.ChangeType(value, type);
                        return objValue;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message + ":" + value);
                    }
                }
            }
            return null;
        }
        #endregion

        #region Object To String
        /// <summary>
        /// 按类型得到字符串值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="root"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetStringValue(Type type, object value, Dictionary<int, WidgetNode> widgetDic)
        {
            if (value == null) return null;

            if (specialTypes.Contains(type))
            {
                return SpecialObjectToString(type, value, widgetDic);
            }
            else if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return value.ToString();
            }
            //资源型引用类型,记录guid
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                var guid = ObjectToString(type, value, widgetDic);
                return guid;
            }
            else if (IsInnerStructure(type))
            {
                var strvalue = InnerStructObjectToString(type, value);
                if (!string.IsNullOrEmpty(strvalue))
                {
                    return strvalue;
                }
            }
            else if (value != null)
            {
                if (type.IsSerializable)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var array = type.GetMethod("ToArray").Invoke(value, new object[] { }) as object[];
                        string[] valueArray = new string[array.Length];
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] != null)
                            {
                                var strValue = GetStringValue(array[i].GetType(), array[i], widgetDic);
                                valueArray[i] = strValue;
                            }
                        }
                        return ArrayToRange(valueArray);
                    }
                    else if (type.IsArray)
                    {
                        object[] array = value as object[];
                        string[] valueArray = new string[array.Length];
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] != null)
                            {
                                var strValue = GetStringValue(array[i].GetType(), array[i], widgetDic);
                                valueArray[i] = strValue;
                            }
                        }
                        return ArrayToRange(valueArray);
                    }
                }
            }
            return null;
        }

        private string ObjectToString(Type type, object value, Dictionary<int, WidgetNode> widgetDic)
        {
            UnityEngine.Object uObj = (UnityEngine.Object)value;
            if (uObj == null) return null;

            string guid = null;
            WidgetNode widgetNode;

            if (IsAssetsType(type))//资源类型
            {
                if (TryGetGUID(uObj, out guid))
                {
                    if (guid == innerasset_guid)//内置资源
                    {
                        Debug.Log("引用内置资源：" + uObj.name);
                        return innerasset_address + uObj.name;
                    }
                    else
                    {
                        return guid;
                    }
                }
            }
            else if (type == typeof(GameObject))//相对引用
            {
                if (TryGetGUID(uObj, out guid))
                {
                    return guid;
                }
                else
                {
                    if (widgetDic.TryGetValue(uObj.GetInstanceID(), out widgetNode))
                    {
                        return widgetNode.guid;
                    }
                }
            }
            else if (type.IsSubclassOf(typeof(Component)))//组件类型
            {
                if (TryGetGUID(uObj, out guid))
                {
                    return guid;
                }
                else
                {
                    var go = (uObj as Component).gameObject;
                    if (widgetDic.TryGetValue(go.GetInstanceID(), out widgetNode))
                    {
                        return widgetNode.guid;
                    }
                }
            }
            return guid;
        }

        /// <summary>
        /// 特殊类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="widgetDic"></param>
        /// <returns></returns>
        public string SpecialObjectToString(Type type, object value, Dictionary<int, WidgetNode> widgetDic)
        {
            if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = value as AnimationCurve;
                List<string> array = new List<string>();
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    array.Add(curve.keys[i].time.ToString("f2"));
                    array.Add(curve.keys[i].value.ToString("f2"));
                }
                return ArrayToGroup(array.ToArray());
            }
            return null;
        }

        public string InnerStructObjectToString(Type type, object value)
        {
            if (type == typeof(Rect))
            {
                return RectToString((Rect)value);
            }
            else if (type == typeof(ColorBlock))
            {
                return ColorBlockToString((ColorBlock)value);
            }
            else if(type == typeof(SpriteState))
            {
                return SpriteStateToString((SpriteState)value);
            }
            else if (type == typeof(Color))
            {
                return ColorToString((Color)value);
            }
            else if (type == typeof(Vector2))
            {
                return Vector2ToString((Vector2)value);
            }
            else if (type == typeof(Vector3))
            {
                return Vector3ToString((Vector3)value);
            }
            else if (type == typeof(Vector4))
            {
                return Vector4ToString((Vector4)value);
            }
            else if (type == typeof(Quaternion))
            {
                return QuaternionToString((Quaternion)value);
            }
            else if (type == typeof(Matrix4x4))
            {
                return Materix4x4ToString((Matrix4x4)value);
            }
            return null;
        }

        public string RectToString(Rect rect)
        {
            var strArray = new string[]
            {
                rect.x.ToString("f2"),
                rect.y.ToString("f2"),
                rect.width.ToString("f2"),
                rect.height.ToString("f2")
            };
            return ArrayToRange(strArray);
        }

        public string ColorBlockToString(ColorBlock colorBlock)
        {
            var strArray = new string[]
            {
               ColorUtility.ToHtmlStringRGBA( colorBlock.normalColor),
               ColorUtility.ToHtmlStringRGBA( colorBlock.highlightedColor),
               ColorUtility.ToHtmlStringRGBA( colorBlock.pressedColor),
               ColorUtility.ToHtmlStringRGBA( colorBlock.disabledColor)
            };
            return ArrayToGroup(strArray);
        }

        public string SpriteStateToString(SpriteState spriteState)
        {
            string[] spritesStr = new string[3];
            Sprite[] sprites = new Sprite []{ spriteState.highlightedSprite, spriteState.pressedSprite ,spriteState.disabledSprite};

            for (int i = 0; i < 3; i++)
            {
                var uObj = sprites[i];
                if (uObj == null) continue;
                var guid = "";
                if (TryGetGUID(uObj, out guid))
                {
                    if (guid == innerasset_guid)//内置资源
                    {
                        Debug.Log("引用内置资源：" + uObj.name);
                        guid = innerasset_address + uObj.name;
                    }
                }
                spritesStr[i] = guid;
            }
            return ArrayToGroup(spritesStr);
        }

        public string ColorToString(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        public string Vector2ToString(Vector2 value)
        {
            var array = new string[2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }

        public string Vector3ToString(Vector3 value)
        {
            var array = new string[3];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public string Vector4ToString(Vector4 value)
        {
            var array = new string[4];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public string QuaternionToString(Quaternion value)
        {
            var array = new string[4];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }
        public string Materix4x4ToString(Matrix4x4 value)
        {
            var array = new string[16];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i].ToString("f2");
            }
            return ArrayToGroup(array);
        }

        public string IntToString(int value)
        {
            return value.ToString();
        }

        public int ToInt(string text)
        {
            var intValue = 0;
            int.TryParse(text, out intValue);
            return intValue;
        }
        public string StructToString(object value)
        {
            return JsonUtility.ToJson(value);
        }

        public string ArrayToGroup(params string[] array)
        {
            var group = group_left;
            group += string.Join(ele_seperator.ToString(), array);
            group += group_right;
            return group;
        }
        public string ArrayToRange(params string[] array)
        {
            var range = range_left;
            range += string.Join(ele_seperator.ToString(), array);
            range += range_right;
            return range;
        }

        #region TypeUtil
        public static bool IsInnerStructure(Type type)
        {
            if (typeof(IConvertible).IsAssignableFrom(type)) return false;

            if (type.IsValueType)
            {
                if (type.Assembly.FullName.Contains("UnityEngine"))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断是否是资源类型
        /// </summary>
        /// <param name="type"></param>
        public static bool IsAssetsType(Type type)
        {
            if (type == typeof(Sprite)) return true;
            if (type == typeof(Texture)) return true;
            if (type == typeof(Texture2D)) return true;
            if (type == typeof(AudioClip)) return true;
            if (type == typeof(Material)) return true;
            if (type == typeof(Shader)) return true;
            return false;
        }

        /// <summary>
        /// 试图获取物体的guid
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static bool TryGetGUID(UnityEngine.Object obj, out string guid)
        {
            var go = (UnityEngine.Object)obj;
            var path = AssetDatabase.GetAssetPath(go);

            if (!string.IsNullOrEmpty(path))
            {
                guid = AssetDatabase.AssetPathToGUID(path);
                return !string.IsNullOrEmpty(guid);
            }
            guid = null;
            return false;
        }

        #endregion
        #endregion
    }
    #endregion

    #region Model
    [System.Serializable]
    public class WidgetGroup
    {
        public HeadInfo headInfo = new HeadInfo();
        public List<WidgeGroupInfo> groupInfo = new List<WidgeGroupInfo>();
        public List<WidgetNode> widgetNodes = new List<WidgetNode>();
    }
    [System.Serializable]
    public class WidgeGroupInfo
    {
        public string name;
        public string[] widgets;
    }
    [System.Serializable]
    public class WidgetNode
    {
        public string guid;
        public string parent_guid;
        public string name;
        public bool active;
        public string desc;
        public byte[] preview_bytes;

        public static long currentID = 0;

        [NonSerialized]
        private Texture2D _previewTexture;
        public Texture2D preview
        {
            get
            {
                if (_previewTexture == null)
                {
                    _previewTexture = ReadAndRecordTexture();
                }
                return _previewTexture;
            }
        }
        public ComponentInfo[] components;

        [NonSerialized]
        private List<WidgetNode> _childNodes;
        public List<WidgetNode> childNodes
        {
            get
            {
                if (_childNodes == null)
                    _childNodes = new List<WidgetNode>();
                return _childNodes;
            }
        }

        protected WidgetNode() { }

        public static WidgetNode CreateOne()
        {
            var node = new WidgetNode();
            node.guid = (currentID++).ToString();
            return node;
        }

        public void AddChildNode(WidgetNode childNode)
        {
            childNode.parent_guid = guid;
            childNodes.Add(childNode);
        }

        public Texture2D ReadAndRecordTexture()
        {
            if (preview_bytes != null && preview_bytes.Length > 0)
            {
                Texture2D texture2d = new Texture2D(1024, 1024);
                texture2d.LoadImage(preview_bytes);
                _previewTexture = texture2d;
                return preview;
            }
            return null;
        }

        public void ClearTexture()
        {
            preview_bytes = null;
            _previewTexture = null;
        }

    }
    [System.Serializable]
    public class ComponentInfo
    {
        public string assemble;
        public string type;
        public bool active;
        public Param[] paramsInfos;
        public ClassInfo[] classInfos;
    }

    [Serializable]
    public class ClassInfo
    {
        public string fieldName;
        public bool isProperty;
        public Param[] parmasInfos;
    }

    [System.Serializable]
    public class Param
    {
        public bool isProperty;
        public string fieldName;
        public string valueStr;

        public Param() { }
        public Param(string key, string value, bool isProperty)
        {
            fieldName = key;
            valueStr = value;
            this.isProperty = isProperty;
        }
    }
    [System.Serializable]
    public class HeadInfo
    {
        public long currentID;
        public string json_importPath;
        public string json_exportPath;
        public string preview_importPath;
        public string preview_exportPath;
    }
    #endregion
}