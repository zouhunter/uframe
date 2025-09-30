using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UFrame.BridgeUI.Editors;

namespace UFrame.BridgeUI.Editors
{
    public static class GenCodeUtil
    {
        public static Type[] supportControls;//支持的控件
        public static string[] supportBaseTypes;//父级类型
        private static List<string> InnerNameSpace;//过滤内部命名空间

        static GenCodeUtil()
        {
            supportBaseTypes = LoadAllBasePanels();
            supportControls = UFrame.BridgeUI.Utility.InitSupportTypes().ToArray();
            InnerNameSpace = new List<string>()
            {
                 "UnityEngine.UI",
                 "UnityEngine",
                 "UnityEngine.EventSystems"
             };
        }

        /// <summary>
        /// 快速进行控件绑定
        /// </summary>
        /// <param name="go"></param>
        /// <param name="components"></param>
        public static void BindingUIComponents(MonoBehaviour behaiver, List<ComponentItem> components)
        {
            if (behaiver == null)
            {
                EditorApplication.Beep();
                return;
            }
            foreach (var item in components)
            {
                BindingUIComponent(behaiver, item);
            }
        }

        public static void BindingUIComponent(MonoBehaviour behaiver, ComponentItem component)
        {
            var filedName = "m_" + component.name;
            UnityEngine.Object obj = component.isScriptComponent ? component.scriptTarget as UnityEngine.Object : component.target;
            if (component.componentType != typeof(GameObject) && !typeof(ScriptableObject).IsAssignableFrom(component.componentType))
            {
                obj = component.target.GetComponent(component.componentType);
            }
            try
            {
                behaiver.GetType().InvokeMember(filedName,
                          BindingFlags.SetField |
                          BindingFlags.Instance |
                          BindingFlags.NonPublic |
                          BindingFlags.Public,
                          null, behaiver, new object[] { obj }, null, null, null);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

        }

        //从uibinding分析
        public static void AnalysisComponentFromUIBinding(UIBinding binding, List<ComponentItem> components, List<ReferenceItem> refrenceItems, GenCodeRule rule)
        {
            var viewScriptType = binding.LoadViewScriptType();
            if (viewScriptType != null)
            {
                rule.nameSpace = viewScriptType.Namespace;
            }
            else
            {
                rule.nameSpace = UISetting.defultNameSpace;
            }

            foreach (var refItem in binding.infos)
            {
                if (refrenceItems.Find(x => refItem != null && x.name == refItem.name) == null)
                    refrenceItems.Add(refItem);
            }

            for (int i = 0; i < binding.infos.Count; i++)
            {
                var refrenceItem = binding.infos[i];

                if (!refrenceItem.referenceTarget)
                    continue;
                var propType = refrenceItem.referenceTarget.GetType();
                var support = typeof(UnityEngine.Component).IsAssignableFrom(propType) || typeof(ScriptableObject).IsAssignableFrom(propType) || propType == typeof(GameObject);
                if (!support)
                    continue;

                var compItem = components.Find(x => x.name == refrenceItem.name);

                if (compItem == null)
                {
                    compItem = new ComponentItem();
                    compItem.name = refrenceItem.name;
                    components.Add(compItem);
                }

                var value = refrenceItem.referenceTarget;
                if (value != null)
                {
                    if (refrenceItem.type == null && !string.IsNullOrEmpty(refrenceItem.typeFullName))
                        refrenceItem.type = Utility.FindTypeInAllAssemble(refrenceItem.typeFullName);

                    if (refrenceItem.type == null)
                        continue;

                    if (typeof(GameObject) == refrenceItem.type)
                    {
                        compItem.target = value as GameObject;
                        compItem.components = SortComponent(compItem.target);
                        var types = Array.ConvertAll(compItem.components, x => x.type);
                        compItem.componentID = Array.IndexOf(types, typeof(GameObject));
                    }
                    else if (typeof(ScriptableObject).IsAssignableFrom(refrenceItem.type))
                    {
                        compItem.UpdateAsScriptObject(value as ScriptableObject);
                    }
                    else
                    {
                        compItem.target = (value as Component).gameObject;
                        compItem.components = SortComponent(compItem.target, refrenceItem.type);
                        var types = Array.ConvertAll(compItem.components, x => x.type);
                        compItem.componentID = Array.IndexOf(types, value.GetType());
                    }
                }
            }
        }

        /// <summary>
        /// 分析代码的的组件信息
        /// </summary>
        /// <param name="component"></param>
        /// <param name="components"></param>
        public static void AnalysisComponent(MonoBehaviour component, List<ComponentItem> components, List<ReferenceItem> refrenceItems, GenCodeRule rule)
        {
            ViewCoder coder = new ViewCoder();
            if (component is UIBinding)
            {
                var uiBinding = component as UIBinding;
                AnalysisComponentFromUIBinding(uiBinding, components, refrenceItems, rule);
                var viewScriptType = uiBinding.LoadViewScriptType();
                coder.AnalysisBinding(component.gameObject, viewScriptType, components.ToArray(), rule);
                return;
            }
            var type = component.GetType();
            rule.nameSpace = type.Namespace;
            var propertys = type.GetProperties(BindingFlags.GetProperty | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in propertys)
            {
                if (typeof(BindingReference) == prop.PropertyType)
                    continue;

                var support = typeof(UnityEngine.Component).IsAssignableFrom(prop.PropertyType)
                    || typeof(ScriptableObject).IsAssignableFrom(prop.PropertyType)
                    || prop.PropertyType == typeof(GameObject);

                if (support)
                {
                    var compItem = components.Find(x => "m_" + x.name == prop.Name || x.name == prop.Name);

                    if (compItem == null)
                    {
                        compItem = new ComponentItem();
                        compItem.name = prop.Name.Replace("m_", "");
                        components.Add(compItem);
                    }

                    var value = prop.GetValue(component, new object[0]);
                    if (value != null)
                    {
                        if (prop.PropertyType == typeof(GameObject))
                        {
                            compItem.target = value as GameObject;
                            compItem.components = SortComponent(compItem.target);
                            var types = Array.ConvertAll(compItem.components, x => x.type);
                            compItem.componentID = Array.IndexOf(types, typeof(GameObject));
                        }
                        else if (typeof(ScriptableObject).IsAssignableFrom(prop.PropertyType))
                        {
                            compItem.UpdateAsScriptObject(value as ScriptableObject);
                        }
                        else
                        {
                            compItem.target = (value as Component).gameObject;
                            compItem.components = SortComponent(compItem.target);
                            var types = Array.ConvertAll(compItem.components, x => x.type);
                            compItem.componentID = Array.IndexOf(types, value.GetType());
                        }
                    }
                }
            }

            var referenceField = type.GetField("m_parameter", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic);
            if (referenceField != null)
            {
                var referenceValue = referenceField.GetValue(component);
                var referenceType = referenceField.FieldType;
                var dataFields = referenceType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);

                foreach (var fieldItem in dataFields)
                {
                    var item = new ReferenceItem();
                    item.name = typeof(UnityEngine.Object).IsAssignableFrom(fieldItem.FieldType) ? fieldItem.Name.Substring(2) : fieldItem.Name;
                    item.type = fieldItem.FieldType.IsArray ? fieldItem.FieldType.GetElementType() : fieldItem.FieldType;
                    var value = fieldItem.GetValue(referenceValue);
                    var arrayValue = value as System.Array;
                    item.isArray = arrayValue != null;
                    refrenceItems.Add(item);
                }
            }
            coder.AnalysisBinding(component.gameObject, null, components.ToArray(), rule);
        }

