using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.SceneManagement;

namespace UFrame.HiveBundle
{
    public class HiveBundleBuildTool
    {
        /// <summary>
        /// 打包资源组
        /// </summary>
        /// <param name="outputDir"></param>
        /// <param name="target"></param>
        public static string BuildAssetBundles(string groupName, string outputDir, BuildTarget target)
        {
            Directory.CreateDirectory(outputDir);
            var group = AssetBundleSetting.Instance.groups.Find(x => x.name == groupName);
            var keyPath = new Dictionary<string, string>();
            var manifest = BuildGroupBundles(group, outputDir, target, keyPath);
            if (manifest != null)
            {
                var sb = new StringBuilder();
                Dictionary<string, string[]> bundleResMap = new Dictionary<string, string[]>();
                foreach (var bundle in manifest.GetAllAssetBundles())
                {
                    var subBundles = manifest.GetAllDependencies(bundle);
                    if (subBundles == null || subBundles.Length == 0)
                        continue;

                    bundleResMap[bundle] = subBundles;
                    Debug.LogError(bundle + "->" + subBundles.Length);
                    sb.Append(bundle);
                    sb.Append(":");
                    int index = subBundles.Length;
                    foreach (var item in subBundles)
                    {
                        sb.Append(item);
                        if (--index > 0)
                        {
                            sb.Append("|");
                        }
                        if (bundleResMap.TryGetValue(item, out var subBundleRef) && subBundleRef != null && Array.IndexOf(subBundleRef, bundle) >= 0)
                        {
                            Debug.LogError("重复引用：" + item + " <-> " + bundle);
                        }
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return null;
        }


        /// <summary>
        /// 分组打包逻辑
        /// </summary>
        /// <param name="outputDir"></param>
        /// <param name="target"></param>
        /// <param name="group"></param>
        /// <param name="keypath"></param>
        public static AssetBundleManifest BuildGroupBundles(AssetBundleGroup group, string outputDir, BuildTarget target, Dictionary<string, string> keypath, bool collectShader = true)
        {
            //添加内置公共资源包
            var assetBundleMap = group.CreateBuildInfo(false, false, keypath);
            foreach (var item in group.infos)
            {
                var path = AssetDatabase.GUIDToAssetPath(item.guid);
                var fileName = System.IO.Path.GetFileName(path);
                if (path.EndsWith("unity"))
                {
                    EditorSceneManager.OpenScene(path);//指定场景下编译shader不会丢失
                }
            }
            if (!assetBundleMap.TryGetValue("shader", out var shaders))
            {
                shaders = assetBundleMap["shader"] = new List<string>();
            }
            var collectedShaders = CollectShaderAssets();
            foreach (var item in collectedShaders)
            {
                if (!shaders.Contains(item))
                    shaders.Add(item);
            }
            return BuildAssetBundles(target, outputDir, assetBundleMap);
        }

        /// <summary>
        /// 打包核心代码
        /// </summary>
        /// <param name="target"></param>
        /// <param name="outputDir"></param>
        /// <param name="assetBundleMap"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static AssetBundleManifest BuildAssetBundles(BuildTarget target, string outputDir, Dictionary<string, List<string>> assetBundleMap)
        {
            System.IO.Directory.CreateDirectory(outputDir);
            List<AssetBundleBuild> abs = new List<AssetBundleBuild>();
            List<AssetBundleBuild> sceneAbs = new List<AssetBundleBuild>();
            foreach (var abPair in assetBundleMap)
            {
                abs.Add(new AssetBundleBuild()
                {
                    assetBundleName = abPair.Key,
                    assetNames = abPair.Value.Distinct().ToArray(),
                });

                if (abPair.Value.Find(x => x.EndsWith(".unity")) != null)
                {
                    sceneAbs.Add(new AssetBundleBuild()
                    {
                        assetBundleName = abPair.Key,
                        assetNames = abPair.Value.Distinct().ToArray(),
                    });
                }
            }
            var result = BuildPipeline.BuildAssetBundles(outputDir, abs.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
            if (result == null)
            {
                throw new Exception("build assetbundle failed!");
            }
            return result;
        }

        /// <summary>
        /// 集成shader包
        /// </summary>
        /// <returns></returns>
        private static List<string> CollectShaderAssets()
        {
            var usedShaders = new List<string>();
            //预热shader变体
            var variantGUIDs = AssetDatabase.FindAssets("t:ShaderVariantCollection");
            foreach (var guid in variantGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log("variant path:" + path);
                var shaderVariants = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path);
                if (shaderVariants)
                {
                    shaderVariants.WarmUp();
                    usedShaders.Add(path);
                }
            }
            Shader.WarmupAllShaders();
            foreach (var group in AssetBundleSetting.Instance.groups)
            {
                var assetMap = group.CreateBuildInfo(false);
                foreach (var pairs in assetMap)
                {
                    foreach (var infoPath in pairs.Value)
                    {
                        var denpendencies = AssetDatabase.GetDependencies(infoPath, true);
                        foreach (var subPath in denpendencies)
                        {
                            if (!subPath.EndsWith(".shader") && !subPath.EndsWith(".shadergraph"))
                                continue;
                            if (subPath.Contains("TMP_SDF"))//loadscene 中需要使用，若剔除，由于资源包启动时还未加载shader包，导致字体材质丢失显示粉红，暂时不单独放shader包中
                            {
                                Debug.Log("ignore tmp:" + subPath);
                                continue;
                            }
                            if (subPath.Contains("com.unity.render-pipelines"))//内置shader变体太多，经常被内置场景使用，导致收集不全
                            {
                                Debug.Log("ignore pipelines:" + subPath);
                                continue;
                            }
                            if (usedShaders.Contains(subPath))
                                continue;
                            usedShaders.Add(subPath);
                            Debug.Log("collect shader:" + subPath);
                        }
                    }
                }
            }
            Debug.Log("collect shader count:" + usedShaders.Count);
            return usedShaders;
        }
    }
}
