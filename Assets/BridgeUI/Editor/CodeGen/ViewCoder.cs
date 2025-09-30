using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UFrame.BridgeUI.Editors;

namespace UFrame.BridgeUI.Editors
{
    //生成与解析[name]_Internal.cs 
    //管理三种编码器
    public class ViewCoder
    {
        public MonoBehaviour target { get; set; }
        public string refClassName { get; set; }
        public string parentClassName { get; set; }
        public string className { get; set; }
        public string nameSpace { get; set; }
        public FieldInfo[] innerFields { get; set; }
        public ComponentItem[] componentItems { get; set; }
        public FieldInfo[] subReferences { get; set; }
        public ReferenceItem[] referenceItems { get; set; }
        public bool forbidHead { get; set; }
        public string path { get; set; }

        protected string compiledScript;
        public string ScriptValue
        {
            get
            {
                return compiledScript;
            }
        }

        private IViewCoderExecuter executer;

        public void AnalysisBinding(GameObject gameObject, Type bindingType, ComponentItem[] componentItems, GenCodeRule rule)
        {
            string fullViewName = null;
            if (bindingType == null)
            {
                fullViewName = rule.nameSpace + "." + gameObject.name + "Internal";
                bindingType = BridgeUI.Utility.FindTypeInAllAssemble(fullViewName);
            }
            else
            {
                while (bindingType.BaseType != null && Array.IndexOf(GenCodeUtil.supportBaseTypes, bindingType.BaseType.ToString()) < 0)
                {
                    bindingType = bindingType.BaseType;
                }
                if(bindingType != null)
                    fullViewName = bindingType.FullName;
            }
            if (bindingType != null)
            {
                rule.baseTypeIndex = Array.IndexOf(GenCodeUtil.supportBaseTypes, bindingType.BaseType.ToString());
                var refClass = gameObject.GetComponent<BindingReference>();
                refClassName = refClass.GetType().FullName;
                SwitchExecuter(bindingType, refClass?.GetType());
                executer.AnalysisBinding(gameObject, componentItems);
            }
            else
            {
                Debug.Log("can`t find bindingView:" + fullViewName);
            }

        }
        /// <summary>
        /// 编译代码
        /// </summary>
        /// <returns></returns>
        public void CompileSave()
        {
            if (string.IsNullOrEmpty(parentClassName)) return;
            var parentType = BridgeUI.Utility.FindTypeInAllAssemble(parentClassName);
            if (parentType == null)
            {
                Debug.LogError("can`t find :" + parentClassName);
                return;
            }
            var refClassType = target?.GetComponent<BindingReference>();
            SwitchExecuter(parentType, refClassType?.GetType());
            if (executer != null)
            {
                compiledScript = executer.GenerateScript();
                SaveToFile(compiledScript);
            }
        }

        private void SwitchExecuter(Type parentType,Type refClassType)
        {
            if (typeof(BindingViewBase).IsAssignableFrom(parentType))
            {
                if (refClassType == null || typeof(UIBinding).IsAssignableFrom(refClassType))
                {
                    executer = new MVVMUIBindingViewCoderExecuter(this);
                }
                else
                {
                    executer = new MVVMBindingViewCoderExecuter(this);
                }
            }
            else if (typeof(ViewBase).IsAssignableFrom(parentType))
            {
                if (refClassType == null || typeof(UIBinding).IsAssignableFrom(refClassType))
                {
                    executer = new UIBindingViewCoderExecuter(this);
                }
                else
                {
                    executer = new NormalBindingViewCoderExecuter(this);
                }
            }
            else if (typeof(SubView).IsAssignableFrom(parentType))
            {
                if (refClassType == null || typeof(UIBinding).IsAssignableFrom(refClassType))
                {
                    executer = new SubUIBindingViewCoderExecuter(this);
                }
                else
                {
                    executer = new SubBindingViewCoderExecuter(this);
                }
            }
            
            else if (typeof(ViewBaseComponent).IsAssignableFrom(parentType))
            {
                executer = new ComponentViewCoder(this); ;// componentViewCoder;
            }
            else
            {
                Debug.LogError("父类型暂不支持：" + parentType);
                return;
            }
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="scriptValue"></param>
        protected void SaveToFile(string scriptValue)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (string.IsNullOrEmpty(scriptValue)) return;
            scriptValue = scriptValue.Replace("\r\n", "\n");
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            System.IO.File.WriteAllText(path, scriptValue, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 创建代码头
        /// </summary>
        /// <param name="author"></param>
        /// <param name="time"></param>
        /// <param name="detailInfo"></param>
        /// <returns></returns>
        public string CreateHead(string author, string time, params string[] detailInfo)
        {
            if (forbidHead)
                return "";

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

    }
}