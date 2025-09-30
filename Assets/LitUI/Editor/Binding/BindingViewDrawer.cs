using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Text;

namespace UFrame.LitUI
{
    [DisallowMultipleComponent]
    [CustomEditor(typeof(BindingView), true)]
    public class BindingViewDrawer : Editor
    {
        public BindingView view;
        private ViewBinderDrawer binderDrawer;
        private SerializedProperty scriptProp;
        private float _lastSelectScriptTime;
        private bool _inedit;
        private string _exportFolder;
        private UICodeGen _codeGen;
        private static int _activeId;
        private static GUIContent _iconContent;
        private static GUIContent _rootIconContent;
        private static Dictionary<int, UnityEngine.Object> _instanceIds;
        private SerializedProperty _viewModelProp;
        private const string m_autoAddVMKey = "BindingViewDrawer.AutoAddVM";
        private const string m_autoAddBinderKey = "BindingViewDrawer.AutoAddBinder";
        private bool _changed;

        private void OnEnable()
        {
            view = target as BindingView;
            _codeGen = new UICodeGen();
            binderDrawer = new ViewBinderDrawer();
            binderDrawer.OpenWith(view, _codeGen);
            scriptProp = serializedObject.FindProperty("m_Script");
            _viewModelProp = serializedObject.FindProperty("viewModel");
            InitInstanceIds();
            EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierachyItem;
            _iconContent = EditorGUIUtility.IconContent("d_AreaEffector2D Icon", "using");
            _rootIconContent = EditorGUIUtility.IconContent("d_CharacterJoint Icon", "using");
            _exportFolder = Application.dataPath + $"/{UISetting.Instance.codePath}/{view.name}";
            _changed = false;
            CheckAutoAddComponent();
        }

        [MenuItem("GameObject/UI/BindingView")]
        public static void CreateUIBindingView()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<BindingView>();
        }

        private void InitInstanceIds()
        {
            _instanceIds = new Dictionary<int, UnityEngine.Object>();
            foreach (var item in view.binding.refs)
            {
                if (!item.obj)
                    continue;

                if (item.obj is ScriptableObject)
                    continue;

                var go = (GameObject)item.obj.GetType()?.GetProperty("gameObject")?.GetValue(item.obj);
                if (!go)
                {
                    Debug.LogError("empty ref:" + item.name);
                    continue;
                }
                _instanceIds[go.GetInstanceID()] = item.obj;
            }
        }

        private static void OnDrawHierachyItem(int instanceID, Rect selectionRect)
        {
            if (instanceID == _activeId)
            {
                var rect = new Rect(selectionRect.x + selectionRect.width - 10, selectionRect.y + 5, 10, selectionRect.height * 0.5f);
                GUI.DrawTexture(rect, _rootIconContent.image);
            }

            if (_instanceIds == null)
            {
                EditorApplication.hierarchyWindowItemOnGUI -= OnDrawHierachyItem;
                return;
            }


            if (_instanceIds.TryGetValue(instanceID, out var target) && target)
            {
                var rect = new Rect(selectionRect.x + selectionRect.width - 10, selectionRect.y + 5, 10, 10);
                var objectContent = EditorGUIUtility.ObjectContent(target, target.GetType());
                if (objectContent == null || objectContent.image == null)
                    objectContent = _iconContent;
                GUI.DrawTexture(rect, objectContent.image);
            }
        }

