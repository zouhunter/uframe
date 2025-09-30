using System.Collections;
//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-06-04
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace UFrame.Decision
{
    public class DecisionTreeNodeDrawer
    {
        private DecisionTreeNodeDrawer _parentTree;
        private DecisionTreeNode _treeInfo;
        private ReorderableList _subTreeList;
        private bool _changed;
        private string _deepth;
        private DecisionTree _tree;
        private DecisionConditionDrawer _conditionDrawer = new DecisionConditionDrawer();
        private Dictionary<DecisionTreeNode, DecisionTreeNodeDrawer> _subDrawers;
        public bool changeTrigger
        {
            get
            {
                if (_changed)
                {
                    _changed = false;
                    return true;
                }
                if (_conditionDrawer != null)
                {
                    if (_conditionDrawer.changed)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public Action<DecisionTreeNode> onChange;
        private DecisionResultDrawer _resultDrawer;
        private static long _lastSelectScriptTime;

        public DecisionTreeNodeDrawer(DecisionTree tree, DecisionTreeNodeDrawer parent, DecisionTreeNode node, Action<DecisionTreeNode> onChange)
        {
            _tree = tree;
            _treeInfo = node;
            _parentTree = parent;
            RebuildSubTreeList();
            InitResultDrawer();
            this.onChange = onChange;
        }

        public void OnChangeNode(DecisionTreeNode node)
        {
            node.condition = _treeInfo.condition;
            _treeInfo = node;
            RebuildSubTreeList();
            _changed = true;
            InitResultDrawer();
            onChange?.Invoke(node);
        }

        private void InitResultDrawer()
        {
            _resultDrawer = null;

            if (_treeInfo is DecisionLeafNode)
            {
                _resultDrawer = new DecisionResultDrawer(_treeInfo as DecisionLeafNode);
            }
        }

        public float GetHeight()
        {
            float height = EditorGUIUtility.singleLineHeight + 10;
            if (_treeInfo != null)
            {
                if (_resultDrawer != null)
                {
                    height += _resultDrawer.GetHeight();
                }
                if (_treeInfo.Children != null)
                {
                    height += _subTreeList.GetHeight();
                }
            }
            return height;
        }

        public void OnInspectorGUI(Rect position, string deepth)
        {
            var fullRect = position;
            position = new Rect(position.x, position.y + 5, position.width, position.height - 10);

            this._deepth = deepth;

            var labelRect = new Rect(position.x + 20, position.y, 50, EditorGUIUtility.singleLineHeight);
            if (string.IsNullOrEmpty(_deepth))
            {
                EditorGUI.LabelField(labelRect, "Nodes");
            }
            else
            {
                EditorGUI.LabelField(labelRect, _deepth);
                labelRect.width = _deepth.Length * 8;
            }
            var nodeRect = new Rect(position.x + labelRect.width + 25, position.y, position.width - labelRect.width - 32, EditorGUIUtility.singleLineHeight);
            var conditionRect = DrawCreateNodeContent(nodeRect, _treeInfo, OnChangeNode);
            var yOffset = position.y + nodeRect.height;
            position.x += 30;
            position.width -= 30;
            if (_treeInfo is DecisionRootNode rootNode)
            {
                var nameRect = new Rect(conditionRect.x, conditionRect.y, conditionRect.width, EditorGUIUtility.singleLineHeight);
                rootNode.name = EditorGUI.TextField(nameRect, rootNode.name);
            }
            else
            {
                _conditionDrawer.OnGUI(conditionRect, _treeInfo);
            }
            yOffset += 5;
            if (_resultDrawer != null)
            {
                var resultRect = conditionRect;
                resultRect.y = yOffset;
                var resultHeight = _resultDrawer.GetHeight();
                _resultDrawer.OnGUI(resultRect);
                yOffset += resultHeight;
            }
            if (_treeInfo.Children != null)
            {
                DrawLineLink(_treeInfo, fullRect, yOffset);
                var subTreeRect = new Rect(position.x, yOffset, position.width, _subTreeList.GetHeight());
                _subTreeList.DoList(subTreeRect);
            }
            DrawWireBox(_treeInfo, new Rect(fullRect.x, fullRect.y, fullRect.width, fullRect.height));
            var menuRect = new Rect(position.x - 80, position.y, 60, EditorGUIUtility.singleLineHeight);
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Double (%d)"), false, Doublicat, 0);
                menu.AddItem(new GUIContent("Insert Before"), false, InsertBefore, 0);
                menu.AddItem(new GUIContent("Insert Next"), false, InsertNext, 0);
                menu.AddItem(new GUIContent("Insert Parent"), false, InsertParent, 0);
                menu.AddItem(new GUIContent("Delete Self (del)"), false, DeleteSelf, 0);
                menu.AddItem(new GUIContent("Delete All"), false, DeleteAll, 0);
                menu.AddItem(new GUIContent("Hierarchy/Copy (%c)"), false, CopyNode, 0);
                menu.AddItem(new GUIContent("Hierarchy/Cut (%x)"), false, CopyNode, 1);
                if (CopyPasteUtil.copyNode != _treeInfo && _treeInfo != null)
                    menu.AddItem(new GUIContent("Hierarchy/Paste (%v)"), false, PasteNode, 0);
                menu.ShowAsContext();
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.D)
            {
                Doublicat(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.C)
            {
                CopyNode(0);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.X)
            {
                CopyNode(1);
            }
            if (menuRect.Contains(Event.current.mousePosition) && Event.current.control && Event.current.keyCode == KeyCode.V)
            {
                if (CopyPasteUtil.copyNode != _treeInfo && _treeInfo != null)
                    PasteNode(0);
            }

            if (labelRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy Node"), false, (GenericMenu.MenuFunction2)((x) =>
                {
                    CopyPasteUtil.copyNode = _treeInfo;
                }), 0);
                if (CopyPasteUtil.copyNode != null)
                {
                    menu.AddItem(new GUIContent("Paste Node"), false, (x) =>
                    {
                        RecordUndo("paste node");
                        CopyPasteUtil.copyNode.CopyTo(_treeInfo);
                        _changed = true;
                    }, 0);
                }

                menu.ShowAsContext();
            }
        }

        private void RebuildSubTreeList()
        {
            if (_treeInfo == null || _treeInfo.Children == null)
                return;

            _subDrawers = new Dictionary<DecisionTreeNode, DecisionTreeNodeDrawer>();
            for (int i = 0; i < _treeInfo.Children.Count; i++)
            {
                var child = _treeInfo.Children[i];
                if (child == null)
                    continue;
                _subDrawers[child] = new DecisionTreeNodeDrawer(_tree, this, child, (child2) =>
                {
                    _treeInfo.Children[i] = child2;
                });
            }
            bool canAddOrDelete = true;
            _subTreeList = new ReorderableList(_treeInfo.Children, typeof(DecisionTreeNode), true, true, canAddOrDelete, canAddOrDelete);
            _subTreeList.headerHeight = EditorGUIUtility.singleLineHeight;
            _subTreeList.drawHeaderCallback = (rect) =>
            {
                float iconWidth = 18f;
                var iconRect = new Rect(rect.x, rect.y, iconWidth, rect.height);
                var labelRect = new Rect(rect.x + iconWidth, rect.y, 60, rect.height);
                var fieldRect = new Rect(rect.x + iconWidth + 60, rect.y, rect.width - iconWidth - 60, rect.height);
                GUIContent iconContent = EditorGUIUtility.IconContent("console.warnicon"); // 可换为其他内置图标
                GUI.Label(iconRect, iconContent);
                EditorGUI.LabelField(labelRect, "Question:");
                _treeInfo.Question = EditorGUI.TextField(fieldRect, _treeInfo.Question);
            };
            _subTreeList.elementHeightCallback = OnSubTreeElementHeight;
            _subTreeList.drawElementCallback = OnDrawSubTreeElement;
            _subTreeList.onAddCallback = OnAddSubTreeElement;
            _subTreeList.onRemoveCallback = OnRemoveSubTreeElement;
        }

        public static Rect DrawCreateNodeContent(Rect rect, DecisionTreeNode node, Action<DecisionTreeNode> onCreate)
        {
            var nameRect = new Rect(rect.x, rect.y, rect.width - 60, rect.height);
            bool createTouched = false;
            if (node != null)
            {
                var iconX = nameRect.max.x + 15;
                var iconRect = new Rect(iconX, rect.y, 20, EditorGUIUtility.singleLineHeight);
                DrawIcon(iconRect, node);
            }
            else
            {
                if (GUI.Button(nameRect, "Null", EditorStyles.textField))
                {
                    createTouched = true;
                }
            }
            var createRect = new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height);
            if (createTouched || GUI.Button(createRect, "", EditorStyles.popup))
            {
                CreateDecisionTreeNode((node) =>
                {
                    onCreate?.Invoke(node);
                });
            }
            return nameRect;
        }
        /// <summary>
        /// 双击打脚本
        /// </summary>
        /// <param name="iconRect"></param>
        /// <param name="node"></param>
        public static void DrawIcon(Rect iconRect, DecisionTreeNode node)
        {
            Texture2D iconContent = EditorGUIUtility.IconContent("console.infoicon").image as Texture2D; // 可换为其他内置图标
            if (node is DecisionSelectNode)
            {
                iconContent = EditorGUIUtility.IconContent("d_ViewToolOrbit").image as Texture2D;
            }
            else if (node is DecisionLeafNode)
            {
                iconContent = EditorGUIUtility.IconContent("d_PlayButton").image as Texture2D;
            }

            if (iconContent != null)
            {

                if (GUI.Button(iconRect, new GUIContent(iconContent)))
                {
                    OpenEditScript(node.GetType(), _lastSelectScriptTime != System.DateTime.Now.Second);
                    _lastSelectScriptTime = System.DateTime.Now.Second;
                }
            }
        }

        private static void OpenEditScript(Type type, bool locateOnly)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    if (locateOnly)
                    {
                        EditorGUIUtility.PingObject(script);
                    }
                    else
                    {
                        AssetDatabase.OpenAsset(script);
                    }
                }
            }
        }
        private DecisionTreeNodeDrawer GetSubDrawer(DecisionTreeNode info)
        {
            if (info == null)
                return null;
            if (!_subDrawers.TryGetValue(info, out var drawer))
            {
                drawer = new DecisionTreeNodeDrawer(_tree, this, info, (newInfo) =>
                {
                    var index = this._treeInfo.Children.IndexOf(info);
                    _treeInfo.Children[index] = newInfo;
                });
                _subDrawers[info] = drawer;
            }
            return drawer;
        }

        private float OnSubTreeElementHeight(int index)
        {
            var elementHeight = 4;
            if (_treeInfo.Children != null && _treeInfo.Children.Count > index)
            {
                var info = _treeInfo.Children[index];
                var drawer = GetSubDrawer(info);
                return drawer?.GetHeight() ?? 0 + elementHeight;
            }
            return elementHeight;
        }

        private void OnDrawSubTreeElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                var innerRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
                GUI.Box(innerRect, GUIContent.none);
                if (_treeInfo.Children.Count > index && _subDrawers.Count > index)
                {
                    var info = _treeInfo.Children[index];
                    var drawer = GetSubDrawer(info);
                    drawer?.OnInspectorGUI(innerRect, _deepth + (1 + index));
                }
                if (changeCheck.changed)
                {
                    _changed = true;
                }
            }
        }


        private static List<Type> FindAllNodeTypes(Type baseType)
        {
            var derivedTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                               from type in assembly.GetTypes()
                               where baseType.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass
                               select type;
            return new List<Type>(derivedTypes);
        }

        private void OnAddSubTreeElement(ReorderableList list)
        {
            RecordUndo("add sub tree element");
            CreateDecisionTreeNode((treeInfo) =>
            {
                _treeInfo.Children.Add(treeInfo);
                _subDrawers[treeInfo] = new DecisionTreeNodeDrawer(_tree, this, treeInfo, (newInfo) =>
                {
                    var index = _treeInfo.Children.IndexOf(treeInfo);
                    _treeInfo.Children[index] = newInfo;
                });
                _changed = true;
            });
        }

        public static void CreateDecisionTreeNode(Action<DecisionTreeNode> onCreate)
        {
            var types = FindAllNodeTypes(typeof(DecisionTreeNode));
            var menu = new GenericMenu();

            foreach (var type in types)
            {
                if (type == typeof(DecisionRootNode))
                    continue;
                menu.AddItem(new GUIContent(type.Name), false, (object typeObj) =>
                {
                    var selectedType = typeObj as Type;
                    var treeInfo = Activator.CreateInstance(selectedType) as DecisionTreeNode;
                    onCreate?.Invoke(treeInfo);
                }, type);
            }
            menu.ShowAsContext();
        }

        private void OnRemoveSubTreeElement(ReorderableList list)
        {
            RecordUndo("remove sub tree element");
            _treeInfo.Children.RemoveAt(list.index);
            _changed = true;
        }

        private void DrawWireBox(DecisionTreeNode node, Rect rect)
        {
            rect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);
            var leftRect = new Rect(rect.x, rect.y, 2, rect.height);
            var rightRect = new Rect(rect.x + rect.width - 2, rect.y, 2, rect.height);
            var topRect = new Rect(rect.x, rect.y, rect.width, 2);
            var bottomRect = new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2);
            var color = Color.gray;
            if (node.status == 1)
                color = Color.green;
            else if (node.status == 2)
                color = Color.red;
            color.a = 0.5f;
            using (var colorScope = new ColorGUIScope(true, color))
            {
                GUI.DrawTexture(leftRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(rightRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(topRect, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(bottomRect, EditorGUIUtility.whiteTexture);
            }
        }

        private void DrawLineLink(DecisionTreeNode node, Rect rect, float yOffset)
        {
            yOffset = yOffset - rect.y;
            float height = (rect.height - yOffset);
            var percent = (height * 0.5f + yOffset - 1.5f * EditorGUIUtility.singleLineHeight) / rect.height;
            var verticllLine = new Rect(rect.x + 5, rect.y + 15, 3, rect.height * percent);
            var horizontallLine = new Rect(verticllLine.x, verticllLine.yMax, 25, 3);
            var color = Color.gray;
            if (node.status == 1)
                color = Color.green;
            else if (node.status == 2)
                color = Color.red;
            color.a = 0.5f;
            using (var colorScope = new ColorGUIScope(true, color))
            {
                GUI.DrawTexture(verticllLine, EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(horizontallLine, EditorGUIUtility.whiteTexture);
            }
        }

        private void CopyNode(object userData)
        {
            CopyPasteUtil.cut = (userData is int u) && u == 1;
            CopyPasteUtil.copyNode = _treeInfo;
            CopyPasteUtil.copyedTreeInfoDrawer = this;
        }

        public void CopyNode(DecisionTreeNode source, DecisionTreeNode target)
        {
            RecordUndo("copy node");
            source.CopyTo(target);
            CopyPasteUtil.copyNode = null;
            RebuildSubTreeList();
            _changed = true;
        }

        private void PasteNode(object userData)
        {
            if (CopyPasteUtil.copyNode != _treeInfo)
            {
                RecordUndo("paste node");
                CopyNode(CopyPasteUtil.copyNode, _treeInfo);
                if (CopyPasteUtil.copyedTreeInfoDrawer != null && CopyPasteUtil.cut)
                {
                    CopyPasteUtil.copyedTreeInfoDrawer.DeleteAll(null);
                }
            }
        }

        private void DeleteAll(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("remove all element");
                _parentTree._treeInfo.Children.Remove(_treeInfo);
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }

        private void InsertParent(object a)
        {
            RecordUndo("insert parent element");
            // 创建新的父节点
            CreateDecisionTreeNode((newParent) =>
            {
                // 获取当前节点在父节点子树中的索引
                var index = _parentTree._treeInfo.Children.IndexOf(_treeInfo);

                // 将当前节点移动到新父节点下
                _parentTree._treeInfo.Children.RemoveAt(index);
                newParent.Children.Add(_treeInfo);

                // 将新父节点插入到原位置
                _parentTree._treeInfo.Children.Insert(index, newParent);

                RebuildSubTreeList();
                _changed = true;
            });

        }
        private void Doublicat(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("duplicate node");
                CreateDecisionTreeNode((newInfo) =>
                {
                    // 创建新的TreeInfo
                    _treeInfo.CopyTo(newInfo);
                    // 复制当前节点的所有信息
                    // 获取当前节点在父节点子树中的索引
                    var index = _parentTree._treeInfo.Children.IndexOf(_treeInfo);

                    // 在当前节点后面插入新节点
                    _parentTree._treeInfo.Children.Insert(index + 1, newInfo);

                    // 重建父节点的子树列表
                    _parentTree.RebuildSubTreeList();
                    _changed = true;
                });
            }
        }

        private void InsertBefore(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("insert before node");
                // 创建新的DecisionTreeNode
                var newInfo = Activator.CreateInstance(_treeInfo.GetType()) as DecisionTreeNode;
                // 获取当前节点在父节点子树中的索引
                var index = _parentTree._treeInfo.Children.IndexOf(_treeInfo);
                // 在当前节点前面插入新节点
                if (index >= 0)
                    _parentTree._treeInfo.Children.Insert(index, newInfo);
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }

        private void InsertNext(object userData)
        {
            if (_parentTree != null)
            {
                RecordUndo("insert next node");
                var newInfo = Activator.CreateInstance(_treeInfo.GetType()) as DecisionTreeNode;
                var index = _parentTree._treeInfo.Children.IndexOf(_treeInfo);
                if (index >= 0)
                    _parentTree._treeInfo.Children.Insert(index + 1, newInfo);
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
        }
        private void DeleteSelf(object arg)
        {
            RecordUndo("delete self element");
            if (_parentTree != null)
            {
                _parentTree._treeInfo.Children.Remove(_treeInfo);
                // 如果有子节点，全部提升到父节点
                if (_treeInfo.Children != null && _treeInfo.Children.Count > 0)
                {
                    _parentTree._treeInfo.Children.AddRange(_treeInfo.Children);
                }
                _parentTree.RebuildSubTreeList();
                _changed = true;
            }
            else
            {
                if (_treeInfo.Children != null && _treeInfo.Children.Count > 1)
                {
                    EditorUtility.DisplayDialog("InValid", "root node can`t delete self, child count > 1", "ok");
                }
                else if (_treeInfo.Children != null && _treeInfo.Children.Count == 1)
                {
                    // 根节点只有一个子节点时，提升
                    if (_tree != null)
                    {
                        _tree.rootNode = _treeInfo.Children[0];
                        _changed = true;
                    }
                }
            }
        }

        private void RecordUndo(string flag)
        {
            _changed = true;
            Undo.RecordObject(_tree, flag);
        }
    }

    public class CopyPasteUtil
    {
        public static DecisionTreeNode copyNode;
        public static DecisionTreeNodeDrawer copyedTreeInfoDrawer;
        public static bool cut;
    }

    public struct ColorScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.contentColor;

            if (_changeColor)
            {
                GUI.contentColor = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
                GUI.contentColor = _oldColor;
        }
    }
    public struct ColorGUIScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorGUIScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.color;
            if (_changeColor)
            {
                GUI.color = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
            {
                GUI.color = _oldColor;
            }
        }
    }
    public struct ColorBgScope : IDisposable
    {
        private Color _oldColor;
        private bool _changeColor;
        public ColorBgScope(bool active, Color color)
        {
            _changeColor = active;
            _oldColor = GUI.backgroundColor;
            if (_changeColor)
            {
                GUI.backgroundColor = color;
            }
        }

        public void Dispose()
        {
            if (_changeColor)
            {
                GUI.backgroundColor = _oldColor;
            }
        }
    }
}
