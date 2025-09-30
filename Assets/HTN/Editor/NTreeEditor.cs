using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UFrame.BehaviourTree;
using System.Linq;

namespace UFrame.HTN
{
    [CustomEditor(typeof(NTree))]
    public class NTreeEditor : UnityEditor.Editor
    {
        protected ReorderableList _nodeList;
        protected NTree _tree;
        [SerializeReference]
        public List<BaseNode> _nodes;
        protected TaskInfoDrawer _treeInfoDrawer;
        protected SerializedProperty _scriptProp;
        protected TaskInfo rootTree;
        public bool drawScript { get; set; } = true;
        private string _matchNodeText;
        private Dictionary<BaseNode, string> _propPaths;
        private bool folderOut;
        private ReorderableList _worldStateList;
        private List<string> _worldStateKeys = new List<string>();

        protected virtual void OnEnable()
        {
            _tree = target as NTree;
            if (!_tree)
                return;

            _scriptProp = serializedObject.FindProperty("m_Script");
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            RebuildView();
            SetupWorldStateList();
        }

        public string GetPropPath(BaseNode node)
        {
            if (node != null && _propPaths != null && _propPaths.TryGetValue(node, out var path))
            {
                return path;
            }
            return null;
        }

        public void RebuildView()
        {
            ReloadNodeList();
            rootTree = _tree.rootTree;
            _treeInfoDrawer = new TaskInfoDrawer(this, null, rootTree);
        }

        protected void ReloadNodeList()
        {
            ReloadNodesByMatchText();
            _nodeList = new ReorderableList(_nodes, typeof(BaseNode), true, true, false, false);
            _nodeList.drawElementCallback = OnDrawNodeElement;
            _nodeList.drawHeaderCallback = OnDrawNodesHeader;
            _nodeList.elementHeightCallback = OnGetNodeElementHeight;
        }