        public override void OnInspectorGUI()
        {
            if (!target)
                return;
            serializedObject.Update();
            DrawHead();
            if (_inedit)
            {
                DrawInEdit();
            }
            else
            {
                DrawNormal();
            }
            var changedProps = serializedObject.ApplyModifiedProperties();
            if (changedProps)
            {
                _changed = true;
            }
        }
        void DrawHeadInEdit()
        {
            if (GUILayout.Button("clear", EditorStyles.miniButtonLeft, GUILayout.Width(60)))
            {
                if (!EditorUtility.DisplayDialog("clean", "clear all reference!", "confer"))
                    return;
                binderDrawer.ClearBindings();
            }
            if (GUILayout.Button("cancel", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                _inedit = false;
            }
            if (GUILayout.Button("apply", EditorStyles.miniButtonRight, GUILayout.Width(60)))
            {
                _inedit = false;
                if (_changed || (binderDrawer != null && binderDrawer.changed))
                {
                    ApplyBinderCode();
                    InitInstanceIds();
                    _changed = false;
                }
            }
        }

        void DrawHeadNormal()
        {
            var allRight = true;
            if (view.binding != null)
            {
                for (int i = 0; i < view.binding.refs.Count; i++)
                {
                    var subProp = view.binding.refs[i];
                    var ok = subProp.obj != null;
                    if (!ok)
                        allRight = false;
                }
            }
            else
            {
                allRight = false;
            }

            var color = GUI.color;
            GUI.color = allRight ? color : Color.red;
            EditorGUILayout.LabelField($"(count = {view.binding.refs.Count})");
            GUI.color = color;
        }

        void DrawHead()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                if (_inedit)
                {
                    GUILayout.FlexibleSpace();
                    DrawHeadInEdit();
                }
            }

