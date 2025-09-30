//using System;
//using System.Reflection;
//using System.Globalization;
//using Microsoft.CSharp;
//using System.CodeDom;
//using System.CodeDom.Compiler;
//using System.Text;
//using UnityEditor;
//using UnityEngine;

//namespace UFrame.BridgeUI.Editors
//{
//    public class BuildEventDLL
//    {
//        //[MenuItem("BridgeUI/BuildEventDLL")]
//        [Obsolete]
//        public static void Build()
//        {
//            // 1.CSharpCodePrivoder
//            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();

//            // 2.ICodeComplier
//            ICodeCompiler objICodeCompiler = objCSharpCodePrivoder.CreateCompiler();

//            // 3.CompilerParameters
//            CompilerParameters objCompilerParameters = new CompilerParameters();
//            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
//            objCompilerParameters.ReferencedAssemblies.Add("UnityEngine.dll");
//            objCompilerParameters.GenerateExecutable = false;
//            objCompilerParameters.GenerateInMemory = true;
//            objCompilerParameters.OutputAssembly = "BridgeUI.Events";
//            // 4.CompilerResults
//            CompilerResults cr = objICodeCompiler.CompileAssemblyFromSource(objCompilerParameters, GenerateCode());
//            if (cr.Errors.HasErrors)
//            {
//               UnityEngine.Debug.LogError("编译错误：");
//                foreach (CompilerError err in cr.Errors)
//                {
//                    UnityEngine.Debug.LogError(err.ErrorText);
//                }
//            }
//        }

//        private static string GenerateCode()
//        {
//            var textPath = AssetDatabase.GUIDToAssetPath("7a63d88c1753c86499a45dd44289a840");
//            var scripts = System.IO.File.ReadAllText(textPath,Encoding.UTF8);
//            return scripts;
//        }
//    }
//}