using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.Text;
using UnityEngine.EventSystems;
namespace ScriptRefine
{
    public static class RefineUtility
    {
        static List<Type> basicTypes { get; set; }
        static List<string> InnerNamespaces { get; set; }
        static RefineUtility()
        {
            basicTypes = new List<Type> {
            typeof(int),
            typeof(short),
            typeof(long),
            typeof(double),
            typeof(float),
            typeof(decimal),
            typeof(string),
            typeof(bool)
        };

            InnerNamespaces = new List<string>
        {
            "System",
            "UnityEngine",
            "UnityEngine.UI",
            "UnityEngine.EventSystems",
            "UnityEngine.Events",
            "UnityEngine.Component",
            "UnityEngine.Behaviour",
            "UnityEngine.Object",
            "UnityEngine.Joint",
            "UnityEditor",
            "UnityEditor.UI"
        };
        }

        /// <summary>
        /// Copies value of <paramref name="sourceProperty"/> into <pararef name="destProperty"/>.
        /// </summary>
        /// <param name="destProperty">Destination property.</param>
        /// <param name="sourceProperty">Source property.</param>
        public static void CopyPropertyValue(SerializedProperty destProperty, SerializedProperty sourceProperty)
        {
            if (destProperty == null)
                throw new ArgumentNullException("destProperty");
            if (sourceProperty == null)
                throw new ArgumentNullException("sourceProperty");

            sourceProperty = sourceProperty.Copy();
            destProperty = destProperty.Copy();

            CopyPropertyValueSingular(destProperty, sourceProperty);

            if (sourceProperty.hasChildren)
            {
                int elementPropertyDepth = sourceProperty.depth;
                while (sourceProperty.Next(true) && destProperty.Next(true) && sourceProperty.depth > elementPropertyDepth)
                    CopyPropertyValueSingular(destProperty, sourceProperty);
            }
        }
        private static void CopyPropertyValueSingular(SerializedProperty destProperty, SerializedProperty sourceProperty)
        {
            switch (destProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    destProperty.boolValue = sourceProperty.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    destProperty.floatValue = sourceProperty.floatValue;
                    break;
                case SerializedPropertyType.String:
                    destProperty.stringValue = sourceProperty.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    destProperty.colorValue = sourceProperty.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    destProperty.objectReferenceValue = sourceProperty.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.Enum:
                    destProperty.enumValueIndex = sourceProperty.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    destProperty.vector2Value = sourceProperty.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    destProperty.vector3Value = sourceProperty.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    destProperty.vector4Value = sourceProperty.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    destProperty.rectValue = sourceProperty.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.Character:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    destProperty.animationCurveValue = sourceProperty.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    destProperty.boundsValue = sourceProperty.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    //!TODO: Amend when Unity add a public API for setting the gradient.
                    break;
            }
        }