            if (!_inedit)
            {
                DrawHeadNormal();
            }
        }
        void DrawNormal()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("[V M Binder]", EditorStyles.boldLabel, GUILayout.Width(120));
                var lastRect = GUILayoutUtility.GetLastRect();
                AcceptBinderDrag(lastRect);
                DrawCreateObject(typeof(VMBinder), view.binder, (x) => view.binder = x);
                if (GUILayout.Button(new GUIContent("new", "create"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
                {
                    EditorApplication.delayCall = CreateNewBinder;
                }
                if (view.binder != null && GUILayout.Button("edit", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    _inedit = true;
                    binderDrawer = new ViewBinderDrawer();
                    binderDrawer.OpenWith(view, _codeGen);
                }
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("[View Model]", EditorStyles.boldLabel, GUILayout.Width(120));
                var lastRect = GUILayoutUtility.GetLastRect();
                AcceptVMDrag(lastRect);
                DrawCreateObject(typeof(ViewModel), view.viewModel, (x) => view.viewModel = x);
                if (GUILayout.Button(new GUIContent("new", "create"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
                {
                    EditorApplication.delayCall = CreateNewViewModel;
                }
                if (view.viewModel != null)
                {
                    if (GUILayout.Button(new GUIContent("copy", "copy bindings"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        CopyViewModelFields();
                    }
                }
            }
            if (view.viewModel != null)
            {
                var height = EditorGUIUtility.singleLineHeight;
                if (_viewModelProp.isExpanded)
                    height = EditorGUI.GetPropertyHeight(_viewModelProp, true);
                var rect = GUILayoutUtility.GetRect(0, height);
                rect.y -= (EditorGUIUtility.singleLineHeight + 2);
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;
                EditorGUI.PropertyField(rect, _viewModelProp, GUIContent.none, true);
                EditorGUIUtility.labelWidth = labelWidth;
            }

            DrawRefs();
        }

        void DrawRefs()
        {
            if (binderDrawer.bindings == null)
                return;
            GUILayout.Label("[Bindings]", EditorStyles.boldLabel);
            foreach (var item in binderDrawer.bindings)
            {
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    var rect = hor.rect;
                    AcceptRefDrag(item, rect);

                    EditorGUILayout.LabelField(item.name, GUILayout.Width(120));
                    EditorGUILayout.ObjectField(item.target, item.targetType, true);
                }
            }
        }

        void AcceptRefDrag(ComponentBinding item, Rect rect)
        {
            if (!rect.Contains(Event.current.mousePosition))
                return;

            if (Event.current.type == EventType.DragUpdated)
            {
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                UnityEngine.Object target = null;
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                {
                    target = DragAndDrop.objectReferences[0];
                }

                if (!target)
                    return;
                UnityEngine.Object obj = target;
                if (item.targetType != target.GetType())
                {
                    if (target is GameObject go && typeof(UnityEngine.Component).IsAssignableFrom(item.targetType))
                    {
                        obj = go.GetComponent(item.targetType);
                    }
                }
                if (!obj)
                    return;
                Undo.RecordObject(view, "change ref target!");
                item.target = obj;
                var refItem = view.binding.refs.Find(x => x.name == item.name);
                refItem.obj = obj;
                _changed = true;
                EditorUtility.SetDirty(view);
            }
        }
        void DrawInEdit()
        {
            using (var changedCheck = new EditorGUI.ChangeCheckScope())
            {
                binderDrawer?.OnGUI();

                if (changedCheck.changed)
                    _changed = true;
            }
        }

        private void CreateNewViewModel()
        {
            if (view.viewModel != null)
            {
                var filePath = _codeGen.FindScriptPath(view.viewModel.GetType());
                if (!string.IsNullOrEmpty(filePath))
                    _exportFolder = System.IO.Path.GetDirectoryName(filePath);
            }
            System.IO.Directory.CreateDirectory(_exportFolder);
            var file = EditorUtility.SaveFilePanel("Create ViewModel", _exportFolder, target.name + "Model", "cs");
            if (!string.IsNullOrEmpty(file))
            {
                _exportFolder = System.IO.Path.GetDirectoryName(file);
                var name = System.IO.Path.GetFileNameWithoutExtension(file);
                var parentName = "ViewModel";
                if (view.binder != null && UISetting.Instance.useBinderVM)
                    parentName = $"{view.binder.GetType().Name}.VM";
                var script = _codeGen.CreateViewModelScript(binderDrawer.bindings, UISetting.Instance.defaultNamespace, name, "", parentName);
                EditorPrefs.SetString(m_autoAddVMKey, System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file));
                System.IO.File.WriteAllText(file, script);
                AssetDatabase.Refresh();
            }
            else
            {
                RemoveEmptyFolder();
            }
        }

        private void CheckAutoAddComponent()
        {
            if (!Selection.activeGameObject)
                return;

            if (EditorPrefs.HasKey(m_autoAddVMKey))
            {
                var file = EditorPrefs.GetString(m_autoAddVMKey);
                if (System.IO.File.Exists(file))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(file);
                    if (script)
                    {
                        view.viewModel = System.Activator.CreateInstance(script.GetClass()) as ViewModel;
                        EditorUtility.SetDirty(view);
                    }
                }
                EditorPrefs.DeleteKey(m_autoAddVMKey);
            }
            if (EditorPrefs.HasKey(m_autoAddBinderKey))
            {
                var file = EditorPrefs.GetString(m_autoAddBinderKey);
                if (System.IO.File.Exists(file))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(file);
                    if (script)
                    {
                        view.binder = System.Activator.CreateInstance(script.GetClass()) as VMBinder;
                        EditorUtility.SetDirty(view);
                    }
                }
                EditorPrefs.DeleteKey(m_autoAddBinderKey);
            }
        }
        private void RemoveEmptyFolder()
        {
            if (System.IO.Directory.GetFileSystemEntries(_exportFolder).Length == 0)
            {
                System.IO.Directory.Delete(_exportFolder);
                var metaFile = _exportFolder + ".meta";
                if (System.IO.File.Exists(metaFile))
                {
                    System.IO.File.Delete(metaFile);
                }
            }

        }
        private void CopyViewModelFields()
        {
            var sb = new StringBuilder();
            _codeGen.GetViewModelProps("\t\t", binderDrawer.bindings, sb);
            GUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log("copyed to clipboard:\n" + sb.ToString());
        }

        private void CreateNewBinder()
        {
            if (view.binder != null)
            {
                var filePath = _codeGen.FindScriptPath(view.binder.GetType());
                if (!string.IsNullOrEmpty(filePath))
                {
                    _exportFolder = System.IO.Path.GetDirectoryName(filePath);
                }
            }
            System.IO.Directory.CreateDirectory(_exportFolder);
            var file = EditorUtility.SaveFilePanel("Create VMBinder", _exportFolder, target.name + "Binder", "cs");
            if (!string.IsNullOrEmpty(file))
            {
                _exportFolder = System.IO.Path.GetDirectoryName(file);
                var name = System.IO.Path.GetFileNameWithoutExtension(file);
                var script = _codeGen.GenerateBinderScript(binderDrawer.bindings, UISetting.Instance.defaultNamespace, name, "");
                EditorPrefs.SetString(m_autoAddBinderKey, System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file));
                System.IO.File.WriteAllText(file, script);
                AssetDatabase.Refresh();
            }
            else
            {
                RemoveEmptyFolder();
            }
        }

        private void ApplyBinderCode()
        {
            view.binding.refs.Clear();
            foreach (var item in binderDrawer.bindings)
            {
                var go = item.target;
                if (go)
                    view.binding.refs.Add(new UIRefItem()
                    {
                        name = item.name,
                        obj = item.target
                    });
            }
            EditorUtility.SetDirty(view);
            AssetDatabase.SaveAssets();
            var filePath = _codeGen.FindScriptPath(view.binder.GetType());
            if (!string.IsNullOrEmpty(filePath))
            {
                var oldScript = System.IO.File.ReadAllText(filePath);
                var name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var script = _codeGen.GenerateBinderScript(binderDrawer.bindings, "Weli", name, "");
                if (oldScript != script)
                {
                    System.IO.File.WriteAllText(filePath, script);
                    AssetDatabase.Refresh();
                }
            }
        }

        private List<Type> SelectSubTypes(Type baseType)
        {
            // 筛选出继承自基类且可实例化的类型
            return view.GetType().Assembly.GetTypes()
                 .Where(type => baseType.IsAssignableFrom(type)    // 检查是否继承
                                && !type.IsAbstract                // 非抽象类
                                && type.IsClass                    // 必须是类
                                && type.GetConstructor(Type.EmptyTypes) != null // 必须有无参构造函数
                                && type != baseType)               //不能是基类自己
                 .ToList();
        }

        /// <summary>
        /// 绘制对象创建按钮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseType"></param>
        /// <param name="value"></param>
        /// <param name="onCreate"></param>
        public void DrawCreateObject<T>(Type baseType, T value, Action<T> onCreate)
        {
            bool createTouched = false;
            if (value != null)
            {
                if (GUILayout.Button(value.GetType().Name, EditorStyles.miniTextField, GUILayout.ExpandWidth(true)))
                {
                    OpenEditScript(value.GetType(), _lastSelectScriptTime != System.DateTime.Now.Second);
                    _lastSelectScriptTime = System.DateTime.Now.Second;
                }

                if (GUILayout.Button(new GUIContent("", "select"), EditorStyles.popup, GUILayout.Width(20)))
                {
                    createTouched = true;
                }
            }
            else
            {
                if (GUILayout.Button("Null", EditorStyles.textField))
                {
                    createTouched = true;
                }
            }

            if (createTouched)
            {
                var types = SelectSubTypes(baseType);
                var menu = new GenericMenu();
                for (int i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        onCreate((T)System.Activator.CreateInstance(type));
                        EditorUtility.SetDirty(target);
                    });
                }
                menu.ShowAsContext();
            }
        }

        private static void OpenEditScript(Type type, bool locateOnly)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && (script.GetClass() == type || script.GetClass() == null))
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
        private void AcceptBinderDrag(Rect rect)
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.objectReferences[0] is MonoScript script && typeof(VMBinder).IsAssignableFrom(script.GetClass()))
                {
                    view.binder = System.Activator.CreateInstance(script.GetClass()) as VMBinder;
                    EditorUtility.SetDirty(view);
                }
            }
        }

        private void AcceptVMDrag(Rect rect)
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                if (DragAndDrop.objectReferences[0] is MonoScript script && typeof(ViewModel).IsAssignableFrom(script.GetClass()))
                {
                    view.viewModel = System.Activator.CreateInstance(script.GetClass()) as ViewModel;
                    EditorUtility.SetDirty(view);
                }
            }
        }
    }
}
