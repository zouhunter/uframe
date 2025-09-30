#region statement
/************************************************************************************* 
    * 作    者：       zouhunter
    * 时    间：       2018-02-02 
    * 详    细：       1.支持枚举、模型、结构和继承等类的模板。
                       2.支持快速创建通用型UI界面脚本
                       3.支持自定义模板类
                       4.自动生成作者、创建时间、描述等功能
                       5.工程不同步（PlayerPrefer）
   *************************************************************************************/
#endregion

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.CodeDom;
using System.IO;
using System.Text;
using UnityEditorInternal;
using System.Linq;

namespace UFrame.ScriptGen
{
    #region Window
    /// <summary>
    /// 一个创建脚本模板的窗口
    /// </summary>
    public class ScriptTempWindow : EditorWindow
    {
        [MenuItem("Window/UFrame/Code-Editor/ScriptTempWindow")]
        static void Open()
        {
            var window = GetWindow<ScriptTempWindow>();
            window.wantsMouseMove = true;
        }
      
        [MenuItem("Assets/UFrame/Create C# Scripts #s", false, 80)]
        static void CreateScripts()
        {
            var tempConfig = ScriptTempConfigObject.Instance;
            var genericMenu = new GenericMenu();
            for (int i = 0; i < tempConfig.templates.Count; i++)
            {
                var temp = tempConfig.templates[i];
                genericMenu.AddItem(new GUIContent(temp.name), ScriptTempEditorConfigObject.Instance.IsDefault(temp.name,temp.type),(x)=> {
                    var template = (ScriptTemplate)x;
                    CreateScriptFromTemplate(template);
                }, temp);
            }
            genericMenu.ShowAsContext();
        }

        [MenuItem("Assets/Create/C# Scripts/Enum", false, 80)]
        static void CreateEnum()
        {
            var tempConfig = ScriptTempEditorConfigObject.Instance;
            QuickCreateScript(tempConfig.activeEnumTemplate, TemplateType.Enum);
        }
        [MenuItem("Assets/Create/C# Scripts/Struct", false, 80)]
        static void CreateStruct()
        {
            var tempConfig = ScriptTempEditorConfigObject.Instance;
            QuickCreateScript(tempConfig.activeStructTemplate, TemplateType.Struct);
        }
        [MenuItem("Assets/Create/C# Scripts/Class", false, 80)]
        static void CreateClass()
        {
            var tempConfig = ScriptTempEditorConfigObject.Instance;
            QuickCreateScript(tempConfig.activeClassTemplate, TemplateType.Class);
        }
        [MenuItem("Assets/Create/C# Scripts/Interface", false, 80)]
        static void CreateInterface()
        {
            var tempConfig = ScriptTempEditorConfigObject.Instance;
            QuickCreateScript(tempConfig.activeInterfaceTemplate, TemplateType.Interface);
        }

        public static void QuickCreateScript(string name, TemplateType type)
        {
            var templates = ScriptTempConfigObject.Instance.templates;
            ScriptTemplate template = null;
            foreach (var item in templates)
            {
                if (template == null && (item.type == type || item.name == name))
                {
                    template = item;
                }
                else if (template != null && item.name == name && item.type == type)
                {
                    template = item;
                }
            }
            if (template == null)
            {
                EditorApplication.ExecuteMenuItem("Assets/Create/C# Script");
                Debug.LogError($"failed find template type:{type} name:{name}");
                return;
            }
            else
            {
                CreateScriptFromTemplate(template);
            }
        }

        public static void CreateScriptFromTemplate(ScriptTemplate template)
        {
            ProjectWindowUtil.CreateAssetWithContent($"New{template.name}.cs", "", (Texture2D)EditorGUIUtility.IconContent("d_cs Script Icon").image);
            EditorApplication.update = () =>
            {
                if (Selection.activeObject)
                {
                    var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                    var scriptName = Path.GetFileNameWithoutExtension(path);
                    var dir = Path.GetDirectoryName(path);
                    var drawer = CreateDrawer(template);
                    var scr = drawer.CreateScript(scriptName);
                    drawer.SaveToFile(dir, scriptName, scr.Replace("\\", "/"));
                    EditorApplication.update = null;
                }
            };
        }

