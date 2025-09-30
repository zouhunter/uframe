using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using static UnityEngine.ShaderVariantCollection;

namespace UFrame.HiveBundle
{
    public class ShaderCollector
    {
        static MethodInfo _GetShaderVariantEntries = null;
        static MethodInfo _GetAllGlobalKeywords = null;
        static MethodInfo _GetShaderLocalKeywords = null;
        public struct ShaderVariantData
        {
            public int[] passTypes;
            public string[] keywordLists;
            public string[] remainingKeywords;
        }

        /// <summary>
        /// 获取所有全局关键字
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllGlobalKeywords()
        {
            if (_GetAllGlobalKeywords == null)
            {
                _GetAllGlobalKeywords = typeof(ShaderUtil).GetMethod("GetAllGlobalKeywords", BindingFlags.NonPublic | BindingFlags.Static);
            }
            return (string[])_GetAllGlobalKeywords.Invoke(null, null);
        }

        /// <summary>
        /// 获取shader的局部关键字
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static string[] GetShaderLocalKeywords(Shader shader)
        {
            if (_GetShaderLocalKeywords == null)
            {
                _GetShaderLocalKeywords = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.NonPublic | BindingFlags.Static);
            }
            return (string[])_GetShaderLocalKeywords.Invoke(null, new object[] { shader });
        }

        private static ShaderVariantData GetShaderVariantEntriesFiltered(Shader shader, string[] SelectedKeywords)
        {
            if (_GetShaderVariantEntries == null)
            {
                _GetShaderVariantEntries = typeof(ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.NonPublic | BindingFlags.Static);
            }

            int[] types = null;
            string[] keywords = null;
            string[] remainingKeywords = null;

            object[] args = new object[]{
            shader,
            32,
            SelectedKeywords,
            new ShaderVariantCollection(),
            types,
            keywords,
            remainingKeywords
        };
            _GetShaderVariantEntries?.Invoke(null, args);

            var passTypes = new List<int>();
            var allTypes = (args[4] as int[]);
            foreach (var type in allTypes)
            {
                if (!passTypes.Contains(type))
                {
                    passTypes.Add(type);
                }
            }

            ShaderVariantData svd = new ShaderVariantData()
            {
                passTypes = passTypes.ToArray(),
                keywordLists = args[5] as string[],
                remainingKeywords = args[6] as string[]
            };

            return svd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Dictionary<Shader, List<Material>> FindAllMaterials(bool reimport)
        {
            var mats = new Dictionary<Shader, List<Material>>();
            var processedmats = new HashSet<string>();
            var allReferences = new List<string>();
            var allMats = new HashSet<Material>();
            foreach (var group in AssetBundleSetting.Instance.groups)
            {
                foreach (var info in group.infos)
                {
                    var path = AssetDatabase.GUIDToAssetPath(info.guid);
                    if (string.IsNullOrEmpty(path))
                        continue;

                    if (System.IO.File.Exists(path))
                    {
                        allReferences.AddRange(AssetDatabase.GetDependencies(path, true));
                    }
                    else if (Directory.Exists(path))
                    {
                        var files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            var assetPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file).Replace("\\", "/");
                            allReferences.AddRange(AssetDatabase.GetDependencies(assetPath, true));
                        }
                    }
                }
            }
            for (int i = 0; i < allReferences.Count; i++)
            {
                var matPath = allReferences[i];

                if (!processedmats.Contains(matPath))
                {
                    processedmats.Add(matPath);

                    if (!matPath.EndsWith(".mat"))
                        continue;

                    var material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (!material)
                        continue;

                    if (!mats.TryGetValue(material.shader, out var list))
                    {
                        mats[material.shader] = list = new List<Material>();
                    }
                    ReimportMat(material);
                    list.Add(material);
                    allMats.Add(material);
                }
            }
            AssetDatabase.SaveAssets();
            return mats;
        }

        private static void ReimportMat(Material mat)
        {
            var path = AssetDatabase.GetAssetPath(mat);
            if (!path.EndsWith(".mat"))
                return;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            EditorUtility.SetDirty(mat);
            AssetImporter.GetAtPath(path).SaveAndReimport();
        }

        /// <summary>
        /// 收集变体
        /// </summary>
        public static void DoCollect(ShaderVariantCollection shaderVariants, bool reimport = true)
        {
            //---------------------------------------------------------------
            // find all materials
            var matDic = FindAllMaterials(reimport);
            //---------------------------------------------------------------
            // collect all key words
            Dictionary<Shader, Dictionary<string, Material>> finalMats = new Dictionary<Shader, Dictionary<string, Material>>();
            List<string> tempKeywards = new List<string>();
            int idx = 0;
            foreach (var pair in matDic)
            {
                var shader = pair.Key;
                EditorUtility.DisplayProgressBar($"Collect Shader {shader}", "Collect Key words", (float)idx++ / matDic.Count);
                if (finalMats.TryGetValue(shader, out var matDict) == false)
                {
                    matDict = new Dictionary<string, Material>();
                    finalMats.Add(shader, matDict);
                }

                foreach (var mat in pair.Value)
                {
                    tempKeywards.Clear();
                    string[] keyWords = mat.shaderKeywords;
                    tempKeywards.AddRange(keyWords);
                    if (mat.enableInstancing)
                    {
                        tempKeywards.Add("enableInstancing");
                    }

                    if (tempKeywards.Count == 0)
                    {
                        continue;
                    }
                    tempKeywards.Sort();
                    string pattern = string.Join("_", tempKeywards);
                    if (!matDict.ContainsKey(pattern))
                    {
                        matDict.Add(pattern, mat);
                        Debug.Log(shader.name + ":" + pattern);
                    }
                }
            }
            //---------------------------------------------------------------
            // collect all variant
            idx = 0;
            foreach (var kv in finalMats)
            {
                var shader = kv.Key;
                var shaderFullName = kv.Key.name;
                var matList = kv.Value;

                if (matList.Count == 0)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar($"Collect Shader {shaderFullName}", "General Shader Variant", (float)idx++ / finalMats.Count);
                if (shaderFullName.Contains("InternalErrorShader"))
                {
                    continue;
                }

                foreach (var kv2 in matList)
                {
                    var svd = GetShaderVariantEntriesFiltered(shader, kv2.Value.shaderKeywords);

                    foreach (var passType in svd.passTypes)
                    {
                        var shaderVariant = new ShaderVariantCollection.ShaderVariant()
                        {
                            shader = shader,
                            passType = (UnityEngine.Rendering.PassType)passType,
                            keywords = kv2.Value.shaderKeywords,
                        };
                        if (!shaderVariants.Contains(shaderVariant))
                        {
                            shaderVariants.Add(shaderVariant);
                        }
                    }
                }
                EditorUtility.SetDirty(shaderVariants);
            }

            //---------------------------------------------------------------
            // save
            EditorUtility.DisplayProgressBar("Collect Shader", "Save Assets", 1f);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();

            Debug.Log("Collect all shader variant completed!");
        }
    }
}
