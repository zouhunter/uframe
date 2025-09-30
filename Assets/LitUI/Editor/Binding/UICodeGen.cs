using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Text;

using UnityEngine;

using UFrame.LitUI;

using System;
using System.Reflection;
using System.Linq;
using Unity.VisualScripting;

namespace UFrame.LitUI
{
    public class UICodeGen
    {
        public static Dictionary<Type, string> optimizedTypeName = new Dictionary<Type, string> {
            {typeof(char),"char" },
            {typeof(string),"string" },
            {typeof(byte),"byte" },
            {typeof(sbyte),"sbyte" },
            {typeof(short),"short" },
            {typeof(ushort),"ushort" },
            {typeof(int),"int" },
            {typeof(uint),"uint" },
            {typeof(long),"long" },
            {typeof(ulong),"ulong" },
            {typeof(float),"float" },
            {typeof(double),"double" },
            {typeof(bool),"bool" },
        };
        public void AnalysisBinding(VMBinder binder, ComponentBinding[] ComponentBindings)
        {
            var scriptPath = FindScriptPath(binder?.GetType());

            if (System.IO.File.Exists(scriptPath))
            {
                var script = System.IO.File.ReadAllText(scriptPath, System.Text.Encoding.UTF8);

                if (ComponentBindings != null)
                {
                    //解析ComponentBinding
                    var bindingMembers = new List<string>();
                    var bindingEvents = new List<string>();
                    var exposeComps = new List<string>();

                    GetInvocations(script, ref bindingMembers, ref bindingEvents, ref exposeComps);
                    bindingMembers.ForEach(x => AnalysisBindingMembers(x, ComponentBindings));
                    bindingEvents.ForEach(x => AnalysisBindingEvents(x, ComponentBindings));
                    exposeComps.ForEach(x => AnalysisExposeComps(x, ComponentBindings));

                }
            }
            else if (!string.IsNullOrEmpty(scriptPath))
            {
                Debug.Assert(System.IO.File.Exists(scriptPath), "未找到：" + scriptPath);
            }
        }