        /// <summary>
        /// 创建viewcode
        /// </summary>
        /// <param name="defultNameSpace"></param>
        /// <param name="name"></param>
        internal static void CreateDefaultViewCode(string className)
        {
            Debug.LogError("CreateDefault:" + className);
            ViewCoder coder = new ViewCoder();
            coder.parentClassName = UISetting.defultNameSpace + "." + className + "Internal";
            var parentType = BridgeUI.Utility.FindTypeInAllAssemble(coder.parentClassName);
            if (parentType == null)
            {
                coder.parentClassName = typeof(ViewBaseComponent).FullName;
            }
            coder.nameSpace = UISetting.defultNameSpace;
            coder.forbidHead = string.IsNullOrEmpty(UISetting.userName);
            coder.className = className;
            coder.path = UISetting.script_path + "/" + className + ".cs";
            coder.CompileSave();
        }

        /// <summary>
        /// 创建代码
        /// </summary>
        /// <param name="go"></param>
        /// <param name="components"></param>
        /// <param name="rule"></param>
        public static void UpdateBindingScripts(GameObject go, List<ComponentItem> components, List<ReferenceItem> referenceItems, GenCodeRule rule)
        {
            string viewName = go.name;
            var bindingRef = go.GetComponent<BindingReference>();
            if (bindingRef)
            {
                viewName = bindingRef.ViewTypeName;
                if (!string.IsNullOrEmpty(viewName) && viewName.Contains("."))
                    viewName = viewName.Substring(viewName.LastIndexOf('.') + 1);
            }
            Action<ViewCoder> onLoad = (uiCoder) =>
            {
                var baseType = GenCodeUtil.supportBaseTypes[rule.baseTypeIndex];
                var needAdd = FilterExisField(baseType, components);
                uiCoder.parentClassName = supportBaseTypes[rule.baseTypeIndex];
                uiCoder.componentItems = needAdd;
                uiCoder.CompileSave();
                UnityEditor.EditorApplication.delayCall += AssetDatabase.Refresh;
            };

            var allRefrenceItems = new List<ReferenceItem>(referenceItems);
            var componentsRefs = WorpReferenceItems(components);
            allRefrenceItems.RemoveAll(x => componentsRefs.Find(y => y.name == x.name) != null);
            allRefrenceItems.AddRange(componentsRefs);
            CreateBindingScriptAsnyc(go, viewName, allRefrenceItems, onLoad, rule);
        }

