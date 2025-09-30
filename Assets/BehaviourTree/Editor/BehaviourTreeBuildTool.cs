using System.Collections;
/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-12-19
 * Version: 1.0.0
 * Description: BehaviourTree构建工具，用于生成link.xml防止脚本被裁切
 *_*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UFrame.BehaviourTree;

namespace UFrame.BehaviourTree.Editors
{
    /// <summary>
    /// BehaviourTree构建工具
    /// 用于生成link.xml文件，防止Unity打包时裁切BTree实例引用的脚本
    /// </summary>
    public static class BehaviourTreeBuildTool
    {
        private const string DEFAULT_LINK_XML_PATH = "Assets/link.xml";
        private const string LINK_XML_ROOT_ELEMENT = "linker";
        private const string ASSEMBLY_ELEMENT = "assembly";
        private const string TYPE_ELEMENT = "type";
        private const string FULLNAME_ATTRIBUTE = "fullname";
        private const string PRESERVE_ATTRIBUTE = "preserve";

        /// <summary>
        /// 为单个BTree实例生成link.xml
        /// </summary>
        /// <param name="bTree">BTree实例</param>
        /// <param name="linkXmlPath">link.xml文件路径，默认为Assets/link.xml</param>
        /// <param name="additionalPaths">额外的link路径参数</param>
        public static void GenerateLinkXmlForBTree(BTree bTree, string linkXmlPath = DEFAULT_LINK_XML_PATH, params string[] additionalPaths)
        {
            if (bTree == null)
            {
                Debug.LogError("BTree实例为空，无法生成link.xml");
                return;
            }

            var types = new HashSet<Type>();
            CollectTypesFromBTree(bTree, types);

            // 添加额外的路径类型
            if (additionalPaths != null)
            {
                foreach (var path in additionalPaths)
                {
                    var type = Type.GetType(path);
                    if (type != null)
                    {
                        types.Add(type);
                    }
                    else
                    {
                        Debug.LogWarning($"无法找到类型: {path}");
                    }
                }
            }

            GenerateLinkXml(types, linkXmlPath);
        }

        /// <summary>
        /// 为BTree实例列表生成link.xml
        /// </summary>
        /// <param name="bTrees">BTree实例列表</param>
        /// <param name="linkXmlPath">link.xml文件路径，默认为Assets/link.xml</param>
        /// <param name="additionalPaths">额外的link路径参数</param>
        public static void GenerateLinkXmlForBTrees(IEnumerable<BTree> bTrees, string linkXmlPath = DEFAULT_LINK_XML_PATH, params string[] additionalPaths)
        {
            if (bTrees == null)
            {
                Debug.LogError("BTree实例列表为空，无法生成link.xml");
                return;
            }

            var types = new HashSet<Type>();

            foreach (var bTree in bTrees)
            {
                if (bTree != null)
                {
                    CollectTypesFromBTree(bTree, types);
                }
            }

            // 添加额外的路径类型
            if (additionalPaths != null)
            {
                foreach (var path in additionalPaths)
                {
                    var type = Type.GetType(path);
                    if (type != null)
                    {
                        types.Add(type);
                    }
                    else
                    {
                        Debug.LogWarning($"无法找到类型: {path}");
                    }
                }
            }

            GenerateLinkXml(types, linkXmlPath);
        }

        /// <summary>
        /// 从BTree实例中收集所有引用的类型
        /// </summary>
        /// <param name="bTree">BTree实例</param>
        /// <param name="types">类型集合</param>
        private static void CollectTypesFromBTree(BTree bTree, HashSet<Type> types)
        {
            if (bTree?.rootTree == null)
                return;

            CollectTypesFromTreeInfo(bTree.rootTree, types);
        }

        /// <summary>
        /// 从TreeInfo中收集所有引用的类型
        /// </summary>
        /// <param name="treeInfo">TreeInfo实例</param>
        /// <param name="types">类型集合</param>
        private static void CollectTypesFromTreeInfo(TreeInfo treeInfo, HashSet<Type> types)
        {
            if (treeInfo == null)
                return;

            // 收集节点类型
            if (treeInfo.node != null)
            {
                var nodeType = treeInfo.node.GetType();
                types.Add(nodeType);

                // 收集节点类型的所有基类
                CollectBaseTypes(nodeType, types);
            }

            // 收集条件节点类型
            if (treeInfo.condition?.conditions != null)
            {
                foreach (var condition in treeInfo.condition.conditions)
                {
                    if (condition.node != null)
                    {
                        var conditionType = condition.node.GetType();
                        types.Add(conditionType);
                        CollectBaseTypes(conditionType, types);
                    }

                    // 收集子条件节点类型
                    if (condition.subConditions != null)
                    {
                        foreach (var subCondition in condition.subConditions)
                        {
                            if (subCondition.node != null)
                            {
                                var subConditionType = subCondition.node.GetType();
                                types.Add(subConditionType);
                                CollectBaseTypes(subConditionType, types);
                            }
                        }
                    }
                }
            }

            // 递归收集子节点类型
            if (treeInfo.subTrees != null)
            {
                foreach (var subTree in treeInfo.subTrees)
                {
                    CollectTypesFromTreeInfo(subTree, types);
                }
            }
        }

