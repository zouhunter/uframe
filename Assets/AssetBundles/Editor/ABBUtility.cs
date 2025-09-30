using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;

namespace UFrame.AssetBundles
{
    public static class ABBUtility
    {
        public const string Menu_ProjectsBuildWindow = "Window/UFrame/AssetBundleBuilder/Builder/ProjectsBuild";
        public const string Menu_ConfigBuildWindow = "Window/UFrame/AssetBundleBuilder/Builder/ConfigBuild";
        public const string Menu_SelectBuildWindow = "Window/UFrame/AssetBundleBuilder/Builder/SelectBuild";
        public const string Menu_BundleName = "Window/UFrame/AssetBundleBuilder/Setter/BundleView";
        public const string Menu_QuickSetter = "Window/UFrame/AssetBundleBuilder/Setter/QuickSetter";

        [MenuItem(Menu_ConfigBuildWindow)]
        public static void CreateSelectBuild()
        {
            ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<ConfigBuildObj>(), "ConfigBuildObj.asset");
        }
        public static void BuildProjectsAssetBundle(string path, BuildAssetBundleOptions option, BuildTarget target)
        {
            BuildAllAssetBundles(path, option, target);
        }
        public static void BuildSelectAssets(string abName, string path, UnityEngine.Object[] obj, BuildTarget target, BuildAssetBundleOptions option = BuildAssetBundleOptions.None)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            AssetBundleBuild[] builds = new AssetBundleBuild[1];
            builds[0].assetBundleName = abName;
            builds[0].assetNames = new string[obj.Length];