        /// <summary>
        /// 将类型转换为人可读的字符串
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeStringName(Type type)
        {
            if (type == null)
            {
                return "";
            }
            var typeName = type.FullName;
            if (type.IsGenericType)
            {
                typeName = type.FullName.Remove(type.FullName.IndexOf("`"));
                var arguments = type.GetGenericArguments();
                typeName += "<";
                typeName += string.Join(",", Array.ConvertAll<Type, string>(arguments, x => TypeStringName(x)));
                typeName += ">";
            }
            return typeName;
        }

        /// <summary>
        /// 按顺序加载组件
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static TypeInfo[] SortComponent(GameObject target,params Type[] types)
        {
            var innercomponentsTypes = new List<TypeInfo>();
            var innercomponents = target.GetComponents<Component>();
            for (int i = 0; i < supportControls.Length; i++)//按指定的顺序添加控件
            {
                var finded = Array.Find(innercomponents, x => supportControls[i].IsAssignableFrom(x.GetType()));
                if (finded)
                {
                    innercomponentsTypes.Add(new TypeInfo(finded.GetType()));
                }
            }

            var userComponentsTypes = target.GetComponents<IUIControl>().Select(x => new TypeInfo(x.GetType()));
            var supportedlist = new List<TypeInfo>();
            supportedlist.AddRange(userComponentsTypes);
            supportedlist.AddRange(innercomponentsTypes);
            supportedlist.Add(new TypeInfo(typeof(GameObject)));

            if(types != null)
            {
                foreach (var subType in types)
                {
                    if (supportedlist.Find(x => x.type == subType).type != null)
                    {
                        continue;
                    }
                    supportedlist.Add(new TypeInfo(subType));
                }
            }
            return supportedlist.ToArray();
        }

        /// <summary>
        /// 选择引用型组件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="onChoise"></param>
        public static void ChoiseAnReferenceMonobehiver(GameObject go, Action<MonoBehaviour> onChoise)
        {
            var behaivers = GetUserReferenceMonobehaiver(go);
            if (behaivers != null && behaivers.Length > 0)
            {
                if (behaivers.Count() == 1)
                {
                    onChoise(behaivers[0]);
                }
                else
                {
                    var rect = new Rect(Event.current.mousePosition, new Vector2(0, 0));
                    var options = Array.ConvertAll<MonoBehaviour, GUIContent>(behaivers, x => new GUIContent(x.GetType().FullName));
                    EditorUtility.DisplayCustomMenu(rect, options, -1, new EditorUtility.SelectMenuItemFunction((obj, _options, index) =>
                    {
                        if (index >= 0)
                        {
                            onChoise(behaivers[index]);
                        }
                    }), null);
                }
            }
            else if (behaivers == null || behaivers.Length == 0)
            {
                var uibehaviour = go.GetComponent<UIBinding>();
                onChoise(uibehaviour);
            }
        }