        /// <summary>
        /// 收集类型的基类
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="types">类型集合</param>
        private static void CollectBaseTypes(Type type, HashSet<Type> types)
        {
            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                types.Add(baseType);
                baseType = baseType.BaseType;
            }
        }

        /// <summary>
        /// 生成link.xml文件
        /// </summary>
        /// <param name="types">需要保留的类型集合</param>
        /// <param name="linkXmlPath">link.xml文件路径</param>
        private static void GenerateLinkXml(HashSet<Type> types, string linkXmlPath)
        {
            if (types.Count == 0)
            {
                Debug.LogWarning("没有找到需要保留的类型");
                return;
            }

            // 按程序集分组
            var assemblyGroups = types.GroupBy(t => t.Assembly.FullName).ToDictionary(g => g.Key, g => g.ToList());

            // 读取现有的link.xml文件
            var existingTypes = new HashSet<string>();
            var existingAssemblies = new HashSet<string>();

            if (File.Exists(linkXmlPath))
            {
                ReadExistingLinkXml(linkXmlPath, existingTypes, existingAssemblies);
            }

            // 创建XML文档
            var xmlDoc = new XmlDocument();
            var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            // 创建根元素
            var linkerElement = xmlDoc.CreateElement(LINK_XML_ROOT_ELEMENT);
            xmlDoc.AppendChild(linkerElement);

            // 为每个程序集创建assembly元素
            foreach (var assemblyGroup in assemblyGroups)
            {
                var assemblyName = assemblyGroup.Key;
                var assemblyTypes = assemblyGroup.Value;

                // 检查是否已存在该程序集
                if (existingAssemblies.Contains(assemblyName))
                {
                    // 如果程序集已存在，需要合并类型
                    MergeAssemblyTypes(xmlDoc, linkerElement, assemblyName, assemblyTypes, existingTypes);
                }
                else
                {
                    // 创建新的assembly元素
                    CreateAssemblyElement(xmlDoc, linkerElement, assemblyName, assemblyTypes);
                }
            }

            // 保存XML文件
            try
            {
                var directory = Path.GetDirectoryName(linkXmlPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                xmlDoc.Save(linkXmlPath);
                Debug.Log($"成功生成link.xml文件: {linkXmlPath}，包含 {types.Count} 个类型");
            }
            catch (Exception ex)
            {
                Debug.LogError($"保存link.xml文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 读取现有的link.xml文件
        /// </summary>
        /// <param name="linkXmlPath">link.xml文件路径</param>
        /// <param name="existingTypes">现有类型集合</param>
        /// <param name="existingAssemblies">现有程序集集合</param>
        private static void ReadExistingLinkXml(string linkXmlPath, HashSet<string> existingTypes, HashSet<string> existingAssemblies)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(linkXmlPath);

                var assemblyNodes = xmlDoc.SelectNodes($"//{ASSEMBLY_ELEMENT}");
                if (assemblyNodes != null)
                {
                    foreach (XmlNode assemblyNode in assemblyNodes)
                    {
                        var assemblyName = assemblyNode.Attributes?["fullname"]?.Value;
                        if (!string.IsNullOrEmpty(assemblyName))
                        {
                            existingAssemblies.Add(assemblyName);

                            var typeNodes = assemblyNode.SelectNodes(TYPE_ELEMENT);
                            if (typeNodes != null)
                            {
                                foreach (XmlNode typeNode in typeNodes)
                                {
                                    var typeName = typeNode.Attributes?[FULLNAME_ATTRIBUTE]?.Value;
                                    if (!string.IsNullOrEmpty(typeName))
                                    {
                                        existingTypes.Add($"{assemblyName}|{typeName}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"读取现有link.xml文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 合并程序集类型到现有XML
        /// </summary>
        private static void MergeAssemblyTypes(XmlDocument xmlDoc, XmlElement linkerElement, string assemblyName, List<Type> assemblyTypes, HashSet<string> existingTypes)
        {
            // 查找现有的assembly元素
            var existingAssemblyElement = linkerElement.SelectSingleNode($"{ASSEMBLY_ELEMENT}[@fullname='{assemblyName}']") as XmlElement;

            if (existingAssemblyElement == null)
            {
                // 如果找不到现有程序集，创建新的
                CreateAssemblyElement(xmlDoc, linkerElement, assemblyName, assemblyTypes);
                return;
            }

            // 添加新的类型到现有程序集
            foreach (var type in assemblyTypes)
            {
                var typeKey = $"{assemblyName}|{type.FullName}";
                if (!existingTypes.Contains(typeKey))
                {
                    var typeElement = xmlDoc.CreateElement(TYPE_ELEMENT);
                    typeElement.SetAttribute(FULLNAME_ATTRIBUTE, type.FullName);
                    typeElement.SetAttribute(PRESERVE_ATTRIBUTE, "all");
                    existingAssemblyElement.AppendChild(typeElement);
                }
            }
        }

        /// <summary>
        /// 创建程序集元素
        /// </summary>
        private static void CreateAssemblyElement(XmlDocument xmlDoc, XmlElement linkerElement, string assemblyName, List<Type> assemblyTypes)
        {
            var assemblyElement = xmlDoc.CreateElement(ASSEMBLY_ELEMENT);
            assemblyElement.SetAttribute(FULLNAME_ATTRIBUTE, assemblyName);

            foreach (var type in assemblyTypes)
            {
                var typeElement = xmlDoc.CreateElement(TYPE_ELEMENT);
                typeElement.SetAttribute(FULLNAME_ATTRIBUTE, type.FullName);
                typeElement.SetAttribute(PRESERVE_ATTRIBUTE, "all");
                assemblyElement.AppendChild(typeElement);
            }

            linkerElement.AppendChild(assemblyElement);
        }

        /// <summary>
        /// 编辑器菜单项：为选中的BTree生成link.xml
        /// </summary>
        [MenuItem("Tools/BehaviourTree/Generate Link.xml for Selected BTree")]
        public static void GenerateLinkXmlForSelectedBTree()
        {
            var selectedObjects = Selection.objects;
            var bTrees = new List<BTree>();

            foreach (var obj in selectedObjects)
            {
                if (obj is BTree bTree)
                {
                    bTrees.Add(bTree);
                }
                else if (obj is GameObject go)
                {
                    var bTreeComponent = go.GetComponent<BTree>();
                    if (bTreeComponent != null)
                    {
                        bTrees.Add(bTreeComponent);
                    }
                }
            }

            if (bTrees.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选择BTree对象", "确定");
                return;
            }

            var linkXmlPath = EditorUtility.SaveFilePanel("保存link.xml", "Assets", "link", "xml");
            if (!string.IsNullOrEmpty(linkXmlPath))
            {
                GenerateLinkXmlForBTrees(bTrees, linkXmlPath);
            }
        }

        /// <summary>
        /// 编辑器菜单项：为项目中所有BTree资产生成link.xml
        /// </summary>
        [MenuItem("Tools/BehaviourTree/Generate Link.xml for All BTree Assets")]
        public static void GenerateLinkXmlForAllBTreeAssets()
        {
            // 查找所有BTree资产
            var bTreeGuids = AssetDatabase.FindAssets("t:BTree");
            if (bTreeGuids.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "项目中没有找到BTree资产", "确定");
                return;
            }

            var bTrees = new List<BTree>();
            foreach (var guid in bTreeGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var bTree = AssetDatabase.LoadAssetAtPath<BTree>(assetPath);
                if (bTree != null)
                {
                    bTrees.Add(bTree);
                }
            }

            if (bTrees.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到有效的BTree资产", "确定");
                return;
            }

            var linkXmlPath = EditorUtility.SaveFilePanel("保存link.xml", "Assets", "link", "xml");
            if (!string.IsNullOrEmpty(linkXmlPath))
            {
                GenerateLinkXmlForBTrees(bTrees, linkXmlPath);
            }
        }

        /// <summary>
        /// 检查选中的BaseNode脚本是否被BTree引用
        /// </summary>
        /// <param name="nodeType">要检查的BaseNode类型</param>
        /// <returns>引用结果信息</returns>
        public static NodeReferenceResult CheckNodeReferenceInProject(Type nodeType)
        {
            if (nodeType == null || !typeof(BaseNode).IsAssignableFrom(nodeType))
            {
                return new NodeReferenceResult
                {
                    NodeType = nodeType,
                    IsReferenced = false,
                    ErrorMessage = "类型为空或不继承自BaseNode"
                };
            }

            var result = new NodeReferenceResult
            {
                NodeType = nodeType,
                IsReferenced = false,
                ReferencedBTrees = new List<BTreeReferenceInfo>(),
                ReferencedAssets = new List<AssetReferenceInfo>()
            };

            // 1. 检查场景中的BTree
            CheckBTreeReferencesInScenes(nodeType, result);

            // 2. 检查Project中的BTree资产
            CheckBTreeReferencesInAssets(nodeType, result);

            result.IsReferenced = result.ReferencedBTrees.Count > 0 || result.ReferencedAssets.Count > 0;
            return result;
        }

        /// <summary>
        /// 检查场景中NodeBehaviour组件引用的BTree
        /// </summary>
        private static void CheckBTreeReferencesInScenes(Type nodeType, NodeReferenceResult result)
        {
            // 查找场景中所有NodeBehaviour组件
            var nodeBehaviours = UnityEngine.Object.FindObjectsOfType<NodeBehaviour>();
            foreach (var nodeBehaviour in nodeBehaviours)
            {
                if (nodeBehaviour.node != null && nodeBehaviour.node.GetType() == nodeType)
                {
                    // 如果NodeBehaviour直接引用了该节点类型
                    result.ReferencedBTrees.Add(new BTreeReferenceInfo
                    {
                        BTree = null, // NodeBehaviour不是BTree，所以为null
                        ReferenceType = "NodeBehaviour"
                    });
                }
            }
        }

        /// <summary>
        /// 检查Project资产中的BTree引用
        /// </summary>
        private static void CheckBTreeReferencesInAssets(Type nodeType, NodeReferenceResult result)
        {
            // 查找所有BTree资产
            var bTreeGuids = AssetDatabase.FindAssets("t:BTree");
            foreach (var guid in bTreeGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var bTree = AssetDatabase.LoadAssetAtPath<BTree>(assetPath);

                if (bTree != null && IsNodeTypeReferencedInBTree(bTree, nodeType))
                {
                    result.ReferencedAssets.Add(new AssetReferenceInfo
                    {
                        AssetPath = assetPath,
                        AssetName = System.IO.Path.GetFileNameWithoutExtension(assetPath),
                        BTree = bTree,
                        ReferenceType = "Asset"
                    });
                }
            }
        }

        /// <summary>
        /// 检查BTree中是否引用了指定的节点类型
        /// </summary>
        private static bool IsNodeTypeReferencedInBTree(BTree bTree, Type nodeType)
        {
            if (bTree?.rootTree == null)
                return false;

            return IsNodeTypeReferencedInTreeInfo(bTree.rootTree, nodeType);
        }

        /// <summary>
        /// 递归检查TreeInfo中是否引用了指定的节点类型
        /// </summary>
        private static bool IsNodeTypeReferencedInTreeInfo(TreeInfo treeInfo, Type nodeType)
        {
            if (treeInfo == null)
                return false;

            // 检查主节点
            if (treeInfo.node != null && treeInfo.node.GetType() == nodeType)
                return true;

            // 检查条件节点
            if (treeInfo.condition?.conditions != null)
            {
                foreach (var condition in treeInfo.condition.conditions)
                {
                    if (condition.node != null && condition.node.GetType() == nodeType)
                        return true;

                    // 检查子条件节点
                    if (condition.subConditions != null)
                    {
                        foreach (var subCondition in condition.subConditions)
                        {
                            if (subCondition.node != null && subCondition.node.GetType() == nodeType)
                                return true;
                        }
                    }
                }
            }

            // 递归检查子节点
            if (treeInfo.subTrees != null)
            {
                foreach (var subTree in treeInfo.subTrees)
                {
                    if (IsNodeTypeReferencedInTreeInfo(subTree, nodeType))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 编辑器菜单项：检查选中的BaseNode脚本是否被BTree引用
        /// </summary>
        [MenuItem("Tools/BehaviourTree/Check Selected BaseNode References")]
        public static void CheckSelectedBaseNodeReferences()
        {
            var selectedObjects = Selection.objects;
            var nodeTypes = new List<Type>();

            foreach (var obj in selectedObjects)
            {
                if (obj is MonoScript monoScript)
                {
                    var type = monoScript.GetClass();
                    if (type != null && typeof(BaseNode).IsAssignableFrom(type))
                    {
                        nodeTypes.Add(type);
                    }
                }
            }

            if (nodeTypes.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选择继承自BaseNode的脚本文件", "确定");
                return;
            }

            // 显示检查结果窗口
            NodeReferenceCheckWindow.ShowWindow(nodeTypes);
        }


    }

    /// <summary>
    /// 节点引用检查结果
    /// </summary>
    [System.Serializable]
    public class NodeReferenceResult
    {
        public Type NodeType;
        public bool IsReferenced;
        public List<BTreeReferenceInfo> ReferencedBTrees;
        public List<AssetReferenceInfo> ReferencedAssets;
        public string ErrorMessage;
    }

    /// <summary>
    /// BTree引用信息
    /// </summary>
    [System.Serializable]
    public class BTreeReferenceInfo
    {
        public BTree BTree;
        public string ReferenceType;
    }

    /// <summary>
    /// 资产引用信息
    /// </summary>
    [System.Serializable]
    public class AssetReferenceInfo
    {
        public string AssetPath;
        public string AssetName;
        public BTree BTree;
        public string ReferenceType;
    }

    /// <summary>
    /// 节点引用检查结果窗口
    /// </summary>
    public class NodeReferenceCheckWindow : EditorWindow
    {
        private List<Type> _nodeTypes;
        private List<NodeReferenceResult> _results;
        private Vector2 _scrollPosition;
        private bool _isChecking = false;

        public static void ShowWindow(List<Type> nodeTypes)
        {
            var window = GetWindow<NodeReferenceCheckWindow>("BaseNode引用检查");
            window._nodeTypes = nodeTypes;
            window._results = new List<NodeReferenceResult>();
            window.Show();

            // 开始检查
            window.StartCheck();
        }

        private void StartCheck()
        {
            _isChecking = true;
            _results.Clear();

            // 在后台线程中执行检查
            EditorApplication.delayCall += () =>
            {
                foreach (var nodeType in _nodeTypes)
                {
                    var result = BehaviourTreeBuildTool.CheckNodeReferenceInProject(nodeType);
                    _results.Add(result);
                }
                _isChecking = false;
                Repaint();
            };
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("BaseNode引用检查结果", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (_isChecking)
            {
                EditorGUILayout.HelpBox("正在检查引用关系，请稍候...", MessageType.Info);
                return;
            }

            if (_results == null || _results.Count == 0)
            {
                EditorGUILayout.HelpBox("没有检查结果", MessageType.Warning);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var result in _results)
            {
                DrawResult(result);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("重新检查"))
            {
                StartCheck();
            }
        }

        private void DrawResult(NodeReferenceResult result)
        {
            EditorGUILayout.BeginVertical("box");

            // 节点类型名称
            var typeName = result.NodeType?.Name ?? "未知类型";
            var statusText = result.IsReferenced ? "已被引用" : "未被引用";
            var statusColor = result.IsReferenced ? Color.green : Color.red;

            var originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField($"{typeName} - {statusText}", EditorStyles.boldLabel);
            GUI.color = originalColor;

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                EditorGUILayout.HelpBox(result.ErrorMessage, MessageType.Error);
                EditorGUILayout.EndVertical();
                return;
            }

            // 显示引用统计
            var totalReferences = result.ReferencedBTrees.Count + result.ReferencedAssets.Count;
            EditorGUILayout.LabelField($"总引用数: {totalReferences}");

            // 显示场景引用
            if (result.ReferencedBTrees.Count > 0)
            {
                EditorGUILayout.LabelField($"场景引用 ({result.ReferencedBTrees.Count}):", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var bTreeRef in result.ReferencedBTrees)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (bTreeRef.BTree != null)
                    {
                        EditorGUILayout.LabelField($"BTree: {bTreeRef.BTree.name}");
                        if (GUILayout.Button("定位", GUILayout.Width(50)))
                        {
                            Selection.activeObject = bTreeRef.BTree;
                            EditorGUIUtility.PingObject(bTreeRef.BTree);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"NodeBehaviour: {bTreeRef.ReferenceType}");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            // 显示资产引用
            if (result.ReferencedAssets.Count > 0)
            {
                EditorGUILayout.LabelField($"资产引用 ({result.ReferencedAssets.Count}):", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var assetRef in result.ReferencedAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"资产: {assetRef.AssetName}");
                    if (GUILayout.Button("定位", GUILayout.Width(50)))
                    {
                        Selection.activeObject = assetRef.BTree;
                        EditorGUIUtility.PingObject(assetRef.BTree);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField($"路径: {assetRef.AssetPath}");
                }
                EditorGUI.indentLevel--;
            }

            if (totalReferences == 0)
            {
                EditorGUILayout.HelpBox("该节点类型在项目中没有被任何BTree引用", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