        public ScriptTempConfigObject tempConfig => ScriptTempConfigObject.Instance;
        public List<ScriptTemplate> templates => ScriptTempConfigObject.Instance.templates;
        ScriptTemplate currentTemplate
        {
            get { if (templates != null && templates.Count > currentIndex) return templates[currentIndex]; return null; }
        }

        private MonoScript script;
        private int inSubUI;
        private int currentIndex;
        private Vector2 titleScrollPos;
        private Vector2 scrollPos;
        private ScriptTemplateDrawer drawer;
        private string[] templateNames;
        private WaitAddTemplate waitAddTemplate => ScriptTempEditorConfigObject.Instance.waitAddTemplate;
        private ReorderableList importList;

        private void OnEnable()
        {
            if (script == null)
                script = MonoScript.FromScriptableObject(this);
            inSubUI = 0;
            templateNames = templates.Select(x => x.name).ToArray();
            drawer = CreateDrawer(currentTemplate);
            importList = new ReorderableList(waitAddTemplate.imports, typeof(string));
            importList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Import NameSpaces"); };
            importList.drawElementCallback = DrawStringElements;
            if (drawer == null)
                inSubUI = 2;
        }

        private void OnDisable()
        {
            ScriptTempConfigObject.Save();
            ScriptTempEditorConfigObject.Save();
        }

        private void OnGUI()
        {
            DrawHead();
            if (inSubUI == 1)
            {
                DrawSettings();
            }
            else if (inSubUI == 2)
            {
                DrawAddTemplates();
            }
            else
            {
                using (var hor0 = new EditorGUILayout.HorizontalScope())
                {
                    using (var ver0 = new EditorGUILayout.VerticalScope(GUILayout.Width(100)))
                    {
                        using (var scroll = new EditorGUILayout.ScrollViewScope(titleScrollPos))
                        {
                            titleScrollPos = scroll.scrollPosition;
                            using (var changeScope = new EditorGUI.ChangeCheckScope())
                            {
                                for (int i = 0; i < templateNames.Length; i++)
                                {
                                    var active = i == currentIndex;
                                    var actived = GUILayout.Toggle(i == currentIndex, templateNames[i], active ? EditorStyles.toolbarDropDown : EditorStyles.toolbarButton);
                                    if (actived)
                                    {
                                        SelectIndex(i);
                                    }
                                }
                            }
                        }
                    }

                    using (var ver0 = new EditorGUILayout.VerticalScope())
                    {
                        if (currentTemplate == null || drawer == null)
                            return;

                        drawer.target = currentTemplate;


                        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
                        {
                            scrollPos = scroll.scrollPosition;
                            drawer.OnBodyGUI();
                        }
                        using (var changeScope = new EditorGUI.ChangeCheckScope())
                        {
                            drawer.OnFootGUI();

                            if (changeScope.changed)
                            {
                                templateNames = templates.Select(x => x.name).ToArray();
                            }
                        }
                    }
                }
            }
        }

        private void SelectIndex(int index)
        {
            if (templates.Count > index)
            {
                this.currentIndex = index;
                drawer = CreateDrawer(currentTemplate);

                string defualtName = ScriptTempEditorConfigObject.Instance.GetDefault(currentTemplate.type);
                if(string.IsNullOrEmpty(defualtName) || templates.Find(x=>x.name == defualtName) == null)
                {
                    ScriptTempEditorConfigObject.Instance.SetDefault(currentTemplate.name, currentTemplate.type);
                }
            }
        }

        private void DrawAddTemplates()
        {
            using (var ver = new EditorGUILayout.VerticalScope())
            {
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Template Name:", GUILayout.Width(100));
                    waitAddTemplate.name = EditorGUILayout.TextField(waitAddTemplate.name);

                    EditorGUILayout.LabelField("Script Type:", GUILayout.Width(80));
                    waitAddTemplate.type = (TemplateType)EditorGUILayout.EnumPopup(waitAddTemplate.type);
                }

                using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
                {
                    scrollPos = scroll.scrollPosition;
                    importList.DoLayoutList();
                }
                if (waitAddTemplate.type != TemplateType.Enum)
                {
                    using (var hor = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("inherit:", GUILayout.Width(60));
                        waitAddTemplate.parentType = EditorGUILayout.TextField(waitAddTemplate.parentType);
                    }
                }
            }
        }