        /// <summary>
        /// 1.如果预制体上有脚本，则保存到预制体脚本所在路径
        /// 2.如果没有脚本，则保存到默认文件夹
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="dress"></param>
        /// <returns></returns>
        public static string InitScriptPath(GameObject prefab, string dress)
        {
            if (prefab == null) return null;
            string defaultName = prefab.name;
            string folder = null;
            var reference = GetUserReferenceMonobehaiver(prefab);
            if (reference != null && reference.Length > 0)
            {
                var script = reference.FirstOrDefault();
                if (script != null)
                {
                    var monoScript = MonoScript.FromMonoBehaviour(script);
                    var scriptPath = AssetDatabase.GetAssetPath(monoScript);
                    if (!string.IsNullOrEmpty(scriptPath))
                    {
                        folder = System.IO.Path.GetDirectoryName(scriptPath);
                    }
                }
            }
            else
            {
                var refView = prefab.GetComponent<BindingReference>();
                if(refView)
                {
                    defaultName = refView.ViewTypeName;
                    if(defaultName.Contains("."))
                    {
                        defaultName = defaultName.Substring(defaultName.LastIndexOf('.') + 1);
                    }
                }
            }

            if (string.IsNullOrEmpty(folder))
            {
                folder = string.Format("{0}/{1}", UISetting.script_path, defaultName);
            }

            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return string.Format("{0}/{1}{2}.cs", folder, defaultName, dress);
        }

        #region private functions
        /// <summary>
        /// 获取所有引用型组件
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        private static MonoBehaviour[] GetUserReferenceMonobehaiver(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("预制体传入不能为空");
                return null;
            }

            var monobehaivers = prefab.GetComponents<MonoBehaviour>();

            var supported = from behaiver in monobehaivers
                            where behaiver != null
                            where behaiver is IClassReference
                            where !InnerNameSpace.Contains(behaiver.GetType().Namespace)
                            where !(behaiver is UIBinding)
                            select behaiver;

            if (supported == null || supported.Count() == 0)
            {
                return null;
            }

            var mainScript = (from main in monobehaivers
                              where MonoScript.FromMonoBehaviour(main).GetClass().Name == prefab.name
                              select main).FirstOrDefault();

            if (mainScript != null)
            {
                return new MonoBehaviour[] { mainScript };
            }
            return supported.ToArray();
        }

        /// <summary>
        /// 所有支持的父级
        /// </summary>
        /// <returns></returns>
        private static string[] LoadAllBasePanels()
        {
            var support = new List<Type>();
            var allTypes = BridgeUI.Utility.GetAllTypes();
            foreach (var item in allTypes)
            {
                var attributes = item.GetCustomAttributes(false);
                if (Array.Find(attributes, x => x is PanelParentAttribute) != null)
                {
                    support.Add(item);
                }
            }
            support.Sort(ComparerBaseTypes);
            return support.ConvertAll(x => x.FullName).ToArray();
        }

