//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-05 08:43:45
//* 描    述： 将依赖的模型单独设置bundle

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
//using System.Linq;

namespace UFrame.AssetBundles
{
    public class AssetBundleTools
    {
        protected static readonly string[] ignoreFliters = {
            ".cs",".shadergraph",".prefab",".shader"
        };

        /// <summary>
        /// 标记为资源包
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetbundleName"></param>
        public static void MarkAssetBundle(string assetPath, string assetbundleName)
        {
            AssetImporter import = AssetImporter.GetAtPath(assetPath);
            if (import && !string.IsNullOrEmpty(assetbundleName) && import.assetBundleName != assetbundleName + ".ab")
            {
                import.assetBundleName = assetbundleName + ".ab";
                Debug.Log("mark bundle:" + assetPath);
                EditorUtility.SetDirty(import);
            }
            else if (import && string.IsNullOrEmpty(assetbundleName) && !string.IsNullOrEmpty(import.assetBundleName))
            {
                import.assetBundleName = null;
                Debug.LogWarning("clear bundle:" + assetPath);
                EditorUtility.SetDirty(import);
            }
        }

        /// <summary>
        /// 按后缀类型进行资源包拆分
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetbundleBase"></param>
        /// <param name="fliters"></param>
        public static void DependenceToBundle(string assetPath, string[] fliters)
        {
            var dependences = AssetDatabase.GetDependencies(assetPath, true);
            for (int i = 0; i < dependences.Length; i++)
            {
                var path = dependences[i];
                if (path == assetPath)
                    continue;

                bool cancel = EditorUtility.DisplayCancelableProgressBar("DependenceToBundle", "processing " + path, i / (float)dependences.Length);
                if (cancel)
                    break;

                string guid = null;
                for (int j = 0; j < fliters.Length; j++)
                {
                    if (path.EndsWith(fliters[j]))
                    {
                        guid = AssetDatabase.AssetPathToGUID(path);
                        break;
                    }
                }
                MarkAssetBundle(path, guid);
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 按资源包大小进行包体拆分
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="assetbundleBase"></param>
        /// <param name="fliters"></param>
        public static void DependenceToBundle(string assetPath, int minSize)
        {
            if (minSize < 10240)
            {
                Debug.LogError("bundle is so small than 10k!");
                return;
            }

            var dependences = AssetDatabase.GetDependencies(assetPath, true);
            for (int i = 0; i < dependences.Length; i++)
            {
                var path = dependences[i];
                if (path == assetPath)
                    continue;

                bool ignore = false;
                for (int j = 0; j < ignoreFliters.Length; j++)
                {
                    if (path.ToLower().EndsWith(ignoreFliters[j]))
                    {
                        ignore = true;
                        continue;
                    }
                }

                bool cancel = EditorUtility.DisplayCancelableProgressBar("DependenceToBundle", "processing " + path, i / (float)dependences.Length);
                if (cancel)
                    continue;

                if (!ignore && System.IO.File.ReadAllBytes(path).Length < minSize)
                {
                    ignore = true;
                }
                string guid = ignore ? null : AssetDatabase.AssetPathToGUID(path);
                MarkAssetBundle(path, guid);
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 打氏所有资源包
        /// </summary>
        /// <returns></returns>
        public static int PrintAssetBundles()
        {
            var assetbundleNames = AssetDatabase.GetAllAssetBundleNames();
            if (assetbundleNames != null)
            {
                return assetbundleNames.Length;
            }
            return 0;
        }

        /// <summary>
        /// 收集AssrtBundleBuild
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="builds"></param>
        /// <param name="collectedAssets"></param>
        /// <param name="checkIgnore"></param>
        /// <returns></returns>
        protected static void CollectAssetBundleBuild(string assetBundleName, List<AssetBundleBuild> builds)
        {
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            Dictionary<string, List<string>> bundleMap = new Dictionary<string, List<string>>();
            for (int i = 0; i < assetPaths.Length; i++)
            {
                var assetPath = assetPaths[i];
                var assetImport = AssetImporter.GetAtPath(assetPath);
                var variant = assetImport.assetBundleVariant ?? "";
                if (!bundleMap.TryGetValue(variant, out var list))
                {
                    list = new List<string>();
                    bundleMap[variant] = list;
                }
                list.Add(assetPath);
            }

            foreach (var pair in bundleMap)
            {
                var build = new AssetBundleBuild();
                build.assetBundleName = assetBundleName;
                build.assetBundleVariant = pair.Key;
                build.assetNames = pair.Value.ToArray();
                //build.addressableNames = pair.Value.Select(x=>System.IO.Path.GetFileNameWithoutExtension(x)).ToArray();
                if (build.assetNames != null && build.assetNames.Length > 0 && builds.FindIndex(x => x.assetBundleName == build.assetBundleName && x.assetBundleVariant == build.assetBundleVariant) < 0)
                {
                    builds.Add(build);
                    Debug.Log("ab:" + build.assetBundleName + " assets:" + string.Join("+\n", build.assetNames));
                }
            }

            var dependences = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);
            for (int j = 0; j < dependences.Length; j++)
            {
                var subBundleName = dependences[j];
                if (subBundleName == assetBundleName)
                    continue;
                CollectAssetBundleBuild(subBundleName, builds);
            }
        }

        /// <summary>
        /// 依赖找包
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="targetPlatform"></param>
        /// <param name="assetBundleOptions"></param>
        public static AssetBundleManifest BuildAssetBundle(string[] rootAssetBundles, string outputPath, BuildTarget targetPlatform, BuildAssetBundleOptions assetBundleOptions)
        {
#if UNITY_ANDROID
            if (targetPlatform != BuildTarget.Android)
                return null;
#elif UNITY_IOS
            if (targetPlatform != BuildTarget.iOS)
                return null;
#endif
            if (!string.IsNullOrEmpty(outputPath) && !System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            for (int i = 0; i < rootAssetBundles.Length; i++)
            {
                CollectAssetBundleBuild(rootAssetBundles[i], builds);
            }
            return BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), assetBundleOptions, targetPlatform);
        }
    }
}