using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UFrame.BridgeUI.Editors
{
    public class Menus
    {
        //private const string menu01 = "GameObject/Foundation/BridgeUI/添加引用脚本（主文件）";
        //private const string menu02 = "GameObject/Foundation/BridgeUI/添加引用脚本（子文件）";
        private const string menu03 = "CONTEXT/BindingReference/编辑绑定";

        //[MenuItem(menu01, priority = 5000, validate = false)]
        //private static void QuickCreateBindingReferenceScript()
        //{
        //    var transform = Selection.activeTransform;
        //    if (transform != null)
        //    {
        //        var component = transform.GetComponent<BridgeUI.Binding.BindingReference>();

        //        if (component == null)
        //        {
        //            var nameSpace = BridgeUI.UISetting.defultNameSpace;
        //            var scriptName = transform.name + "Reference";
        //            var fullName = string.IsNullOrEmpty(nameSpace) ? scriptName : nameSpace + "." + scriptName;
        //            var oldScript = BridgeUI.Utility.GameAssembly.GetType(fullName);
        //            if (oldScript != null)
        //            {
        //                if (typeof(Component).IsAssignableFrom(oldScript))
        //                {
        //                    transform.gameObject.AddComponent(oldScript);
        //                }
        //                else
        //                {
        //                    Debug.LogError("已经存在名为" + scriptName + "的非component脚本！！！");
        //                }
        //            }
        //            else
        //            {
        //                var scriptPath = string.Format("{0}/{1}/{2}.cs", BridgeUI.UISetting.script_path, transform.name, scriptName);
        //                CreateBindingReferenceScriptToPath(nameSpace, scriptName, scriptPath);
        //                AssetDatabase.Refresh();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        EditorUtility.DisplayDialog("提示", "按GameObject的名字创建引用代码", "OK");
        //    }
        //}

        //[MenuItem(menu02, priority = 5000, validate = false)]
        //private static void QuickCreateBindingReferenceScript_Sub()
        //{
        //    var transform = Selection.activeTransform;
        //    if (transform != null)
        //    {
        //        var component = transform.GetComponent<BridgeUI.Binding.BindingReference>();

        //        if (component == null)
        //        {
        //            var nameSpace = BridgeUI.UISetting.defultNameSpace;
        //            var scriptName = transform.name + "Reference";

        //            var fullName = string.IsNullOrEmpty(nameSpace) ? scriptName : nameSpace + "." + scriptName;
        //            var oldScript = BridgeUI.Utility.GameAssembly.GetType(fullName);
        //            if (oldScript != null)
        //            {
        //                if (typeof(Component).IsAssignableFrom(oldScript))
        //                {
        //                    transform.gameObject.AddComponent(oldScript);
        //                }
        //                else
        //                {
        //                    Debug.LogError("已经存在名为" + scriptName + "的非component脚本！！！");
        //                }
        //            }
        //            else
        //            {
        //                var scriptPath = string.Format("{0}/{1}/{2}.cs", BridgeUI.UISetting.script_path, transform.name, scriptName);

        //                var parentReference = transform.gameObject.GetComponentInParent<BridgeUI.Binding.BindingReference>();
        //                if (parentReference != null)
        //                {
        //                    var folder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(parentReference)));
        //                    scriptPath = string.Format("{0}/{1}.cs", folder, scriptName);
        //                }

        //                CreateBindingReferenceScriptToPath(nameSpace, scriptName, scriptPath);
        //                AssetDatabase.Refresh();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        EditorUtility.DisplayDialog("提示", "按GameObject的名字创建引用代码", "OK");
        //    }
        //}

        [MenuItem(menu03, false, 1)]
        public static void CONTEXT_PanelBase_LoadDefultViewModel(MenuCommand command)
        {
            var component = command.context as BindingReference;
            if (component != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(component.gameObject);
                if(string.IsNullOrEmpty(assetPath))
                {
                    EditorWindow.GetWindow<BindingWindow>().OpenWith(component);
                }
                else
                {
                    EditorUtility.DisplayDialog("error", "can`t edit prefab directly!", "ok");
                }
            }
        }

        private static void CreateBindingReferenceScriptToPath(string nameSpace, string scriptName, string scriptPath)
        {
            var folder = System.IO.Path.GetDirectoryName(scriptPath);

            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            var scriptStr = BindingReferenceEditor.CreateScript(nameSpace, scriptName, new List<ReferenceItem>());
            System.IO.File.WriteAllText(scriptPath, scriptStr);
        }
    }
}