        /// <summary>
        /// 类型比较
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static int ComparerBaseTypes(Type x, Type y)
        {
            var att_x = GetAttribute(x);
            var att_y = GetAttribute(y);
            if (att_x.sortIndex > att_y.sortIndex)
            {
                return -1;
            }
            else if (att_x.sortIndex < att_y.sortIndex)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取指定类型的特性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static PanelParentAttribute GetAttribute(Type type)
        {
            var attributes = type.GetCustomAttributes(false);
            var attribute = Array.Find(attributes, x => x is PanelParentAttribute);
            return attribute as PanelParentAttribute;
        }

        /// <summary>
        /// 异步从已经存在的脚本加载
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="nameSpace"></param>
        /// <param name="onGet"></param>
        private static void CreateBindingScriptAsnyc(GameObject prefab, string viewName, List<ReferenceItem> referenceItems, Action<ViewCoder> onGet, GenCodeRule rule)
        {
            ChoiseAnReferenceMonobehiver(prefab, (x) =>
            {
                ViewCoder coder = new ViewCoder();
                coder.target = x;
                if (string.IsNullOrEmpty(viewName))
                    viewName = prefab.name;

                coder.className = viewName + "Internal";
                coder.refClassName = viewName + "Reference";
                coder.nameSpace = rule.nameSpace;
                coder.path = InitScriptPath(prefab, "Internal");
                coder.referenceItems = referenceItems.ToArray();
                if (!prefab.GetComponent<UIBinding>())
                {
                    var script = BindingReferenceEditor.CreateScript(coder.nameSpace, coder.refClassName, referenceItems);
                    var path = InitScriptPath(prefab, "Reference");
                    System.IO.File.WriteAllText(path, script);
                }

                if (x != null && !(x is UIBinding))
                {
                    var refClassType = x.GetType();
                    coder.nameSpace = refClassType.Namespace;
                    coder.refClassName = refClassType.Name;

                    var dataField = refClassType.GetField("m_data", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (dataField != null)
                    {
                        coder.innerFields = dataField.FieldType.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
                    }
                    var refFields = refClassType.GetFields(BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => typeof(BindingReference).IsAssignableFrom(prop.FieldType)).ToArray();
                    if (refFields.Length > 0)
                    {
                        coder.subReferences = refFields;
                    }
                    onGet(coder);
                }
                else
                {
                    var scriptType = BridgeUI.Utility.FindTypeInAllAssemble(coder.nameSpace + "." + coder.refClassName);
                    if (scriptType != null)
                    {
                        prefab.AddComponent(scriptType);
                        if (referenceItems.Count > 0)
                        {
                            prefab.AddComponent<ReferenceCatchBehaiver>().SetReferenceItems(referenceItems);
                        }
                    }
                    onGet(coder);
                }
            });
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public static List<ReferenceItem> WorpReferenceItems(List<ComponentItem> components)
        {
            var referenceItems = new List<ReferenceItem>();
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                var item = new ReferenceItem();
                item.isArray = false;
                if (component.componentType == typeof(GameObject))
                {
                    item.referenceTarget = component.target;
                }
                else if (typeof(Component).IsAssignableFrom(component.componentType))
                {
                    if (component.target != null)
                    {
                        item.referenceTarget = component.target.GetComponent(component.componentType);
                    }
                }
                item.type = component.componentType;
                item.name = component.name;
                referenceItems.Add(item);
            }
            return referenceItems;
        }


        /// <summary>
        /// 过虑已经存在的变量
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        private static ComponentItem[] FilterExisField(string typeName, List<ComponentItem> components)
        {
            var type = BridgeUI.Utility.FindTypeInAllAssemble(typeName);
            var list = new List<ComponentItem>();
            if (type != null)
            {
                foreach (var item in components)
                {
                    if (type.GetField("m_" + item.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField) == null)
                    {
                        list.Add(item);
                    }
                }
                return list.ToArray();
            }
            else
            {
                return components.ToArray();
            }
        }

        /// <summary>
        /// 按优先级获取类型
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="membername"></param>
        /// <returns></returns>
        public static Type GetTypeClamp(Type baseType, string membername)
        {
            var flag = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            Type infoType = null;
            var prop = baseType.GetProperty(membername, System.Reflection.BindingFlags.GetProperty | flag);
            if (prop != null)
            {
                infoType = prop.PropertyType;
            }
            var field = baseType.GetField(membername, System.Reflection.BindingFlags.GetField | flag);
            if (field != null)
            {
                infoType = field.FieldType;
            }
            try
            {
                var members = baseType.GetMember(membername, BindingFlags.FlattenHierarchy | flag);
                for (int i = 0; i < members.Length; i++)
                {
                    var member = members[i];
                    if (member is MethodInfo)
                    {
                        var func = member as MethodInfo;
                        if (func != null && func.GetParameters().Count() == 1)
                        {
                            infoType = func.GetParameters()[0].ParameterType;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return infoType;
        }
        #endregion


    }
}