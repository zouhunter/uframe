using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public class CheckDrawer
    {
        private ReorderableList _checkList;
        private TaskInfo _treeInfo;
        private NTree _tree;
        private NTreeEditor _treeDrawer;
        private bool _changed;
        public bool changed
        {
            get
            {
                if (_changed)
                {
                    _changed = false;
                    return true;
                }
                return false;
            }
        }
        public CheckDrawer(NTree bTree, TaskInfo info, NTreeEditor treeDrawer)
        {
            _tree = bTree;
            _treeInfo = info;
            _treeDrawer = treeDrawer;
            if (_treeInfo.checks == null)
                _treeInfo.checks = new List<CheckInfo>();
            _checkList = new ReorderableList(_treeInfo.checks, typeof(CheckInfo), true, true, true, true);
            _checkList.drawHeaderCallback = OnDrawCheckHead;
            _checkList.onAddCallback = OnAdd;
            _checkList.onRemoveCallback = OnDelete;
            _checkList.elementHeightCallback = OnDrawConditonHight;
            _checkList.drawElementCallback = OnDrawCheck;
        }

        public float GetHeight()
        {
            return _checkList.GetHeight();
        }

        public void OnGUI(Rect rect)
        {
            _checkList.DoList(rect);
        }

        private void OnDrawCheckHead(Rect rect)
        {
            var color = Color.yellow;
            if (_treeInfo.node && _treeInfo.status == Status.Success)
                color = Color.green;

            using (var c = new ColorScope(true, Color.yellow))
                EditorGUI.LabelField(rect, "Checks");
        }

        private float OnDrawConditonHight(int index)
        {
            var height = EditorGUIUtility.singleLineHeight;
            return height;
        }

        private void OnDrawCheck(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_treeInfo.checks == null || index >= _treeInfo.checks.Count)
                return;
            var check = _treeInfo.checks[index];
            // 右键菜单：切换Check类型
            var mouthRect = new Rect(rect.x - 20, rect.y, 20, EditorGUIUtility.singleLineHeight);
            if (mouthRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick)
            {
                var checkTypes = GetAllCheckTypes();
                var menu = new GenericMenu();
                foreach (var type in checkTypes)
                {
                    bool isCurrent = check != null && check.GetType() == type;
                    menu.AddItem(new GUIContent(type.Name), isCurrent, (GenericMenu.MenuFunction)(() =>
                    {
                        if (!isCurrent)
                        {
                            var newCheck = (CheckInfo)System.Activator.CreateInstance(type);
                            _treeInfo.checks[index] = newCheck;
                            _changed = true;
                        }
                    }));
                }
                menu.ShowAsContext();
                Event.current.Use();
            }

            float padding = 4f;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float y = rect.y + (rect.height - lineHeight) / 2f;
            float x = rect.x + padding;
            float width = rect.width - 2 * padding;

            // state（图标按钮切换，参考ConditionDrawer）
            float stateWidth = 20f;
            var normaIcon = EditorGUIUtility.IconContent("d_toggle_on_focus").image;
            var inverseIcon = EditorGUIUtility.IconContent("d_console.warnicon").image;
            var inValidIcon = EditorGUIUtility.IconContent("d_console.erroricon").image;
            var stateIcon = (check.state == 0) ? normaIcon : (check.state == 1) ? inverseIcon : inValidIcon;
            if (GUI.Button(new Rect(x, y, stateWidth, lineHeight), stateIcon))
                check.state = (++check.state) % 3;
            x += stateWidth + padding;
            using (var disableScope = new EditorGUI.DisabledScope(check.state > 1))
            {
                // check.Key (name)
                float compireWidth = 15f;
                float valueWidth = 60f;
                float remainWidth = width - stateWidth - compireWidth - valueWidth - 3 * padding;
                float keyWidth = remainWidth > 0 ? remainWidth : 0;
                var keyRect = new Rect(x, y, keyWidth, lineHeight);
                if (check != null)
                {
                    check.Key = EditorGUI.TextField(keyRect, check.Key);
                }
                x += keyWidth + padding;

                // checkCompire
                var compireRect = new Rect(x + 5, y, compireWidth, lineHeight);
                if (check != null)
                {
                    // 动态生成符号和选项
                    var supportTypes = check.supportTypes;
                    string[] compireSymbols = new string[supportTypes.Length];
                    for (int i = 0; i < supportTypes.Length; i++)
                    {
                        switch (supportTypes[i])
                        {
                            case CheckCompire.Equal: compireSymbols[i] = "="; break;
                            case CheckCompire.Bigger: compireSymbols[i] = ">"; break;
                            case CheckCompire.Lower: compireSymbols[i] = "<"; break;
                            default: compireSymbols[i] = supportTypes[i].ToString(); break;
                        }
                    }
                    // 当前checkCompire在supportTypes中的索引
                    int compireIndex = System.Array.IndexOf(supportTypes, check.checkCompire);
                    if (compireIndex < 0) compireIndex = 0;
                    int newIndex = EditorGUI.Popup(compireRect, compireIndex, compireSymbols, EditorStyles.iconButton);
                    check.checkCompire = supportTypes[newIndex];
                }
                x += compireWidth + padding;

                // Value 通过反射
                var valueRect = new Rect(x, y, valueWidth, lineHeight);
                if (check != null)
                {
                    var valueProp = check.GetType().GetField("Value");
                    if (valueProp != null)
                    {
                        object value = valueProp.GetValue(check);
                        System.Type valueType = valueProp.FieldType;
                        if (valueType == typeof(int))
                        {
                            int newValue = EditorGUI.IntField(valueRect, value != null ? (int)value : 0);
                            valueProp.SetValue(check, newValue);
                        }
                        else if (valueType == typeof(float))
                        {
                            float newValue = EditorGUI.FloatField(valueRect, value != null ? (float)value : 0f);
                            valueProp.SetValue(check, newValue);
                        }
                        else if (valueType == typeof(bool))
                        {
                            bool newValue = EditorGUI.Toggle(valueRect, value != null ? (bool)value : false);
                            valueProp.SetValue(check, newValue);
                        }
                        else if (valueType == typeof(string))
                        {
                            string newValue = EditorGUI.TextField(valueRect, value != null ? (string)value : "");
                            valueProp.SetValue(check, newValue);
                        }
                        else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                        {
                            UnityEngine.Object newValue = EditorGUI.ObjectField(valueRect, value as UnityEngine.Object, valueType, true);
                            valueProp.SetValue(check, newValue);
                        }
                        else
                        {
                            EditorGUI.LabelField(valueRect, value != null ? value.ToString() : "null");
                        }
                    }
                    else
                    {
                        EditorGUI.LabelField(valueRect, "无Value属性");
                    }
                }
                else
                {
                    EditorGUI.LabelField(valueRect, "无Check对象");
                }
            }
        }

        private void OnDelete(ReorderableList list)
        {
            RecordUndo("remove check element");
            _treeInfo.checks.RemoveAt(list.index);
        }

        private void OnAdd(ReorderableList list)
        {
            var checkTypes = GetAllCheckTypes();
            var menu = new GenericMenu();
            foreach (var type in checkTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, (GenericMenu.MenuFunction)(() =>
                {
                    var check = (CheckInfo)System.Activator.CreateInstance(type);
                    _treeInfo.checks.Add(check);
                    _changed = true;
                }));
            }
            menu.ShowAsContext();
        }

        private void RecordUndo(string info)
        {
            _changed = true;
            Undo.RecordObject(_tree, info);
        }

        // 提取公共方法：获取所有可实例化Check子类
        private static List<System.Type> GetAllCheckTypes()
        {
            var checkBaseType = typeof(CheckInfo);
            var checkTypes = new List<System.Type>();
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (checkBaseType.IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(System.Type.EmptyTypes) != null && type != checkBaseType)
                    {
                        checkTypes.Add(type);
                    }
                }
            }
            return checkTypes;
        }
    }
}