        /// <summary>
        /// 生成新的脚本
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        internal static string GenerateNewScirpt(Type type, List<AttributeInfo> attributes, List<Argument> arguments, List<RefineItem> refineList)
        {
            //声明代码的部分
            CodeCompileUnit compunit = new CodeCompileUnit();

            CodeNamespace sample = new CodeNamespace(type.Namespace);
            compunit.Namespaces.Add(sample);

            //引用命名空间
            sample.Imports.Add(new CodeNamespaceImport("System"));
            //sample.Imports.Add(new CodeNamespaceImport("UnityEngine"));

            if (type.IsClass)
            {
                var cls = GenerateClass(type, attributes, arguments, refineList);
                sample.Types.Add(cls);//把这个类添加到命名空间 ,待会儿才会编译这个类
            }
            else if (type.IsEnum)
            {
                sample.Types.Add(GenerateEnum(type, arguments));
            }

            CSharpCodeProvider cprovider = new CSharpCodeProvider();
            StringBuilder fileContent = new StringBuilder();
            using (StringWriter sw = new StringWriter(fileContent))
            {
                cprovider.GenerateCodeFromCompileUnit(compunit, sw, new CodeGeneratorOptions());//想把生成的代码保存为cs文件
            }

            return fileContent.ToString();
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static CodeTypeDeclaration GenerateClass(Type type, List<AttributeInfo> attributes, List<Argument> arguments, List<RefineItem> refineList)
        {
            var className = type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf('`')) : type.Name;
            //在命名空间下添加一个类
            CodeTypeDeclaration wrapProxyClass = new CodeTypeDeclaration(className);
            if (type.IsGenericType)
            {
                var start = 116;
                var count = int.Parse(type.Name.Substring(type.Name.IndexOf('`') + 1, 1));

                for (int i = 0; i < count; i++)
                {
                    string p = ((char)(start++)).ToString();
                    wrapProxyClass.TypeParameters.Add(new CodeTypeParameter(p));
                }
            }

            if (type.BaseType != null)
            {
                wrapProxyClass.BaseTypes.Add(new CodeTypeReference(type.BaseType));// 如果需要的话 在这里声明继承关系 (基类 , 接口)
            }

            if (attributes != null)
            {
                wrapProxyClass.CustomAttributes = GenerateAttributeCollection(attributes);//添加一个Attribute到class上
            }

            foreach (var item in arguments)
            {
                System.CodeDom.CodeMemberField field = new CodeMemberField();
                field.Type = new CodeTypeReference(item.type);
                field.Name = item.name;
                field.Attributes = MemberAttributes.Public;
                field.CustomAttributes = GenerateAttributeCollection(item.attributes);
                if (!string.IsNullOrEmpty(item.defultValue) && item.type != null && Type.GetType(item.type) != null)
                {
                    var value = Convert.ChangeType(item.defultValue, Type.GetType(item.type));
                    if (Type.GetType(item.type) == typeof(float) && value.ToString() == "Infinity")
                    {
                        Debug.Log("Infinity to " + float.MaxValue);
                        value = float.MaxValue;
                    }
                    field.InitExpression = new CodePrimitiveExpression(value);
                }

                wrapProxyClass.Members.Add(field);
            }

            var innserItems = refineList.FindAll(x => x.type == type.FullName + "+" + x.name);

            if (innserItems != null)
            {
                foreach (var item in innserItems)
                {
                    var innerType = Assembly.Load(item.assemble).GetType(item.type);
                    CodeTypeDeclaration innerClass = null;
                    if (innerType.IsClass)
                    {
                        innerClass = GenerateClass(innerType, item.attributes, item.arguments, refineList);
                    }
                    else if (innerType.IsEnum)
                    {
                        innerClass = GenerateEnum(innerType, item.arguments);
                    }

                    if (innerClass != null)
                    {
                        wrapProxyClass.Members.Add(innerClass);
                    }
                }
            }
            return wrapProxyClass;
        }