        public string FindScriptPath(Type type)
        {
            if (type == null)
                return null;

            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && (script.GetClass() == null || script.GetClass() == type))
                {
                    return path;
                }
            }
            Debug.LogError("not file path for " + type.Name);
            return null;
        }

        /// <summary>
        /// 分析代码中的绑定信息
        /// </summary>
        /// <param name="scripts"></param>
        /// <param name="bindingMembers"></param>
        /// <param name="bindingEvents"></param>
        private static void GetInvocations(string scripts, ref List<string> bindingMembers, ref List<string> bindingEvents, ref List<string> exposeComps)
        {
            var reg_0 = @"RegistValueChange<.*>\((.*)\);";
            var matchs_0 = Regex.Matches(scripts, reg_0);
            for (int i = 0; i < matchs_0.Count; i++)
            {
                var match = matchs_0[i];
                if (match.Success)
                {
                    var script = match.Groups[1].Value;
                    bindingMembers.Add(script);
                }
            }


            var reg_1 = @"RegistEvent\((.*)\);";
            var matchs_1 = Regex.Matches(scripts, reg_1);
            for (int i = 0; i < matchs_1.Count; i++)
            {
                var match = matchs_1[i];
                if (match.Success)
                {
                    var script = match.Groups[1].Value;
                    bindingEvents.Add(script);
                }
            }

            var reg_2 = @"binding.(\w+) = binder.(\w+);";
            var matchs_2 = Regex.Matches(scripts, reg_2);
            for (int i = 0; i < matchs_2.Count; i++)
            {
                var match = matchs_2[i];
                if (match.Success)
                {
                    var script = match.Groups[1].Value;
                    exposeComps.Add(script);
                }
            }
        }

        /// <summary>
        /// 分析代码中的绑定信息
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="components"></param>
        private static void AnalysisBindingMembers(string invocation, ComponentBinding[] components)
        {
            var arguments = invocation.Replace(" ", "").Split(',');
            bool isMethod;
            var lamdaArgs = AnalysisTargetFromLamdaArgument(arguments[0], out isMethod);
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var sourceName = arguments[1].Replace("\"", "");
                var targetName = lamdaArgs[1];
                if (component.name == lamdaArgs[0])
                {
                    BindingShow info = component.viewItems.Find(x => x.bindingTarget == targetName);
                    if (info == null)
                    {
                        info = new BindingShow();
                        component.viewItems.Add(info);
                    }
                    info.bindingSource = sourceName;
                    info.bindingTarget = targetName;
                    info.isMethod = isMethod;
                    var type = GetTypeClamp(component.targetType, targetName);
                    info.bindingTargetType.Update(type);
                    info.changeEvent = AnalysisValueChangeName(invocation);
                }
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

            if (infoType == null)
            {
                Debug.LogError($"info type {baseType} emtpy:{membername}");
            }

            return infoType;
        }
        private static string AnalysisValueChangeName(string arg)
        {
            arg = arg.Replace(" ", "");
            var pattem = ",\\w+.(\\w+)$";
            var match = System.Text.RegularExpressions.Regex.Match(arg, pattem);
            if (match != null)
            {
                var value = match.Groups[1].Value;
                return value.Trim();
            }
            return null;

        }
        private static string[] AnalysisTargetFromLamdaArgument(string arg, out bool isMethod)
        {
            arg = arg.Replace(" ", "");

            if (arg.Contains("=>"))
            {
                isMethod = false;
                var pattem = "x=>(.*)=x";
                var match = System.Text.RegularExpressions.Regex.Match(arg, pattem);
                if (match != null)
                {
                    var value = match.Groups[1].Value;
                    return value.Split('.');
                }
                return null;
            }
            else
            {
                var value = arg;
                if (arg.Contains("\""))
                {
                    value = arg.Replace("\"", "");
                    isMethod = false;
                }
                else
                {
                    isMethod = true;
                }
                return value.Split('.');
            }

        }
        /// <summary>
        /// 事件绑定信息解析
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="components"></param>
        private static void AnalysisBindingEvents(string invocation, ComponentBinding[] components)
        {
            var arguments = invocation.Replace(" ", "").Split(',');
            var arg0s = arguments[0].Split('.');
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];

                var targetName = arg0s[1];
                var sourceName = arguments[1].Replace("\"", "");

                if (component.name == arg0s[0])
                {
                    Type infoType = GetTypeClamp(component.targetType, targetName);
                    var info = component.eventItems.Find(x => x.bindingSource == sourceName);

                    if (info == null)
                    {
                        info = new BindingEvent();
                        component.eventItems.Add(info);
                    }

                    info.bindingSource = sourceName;
                    info.bindingTarget = targetName;
                    info.bindingTargetType.Update(infoType);

                    if (arguments.Length > 2)
                    {
                        info.type = BindingType.Full;//3个参数
                    }
                    else
                    {
                        info.type = BindingType.Simple;//2个参数
                    }
                }
            }
        }
        /// <summary>
        /// 组件暴露信息解析
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="components"></param>
        private static void AnalysisExposeComps(string invocation, ComponentBinding[] components)
        {
            var component = Array.Find(components, x => x.name == invocation);
            if (component != null)
            {
                component.expose = true;
            }
        }
        public static string CreateHead(string author, string time, params string[] detailInfo)
        {
            var str1 =
               @"///*************************************************************************************
///* 作    者：       {0}
///* 创建时间：       {1}
///* 说    明：       ";
            var str2 = "///                   ";
            var str3 = "///* ************************************************************************************/";

            System.Text.StringBuilder headerStr = new System.Text.StringBuilder();
            headerStr.Append(string.Format(str1, author, time));
            for (int i = 0; i < detailInfo.Length; i++)
            {
                if (i == 0)
                {
                    headerStr.AppendLine(string.Format("{0}.{1}", i + 1, detailInfo[i]));
                }
                else
                {
                    headerStr.AppendLine(string.Format("{0}{1}.{2}", str2, i + 1, detailInfo[i]));
                }
            }
            headerStr.AppendLine(str3);
            return headerStr.ToString();
        }
        /// <summary>
        /// 生成代码
        /// </summary>
        /// <returns></returns>
        public string GenerateBinderScript(ComponentBinding[] ComponentBindings, string nameSpace, string className, string userName)
        {
            var types = new List<Type>();
            var protocals = GetProtocals(ComponentBindings, types);
            var parentName = UISetting.Instance.vmBinderBase;
            var headString = CreateHead(userName, System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                "1.脚本由工具生成，",
                "2.可能被复写，",
                "3.请使用继承方式使用。");
            var defaultNamespace = typeof(UIView).Namespace;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(headString);
            if (nameSpace != defaultNamespace)
                sb.AppendLine($"using {defaultNamespace};");
            sb.AppendLine("using System;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.Scripting;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}", nameSpace); sb.AppendLine();
            sb.AppendLine("{");
            CreateProtocal(protocals, sb);
            string classHead = "public class {0}:{1}";
            sb.Append("\t"); sb.AppendLine("[Preserve]");
            sb.Append("\t"); sb.AppendFormat(classHead, className, parentName); sb.AppendLine();
            sb.Append("\t"); sb.AppendLine("{");
            for (int i = 0; i < ComponentBindings.Length; i++)
            {
                var name = ComponentBindings[i].name;
                sb.Append("\t\t"); sb.AppendFormat("public {0} {1};", TypeStringName(ComponentBindings[i].targetType), name); sb.AppendLine();
            }
            sb.AppendLine();
            CreateBinderClassScript("\t\t", ComponentBindings, sb);
            sb.AppendLine();
            CreateInerViewModel("\t\t", className, ComponentBindings, sb);
            sb.Append("\t"); sb.AppendLine("}");
            sb.AppendLine("}");
            var compiledScript = sb.ToString();
            return compiledScript;
        }

        /// <summary>
        /// 协议字符串
        /// </summary>
        /// <param name="typeList"></param>
        /// <returns></returns>
        private SortedDictionary<string, string> GetProtocals(ComponentBinding[] ComponentBindings, List<Type> typeList = null)
        {
            var list = new SortedDictionary<string, string>();

            if (ComponentBindings != null)
                for (int i = 0; i < ComponentBindings.Length; i++)
                {
                    var binding = ComponentBindings[i];
                    var compName = binding.name;
                    var viewBindings = binding.viewItems;
                    for (int j = 0; j < viewBindings.Count; j++)
                    {
                        var viewItem = viewBindings[j];
                        var bindingKey = viewItem.bindingSource;
                        if (!list.ContainsKey(bindingKey))
                        {
                            list.Add(bindingKey, compName + "." + viewItem.bindingTarget);
                            if (typeList != null)
                            {
                                typeList.Add(viewItem.bindingTargetType.type);
                            }
                        }
                    }
                    var eventBindings = binding.eventItems;
                    for (int j = 0; j < eventBindings.Count; j++)
                    {
                        var eventItem = eventBindings[j];
                        var bindingKey = eventItem.bindingSource;
                        if (!list.ContainsKey(bindingKey))
                        {
                            list.Add(bindingKey, compName + "." + eventItem.bindingTarget);
                            if (typeList != null)
                            {
                                var type = GetUIActionType(binding, eventItem);
                                typeList.Add(type);
                            }
                        }
                    }
                }
            return list;
        }

        /// <summary>
        /// Action类型
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventItem"></param>
        /// <returns></returns>
        private Type GetUIActionType(ComponentBinding component, BindingEvent eventItem)
        {
            var type = eventItem.bindingTargetType.type;

            if (type != null)
            {
                Type typevalue = type;

                if (type.IsGenericType || type.BaseType.IsGenericType)
                {
                    var genericType = type;
                    if (!type.IsGenericType)
                        genericType = type.BaseType;

                    var argumentList = new List<Type>(genericType.GetGenericArguments());
                    Type[] arguments = null;
                    switch (eventItem.type)
                    {
                        case BindingType.Simple:
                            arguments = argumentList.ToArray();
                            break;
                        case BindingType.Full:
                            argumentList.Add(component.targetType);
                            arguments = argumentList.ToArray();
                            break;
                        default:
                            break;
                    }

                    if (arguments.Length == 1)
                    {
                        typevalue = typeof(Action<>).MakeGenericType(arguments);
                    }
                    else if (arguments.Length == 2)
                    {
                        typevalue = typeof(Action<,>).MakeGenericType(arguments);
                    }
                    else if (arguments.Length == 3)
                    {
                        typevalue = typeof(Action<,,>).MakeGenericType(arguments);
                    }
                    else if (arguments.Length == 4)
                    {
                        typevalue = typeof(Action<,,,>).MakeGenericType(arguments);
                    }
                }
                else
                {
                    if (eventItem.type == BindingType.Simple)
                    {
                        typevalue = typeof(Action);
                    }
                    else
                    {
                        typevalue = typeof(Action<>).MakeGenericType(component.targetType);
                    }
                }
                return typevalue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 协议说明
        /// </summary>
        /// <param name="sb"></param>
        private void CreateProtocal(SortedDictionary<string, string> protocals, StringBuilder sb)
        {
            sb.Append("\t"); sb.AppendLine("/*绑定协议");
            foreach (var item in protocals)
            {
                sb.Append("\t"); sb.AppendFormat(" * {0} : {1}\n", item.Key, item.Value);
            }
            sb.Append("\t"); sb.AppendLine(" */");
        }

        /// <summary>
        /// View部分代码生成
        /// </summary>
        /// <param name="sb"></param>
        private void CreateBinderClassScript(string span, ComponentBinding[] ComponentBindings, StringBuilder sb)
        {
            sb.Append(span); sb.AppendLine("/// <summary>");
            sb.Append(span); sb.AppendLine("/// 代码绑定");
            sb.Append(span); sb.AppendLine("/// </summary>");
            sb.Append(span); sb.AppendLine("public override void SetView(BindingView panel)");
            sb.Append(span); sb.AppendLine("{");
            sb.Append(span + "\t"); sb.AppendLine("base.SetView(panel);");
            sb.Append(span + "\t"); sb.AppendLine();


            sb.Append(span + "\t"); sb.AppendLine("#region Refs");
            for (int i = 0; i < ComponentBindings.Length; i++)
            {
                var ComponentBinding = ComponentBindings[i];
                sb.Append(span + "\t");
                sb.AppendLine(string.Format("{0} = GetRef<{1}>(\"{0}\");", ComponentBinding.name, TypeStringName(ComponentBinding.targetType)));
            }
            sb.Append(span + "\t"); sb.AppendLine("#endregion");
            sb.AppendLine();

            sb.Append(span + "\t"); sb.AppendLine("#region Props");
            for (int i = 0; i < ComponentBindings.Length; i++)
            {
                var ComponentBinding = ComponentBindings[i];
                var viewBindings = ComponentBinding.viewItems;
                for (int j = 0; j < viewBindings.Count; j++)
                {
                    var viewItem = viewBindings[j];
                    bool existValueChanged = false;
                    if (!string.IsNullOrEmpty(viewItem.changeEvent))
                        existValueChanged = true;
                    sb.Append(span + "\t");
                    if (viewItem.isMethod)
                    {
                        sb.AppendFormat("RegistValueChange<{0}>({1}.{2},\"{3}\"", TypeStringName(viewItem.bindingTargetType.type), ComponentBinding.name, viewItem.bindingTarget, viewItem.bindingSource);
                        if (existValueChanged)
                            sb.Append($",{ComponentBinding.name}.{viewItem.changeEvent}");
                        sb.AppendLine(");");
                    }
                    else
                    {
                        sb.AppendLine(string.Format("SetValue({2}.{0}, \"{1}\");", viewItem.bindingTarget, viewItem.bindingSource, ComponentBinding.name));
                        sb.Append(span + "\t");
                        sb.AppendFormat("RegistValueChange<{0}>(x => {1}.{2} = x,\"{3}\"", TypeStringName(viewItem.bindingTargetType.type), ComponentBinding.name, viewItem.bindingTarget, viewItem.bindingSource);
                        if (existValueChanged)
                            sb.Append($",{ComponentBinding.name}.{viewItem.changeEvent}");
                        sb.AppendLine(");");
                    }
                }
            }
            sb.Append(span + "\t"); sb.AppendLine("#endregion");
            sb.AppendLine();
            sb.Append(span + "\t"); sb.AppendLine("#region Events");
            for (int i = 0; i < ComponentBindings.Length; i++)
            {
                var ComponentBinding = ComponentBindings[i];
                var eventBindings = ComponentBinding.eventItems;
                for (int j = 0; j < eventBindings.Count; j++)
                {
                    var eventItem = eventBindings[j];
                    sb.Append(span + "\t");
                    switch (eventItem.type)
                    {
                        case BindingType.Simple:
                            sb.AppendFormat("RegistEvent({0}.{1}, \"{2}\");", ComponentBinding.name, eventItem.bindingTarget, eventItem.bindingSource);
                            break;
                        case BindingType.Full:
                            sb.AppendFormat("RegistEvent({0}.{1}, \"{2}\",{3});", ComponentBinding.name, eventItem.bindingTarget, eventItem.bindingSource, ComponentBinding.name);
                            break;
                        default:
                            break;
                    }
                    sb.AppendLine();
                }
            }
            sb.Append(span + "\t"); sb.AppendLine("#endregion");
            sb.Append(span); sb.AppendLine("}");
        }

        /// <summary>
        /// 编写内置vm
        /// </summary>
        /// <param name="ComponentBindings"></param>
        /// <param name="sb"></param>
        /// <returns></returns>
        public void CreateInerViewModel(string span, string binderName, ComponentBinding[] ComponentBindings, StringBuilder sb)
        {
            sb.Append(span); sb.AppendLine("/// <summary>");
            sb.Append(span); sb.AppendLine("/// 代码绑定");
            sb.Append(span); sb.AppendLine("/// </summary>");
            sb.Append(span); sb.AppendLine($"public abstract class VM : ViewModel");
            sb.Append(span); sb.AppendLine("{");
            GetViewModelProps(span + "\t", ComponentBindings, sb);

            bool needExpose = Array.Find(ComponentBindings, x => x.expose) != null;
            if (needExpose)
                CreateViewModelInnerBindings(span, ComponentBindings, sb, binderName);

            sb.Append(span); sb.AppendLine("}");
        }

        /// <summary>
        /// 创建内置暴露元素
        /// </summary>
        /// <param name="span"></param>
        /// <param name="bindings"></param>
        /// <param name="sb"></param>
        /// <param name="binderName"></param>
        private void CreateViewModelInnerBindings(string span, ComponentBinding[] bindings, StringBuilder sb, string binderName)
        {
            sb.Append(span); sb.AppendLine("\tpublic struct Binding");
            sb.Append(span); sb.AppendLine("\t{");
            for (int i = 0; i < bindings.Length; i++)
            {
                if (!bindings[i].expose)
                    continue;
                var name = bindings[i].name;
                sb.Append(span); sb.Append("\t\t"); sb.AppendFormat("public {0} {1};", TypeStringName(bindings[i].targetType), name); sb.AppendLine();
            }
            sb.Append(span); sb.AppendLine("\t}");
            sb.Append(span); sb.Append("\t"); sb.AppendLine($"public Binding binding;");
            sb.Append(span); sb.Append("\t"); sb.AppendLine("public override void OnAfterBinding(BindingView panel)");
            sb.Append(span); sb.Append("\t"); sb.AppendLine("{");
            sb.Append(span); sb.Append("\t\t"); sb.AppendLine("base.OnAfterBinding(panel);");
            sb.Append(span); sb.Append("\t\t"); sb.AppendLine($"var binder = panel.binder as {binderName};");
            for (int i = 0; i < bindings.Length; i++)
            {
                if (!bindings[i].expose)
                    continue;
                var name = bindings[i].name;
                sb.Append(span); sb.Append("\t\t"); sb.AppendLine(string.Format("binding.{0} = binder.{0};", name));
            }
            sb.Append(span); sb.AppendLine("\t}");
        }


        /// <summary>
        /// ViewModel部分代码生成
        /// </summary>
        /// <param name="protocals"></param>
        /// <param name="types"></param>
        /// <param name="sb"></param>
        /// 
        public string CreateViewModelScript(ComponentBinding[] ComponentBindings, string nameSpace, string className, string userName, string parentName)
        {
            var types = new List<Type>();
            var protocals = GetProtocals(ComponentBindings, types);
            var headString = CreateHead(userName, System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                "脚本由工具初步生成，",
                "不能再次生成覆盖.");
            var defaultNamespace = typeof(UIView).Namespace;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(headString);
            if (nameSpace != defaultNamespace)
                sb.AppendLine($"using {defaultNamespace};");
            sb.AppendLine("using System;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.Scripting;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}", nameSpace); sb.AppendLine();
            sb.AppendLine("{");
            sb.Append("\t"); sb.AppendLine("/// <summary>");
            sb.Append("\t"); sb.AppendLine("/// 模板");
            sb.Append("\t"); sb.AppendLine("/// <summary>");
            sb.Append("\t"); sb.AppendLine("[Preserve]");
            sb.Append("\t"); sb.AppendLine($"public class {className} : {parentName}");
            sb.Append("\t"); sb.AppendLine("{");
            if (!UISetting.Instance.useBinderVM)
                GetViewModelProps("\t\t", ComponentBindings, sb);
            sb.Append("\t"); sb.AppendLine("}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public void GetViewModelProps(string span, ComponentBinding[] ComponentBindings, StringBuilder sb)
        {
            for (int i = 0; i < ComponentBindings.Length; i++)
            {
                var binding = ComponentBindings[i];
                if (binding.viewItems != null && binding.viewItems.Count > 0)
                {
                    foreach (var viewItem in binding.viewItems)
                    {
                        sb.Append(span);
                        sb.AppendLine(string.Format("protected Var<{0}> m_{1} = new (\"{2}\");", TypeStringName(viewItem.bindingTargetType.type), viewItem.bindingSource.Replace(".", "_"), viewItem.bindingSource));
                    }
                }
                if (binding.eventItems != null && binding.eventItems.Count > 0)
                {
                    foreach (var eventItem in binding.eventItems)
                    {
                        sb.Append(span);
                        var type = TypeStringName(GetUIActionType(binding, eventItem));
                        var genArgs = type.Substring(typeof(Action).FullName.Length);
                        sb.AppendLine(string.Format("protected Evt{0} m_{1} = new (\"{2}\");", genArgs, eventItem.bindingSource.Replace(".", "_"), eventItem.bindingSource));
                    }
                }
            }
        }
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
            else if (optimizedTypeName.TryGetValue(type, out var optimizedName))
                return optimizedName;
            return typeName;
        }
    }
}
