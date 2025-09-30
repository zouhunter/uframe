using System;
using UnityEngine;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UFrame.BridgeUI.Editors
{
    public class MVVMUIBindingViewCoderExecuter : ViewCoderExecuter
    {
        public const string keyword_address = "keyword_";

        public MVVMUIBindingViewCoderExecuter(ViewCoder viewCoder) : base(viewCoder) { }

        public override void AnalysisBinding(GameObject gameObject, ComponentItem[] componentItems)
        {
            var scriptPath = GenCodeUtil.InitScriptPath(gameObject, "Internal");

            if (System.IO.File.Exists(scriptPath))
            {
                var script = System.IO.File.ReadAllText(scriptPath, System.Text.Encoding.UTF8);

                if (componentItems != null)
                {
                    //解析componentItem
                    var bindingMembers = new List<string>();
                    var bindingEvents = new List<string>();
                    GetInvocations(script, ref bindingMembers, ref bindingEvents);
                    bindingMembers.ForEach(x => AnalysisBindingMembers(x, componentItems));
                    bindingEvents.ForEach(x => AnalysisBindingEvents(x, componentItems));
                }
            }
            else
            {
                Debug.Assert(System.IO.File.Exists(scriptPath), "未找到：" + scriptPath);
            }
        }

        /// <summary>
        /// 分析代码中的绑定信息
        /// </summary>
        /// <param name="scripts"></param>
        /// <param name="bindingMembers"></param>
        /// <param name="bindingEvents"></param>
        private static void GetInvocations(string scripts, ref List<string> bindingMembers, ref List<string> bindingEvents)
        {
            var reg_0 = @"Binder.RegistValueChange<.*>\((.*)\);";
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


            var reg_1 = @"Binder.RegistEvent\((.*)\);";
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

        }

        /// <summary>
        /// 分析代码中的绑定信息
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="components"></param>
        private static void AnalysisBindingMembers(string invocation, ComponentItem[] components)
        {
            var arguments = invocation.Replace(" ", "").Split(',');
            bool isMethod;
            var lamdaArgs = AnalysisTargetFromLamdaArgument(arguments[0], out isMethod);
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var sourceName = arguments[1].Replace(keyword_address, "");
                var targetName =  lamdaArgs[1];
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
                    var type = GenCodeUtil.GetTypeClamp(component.componentType, targetName);
                    info.bindingTargetType.Update(type);
                }
            }
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
        private static void AnalysisBindingEvents(string invocation, ComponentItem[] components)
        {
            var arguments = invocation.Replace(" ", "").Split(',');
            var arg0s = arguments[0].Split('.');
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var targetName = arg0s[1];
                var sourceName = arguments[1].Replace(keyword_address, "");

                if (component.name == arg0s[0])
                {
                    Type infoType = GenCodeUtil.GetTypeClamp(component.componentType, targetName);
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
        /// 生成代码
        /// </summary>
        /// <returns></returns>
        public override string GenerateScript()
        {
            var nameSpace = string.IsNullOrEmpty(this.nameSpace) ? UISetting.defultNameSpace : this.nameSpace;
            var types = new List<Type>();
            var protocals = GetProtocals(types);
            var parentName = string.IsNullOrEmpty(parentClassName) ? "BindingViewBaseComponent" : parentClassName;
            var headString = viewCoder.CreateHead(UISetting.userName, System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                "1.脚本由工具生成，",
                "2.可能被复写，",
                "3.请使用继承方式使用。");
            var bridgeUINameSpace = typeof(BridgeUI.UIBindingController).Namespace;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UFrame.BridgeUI;");
            sb.AppendLine();
            sb.AppendLine(headString);
            sb.AppendFormat("namespace {0}", nameSpace); sb.AppendLine();
            sb.AppendLine("{");
            CreateProtocal(protocals, sb);
            string classHead = "public class {0}:{1}";
            if (parentName.Equals(typeof(UFrame.BridgeUI.ViewBase).FullName))
            {
                classHead = "public abstract class {0}:{1}";
            }
            sb.Append("\t"); sb.AppendFormat(classHead, className, parentName); sb.AppendLine();
            sb.Append("\t"); sb.AppendLine("{");
            for (int i = 0; i < protocals.Count; i++)
            {
                sb.Append("\t\t"); sb.AppendFormat("public const byte {0} = {1};", GetKeyword(protocals[i]), (i + 1).ToString("000")); sb.AppendLine();
            }
            sb.AppendLine();
            CreateViewScript(sb);
            sb.AppendLine();
            CreateViewModelScript(protocals, types, sb);
            sb.Append("\t"); sb.AppendLine("}");
            sb.AppendLine("}");
            var compiledScript = sb.ToString();
            return compiledScript;
        }

        /// <summary>
        /// 关键字
        /// </summary>
        /// <param name="bindingSource"></param>
        /// <returns></returns>
        private string GetKeyword(string bindingSource)
        {
            return keyword_address + bindingSource;
        }

        /// <summary>
        /// 协议字符串
        /// </summary>
        /// <param name="typeList"></param>
        /// <returns></returns>
        private List<string> GetProtocals(List<Type> typeList = null)
        {
            var list = new List<string>();

            if (componentItems != null)
                for (int i = 0; i < componentItems.Length; i++)
                {
                    var componentItem = componentItems[i];
                    var viewBindings = componentItem.viewItems;
                    for (int j = 0; j < viewBindings.Count; j++)
                    {
                        var viewItem = viewBindings[j];
                        var bindingKey = viewItem.bindingSource;
                        if (!list.Contains(bindingKey))
                        {
                            list.Add(bindingKey);
                            if (typeList != null)
                            {
                                typeList.Add(viewItem.bindingTargetType.type);
                            }
                        }
                    }
                    var eventBindings = componentItem.eventItems;
                    for (int j = 0; j < eventBindings.Count; j++)
                    {
                        var eventItem = eventBindings[j];
                        var bindingKey = eventItem.bindingSource;
                        if (!list.Contains(bindingKey))
                        {
                            list.Add(bindingKey);
                            if (typeList != null)
                            {
                                var type = GetPanelActionType(componentItem, eventItem);
                                typeList.Add(type);
                            }
                        }
                    }
                }

            if (innerFields != null)
                for (int i = 0; i < innerFields.Length; i++)
                {
                    var field = innerFields[i];
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    {
                        var fieldName = field.Name;
                        if (!list.Contains(fieldName))
                        {
                            list.Add(fieldName);
                            if (typeList != null)
                            {
                                typeList.Add(field.FieldType);
                            }
                        }
                    }
                }
            return list;
        }

        /// <summary>
        /// PanelAction类型
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventItem"></param>
        /// <returns></returns>
        private Type GetPanelActionType(ComponentItem component, BindingEvent eventItem)
        {
            var type = eventItem.bindingTargetType.type;

            if (type != null)
            {
                Type typevalue = type;

                if (type.BaseType.IsGenericType)
                {
                    var argumentList = new List<Type>(type.BaseType.GetGenericArguments());
                    Type[] arguments = null;
                    switch (eventItem.type)
                    {
                        case BindingType.Simple:
                            arguments = argumentList.ToArray();
                            break;
                        case BindingType.Full:
                            argumentList.Add(component.componentType);
                            arguments = argumentList.ToArray();
                            break;
                        default:
                            break;
                    }

                    if (arguments.Length == 1)
                    {
                        typevalue = typeof(PanelAction<>).MakeGenericType(arguments);
                    }
                    else if (arguments.Length == 2)
                    {
                        typevalue = typeof(PanelAction<,>).MakeGenericType(arguments);
                    }
                    else if (arguments.Length == 3)
                    {
                        typevalue = typeof(PanelAction<,,>).MakeGenericType(arguments);
                    }
                    else if (arguments.Length == 4)
                    {
                        typevalue = typeof(PanelAction<,,,>).MakeGenericType(arguments);
                    }
                }
                else
                {
                    if (eventItem.type == BindingType.Simple)
                    {
                        typevalue = typeof(PanelAction);
                    }
                    else
                    {
                        typevalue = typeof(PanelAction<>).MakeGenericType(component.componentType);
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
        private void CreateProtocal(List<string> protocals, StringBuilder sb)
        {
            sb.Append("\t"); sb.AppendLine("/*绑定协议");
            for (int i = 0; i < protocals.Count; i++)
            {
                sb.Append("\t"); sb.AppendFormat(" * {0} : {1}\n", (i + 1).ToString("000"), protocals[i]);
            }
            sb.Append("\t"); sb.AppendLine(" */");
        }
        /// <summary>
        /// View部分代码生成
        /// </summary>
        /// <param name="sb"></param>
        private void CreateViewScript(StringBuilder sb)
        {
            var uicontrols = componentItems.Where(x => typeof(IUIControl).IsAssignableFrom(x.componentType)).ToArray();

            sb.Append("\t\t"); sb.AppendLine("/// <summary>");
            sb.Append("\t\t"); sb.AppendLine("/// 代码绑定");
            sb.Append("\t\t"); sb.AppendLine("/// </summary>");
            sb.Append("\t\t"); sb.AppendLine("protected override void OnBinding(UnityEngine.GameObject target)");
            sb.Append("\t\t"); sb.AppendLine("{");
            sb.Append("\t\t\t"); sb.AppendLine("base.OnBinding(target);");
            sb.Append("\t\t\t"); sb.AppendLine("var binding = target.GetComponent<UIBinding>();");
            sb.Append("\t\t\t"); sb.AppendLine("if (binding != null)");
            sb.Append("\t\t\t"); sb.AppendLine("{");
            FindElementBlock(sb,4);
            for (int i = 0; i < componentItems.Length; i++)
            {
                var componentItem = componentItems[i];
                var viewBindings = componentItem.viewItems;
                for (int j = 0; j < viewBindings.Count; j++)
                {
                    var viewItem = viewBindings[j];
                    sb.Append("\t\t\t\t");
                    sb.AppendFormat("Binder.RegistValueChange<{0}>(x => {1}.{2} = x, {3});", GenCodeUtil.TypeStringName(viewItem.bindingTargetType.type), componentItem.name, viewItem.bindingTarget, GetKeyword(viewItem.bindingSource));
                    sb.AppendLine();
                }
                var eventBindings = componentItem.eventItems;
                for (int j = 0; j < eventBindings.Count; j++)
                {
                    var eventItem = eventBindings[j];
                    sb.Append("\t\t\t\t");
                    switch (eventItem.type)
                    {
                        case BindingType.Simple:
                            sb.AppendFormat("Binder.RegistEvent({0}.{1}, {2});", componentItem.name, eventItem.bindingTarget, GetKeyword(eventItem.bindingSource));
                            break;
                        case BindingType.Full:
                            sb.AppendFormat("Binder.RegistEvent({0}.{1}, {2}, {3});", componentItem.name, eventItem.bindingTarget, GetKeyword(eventItem.bindingSource), componentItem.name);
                            break;
                        default:
                            break;
                    }

                    sb.AppendLine();
                }
            }

            if (innerFields != null)
            {
                for (int i = 0; i < innerFields.Length; i++)
                {
                    var field = innerFields[i];
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    {
                        sb.Append("\t\t\t\t");
                        sb.AppendFormat("Binder.SetValue({0}, {1});", field.Name, GetKeyword(field.Name));
                        sb.AppendLine();
                    }
                }
            }


            if (uicontrols != null && uicontrols.Length > 0)
            {
                for (int i = 0; i < uicontrols.Length; i++)
                {
                    var item = uicontrols[i];
                    sb.Append("\t\t\t\t");
                    sb.AppendFormat("RegistUIControl({0});", item.name);
                    sb.AppendLine();
                }
            }


            sb.Append("\t\t\t"); sb.AppendLine("}");
            sb.Append("\t\t"); sb.AppendLine("}");
        }


        /// <summary>
        /// 元素查找
        /// </summary>
        private void FindElementBlock(StringBuilder sb, int deepth)
        {
            for (int i = 0; i < referenceItems.Length; i++)
            {
                var refItem = referenceItems[i];
                Span(sb, deepth);
                var funcName = "GetValue";
                if (typeof(UnityEngine.Object).IsAssignableFrom(refItem.type))
                {
                    funcName = "GetRef";
                }
                refItem.typeFullName = refItem.type.FullName;
                if (refItem.isArray)
                {
                    sb.AppendLine(string.Format("var {0} = binding.{1}s<{2}>(\"{0}\");", refItem.name, funcName, refItem.typeFullName));
                }
                else
                {
                    sb.AppendLine(string.Format("var {0} = binding.{1}<{2}>(\"{0}\");", refItem.name, funcName, refItem.typeFullName));
                }
            }
        }

        private void Span(StringBuilder sb, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append("\t");
            }
        }

        /// <summary>
        /// ViewModel部分代码生成
        /// </summary>
        /// <param name="protocals"></param>
        /// <param name="types"></param>
        /// <param name="sb"></param>
        private void CreateViewModelScript(List<string> protocals, List<Type> types, StringBuilder sb)
        {
            var bridgeUINameSpace = typeof(BridgeUI.UIBindingController).Namespace;

            sb.Append("\t\t"); sb.AppendLine("/// <summary>");
            sb.Append("\t\t"); sb.AppendLine("/// 显示模型模板");
            sb.Append("\t\t"); sb.AppendLine("/// <summary>");
            sb.Append("\t\t"); sb.AppendLine("public class LogicBase : ViewModel");
            sb.Append("\t\t"); sb.AppendLine("{");
            sb.Append("\t\t\t"); sb.AppendLine("#region BindablePropertys");
            for (int i = 0; i < protocals.Count; i++)
            {
                var protocal = protocals[i];
                var typeName = GenCodeUtil.TypeStringName(types[i]);
                sb.Append("\t\t\t");
                sb.AppendFormat("private BindableProperty<{0}> _{1};", typeName, protocal);
                sb.AppendLine();
            }
            sb.Append("\t\t\t"); sb.AppendLine("#endregion BindablePropertys");
            sb.AppendLine();
            sb.Append("\t\t\tpublic LogicBase(){");
            sb.AppendLine();
            for (int i = 0; i < protocals.Count; i++)
            {
                var protocal = protocals[i];
                var typeName = GenCodeUtil.TypeStringName(types[i]);
                sb.Append("\t\t\t\t"); sb.AppendFormat("_{0} = GetBindableProperty<{1}>({2});", protocal, typeName, GetKeyword(protocal)); sb.AppendLine();
            }
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.Append("\t\t\t"); sb.AppendLine("#region Propertys");
            for (int i = 0; i < protocals.Count; i++)
            {
                var protocal = protocals[i];
                var typeName = GenCodeUtil.TypeStringName(types[i]);
                sb.Append("\t\t\t"); sb.AppendFormat("public {0} m_{1} {{ get => _{1}.Value; set=> _{1}.Value = value; }}", typeName,protocal); sb.AppendLine();
            }

            sb.Append("\t\t\t"); sb.AppendLine("#endregion Propertys");
            sb.Append("\t\t"); sb.AppendLine("}");
        }

        /// <summary>
        /// 从关键字解析
        /// </summary>
        /// <param name="sourceKey"></param>
        /// <returns></returns>
        internal static string FromSourceKey(string sourceKey)
        {
            if (sourceKey.Contains(keyword_address))
            {
                return sourceKey.Replace(keyword_address, "");
            }
            if (sourceKey.Contains("\""))
            {
                sourceKey = sourceKey.Replace("\"", "");
            }
            return sourceKey;
        }

    }
}