        private void DrawHead()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var disableScope = new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
                }

                if (inSubUI == 0 && GUILayout.Button("setting", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    inSubUI = 1;
                }
                if (inSubUI == 0 && GUILayout.Button("new template", EditorStyles.miniButtonRight, GUILayout.Width(120)))
                {
                    inSubUI = 2;
                }
                else if (inSubUI == 1 && GUILayout.Button("confer", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    inSubUI = 0;
                }
                else if (inSubUI == 2 && GUILayout.Button("add", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    if (TryAddScriptTemplate())
                        inSubUI = 0;
                }
                else if (inSubUI == 2 && GUILayout.Button("cancel"))
                {
                    inSubUI = 0;
                }
            }
        }

        private bool TryAddScriptTemplate()
        {
            if (string.IsNullOrEmpty(waitAddTemplate.name))
            {
                Debug.LogError("template name is empty!");
                return false;
            }
            if (templates.FindIndex(x => x.name == waitAddTemplate.name) > -1)
            {
                Debug.LogError($"template: {waitAddTemplate.name} allready exists!");
                return false;
            }
            templates.Add(waitAddTemplate.GetScriptNewTempalte());
            templateNames = templates.Select(x => x.name).ToArray();
            SelectIndex(templates.Count - 1);
            return true;
        }

        private void DrawSettings()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Author(Private):", GUILayout.Width(100));
                ScriptTempEditorConfigObject.Instance.author = EditorGUILayout.TextField(ScriptTempEditorConfigObject.Instance.author);
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("WorkingNameSpace(Private):", GUILayout.Width(180));
                ScriptTempEditorConfigObject.Instance.workingNameSpace = EditorGUILayout.TextField(ScriptTempEditorConfigObject.Instance.workingNameSpace);
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("WorkingEditorNameSpace(Private):", GUILayout.Width(220));
                ScriptTempEditorConfigObject.Instance.workingEditorNameSpace = EditorGUILayout.TextField(ScriptTempEditorConfigObject.Instance.workingEditorNameSpace);
            }

            using (var scrop = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrop.scrollPosition;
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUILayout.LabelField("Header(Public):", GUILayout.Width(120));
                    tempConfig.header = EditorGUILayout.TextArea(tempConfig.header,GUILayout.ExpandHeight(true));
                }
            }
           
        }

        public static ScriptTemplateDrawer CreateDrawer(ScriptTemplate currentTemplate)
        {
            if (currentTemplate == null)
            {
                return null;
            }
            switch (currentTemplate.type)
            {
                case TemplateType.Enum:
                    return new EnumScriptDrawer(currentTemplate);
                case TemplateType.Struct:
                    return new StructTempateDrawer(currentTemplate);
                case TemplateType.Class:
                    return new ClassTemplateDrawer(currentTemplate);
                case TemplateType.Interface:
                    return new InterfaceTempateDrawer(currentTemplate);
                default:
                    return null;
            }
        }

        private void DrawStringElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (waitAddTemplate.imports.Count <= index)
                return;
            var recti = new Rect(rect.x - 10, rect.y + 2, 10, 20);
            var rectItem = new Rect(rect.x, rect.y + 4, rect.width - 20, rect.height - 8);
            EditorGUI.LabelField(recti, new GUIContent((index + 1).ToString()));
            waitAddTemplate.imports[index] = EditorGUI.TextField(rectItem, waitAddTemplate.imports[index]);
        }

    }
    #endregion

    #region Templates
    [System.Serializable]
    public class FieldItem
    {
        public string type;
        public string elementName;
        public string comment;
    }

    [System.Serializable]
    public class PropertyItem
    {
        public string type;
        public string elementName;
        public string comment;
        public bool get;
        public bool set;
        public PropertyItem()
        {
            get = set = true;
        }
    }
    [System.Serializable]
    public class TypeInfo
    {
        public string assembly;
        public string name;

        public TypeInfo(Type type)
        {
            assembly = type.Assembly.FullName;
            name = type.FullName;
        }
        public static implicit operator Type(TypeInfo info)
        {
            if (string.IsNullOrEmpty(info.assembly) || string.IsNullOrEmpty(info.name))
                return null;
            return System.Reflection.Assembly.Load(info.assembly)?.GetType(info.name);
        }
    }

    public enum TemplateType
    {
        Enum,
        Struct,
        Class,
        Interface
    }

    [System.Serializable]
    public class WaitAddTemplate
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public TemplateType type;
        [SerializeField]
        public string parentType;
        [SerializeField]
        public List<string> imports = new List<string>();

        public ScriptTemplate GetScriptNewTempalte()
        {
            var template = new ScriptTemplate();
            template.name = name;
            template.type = type;
            template.parentType = parentType;
            template.imports = new List<string>(imports);
            return template;
        }
    }

    /// <summary>
    /// 代码创建模板的模板
    /// </summary>
    [System.Serializable]
    public sealed class ScriptTemplate
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public TemplateType type;
        [SerializeField]
        public string nameSpace;
        [SerializeField]
        public string parentType;
        [SerializeField]
        public List<FieldItem> fields = new List<FieldItem>();
        [SerializeField]
        public List<PropertyItem> propertys = new List<PropertyItem>();
        [SerializeField]
        public List<string> imports = new List<string>();
        [SerializeField]
        public string description;
        [SerializeField]
        public List<string> detailInfo = new List<string>();
    }

    public abstract class ScriptTemplateDrawer
    {
        public ScriptTemplate target;
        private ReorderableList detailList;
        public ref string name => ref target.name;
        public List<FieldItem> fields => target.fields;
        public List<PropertyItem> propertys => target.propertys;
        public List<string> imports => target.imports;
        public ref TemplateType type => ref target.type;
        public ref string description => ref target.description;
        public List<string> detailInfo => target.detailInfo;
        public ref string nameSpace => ref target.nameSpace;
        public ref string parentType => ref target.parentType;
        
        public abstract void OnBodyGUI();

        public virtual void OnFootGUI()
        {
            if (detailList == null)
            {
                InitDetailList();
            }

            if (detailList.list != detailInfo)
            {
                detailList.list = detailInfo;
            }

            using (var horRoot = new EditorGUILayout.HorizontalScope())
            {
                using (var vertical0 = new EditorGUILayout.VerticalScope())
                {
                    using (var hor = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("模 板 名:", GUILayout.Width(60));
                        name = EditorGUILayout.TextField(name, GUILayout.Width(120));
                    }

                    using (var horm = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("命名空间:", GUILayout.Width(60));
                        nameSpace = EditorGUILayout.TextField(nameSpace, GUILayout.Width(120));
                    }

                    using (var horm = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("简     介:", GUILayout.Width(60));
                        description = EditorGUILayout.TextField(description, GUILayout.Width(120));
                    }

                    using (var horm = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("继 承 于:", GUILayout.Width(60));
                        parentType = EditorGUILayout.TextField(parentType, GUILayout.Width(120));
                    }
                }

                using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(600)))
                {
                    detailList.DoLayoutList();
                }
            }
        }

        public string CreateScript(string scriptName)
        {
            var cb = new StringBuilder();
            cb.AppendLine(GetHeaderStr());
            foreach (var import in imports)
            {
                cb.AppendLine($"using {import};");
            }
            cb.AppendLine();
            var ns = CreateNameSpace(scriptName);
            if (ns != null)
            {
                var nsstr = ComplieNameSpaceToString(ns);
                cb.AppendLine(nsstr);
            }
            return cb.ToString().Replace("\r\n", "\n");
        }

        protected void InitDetailList()
        {
            detailList = new ReorderableList(detailInfo, typeof(string), true, true, true, true);
            detailList.onAddCallback += (x) => { detailInfo.Add(""); };
            detailList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "描述信息");

                var defaltbtnRect = new Rect(rect.x + rect.width - 130, rect.y, 60, rect.height);
                var isDefault = ScriptTempEditorConfigObject.Instance.IsDefault(name,type);
                if (isDefault)
                {
                    EditorGUI.LabelField(defaltbtnRect, "(default)", EditorStyles.miniButtonLeft);
                }
                else
                {
                    if (GUI.Button(defaltbtnRect, "Default", EditorStyles.miniButtonLeft))
                    {
                        ScriptTempEditorConfigObject.Instance.SetDefault(name,type);
                    }
                }
                var createbtnRect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
                if (GUI.Button(createbtnRect, "Create", EditorStyles.miniButtonRight))
                {
                    OnCreateButtonClicked();
                }
            };
            detailList.drawElementCallback += (x, y, z, w) => { detailInfo[y] = EditorGUI.TextField(x, detailInfo[y]); };
        }

        protected void DrawFieldItem(Rect rect, FieldItem dataItem, bool haveType)
        {
            if (haveType)
            {
                var rect01 = new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
                var typeRect = new Rect(rect.x + 0.2f * rect.width, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
                var rect02 = new Rect(rect.x + rect.width * 0.3f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                var commentRect = new Rect(rect.x + 0.6f * rect.width, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
                var rect03 = new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);

                dataItem.elementName = EditorGUI.TextField(rect01, dataItem.elementName);
                EditorGUI.LabelField(typeRect, "Type");
                dataItem.type = EditorGUI.TextField(rect02, dataItem.type);
                EditorGUI.LabelField(commentRect, "Desc");
                dataItem.comment = EditorGUI.TextField(rect03, dataItem.comment);
            }
            else
            {
                var left = new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                var right = new Rect(rect.x + rect.width * 0.4f, rect.y, rect.width * 0.6f, EditorGUIUtility.singleLineHeight);
                var center = new Rect(rect.x + rect.width * 0.31f, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
                dataItem.elementName = EditorGUI.TextField(left, dataItem.elementName);
                EditorGUI.LabelField(center, "Desc:");
                dataItem.comment = EditorGUI.TextField(right, dataItem.comment);
            }

        }

        protected void DrawPropertyItem(Rect rect, PropertyItem propertyItem)
        {
            var rect01 = new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(rect.x + 0.2f * rect.width, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
            var rect02 = new Rect(rect.x + rect.width * 0.3f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
            var commentRect = new Rect(rect.x + 0.6f * rect.width, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
            var rect03 = new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);

            propertyItem.elementName = EditorGUI.TextField(rect01, propertyItem.elementName);
            EditorGUI.LabelField(typeRect, "Type");
            propertyItem.type = EditorGUI.TextField(rect02, propertyItem.type);
            EditorGUI.LabelField(commentRect, "Desc");
            propertyItem.comment = EditorGUI.TextField(rect03, propertyItem.comment);

            var getLabelRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
            var getRect = new Rect(rect.x + 0.1f * rect.width, rect.y + EditorGUIUtility.singleLineHeight, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
            var setLabelRect = new Rect(rect.x + 0.2f * rect.width, rect.y + EditorGUIUtility.singleLineHeight, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);
            var setRect = new Rect(rect.x + 0.3f * rect.width, rect.y + EditorGUIUtility.singleLineHeight, rect.width * 0.1f, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(getLabelRect, "get");
            propertyItem.get = EditorGUI.Toggle(getRect, propertyItem.get);
            EditorGUI.LabelField(setLabelRect, "set");
            propertyItem.set = EditorGUI.Toggle(setRect, propertyItem.set);

        }
        protected string DrawNameSpace(Rect rect, string dataItem)
        {
            var rect1 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            return EditorGUI.TextField(rect1, dataItem);
        }
        protected virtual CodeTypeDeclaration CreateClassDeclaration(string scriptName) { return null; }

        protected CodeNamespace CreateNameSpace(string scriptName)
        {
            var type = CreateClassDeclaration(scriptName);
            if (type == null)
                return null;

            var namespaceStr = target.nameSpace;
            if (target.imports.Contains("UnityEditor") && !string.IsNullOrEmpty(ScriptTempEditorConfigObject.Instance.workingEditorNameSpace))
                namespaceStr = ScriptTempEditorConfigObject.Instance.workingEditorNameSpace;
            else if(!string.IsNullOrEmpty(ScriptTempEditorConfigObject.Instance.workingNameSpace))
                namespaceStr = ScriptTempEditorConfigObject.Instance.workingNameSpace;
            CodeNamespace nameSpace = new CodeNamespace(namespaceStr);
            nameSpace.Types.Add(type);
            return nameSpace;
        }
        public string GetHeaderStr()
        {
            var headerStr = ScriptTempConfigObject.Instance.header;
            headerStr = headerStr.Replace("$author", ScriptTempEditorConfigObject.Instance.author).Replace("$time", System.DateTime.Now.ToString("yyyy-MM-dd"));
            var descInfo = "";
            for (int i = 0; i < target.detailInfo.Count; i++)
            {
                descInfo += string.Format("//             {0}.{1}", i + 1, target.detailInfo[i]);
            }
            headerStr = headerStr.Replace("$desc", descInfo);
            return headerStr;
        }

        protected string ComplieNameSpaceToString(CodeNamespace nameSpace)
        {
            using (Microsoft.CSharp.CSharpCodeProvider cprovider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                StringBuilder fileContent = new StringBuilder();
                var option = new System.CodeDom.Compiler.CodeGeneratorOptions();
                option.BlankLinesBetweenMembers = false;
                using (StringWriter sw = new StringWriter(fileContent))
                {
                    cprovider.GenerateCodeFromNamespace(nameSpace, sw, option);
                }
                return fileContent.ToString();
            }
        }
        /// <summary>
        /// 点击创建
        /// </summary>
        private void OnCreateButtonClicked()
        {
            ScriptTempWindow.QuickCreateScript(name, type);

        }
        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scriptStr"></param>
        public void SaveToFile(string path, string scriptName, string scriptStr)
        {
            var scriptPath = string.Format("{0}/{1}.cs", path, scriptName);
            System.IO.File.WriteAllText(scriptPath, scriptStr, System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }

    public class ClassTemplateDrawer : ScriptTemplateDrawer
    {
        protected ReorderableList nameSpaceList;
        protected ReorderableList fieldsList;
        protected ReorderableList propertyList;

        public ClassTemplateDrawer(ScriptTemplate template)
        {
            this.target = template;
            fieldsList = new ReorderableList(fields, typeof(string));
            fieldsList.onAddCallback += (x) => { fields.Add(new FieldItem()); };
            fieldsList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "字段"); };
            fieldsList.drawElementCallback += (x, y, z, w) =>
            {
                DrawFieldItem(x, fields[y], true);
            };

            nameSpaceList = new ReorderableList(imports, typeof(string));
            nameSpaceList.onAddCallback += (x) => { imports.Add(""); };
            nameSpaceList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "引用空间"); };
            nameSpaceList.drawElementCallback += (x, y, z, w) =>
            {
                imports[y] = DrawNameSpace(x, imports[y]);
            };

            propertyList = new ReorderableList(propertys, typeof(string));
            propertyList.onAddCallback += (x) => { propertys.Add(new PropertyItem()); };
            propertyList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "属性"); };
            propertyList.elementHeightCallback = (x) => { return 2 * EditorGUIUtility.singleLineHeight; };
            propertyList.drawElementCallback += (x, y, z, w) =>
            {
                DrawPropertyItem(x, propertys[y]);
            };

        }
        protected override CodeTypeDeclaration CreateClassDeclaration(string scriptName)
        {
            List<CodeMemberField> fields = new List<CodeMemberField>();
            foreach (var item in base.fields)
            {
                CodeMemberField prop = new CodeMemberField();
                prop.Type = new CodeTypeReference(item.type, CodeTypeReferenceOptions.GenericTypeParameter);
                prop.Attributes = MemberAttributes.Public;
                prop.Name = item.elementName;
                prop.Comments.Add(new CodeCommentStatement(item.comment));
                fields.Add(prop);
            }

            List<CodeMemberProperty> propertysMemper = new List<CodeMemberProperty>();
            foreach (var item in propertys)
            {
                CodeMemberProperty prop = new CodeMemberProperty();
                prop.Type = new CodeTypeReference(item.type, CodeTypeReferenceOptions.GenericTypeParameter);
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                prop.Name = item.elementName;
                prop.HasGet = item.get;
                prop.HasSet = item.set;
                //CodeExpression invokeExpression = new CodePropertyReferenceExpression();
                if (item.get) prop.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                prop.Comments.Add(new CodeCommentStatement(item.comment));
                propertysMemper.Add(prop);
            }

            CodeTypeDeclaration wrapProxyClass = new CodeTypeDeclaration(scriptName);
            wrapProxyClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            wrapProxyClass.IsClass = true;

            foreach (var field in fields)
            {
                wrapProxyClass.Members.Add(field);
            }
            foreach (var prop in propertysMemper)
            {
                wrapProxyClass.Members.Add(prop);
            }
            if (!string.IsNullOrEmpty(target.parentType))
                wrapProxyClass.BaseTypes.Add(new CodeTypeReference(target.parentType));
            return wrapProxyClass;
        }

        public override void OnBodyGUI()
        {
            nameSpaceList.DoLayoutList();
            fieldsList.DoLayoutList();
            propertyList.DoLayoutList();
        }
    }
    #endregion

    #region Enum
    /// <summary>
    /// 1.枚举类型脚本
    /// </summary>
    [Serializable]
    public class EnumScriptDrawer : ScriptTemplateDrawer
    {
        private ReorderableList reorderableList;
        public EnumScriptDrawer(ScriptTemplate target)
        {
            this.target = target;
            reorderableList = new ReorderableList(fields, typeof(string));
            reorderableList.onAddCallback += (x) => { fields.Add(new FieldItem()); };
            reorderableList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "枚举列表"); };
            reorderableList.drawElementCallback += (x, y, z, w) =>
            {
                DrawFieldItem(x, fields[y], false);
            };
        }

        protected override CodeTypeDeclaration CreateClassDeclaration(string scriptName)
        {
            List<CodeMemberField> fields = new List<CodeMemberField>();
            foreach (var item in base.fields)
            {
                CodeMemberField prop = new CodeMemberField();
                prop.Name = item.elementName;
                prop.Comments.Add(new CodeCommentStatement(item.comment));
                fields.Add(prop);
            }

            CodeTypeDeclaration wrapProxyClass = new CodeTypeDeclaration(scriptName);
            wrapProxyClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            wrapProxyClass.IsEnum = true;
            if (!string.IsNullOrEmpty(description))
            {
                wrapProxyClass.Comments.Add(new CodeCommentStatement("<summary>", true));
                wrapProxyClass.Comments.Add(new CodeCommentStatement(description, true));
                wrapProxyClass.Comments.Add(new CodeCommentStatement("<summary>", true));
            }

            foreach (var field in fields)
            {
                wrapProxyClass.Members.Add(field);
            }

            return wrapProxyClass;
        }
        public override void OnBodyGUI()
        {
            reorderableList.DoLayoutList();
        }
    }
    #endregion

    #region Struct
    /// <summary>
    /// 5.结构体模板
    /// </summary>
    [Serializable]
    public class StructTempateDrawer : ScriptTemplateDrawer
    {
        private ReorderableList nameSpaceList;
        private ReorderableList reorderableList;

        public StructTempateDrawer(ScriptTemplate target)
        {
            this.target = target;
            reorderableList = new ReorderableList(fields, typeof(string));
            reorderableList.onAddCallback += (x) => { fields.Add(new FieldItem()); };
            reorderableList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "字段"); };
            reorderableList.drawElementCallback += (x, y, z, w) =>
            {
                DrawFieldItem(x, fields[y], true);
            };

            nameSpaceList = new ReorderableList(imports, typeof(string));
            nameSpaceList.onAddCallback += (x) => { imports.Add(""); };
            nameSpaceList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "引用空间"); };
            nameSpaceList.drawElementCallback += (x, y, z, w) =>
            {
                imports[y] = DrawNameSpace(x, imports[y]);
            };
        }

        protected override CodeTypeDeclaration CreateClassDeclaration(string scriptName)
        {
            List<CodeMemberField> fields = new List<CodeMemberField>();
            foreach (var item in base.fields)
            {
                CodeMemberField prop = new CodeMemberField();
                prop.Type = new CodeTypeReference(item.type, CodeTypeReferenceOptions.GenericTypeParameter);
                prop.Attributes = MemberAttributes.Public;
                prop.Name = item.elementName;
                prop.Comments.Add(new CodeCommentStatement(item.comment));
                fields.Add(prop);
            }

            CodeTypeDeclaration wrapProxyClass = new CodeTypeDeclaration(scriptName);
            wrapProxyClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            wrapProxyClass.IsStruct = true;
            if (!string.IsNullOrEmpty(description))
            {
                var destription = description;
                wrapProxyClass.Comments.Add(new CodeCommentStatement("<summary>", true));
                wrapProxyClass.Comments.Add(new CodeCommentStatement(destription, true));
                wrapProxyClass.Comments.Add(new CodeCommentStatement("<summary>", true));
            }

            foreach (var field in fields)
            {
                wrapProxyClass.Members.Add(field);
            }

            return wrapProxyClass;
        }

        public override void OnBodyGUI()
        {
            nameSpaceList.DoLayoutList();
            reorderableList.DoLayoutList();
        }
    }
    #endregion

    #region Interface
    /// <summary>
    /// 6.接口创建模板
    /// </summary>
    [Serializable]
    public class InterfaceTempateDrawer : ScriptTemplateDrawer
    {
        private ReorderableList nameSpaceList;
        private ReorderableList reorderableList;

        public InterfaceTempateDrawer(ScriptTemplate target)
        {
            this.target = target;
            reorderableList = new ReorderableList(propertys, typeof(string));
            reorderableList.onAddCallback += (x) => { propertys.Add(new PropertyItem()); };
            reorderableList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "属性"); };
            reorderableList.elementHeightCallback = (x) => { return 2 * EditorGUIUtility.singleLineHeight; };
            reorderableList.drawElementCallback += (x, y, z, w) =>
            {
                DrawPropertyItem(x, propertys[y]);
            };

            nameSpaceList = new ReorderableList(imports, typeof(string));
            nameSpaceList.onAddCallback += (x) => { imports.Add(""); };
            nameSpaceList.drawHeaderCallback += (x) => { EditorGUI.LabelField(x, "引用空间"); };
            nameSpaceList.drawElementCallback += (x, y, z, w) =>
            {
                imports[y] = DrawNameSpace(x, imports[y]);
            };
        }

        protected override CodeTypeDeclaration CreateClassDeclaration(string scriptName)
        {
            List<CodeMemberProperty> propertysMemper = new List<CodeMemberProperty>();
            foreach (var item in propertys)
            {
                CodeMemberProperty prop = new CodeMemberProperty();
                prop.Type = new CodeTypeReference(item.type, CodeTypeReferenceOptions.GenericTypeParameter);
                prop.Attributes = MemberAttributes.Public;
                prop.Name = item.elementName;
                prop.HasGet = item.get;
                prop.HasSet = item.set;
                prop.Comments.Add(new CodeCommentStatement(item.comment));
                propertysMemper.Add(prop);
            }

            CodeTypeDeclaration wrapProxyClass = new CodeTypeDeclaration(scriptName);
            wrapProxyClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            wrapProxyClass.IsInterface = true;
            if (!string.IsNullOrEmpty(description))
            {
                var destription = description;
                wrapProxyClass.Comments.Add(new CodeCommentStatement("<summary>", true));
                wrapProxyClass.Comments.Add(new CodeCommentStatement(destription, true));
                wrapProxyClass.Comments.Add(new CodeCommentStatement("<summary>", true));
            }
            foreach (var prop in propertysMemper)
            {
                wrapProxyClass.Members.Add(prop);
            }
            return wrapProxyClass;
        }

        public override void OnBodyGUI()
        {
            nameSpaceList.DoLayoutList();
            reorderableList.DoLayoutList();
        }
    }
    #endregion
}