            for (int i = 0; i < obj.Length; i++)
            {
                builds[0].assetNames[i] = AssetDatabase.GetAssetPath(obj[i]);
            }
            BuildGroupBundles(path, builds, option, target);
        }
        public static void BuildGroupBundles(string path, AssetBundleBuild[] builds, BuildAssetBundleOptions option, BuildTarget target)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            bool appendHash = false;
            if ((option & BuildAssetBundleOptions.AppendHashToAssetBundleName) == BuildAssetBundleOptions.AppendHashToAssetBundleName)
            {
                option ^= BuildAssetBundleOptions.AppendHashToAssetBundleName;
                appendHash = true;
            }
            var manifest = BuildPipeline.BuildAssetBundles(Path.GetFullPath(path), builds, option, target);
            if (appendHash)
                AppendBundleHashs(target.ToString(), path, manifest);
            AssetDatabase.Refresh();
        }

        public static void BuildAllAssetBundles(string path, BuildAssetBundleOptions option, BuildTarget target)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            bool appendHash = false;
            if ((option & BuildAssetBundleOptions.AppendHashToAssetBundleName) == BuildAssetBundleOptions.AppendHashToAssetBundleName)
            {
                option ^= BuildAssetBundleOptions.AppendHashToAssetBundleName;
                appendHash = true;
            }
            var manifest = BuildPipeline.BuildAssetBundles(Path.GetFullPath(path), option, target);
            if (appendHash)
                AppendBundleHashs(target.ToString(),path, manifest);
        }

        private static void AppendBundleHashs(string menuName,string buildPath, AssetBundleManifest manifest)
        {
            var exportPath = System.IO.Path.GetDirectoryName(buildPath) + "/Output";
            if (System.IO.Directory.Exists(exportPath))
                System.IO.Directory.Delete(exportPath, true);
            System.IO.Directory.CreateDirectory(exportPath);
            var bundles = manifest.GetAllAssetBundles();
            foreach (var bundleName in bundles)
            {
                var filePath = System.IO.Path.Join(buildPath, bundleName);
                var fileName = System.IO.Path.GetRelativePath(buildPath, filePath);
                var fileHash = manifest.GetAssetBundleHash(fileName);
                var newFilePath = $"{exportPath}/{fileName}_{fileHash}.bundle";
                System.IO.File.Copy(filePath, newFilePath);
            }
            var menuPath = $"{buildPath}/{menuName}";
            var newMenuPath = $"{exportPath}/{menuName}.txt";
            System.IO.File.Copy(menuPath,newMenuPath);
        }

        /// <summary>
        /// 遍历目录及其子目录
        /// </summary>
        public static void RecursiveSub(string path, string ignoreFileExt = ".meta", string ignorFolderEnd = "_files", Action<string> action = null)
        {
            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string filename in names)
            {
                string ext = Path.GetExtension(filename);
                if (ext.Equals(ignoreFileExt)) continue;
                action(filename.Replace('\\', '/'));
            }
            foreach (string dir in dirs)
            {
                if (dir.EndsWith(ignorFolderEnd)) continue;
                RecursiveSub(dir, ignoreFileExt, ignorFolderEnd, action);
            }
        }
        /// <summary>
        /// 遍历目录及其子目录
        /// </summary>
        public static void Recursive(string path, string fileExt, bool deep = true, Action<string> action = null)
        {
            string[] names = Directory.GetFiles(path);
            foreach (string filename in names)
            {
                string ext = Path.GetExtension(filename);
                if (ext.ToLower().Contains(fileExt.ToLower()))
                    action(filename.Replace('\\', '/'));
            }
            if (deep)
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    Recursive(dir, fileExt, deep, action);
                }
            }
        }

        public static AssetBundleBuild[] GetBundleBuilds(List<ConfigBuildObj.ObjectItem> needBuilds)
        {
            Dictionary<string, AssetBundleBuild> bundleDic = new Dictionary<string, AssetBundleBuild>();
            foreach (var item in needBuilds)
            {
                if (string.IsNullOrEmpty(item.assetBundleName))
                    continue;
                if (!bundleDic.ContainsKey(item.assetBundleName))
                {
                    bundleDic.Add(item.assetBundleName, new AssetBundleBuild());
                }
                var asb = bundleDic[item.assetBundleName];
                if (asb.assetNames == null)
                    asb.assetNames = new string[0];
                List<string> assetNames = new List<string>(asb.assetNames);
                asb.assetBundleName = item.assetBundleName;
                if (item.obj is DefaultAsset)
                {
                    var folderPath = AssetDatabase.GetAssetPath(item.obj);
                    var subFiles = new List<string>();
                    CollectEmptyBundleFiles(folderPath, subFiles);
                    List<string> supportedSubFiles = new List<string>();
                    foreach (var subFile in subFiles)
                    {
                        if (needBuilds.Find(x => x.assetPath == subFile) != null)
                        {
                            supportedSubFiles.Add(subFile);
                        }
                    }
                    if (supportedSubFiles.Count == 0)
                    {
                        supportedSubFiles.AddRange(subFiles);
                    }
                    foreach (var assetPath in supportedSubFiles)
                    {
                        if (!assetNames.Contains(assetPath))
                            assetNames.Add(assetPath);
                    }
                }
                else
                {
                    var fullPath = item.assetPath;
                    if (!assetNames.Contains(fullPath))
                        assetNames.Add(fullPath);
                }
                asb.assetNames = assetNames.ToArray();
                asb.addressableNames = new string[asb.assetNames.Length];
                for (int i = 0; i < asb.assetNames.Length; i++)
                {
                    asb.addressableNames[i] = GetAddressableName(asb.assetNames[i]);
                }
                bundleDic[item.assetBundleName] = asb;
            }

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>(bundleDic.Values);
            return builds.ToArray();
        }

        public static string GetAddressableName(string filePath)
        {
            var importer = AssetImporter.GetAtPath(filePath);
            if (importer && !string.IsNullOrEmpty(importer.assetBundleName))
                return System.IO.Path.GetFileNameWithoutExtension(filePath);
            else
            {
                var directory = System.IO.Path.GetDirectoryName(filePath);
                importer = AssetImporter.GetAtPath(directory);
                while (importer && string.IsNullOrEmpty(importer.assetBundleName))
                {
                    directory = System.IO.Path.GetDirectoryName(directory);
                    importer = AssetImporter.GetAtPath(directory);
                }

                if (importer && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    var dirPath = directory.Replace('\\', '/').Replace(Application.dataPath, "Assets");
                    return filePath.Replace(dirPath + "/", "");
                }
                return System.IO.Path.GetFileNameWithoutExtension(filePath);
            }
        }

        public static void CollectEmptyBundleFiles(string folder, List<string> fileList)
        {
            var dirs = System.IO.Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);
            foreach (var dir in dirs)
            {
                var importer = AssetImporter.GetAtPath(dir);
                if (string.IsNullOrEmpty(importer.assetBundleName))
                {
                    CollectEmptyBundleFiles(dir, fileList);
                }
            }
            var files = System.IO.Directory.GetFiles(folder, "*");
            foreach (var filePath in files)
            {
                if (filePath.EndsWith(".meta"))
                    continue;
                if (filePath.EndsWith(".cs"))
                    continue;
                var supportFilePath = filePath.Replace('\\', '/').Replace(Application.dataPath, "Assets");
                fileList.Add(supportFilePath);
            }
        }

    }
}