        [MenuItem("CONTEXT/NTree/Set Main Object")]
        static void SetAsMain(MenuCommand command)
        {
            var path = AssetDatabase.GetAssetPath(command.context);
            AssetDatabase.SetMainObject(command.context, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("CONTEXT/NTree/Remove From Asset")]
        static void RemoveFromAsset(MenuCommand command)
        {
            AssetDatabase.RemoveObjectFromAsset(command.context);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            if (EditorApplication.isPlaying)
                return;

            if (_tree)
            {
                Undo.RecordObject(_tree, "tree disabled!");
                EditorUtility.SetDirty(_tree);
            }
        }

        public override void OnInspectorGUI()
        {
            if (rootTree != _tree.rootTree && _tree.rootTree != null && _tree.rootTree.node != null && EditorApplication.isPlaying)
            {
                rootTree = _tree.rootTree;
                _treeInfoDrawer = new TaskInfoDrawer(this, null, rootTree);
            }
            if (drawScript)
            {
                if (_scriptProp == null || _scriptProp.objectReferenceValue == null)
                    return;

                using (var disable = new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(_scriptProp);
                }
            }
            serializedObject.Update();
            DrawNTreeHeader();
            DrawTreeView();
            GUILayout.Space(8);
            if (_worldStateList != null)
            {
                EditorGUI.BeginChangeCheck();
                _worldStateList.DoLayoutList();
                if (EditorGUI.EndChangeCheck())
                {
                    // 同步到nTree.worldState.keys
                    if (_tree != null && _tree.worldState != null)
                    {
                        _tree.worldState.keys = _worldStateKeys;
                        EditorUtility.SetDirty(_tree);
                    }
                }
            }
            _nodeList?.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawNTreeHeader()
        {
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
            centeredStyle.alignment = TextAnchor.MiddleLeft;
            centeredStyle.fontSize = 14; // 设置字体大小
            centeredStyle.richText = true;
            GUIContent content = new GUIContent($"Unity-HTN <size=12><b><color=black>v1.0</color></b></size> <size=12><b>({target.name})</b></size>");

            var lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(lastRect, "");
            var readMeRect = new Rect(lastRect.x + lastRect.width - 60, lastRect.max.y - 20 + EditorGUIUtility.singleLineHeight, 60, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.LinkButton(readMeRect, "README"))
                Application.OpenURL("https://web-alidocs.dingtalk.com/i/nodes/QOG9lyrgJP3BQREofr345nLxVzN67Mw4?doc_type=wiki_doc");

            if (GUILayout.Button(content, centeredStyle))
                NTreeGraphWindow.OpenWindow(_tree);

            GUILayout.Box("", GUILayout.Height(3), GUILayout.ExpandWidth(true));
        }

        private void DrawTreeView()
        {
            if (_treeInfoDrawer == null)
                return;
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                var rect = GUILayoutUtility.GetRect(0, _treeInfoDrawer.GetHeight());
                _treeInfoDrawer?.OnInspectorGUI(rect, "");
                if (changeCheck.changed && _treeInfoDrawer.changeTrigger)
                {
                    OnTreeInfoChanged();
                }
            }
        }

        protected virtual void OnTreeInfoChanged()
        {
            ReloadNodesByMatchText();
            EditorUtility.SetDirty(_tree);
        }

        private static List<Type> FindAllNodeTypes(Type baseType)
        {
            var derivedTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                               from type in assembly.GetTypes()
                               where baseType.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass
                               select type;
            return new List<Type>(derivedTypes);
        }

        private static string GetTypeMenuName(Type type)
        {
            var menuName = type.FullName;
            string address = "";
            if (menuName.Contains('.'))
            {
                address = menuName.Substring(0, menuName.IndexOf('.') + 1);
                menuName = menuName.Substring(menuName.IndexOf('.') + 1);
            }
            menuName = address + menuName.Replace('.', '/');
            if (type.GetCustomAttributes(false).Any(x => x is NodePathAttribute))
            {
                var attribute = type.GetCustomAttributes(false).First(x => x is NodePathAttribute) as NodePathAttribute;
                menuName = menuName.Substring(0, menuName.Length - type.Name.Length) + attribute.desc;
            }
            return menuName;
        }

        private void ReloadNodesByMatchText()
        {
            _nodes = _nodes ?? new List<BaseNode>();
            _propPaths = new Dictionary<BaseNode, string>();
            _nodes.Clear();
            CollectNodeDeepth(_tree.rootTree, "rootTree", _nodes, _propPaths);
            if (!string.IsNullOrEmpty(_matchNodeText))
                _nodes.RemoveAll(x => !x.name.ToLower().Contains(_matchNodeText.ToLower()) && !x.GetType().Name.ToLower().Contains(_matchNodeText.ToLower()));
            _nodes.Sort((x, y) => string.Compare(x.name, y.name));
        }

        public static void CollectNodeDeepth(TaskInfo info, string path, List<BaseNode> nodes, Dictionary<BaseNode, string> _propPaths)
        {
            if (nodes == null || info == null)
                return;

            if (info.node && !nodes.Contains(info.node))
            {
                nodes.Add(info.node);
                _propPaths[info.node] = path + ".node";
            }
            if (info.condition != null && info.condition.conditions != null)
            {
                int i = 0;
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.node && !nodes.Contains(condition.node))
                    {
                        nodes.Add(condition.node);
                        _propPaths[condition.node] = path + $".condition.conditions.Array.data[{i}].node";
                    }

                    if (condition.subConditions != null)
                    {
                        int j = 0;
                        foreach (var subNode in condition.subConditions)
                        {
                            if (subNode != null && subNode.node && !nodes.Contains(subNode.node))
                            {
                                nodes.Add(subNode.node);
                                _propPaths[subNode.node] = path + $".condition.conditions.Array.data[{i}].subConditions.Array.data[{j}].node";
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            if (info.subTrees != null)
            {
                for (int i = 0; i < info.subTrees.Count; i++)
                {
                    var item = info.subTrees[i];
                    CollectNodeDeepth(item as TaskInfo, path + $".subTrees.Array.data[{i}]", nodes, _propPaths);
                }
            }
        }

        private void OnDrawNodesHeader(Rect rect)
        {
            var expandRect = new Rect(rect.x + 20, rect.y, 60, rect.height);
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                folderOut = EditorGUI.Foldout(expandRect, folderOut, "Nodes");
                if (scope.changed)
                {
                    foreach (var pair in _propPaths)
                    {
                        var path = pair.Value;
                        var drawer = serializedObject.FindProperty(path);
                        if (drawer != null)
                        {
                            drawer.isExpanded = folderOut;
                        }
                    }
                    Repaint();
                }
            }
            var searchRect = new Rect(rect.x + rect.width - 200, rect.y, 180, rect.height);
            using (var chage = new EditorGUI.ChangeCheckScope())
            {
                _matchNodeText = EditorGUI.TextField(searchRect, _matchNodeText);
                if (chage.changed)
                {
                    ReloadNodesByMatchText();
                }
            }
            var refreshRect = new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height);
            if (GUI.Button(refreshRect, EditorGUIUtility.IconContent("d_Refresh"), EditorStyles.iconButton))
            {
                ReloadNodesByMatchText();
            }
        }

        private float OnGetNodeElementHeight(int index)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var node = _nodes[index];
            SerializedProperty drawer = null;
            if (_propPaths.TryGetValue(node, out var path))
                drawer = serializedObject.FindProperty(path);

            if (drawer != null && drawer.isExpanded)
            {
                height += EditorGUI.GetPropertyHeight(drawer, true) - EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        private void OnDrawNodeElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_nodes == null || _nodes.Count <= index)
                return;

            var indexRect = new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(indexRect, index.ToString("00"));
            var nameRect = new Rect(rect.x + 20, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
            var objRect = new Rect(rect.x + 20 + rect.width * 0.32f, rect.y, rect.width * 0.68f - 40, EditorGUIUtility.singleLineHeight);
            var node = _nodes[index];
            node.name = EditorGUI.TextField(nameRect, node.name);
            using (var disable = new EditorGUI.DisabledGroupScope(true))
            {
                var nodeType = node.GetType();
                var nodeBaseType = node.GetType().BaseType;
                using (var color = new ColorScope(TaskCopyPasteUtil.copyNode == node, Color.yellow))
                {
                    EditorGUI.LabelField(objRect, $"{nodeType.Name} ({nodeBaseType.Name})", EditorStyles.selectionRect);
                }
            }
            SerializedProperty drawer = null;
            if (_propPaths.TryGetValue(node, out var path))
                drawer = serializedObject.FindProperty(path);
            if (drawer != null)
            {
                if (!drawer.isExpanded && TaskCopyPasteUtil.copyNode == node)
                    drawer.isExpanded = true;

                if (drawer.isExpanded)
                {
                    var height = EditorGUI.GetPropertyHeight(drawer, true);
                    var detailRect = new Rect(rect.x + 20, rect.y, rect.width - 20, height);
                    EditorGUI.indentLevel--;
                    EditorGUI.PropertyField(detailRect, drawer, GUIContent.none, true);
                    EditorGUI.indentLevel++;
                }
                var expandRect = new Rect(rect.x - 10, rect.y, 30, EditorGUIUtility.singleLineHeight);
                drawer.isExpanded = EditorGUI.Toggle(expandRect, drawer.isExpanded, EditorStyles.foldout);
            }
            var iconRect = new Rect(rect.x + rect.width - 15, rect.y, 20, EditorGUIUtility.singleLineHeight);
            IconCacheUtil.DrawIcon(iconRect, node);
            var copyRect = new Rect(rect.x - 100, rect.y, 100, EditorGUIUtility.singleLineHeight);
            if (node && copyRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy"), false, (x) =>
                {
                    TaskCopyPasteUtil.copyNode = node;
                }, 0);
                menu.ShowAsContext();
            }
        }

        private void OnUndoRedoPerformed()
        {
            RebuildView();
        }

        internal void RecordUndo(string flag)
        {
            if (EditorApplication.isPlaying)
                return;

            if (_tree)
            {
                Undo.RecordObject(_tree, flag);
                EditorUtility.SetDirty(_tree);
                EditorApplication.delayCall += RebuildView;
            }
        }

        private void SetupWorldStateList()
        {
            _worldStateKeys.Clear();
            if (_tree != null && _tree.worldState != null && _tree.worldState.keys != null)
                _worldStateKeys.AddRange(_tree.worldState.keys);
            _worldStateList = new ReorderableList(_worldStateKeys, typeof(string), true, true, true, true);
            _worldStateList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "世界状态Key列表 (WorldState Keys)", EditorStyles.boldLabel);
            _worldStateList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index >= _worldStateKeys.Count) return;
                var keyRect = new Rect(rect.x, rect.y, rect.width, rect.height);
                _worldStateKeys[index] = EditorGUI.TextField(keyRect, _worldStateKeys[index]);
            };
            _worldStateList.onAddCallback = list =>
            {
                _worldStateKeys.Add("");
            };
            _worldStateList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < _worldStateKeys.Count)
                    _worldStateKeys.RemoveAt(list.index);
            };
        }
    }

    [CustomPropertyDrawer(typeof(NTree))]
    public class NTreePropDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.width -= 20;
            EditorGUI.ObjectField(position, property, label);
            var btRect = new Rect(position.x + position.width, position.y, 20, position.height);
            //绘制创建按钮
            if (GUI.Button(btRect, EditorGUIUtility.IconContent("d_Toolbar Plus"), EditorStyles.label))
            {
                var tree = ScriptableObject.CreateInstance<NTree>();
                ProjectWindowUtil.CreateAsset(tree, "NewTree.asset");
                property.objectReferenceValue = Selection.activeObject;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