        private static CodeAttributeDeclarationCollection GenerateAttributeCollection(List<AttributeInfo> attributes)
        {
            var collection = new CodeAttributeDeclarationCollection();
            foreach (var item in attributes)
            {
                var att = new CodeAttributeDeclaration(item.attribute);
                switch (item.attType)
                {
                    case AttributeInfo.SupportAttributes.RequireComponent:
                        for (int i = 0; i < item.values.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(item.values[i]))
                            {
                                var arg = new CodeAttributeArgument(new CodeTypeOfExpression(item.values[i]));
                                att.Arguments.Add(arg);
                            }
                        }
                        break;
                    case AttributeInfo.SupportAttributes.CreateAssetMenu:
                        for (int i = 0; i < item.keys.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(item.values[i]))
                            {
                                var arg = new CodeAttributeArgument();
                                arg.Name = item.keys[i];
                                arg.Value = new CodePrimitiveExpression(item.values[i]);
                                att.Arguments.Add(arg);
                            }
                        }
                        break;
                    default:
                        break;
                }
                collection.Add(att);
            }
            return collection;
        }

        private static CodeTypeDeclaration GenerateEnum(Type type, List<Argument> arguments)
        {
            CodeTypeDeclaration warpEnum = new CodeTypeDeclaration(type.Name);
            warpEnum.IsEnum = true;
            foreach (var item in arguments)
            {
                System.CodeDom.CodeMemberField field = new CodeMemberField();
                field.Type = new CodeTypeReference(item.type);
                field.Name = item.name;
                if (!string.IsNullOrEmpty(item.defultValue))
                {
                    var value = 0;
                    int.TryParse(item.defultValue, out value);
                    field.InitExpression = new CodePrimitiveExpression(value);
                }

                warpEnum.Members.Add(field);
            }
            return warpEnum;
        }

        /// <summary>
        /// 记录脚本的属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        public static void AnalysisAttributes(object[] atts, List<AttributeInfo> attributes)
        {
            foreach (var item in atts)
            {
                var att = new AttributeInfo();
                att.attribute = item.ToString();
                if (item is RequireComponent)
                {
                    att.attType = AttributeInfo.SupportAttributes.RequireComponent;
                    var req = item as RequireComponent;
                    att.values = new string[] { req.m_Type0 == null ? null : req.m_Type0.ToString(), req.m_Type1 == null ? null : req.m_Type1.ToString(), req.m_Type2 == null ? null : req.m_Type2.ToString() };
                }
                if (item is CreateAssetMenuAttribute)
                {
                    att.attType = AttributeInfo.SupportAttributes.CreateAssetMenu;
                    var create = item as CreateAssetMenuAttribute;
                    att.keys = new string[] { "fileName", "menuName", "order" };
                    att.values = new string[] { create.fileName, create.menuName, create.order == 0 ? null : create.order.ToString() };
                }
                attributes.Add(att);
            }
        }

        /// <summary>
        /// 将脚本上的变量记录
        /// </summary>
        /// <param name="behaiver"></param>
        /// <param name="arguments"></param>
        public static void AnalysisArguments(Type type, List<Argument> arguments)
        {
            if (type.IsClass)
            {
                AnalysisClassArguments(type, arguments);
            }
            else if (type.IsEnum)
            {
                AnalysisEnumArguments(type, arguments);
            }
        }
        /// <summary>
        /// 分析类中的参数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arguments"></param>
        private static void AnalysisClassArguments(Type type, List<Argument> arguments)
        {
            object instence = null;
            GameObject temp = null;

            if (type.IsSubclassOf(typeof(MonoBehaviour)) && !type.IsAbstract)
            {
                temp = new GameObject("temp");
                instence = temp.AddComponent(type);
            }
            else
            {
                try
                {
                    if (Array.Find(type.GetConstructors(), x => x.IsPublic) != null)
                    {
                        if (type.IsSubclassOf(typeof(ScriptableObject)))
                        {
                            instence = ScriptableObject.CreateInstance(type);
                        }
                        else
                        {
                            instence = System.Activator.CreateInstance(type);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.GetField | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var item in fields)
            {
                if (!IsFieldNeed(item)) continue;

                if (item.FieldType.IsValueType || item.FieldType.IsEnum || item.FieldType.IsClass)
                {
                    var variable = CreateArgument(item, instence);
                    arguments.Add(variable);
                }
            }
            if (temp != null)
            {
                UnityEngine.Object.DestroyImmediate(temp);
            }

        }

        /// <summary>
        /// 判断寡字段能否序列化
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool IsFieldNeed(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;

            //排除字典
            if (type.IsGenericType && type.Name.Contains("Dictionary`"))
            {
                return false;
            }

            //排除非公有变量
            if (fieldInfo.Attributes != FieldAttributes.Public)
            {
                var attrs = fieldInfo.GetCustomAttributes(false);
                if (attrs.Length == 0 || (attrs.Length > 0 && Array.Find(attrs, x => x is SerializeField) == null))
                {
                    return false;
                }
            }

            //排出接口
            if (type.IsInterface)
            {
                return false;
            }

            //修正type
            if (type.IsArray || type.IsGenericType)
            {
                if (type.IsGenericType)
                {
                    type = type.GetGenericArguments()[0];
                }
                else
                {
                    type = type.GetElementType();
                }
            }

            //排出修正后的接口
            if (type.IsInterface)
            {
                return false;
            }

            //排除不能序列化的类
            if (type.IsClass)
            {
                if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    var atts = type.GetCustomAttributes(false);
                    var seri = Array.Find(atts, x => x is System.SerializableAttribute);
                    if (seri == null)
                    {
                        return false;
                    }
                }
            }

            //排除内置变量
            if (fieldInfo.Name.Contains("k__BackingField"))
            {
                return false;
            }

            return true;
        }
        internal static bool IsInternalScript(Type type)
        {
            if (type == null) return false;
            return InnerNamespaces.Contains(type.Namespace);
        }
        internal static bool IsMonoBehaiverOrScriptObjectRuntime(MonoScript mono)
        {
            if (mono != null && mono.GetClass() != null)
            {
                if (IsInternalScript(mono.GetClass()))
                {
                    return false;
                }
                else if (mono.GetClass().IsSubclassOf(typeof(Editor)))
                {
                    return false;
                }
                else if (mono.GetClass().IsSubclassOf(typeof(MonoBehaviour)) || mono.GetClass().IsSubclassOf(typeof(ScriptableObject)))
                {
                    return true;
                }
            }
            return false;
        }

        private static void AnalysisEnumArguments(Type type, List<Argument> arguments)
        {
            FieldInfo[] fieldInfo = type.GetFields();
            foreach (var item in fieldInfo)
            {
                if (item.Name != "value__")
                {
                    //var old = arguments.Find(x => x.name == item.Name);
                    var arg = new Argument();
                    arg.name = item.Name;
                    arg.defultValue = Convert.ToString(((int)item.GetValue(null)));
                    arguments.Add(arg);
                }
            }
        }

        internal static void ExportScripts(string path, List<RefineItem> refineList)
        {
            for (int i = 0; i < refineList.Count; i++)
            {
                var item = refineList[i];
                if (item.type.Contains("+")) continue;
                string typeName = item.type;
                if (item.type.Contains("`"))
                {
                    var count = int.Parse(typeName.Substring(typeName.IndexOf('`') + 1, 1));
                    typeName = typeName.Remove(typeName.IndexOf('`')) + count.ToString();
                }
                var scriptPath = path + "\\" + typeName.Replace(".", "\\") + ".cs";
                var dir = System.IO.Path.GetDirectoryName(scriptPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var metaPath = scriptPath + ".meta";
                var type = Assembly.Load(item.assemble).GetType(item.type);
                if (type == null)
                {
                    type = Type.GetType(item.type);
                }
                if (type == null)
                {
                    Debug.Log(item.type + ":empty");
                    continue;
                }

                var newScript = RefineUtility.GenerateNewScirpt(type, item.attributes, item.arguments, refineList);
                System.IO.File.WriteAllText(scriptPath, newScript);
                if (!string.IsNullOrEmpty(item.metaFilePath))
                {
                    var metaFile = System.IO.File.ReadAllText(item.metaFilePath);
                    System.IO.File.WriteAllText(metaPath, metaFile);
                }

            }
        }

        /// <summary>
        /// 反射生成Argument
        /// </summary>
        /// <param name="item"></param>
        /// <param name="behaiver"></param>
        /// <returns></returns>
        private static Argument CreateArgument(FieldInfo item, object defult = null)
        {
            Argument arg = new Argument();
            arg.name = item.Name;
            var type = item.FieldType;
            arg.type = type.ToString();
            arg.typeAssemble = type.Assembly.ToString();

            arg.attributes = new List<AttributeInfo>();
            AnalysisAttributes(item.GetCustomAttributes(false), arg.attributes);

            if (defult != null && basicTypes.Contains(type))
            {
                arg.defultValue = item.GetValue(defult) == null ? null : item.GetValue(defult).ToString();
            }

            arg.subType = "";

            if (type.IsClass || type.IsEnum || type.IsArray || type.IsGenericType)
            {
                if (type.IsArray || (!basicTypes.Contains(type) && !InnerNamespaces.Contains(type.Namespace)))
                {
                    if (type.IsGenericType)
                    {
                        var arrayType = type.GetGenericArguments()[0];
                        if (!basicTypes.Contains(arrayType) && !InnerNamespaces.Contains(arrayType.Namespace))
                        {
                            arg.subType = arrayType.ToString();
                            arg.subAssemble = arrayType.Assembly.ToString();
                        }
                    }
                    else if (type.IsArray)
                    {
                        var arrayType = type.GetElementType();
                        if (!basicTypes.Contains(arrayType) && !InnerNamespaces.Contains(arrayType.Namespace))
                        {
                            arg.subType = arrayType.ToString();
                            arg.subAssemble = arrayType.Assembly.ToString();
                        }
                    }
                    else
                    {
                        arg.subType = type.ToString();
                        arg.subAssemble = type.Assembly.ToString();
                    }
                }
            }


            return arg;
        }


    }
}