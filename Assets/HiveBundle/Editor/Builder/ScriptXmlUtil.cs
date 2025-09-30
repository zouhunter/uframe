using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace UFrame.HiveBundle
{
    public class ScriptXmlUtil
    {
        /// <summary>
        /// 收集资源用到的脚本
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="checkedAssets"></param>
        /// <param name="collectedTypes"></param>
        private static void CollectAssetScriptsRef(string assetPath, HashSet<string> checkedAssets, HashSet<Type> collectedTypes)
        {
            if (string.IsNullOrEmpty(assetPath))
                return;

            //文件夹类型的资源无法使用AssetDatabase.GetDependencies
            if (System.IO.Directory.Exists(assetPath))
            {
                var files = System.IO.Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (!file.EndsWith(".cs") && !file.EndsWith(".prefab") && !file.EndsWith(".unity"))
                        continue;
                    var subAssetPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file).Replace("\\", "/");
                    if (checkedAssets.Contains(subAssetPath))
                        continue;
                    var subGuid = AssetDatabase.AssetPathToGUID(subAssetPath);
                    var path = AssetDatabase.GUIDToAssetPath(subGuid);
                    CollectAssetScriptsRef(path, checkedAssets, collectedTypes);
                }
            }
            else
            {
                var allDepends = AssetDatabase.GetDependencies(assetPath, true);
                foreach (var path in allDepends)
                {
                    if (checkedAssets.Contains(path))
                        continue;
                    if (path.Contains("/Editor/"))
                        continue;
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (path.EndsWith(".cs"))
                    {
                        var script = obj as MonoScript;
                        if (script && script)
                        {
                            collectedTypes.Add(script.GetClass());
                        }
                    }
                    else if (path.EndsWith(".prefab"))
                    {
                        Debug.Assert(obj is GameObject, obj.GetType());
                        var components = (obj as GameObject).GetComponentsInChildren<Component>(true);
                        foreach (var component in components)
                        {
                            if (!component)
                                continue;
                            collectedTypes.Add(component.GetType());
                        }
                    }
                    else if (path.EndsWith(".asset") && obj is ScriptableObject)
                    {
                        var scriptobjs = AssetDatabase.LoadAllAssetsAtPath(path);
                        foreach (var subObj in scriptobjs)
                        {
                            collectedTypes.Add(subObj.GetType());
                        }
                        var scriptObj = obj as ScriptableObject;
                        collectedTypes.Add(scriptObj.GetType());
                    }
                }
            }

        }

        /// <summary>
        /// 资源引用到的代码防止被裁剪
        /// </summary>
        private static void GenerateSourceLink(string loadSceneGUID,string outLinkFilePath)
        {
            Dictionary<string, List<string>> assemblyDict = new Dictionary<string, List<string>>();
            using (var reader = System.Xml.XmlReader.Create(outLinkFilePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        if (reader.Name == "assembly")
                        {
                            var assemblyName = reader.GetAttribute("fullname");
                            if (!assemblyDict.ContainsKey(assemblyName))
                            {
                                assemblyDict[assemblyName] = new List<string>();
                            }

                            if (reader.ReadToDescendant("type"))
                            {
                                do
                                {
                                    var typeName = reader.GetAttribute("fullname");
                                    assemblyDict[assemblyName].Add(typeName);
                                } while (reader.ReadToNextSibling("type"));
                            }
                        }
                    }
                }
            }
            var checkedAssets = new HashSet<string>();
            var collectedTypes = new HashSet<Type>();
            int totalCount = 0;
            float currentCount = 0;
            foreach (var group in AssetBundleSetting.Instance.groups)
            {
                totalCount += group.infos.Count;
                var buildMap = group.CreateBuildInfo(false);
                foreach (var pair in buildMap)
                {
                    var cancel = EditorUtility.DisplayCancelableProgressBar("GenerateSourceLink", "Collecting:" + pair.Key, ++currentCount / totalCount);
                    if (cancel)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new Exception("User Stoped!");
                    }
                    foreach (var path in pair.Value)
                    {
                        CollectAssetScriptsRef(path, checkedAssets, collectedTypes);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            var loadSceneCheckedAssets = new HashSet<string>();
            var loadSceneCollectedTypes = new HashSet<Type>();

            var loadScenePath = AssetDatabase.GUIDToAssetPath(loadSceneGUID);
            CollectAssetScriptsRef(loadScenePath, loadSceneCheckedAssets, loadSceneCollectedTypes);
            foreach (var type in collectedTypes)
            {
                var assembleName = type.Assembly.GetName().Name;
                if (assembleName == "Assembly-CSharp")
                    continue;
                if (assemblyDict.TryGetValue(assembleName, out var typeList) && typeList.Contains(type.FullName))
                    continue;
                if (loadSceneCollectedTypes.Contains(type))
                    continue;
                if (!assemblyDict.TryGetValue(assembleName, out typeList))
                    assemblyDict[assembleName] = typeList = new List<string>();
                typeList.Add(type.FullName);
                Debug.Log("collected asset script ref:" + assembleName + ":" + type.Name);
            }

            var writer = System.Xml.XmlWriter.Create(outLinkFilePath,
               new System.Xml.XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true });

            writer.WriteStartDocument();
            writer.WriteStartElement("linker");

            var keyList = new List<string>(assemblyDict.Keys);
            keyList.Sort(string.CompareOrdinal);
            foreach (var asmName in keyList)
            {
                var types = assemblyDict[asmName];
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", asmName);
                List<string> assTypeNames = types;
                assTypeNames.Sort(string.CompareOrdinal);
                foreach (var typeName in assTypeNames)
                {
                    writer.WriteStartElement("type");
                    writer.WriteAttributeString("fullname", typeName);
                    writer.WriteAttributeString("preserve", "all");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
    }
}
