using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace UFrame.BridgeUI.Editors
{
    public class UIBindingViewCoderExecuter : ViewCoderExecuter
    {
        private string view_binding_partton = @"(\w+)\s*\{\s*set\s*=>\s*m_(\w+).(\w+)\s*=\s*(\w+)";
        private string view_binding_func_partton = @"(\w+)\s*\{\s*set\s*=>\s*m_(\w+)\?.(\w+)\((\w+)\);";
        private string event_binding_partton = @"m_(\w+).(\w+).AddListener\(On(\w+)\)";

        public UIBindingViewCoderExecuter(ViewCoder coder) : base(coder) { }

        public override void AnalysisBinding(GameObject gameObject, ComponentItem[] componentItems)
        {
            if (componentItems == null)
            {
                Debug.LogError("AnalysisBinding componetItems == null");
                return;
            }
            var scriptPath = GenCodeUtil.InitScriptPath(gameObject, "Internal");

            if (System.IO.File.Exists(scriptPath))
            {
                var script = System.IO.File.ReadAllText(scriptPath, System.Text.Encoding.UTF8);
                //解析componentItem
                AnalysisBindingMembers(script, componentItems);
                AnalysisBindingEvents(script.Replace(" ", ""), componentItems);
            }
            else
            {
                Debug.Assert(System.IO.File.Exists(scriptPath), "未找到：" + scriptPath);
            }
        }


        private void AnalysisBindingMembers(string script, ComponentItem[] componentItems)
        {
            var matchs_0 = Regex.Matches(script, view_binding_partton);

            for (int i = 0; i < matchs_0.Count; i++)
            {
                var match = matchs_0[i];
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var componentName = match.Groups[2].Value;
                    var targetName = match.Groups[3].Value;
                    AnalysisBindingMembers(componentName, targetName, key, false, componentItems);
                }
            }

            var matchs_1 = Regex.Matches(script, view_binding_func_partton);

            for (int i = 0; i < matchs_1.Count; i++)
            {
                var match = matchs_1[i];
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    var componentName = match.Groups[2].Value;
                    var targetName = match.Groups[3].Value;
                    AnalysisBindingMembers(componentName, targetName, key, true, componentItems);
                }
            }
        }

        private void AnalysisBindingMembers(string componentName, string targetName, string sourceName, bool isMethod, ComponentItem[] components)
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component.name == componentName)
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

        private void AnalysisBindingEvents(string script, ComponentItem[] componentItems)
        {
            var matchs_0 = Regex.Matches(script, event_binding_partton);
            for (int i = 0; i < matchs_0.Count; i++)
            {
                var match = matchs_0[i];
                if (match.Success)
                {
                    var componentName = match.Groups[1].Value;
                    var targetName = match.Groups[2].Value;
                    var key = match.Groups[3].Value;
                    AnalysisBindingEvents(componentName, targetName, key, componentItems);
                }
                else
                {
                    Debug.LogError("match.Success == false:" + event_binding_partton);
                }
            }
        }

        private void AnalysisBindingEvents(string componentName, string targetName, string sourceName, ComponentItem[] components)
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component.name == componentName)
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
                    info.type = BindingType.Simple;
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
            //var types = new List<Type>();
            var headString = viewCoder.CreateHead(UISetting.userName, System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                "此代码由工具生成",
                "不支持编写的代码",
                "请使用继承方式使用");
            var bridgeUINameSpace = typeof(BridgeUI.UIBindingController).Namespace;
            StringBuilder sb = new StringBuilder();
            sb.Append(headString);

            if (!string.IsNullOrEmpty(headString))
                sb.AppendLine();

            sb.AppendLine("using System;");
            sb.AppendLine("using " + bridgeUINameSpace + ";");
            sb.AppendLine("using UnityEngine;");

            sb.AppendFormat("namespace {0}", nameSpace); sb.AppendLine();
            sb.AppendLine("{");
            NameSpaceBlock(sb, 1);
            sb.AppendLine("}");
            var compiledScript = sb.ToString();
            return compiledScript;
        }

        /// <summary>
        /// 命名空间包括
        /// </summary>
        /// <param name="sb"></param>
        private void NameSpaceBlock(StringBuilder sb, int deepth)
        {
            var parentName = string.IsNullOrEmpty(parentClassName) ? "ViewBase" : parentClassName;

            Span(sb, deepth); sb.AppendLine("/// <summary>");
            Span(sb, deepth); sb.AppendLine("/// 模板方法");
            Span(sb, deepth); sb.AppendLine("/// <summary>");

            Span(sb, deepth);
            if (componentItems != null && componentItems.Length > 0)
                sb.AppendFormat("public abstract class {0}:{1}", className, parentName);
            else
                sb.AppendFormat("public class {0}:{1}", className, parentName);
            sb.AppendLine();
            Span(sb, deepth); sb.AppendLine("{");
            ClassBlock(sb, deepth + 1);
            Span(sb, deepth); sb.AppendLine("}");
        }

        /// <summary>
        /// 类包括
        /// </summary>
        /// <param name="sb"></param>
        private void ClassBlock(StringBuilder sb, int deepth)
        {
            if (subReferences != null)
            {
                for (int i = 0; i < subReferences.Length; i++)
                {
                    Span(sb, deepth);
                    var prop = subReferences[i];
                    var referenceItem = prop.GetValue(target) as BindingReference;
                    var classType = referenceItem.LoadViewScriptType();
                    sb.AppendLine(string.Format("protected {0} {1};", classType.FullName, prop.Name + "View"));
                }
            }

            if (referenceItems != null && referenceItems.Length > 0)
            {
                HashSet<string> elementCreated = new HashSet<string>();
                for (int i = 0; i < referenceItems.Length; i++)
                {
                    var refItem = referenceItems[i];
                    if (string.IsNullOrEmpty(refItem.typeFullName) && refItem.type != null)
                        refItem.typeFullName = refItem.type.FullName;
                    if (string.IsNullOrEmpty(refItem.typeFullName))
                        continue;
                    if (elementCreated.Contains(refItem.name))
                        continue;
                    elementCreated.Add(refItem.name);
                    Span(sb, deepth);
                    if (refItem.isArray)
                    {
                        sb.AppendLine(string.Format("protected {0}[] m_{1};", refItem.typeFullName, refItem.name));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("protected {0} m_{1};", refItem.typeFullName, refItem.name));
                    }
                }
            }

            //sb.AppendLine();
            if (componentItems != null)
            {
                Span(sb, deepth); sb.AppendLine();
                SetActionsFunctions(sb, deepth);

                sb.AppendLine();
                Span(sb, deepth); sb.AppendLine("/// <summary>");
                Span(sb, deepth); sb.AppendLine("/// 元素查找");
                Span(sb, deepth); sb.AppendLine("/// </summary>");
                Span(sb, deepth); sb.AppendLine("protected virtual void FindElements(UIBinding binding)");
                Span(sb, deepth); sb.AppendLine("{");
                FindElementBlock(sb, deepth + 1);
                Span(sb, deepth); sb.AppendLine("}");
                sb.AppendLine();

                sb.AppendLine();
                Span(sb, deepth); sb.AppendLine("/// <summary>");
                Span(sb, deepth); sb.AppendLine("/// 绑定");
                Span(sb, deepth); sb.AppendLine("/// </summary>");
                Span(sb, deepth); sb.AppendLine("protected override void OnBinding(UnityEngine.GameObject target)");
                Span(sb, deepth); sb.AppendLine("{");
                OnBindingBlock(sb, deepth + 1);
                Span(sb, deepth); sb.AppendLine("}");
                sb.AppendLine();

                Span(sb, deepth); sb.AppendLine("/// <summary>");
                Span(sb, deepth); sb.AppendLine("/// 解绑定");
                Span(sb, deepth); sb.AppendLine("/// </summary>");
                Span(sb, deepth); sb.AppendLine("protected override void OnUnBinding()");
                Span(sb, deepth); sb.AppendLine("{");
                OnUnBindingBlock(sb, deepth + 1);
                Span(sb, deepth); sb.AppendLine("}");
                Span(sb, deepth); sb.AppendLine();
                AbstructFunctions(sb, deepth);
            }
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
                if (refItem.isArray)
                {
                    sb.AppendLine(string.Format("m_{0} = binding.{1}s<{2}>(\"{0}\");", refItem.name, funcName, refItem.typeFullName));
                }
                else
                {
                    sb.AppendLine(string.Format("m_{0} = binding.{1}<{2}>(\"{0}\");", refItem.name, funcName, refItem.typeFullName));
                }
            }
        }

        /// <summary>
        /// 绑定代码
        /// </summary>
        /// <param name="sb"></param>
        private void OnBindingBlock(StringBuilder sb, int deepth)
        {
            ComponentItem[] uicontrols = componentItems.Where(x => typeof(IUIControl).IsAssignableFrom(x.componentType)).ToArray();

            Span(sb, deepth); sb.AppendLine("base.OnBinding(target);");
            Span(sb, deepth); sb.AppendLine("var binding = target.GetComponent<UIBinding>();");
            Span(sb, deepth); sb.AppendLine("if (binding) FindElements(binding);");

            var deepth1 = deepth + 1;

            //if (subReferences != null)
            //{
            //    for (int i = 0; i < subReferences.Length; i++)
            //    {
            //        var propName = subReferences[i].Name;
            //        Span(sb, deepth1);
            //        var referenceItem = subReferences[i].GetValue(target) as BindingReference;
            //        var classType = referenceItem.LoadViewScriptType();
            //        sb.AppendLine(string.Format("{0} = BindingSubReference<{1}>(m_{2});", propName + "View", classType.FullName, propName));
            //    }
            //}

            for (int i = 0; i < componentItems.Length; i++)
            {
                var componentItem = componentItems[i];
                var eventBindings = componentItem.eventItems;
                for (int j = 0; j < eventBindings.Count; j++)
                {
                    var eventItem = eventBindings[j];
                    Span(sb, deepth);
                    sb.AppendFormat("if(m_{0}) m_{0}.{1}.AddListener(On{2});", componentItem.name, eventItem.bindingTarget, eventItem.bindingSource);
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
                        Span(sb, deepth1);
                        sb.AppendFormat("Set{0}(m_{1});", field.Name, field.Name);
                        sb.AppendLine();
                    }
                }
            }

            if (uicontrols != null && uicontrols.Length > 0)
            {
                for (int i = 0; i < uicontrols.Length; i++)
                {
                    var item = uicontrols[i];
                    Span(sb, deepth);
                    sb.AppendFormat("RegistUIControl(m_{0});", item.name);
                    sb.AppendLine();
                }
            }


            //Span(sb, deepth); sb.AppendLine("}");
        }

        /// <summary>
        /// 解绑代码
        /// </summary>
        /// <param name="sb"></param>
        private void OnUnBindingBlock(StringBuilder sb, int deepth)
        {
            //Span(sb, deepth); sb.AppendLine("if (m_binding != null)");
            //Span(sb, deepth); sb.AppendLine("{");

            var deepth1 = deepth + 1;

            //if (subReferences != null)
            //{
            //    for (int i = 0; i < subReferences.Length; i++)
            //    {
            //        Span(sb, deepth1);
            //        sb.AppendLine(string.Format("UnBindingSubReference({0}); ", subReferences[i].Name + "View"));
            //    }
            //}

            for (int i = 0; i < componentItems.Length; i++)
            {
                var componentItem = componentItems[i];
                var eventBindings = componentItem.eventItems;
                for (int j = 0; j < eventBindings.Count; j++)
                {
                    var eventItem = eventBindings[j];
                    Span(sb, deepth);
                    sb.AppendFormat("if(m_{0}) m_{0}.{1}.RemoveListener(On{2});", componentItem.name, eventItem.bindingTarget, eventItem.bindingSource);
                    sb.AppendLine();
                }
            }

            ComponentItem[] uicontrols = componentItems.Where(x => typeof(IUIControl).IsAssignableFrom(x.componentType)).ToArray();

            if (uicontrols != null && uicontrols.Length > 0)
            {
                for (int i = 0; i < uicontrols.Length; i++)
                {
                    var item = uicontrols[i];
                    Span(sb, deepth);
                    sb.AppendFormat("RemoveUIControl(m_{0});", item.name);
                    sb.AppendLine();
                }
            }

            //Span(sb, deepth); sb.AppendLine("}");
            Span(sb, deepth); sb.AppendLine("base.OnUnBinding();");
        }

        private void SetActionsFunctions(StringBuilder sb, int deepth)
        {
            Dictionary<string, Type> typeDic = new Dictionary<string, Type>();
            Dictionary<string, Dictionary<string, List<BindingShow>>> dic = new Dictionary<string, Dictionary<string, List<BindingShow>>>();

            for (int i = 0; i < componentItems.Length; i++)
            {
                var componentItem = componentItems[i];
                var viewBindings = componentItem.viewItems;

                for (int j = 0; j < viewBindings.Count; j++)
                {
                    var viewItem = viewBindings[j];
                    if (!dic.ContainsKey(viewItem.bindingSource))
                    {
                        dic[viewItem.bindingSource] = new Dictionary<string, List<BindingShow>>();
                    }

                    var innerDic = dic[viewItem.bindingSource];
                    if (!innerDic.ContainsKey(componentItem.name))
                    {
                        innerDic[componentItem.name] = new List<BindingShow>();
                    }

                    innerDic[componentItem.name].Add(viewItem);

                    if (viewItem.bindingTargetType.type == null)
                    {
                        Debug.LogError("empty bindingTargetType:" + viewItem.bindingSource);

                    }

                    if (!typeDic.ContainsKey(viewItem.bindingSource))
                    {
                        typeDic.Add(viewItem.bindingSource, viewItem.bindingTargetType.type);
                    }
                }
            }

            if (innerFields != null && innerFields.Length > 0)
            {
                for (int i = 0; i < innerFields.Length; i++)
                {
                    var item = innerFields[i];
                    if (!dic.ContainsKey(item.Name))
                    {
                        dic.Add(item.Name, null);
                    }
                    if (!typeDic.ContainsKey(item.Name))
                    {
                        typeDic.Add(item.Name, item.FieldType);
                    }
                }
            }

            if (dic.Count <= 0)
                return;

            //Span(sb, deepth);/* sb.AppendLine("#region Set_Functions")*/;
            using (var enumerator = dic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;
                    var innerDic = enumerator.Current.Value;
                    if (innerDic != null)
                    {
                        using (var innerEnumerator = innerDic.GetEnumerator())
                        {
                            while (innerEnumerator.MoveNext())
                            {
                                var target = innerEnumerator.Current.Key;
                                var bindingShows = innerEnumerator.Current.Value;
                                for (int i = 0; i < bindingShows.Count; i++)
                                {
                                    var bindingShow = bindingShows[i];
                                    if (bindingShow.isMethod)
                                    {
                                        //protected string Text_text1 { set => m_Text.SendMessage(value); }
                                        Span(sb, deepth); sb.AppendFormat("protected {0} {1} {{ set => m_{2}?.{3}(value); }} ", typeDic[key].FullName, key, target, bindingShow.bindingTarget); sb.AppendLine();
                                    }
                                    else
                                    {
                                        //protected string Text_text { get => m_Text.text; set => m_Text.text = value; }
                                        Span(sb, deepth); sb.AppendFormat("protected {0} {1} {{ set => m_{2}.{3} = value; get => m_{2}.{3}; }}", typeDic[key].FullName, key, target, bindingShow.bindingTarget); sb.AppendLine();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Span(sb, deepth); 
        }

        private void AbstructFunctions(StringBuilder sb, int deepth)
        {
            if (componentItems.Length <= 0)
                return;

            //Span(sb, deepth); /*sb.AppendLine("#region Abstruct_Functions")*/;
            for (int i = 0; i < componentItems.Length; i++)
            {
                var componentItem = componentItems[i];
                var eventBindings = componentItem.eventItems;
                for (int j = 0; j < eventBindings.Count; j++)
                {
                    var eventItem = eventBindings[j];
                    var argumentString = GetArgumentString(eventItem.bindingTargetType.type);
                    Span(sb, deepth);
                    sb.AppendFormat("protected abstract void On{0}({1});", eventItem.bindingSource, argumentString);
                    sb.AppendLine("");
                }
            }
            //Span(sb, deepth); /*sb.AppendLine("#endregion Abstruct_Functions")*/;
        }

        private string GetArgumentString(Type type)
        {
            if (type == null)
            {
                Debug.LogError("type error!");
                return "";
            }

            string argumentsStr = null;
            var method = type.GetMethod("AddListener");
            var parameter = method.GetParameters()[0];
            var genericArguments = parameter.ParameterType.GetGenericArguments();
            var arguments = new List<string>();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                arguments.Add(string.Format("{0} {1}", genericArguments[i].FullName, "arg" + i));
            }
            if (arguments.Count > 0)
            {
                argumentsStr = string.Join(",", arguments.ToArray());
            }
            return argumentsStr;
        }


        private void Span(StringBuilder sb, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append("\t");
            }
        }

    }
}