using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using UFrame.BehaviourTree;

namespace UFrame.HTN
{
    public class EffectDrawer
    {
        private ReorderableList _effectList;
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
        public EffectDrawer(NTree bTree, TaskInfo info, NTreeEditor treeDrawer)
        {
            _tree = bTree;
            _treeInfo = info;
            _treeDrawer = treeDrawer;
            if (_treeInfo.effects == null)
                _treeInfo.effects = new List<EffectInfo>();
            _effectList = new ReorderableList(_treeInfo.effects, typeof(EffectInfo), true, true, true, true);
            _effectList.drawHeaderCallback = OnDrawHead;
            _effectList.onAddCallback = OnAdd;
            _effectList.onRemoveCallback = OnDelete;
            _effectList.elementHeightCallback = OnDrawHight;
            _effectList.drawElementCallback = OnDrawElement;
        }

        public float GetHeight()
        {
            return _effectList.GetHeight();
        }

        public void OnGUI(Rect rect)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                _effectList.DoList(rect);
                if (check.changed)
                {
                    _changed = true;
                }
            }
        }

        protected TaskInfo TaskInfoInBase(TaskInfo info)
        {
            return null;
        }

        private void OnDrawHead(Rect rect)
        {
            var color = Color.yellow;
            if (_treeInfo.node && _treeInfo.status == Status.Success)
                color = Color.green;

            using (var c = new ColorScope(true, Color.yellow))
                EditorGUI.LabelField(rect, "Effects");
        }

        private float OnDrawHight(int index)
        {
            var height = EditorGUIUtility.singleLineHeight;
            return height;
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_treeInfo.effects == null || index >= _treeInfo.effects.Count)
                return;
            var effect = _treeInfo.effects[index];
            if (effect == null) return;

            // 右键菜单：切换EffectInfo类型
            var mouthRect = new Rect(rect.x - 20, rect.y, 20, EditorGUIUtility.singleLineHeight);
            if (mouthRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick)
            {
                var effectTypes = GetAllEffectTypes();
                var menu = new GenericMenu();
                foreach (var type in effectTypes)
                {
                    bool isCurrent = effect.GetType() == type;
                    menu.AddItem(new GUIContent(type.Name), isCurrent, () =>
                    {
                        if (!isCurrent)
                        {
                            var newEffect = (EffectInfo)System.Activator.CreateInstance(type);
                            // 保留key、apply等通用字段
                            newEffect.key = effect.key;
                            newEffect.apply = effect.apply;
                            newEffect.effectType = effect.effectType;
                            _treeInfo.effects[index] = newEffect;
                            _changed = true;
                        }
                    });
                }
                menu.ShowAsContext();
                Event.current.Use();
            }

            float padding = 4f;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float y = rect.y + (rect.height - lineHeight) / 2f;
            float x = rect.x + padding;
            float width = rect.width - 2 * padding;

            float applyWidth = 50f;
            float arrowWidth = 20f;
            // 动态调整valueWidth
            var valueProp = effect.GetType().GetField("value");
            float valueWidth = 40f;
            if (valueProp != null && valueProp.FieldType == typeof(bool))
            {
                valueWidth = 20f;
            }
            float nameWidth = width - applyWidth - valueWidth - arrowWidth - 3 * padding;
            if (nameWidth < 0) nameWidth = 0;

            // name (key)
            var keyRect = new Rect(x, y, nameWidth, lineHeight);
            effect.key = EditorGUI.TextField(keyRect, effect.key);
            x += nameWidth + padding;

            // 箭头区域Popup，允许切换effectType
            var arrowRect = new Rect(x, y, arrowWidth, lineHeight);
            var supportTypes = effect.supportTypes;
            var effectTypeOptions = new string[supportTypes.Length];
            var effectTypeValues = new EffectType[supportTypes.Length];
            int selected = 0;

            for (int i = 0; i < supportTypes.Length; i++)
            {
                switch (supportTypes[i])
                {
                    case EffectType.Set:
                        effectTypeOptions[i] = "->";
                        break;
                    case EffectType.Add:
                        effectTypeOptions[i] = "+=";
                        break;
                    case EffectType.Mult:
                        effectTypeOptions[i] = "*=";
                        break;
                }
                effectTypeValues[i] = supportTypes[i];
                if (effect.effectType == supportTypes[i])
                {
                    selected = i;
                }
            }

            int newSelected = EditorGUI.Popup(arrowRect, selected, effectTypeOptions, EditorStyles.boldLabel);
            if (newSelected != selected)
            {
                effect.effectType = effectTypeValues[newSelected];
                _changed = true;
            }
            x += arrowWidth + padding;

            // value 通过反射查找属性
            var valueRect = new Rect(x, y, valueWidth, lineHeight);
            if (valueProp != null)
            {
                object value = valueProp.GetValue(effect);
                System.Type valueType = valueProp.FieldType;
                if (valueType == typeof(int))
                {
                    int newValue = EditorGUI.IntField(valueRect, value != null ? (int)value : 0);
                    valueProp.SetValue(effect, newValue);
                }
                else if (valueType == typeof(float))
                {
                    float newValue = EditorGUI.FloatField(valueRect, value != null ? (float)value : 0f);
                    valueProp.SetValue(effect, newValue);
                }
                else if (valueType == typeof(bool))
                {
                    bool newValue = EditorGUI.Toggle(valueRect, value != null ? (bool)value : false);
                    valueProp.SetValue(effect, newValue);
                }
                else if (valueType == typeof(string))
                {
                    string newValue = EditorGUI.TextField(valueRect, value != null ? (string)value : "");
                    valueProp.SetValue(effect, newValue);
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    UnityEngine.Object newValue = EditorGUI.ObjectField(valueRect, value as UnityEngine.Object, valueType, true);
                    valueProp.SetValue(effect, newValue);
                }
                else
                {
                    EditorGUI.LabelField(valueRect, value != null ? value.ToString() : "null");
                }
            }
            else
            {
                EditorGUI.LabelField(valueRect, "无value属性");
            }
            x += valueWidth + padding;

            // apply
            var applyRect = new Rect(rect.x + rect.width - applyWidth - padding, y, applyWidth, lineHeight);
            effect.apply = (EffectApply)EditorGUI.EnumPopup(applyRect, effect.apply);
        }

        private void OnDelete(ReorderableList list)
        {
            RecordUndo("remove check element");
            _treeInfo.effects.RemoveAt(list.index);
        }

        private void OnAdd(ReorderableList list)
        {
            var effectTypes = GetAllEffectTypes();
            var menu = new GenericMenu();
            foreach (var type in effectTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    var effect = (EffectInfo)System.Activator.CreateInstance(type);
                    _treeInfo.effects.Add(effect);
                    _changed = true;
                });
            }
            menu.ShowAsContext();
        }

        private void RecordUndo(string info)
        {
            _changed = true;
            Undo.RecordObject(_tree, info);
        }

        // 提取公共方法：获取所有可实例化EffectInfo子类
        private static List<System.Type> GetAllEffectTypes()
        {
            var effectBaseType = typeof(EffectInfo);
            var effectTypes = new List<System.Type>();
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (effectBaseType.IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(System.Type.EmptyTypes) != null && type != effectBaseType)
                    {
                        effectTypes.Add(type);
                    }
                }
            }
            return effectTypes;
        }
    }
}
