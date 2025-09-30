using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
namespace UFrame.HiveBundle.AssetBundleDataSource
{
    internal class HiveBundleDataSource : ABDataSource
    {
        private string[] emptyArray = new string[0];

        public AssetBundleGroup group { get; private set; }

        public static List<ABDataSource> CreateDataSources()
        {
            var retList = new List<ABDataSource>();
            var groups = AssetBundleSetting.Instance.groups;
            foreach (var group in groups)
            {
                var op = new HiveBundleDataSource();
                op.group = group;
                retList.Add(op);
            }
            return retList;
        }

        public string Name
        {
            get
            {
                if (group == null)
                    return "AB";
                return group.name;
            }
        }

        public string ProviderName
        {
            get
            {
                return "AB";
            }
        }

        public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
            if (group != null)
            {
                var guids = new List<string>();
                guids.AddRange(group.infos.FindAll(x => x.bundlePath == assetBundleName).Select(x => x.guid));
                var resPaths = new List<string>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (System.IO.Directory.Exists(path))
                        {
                            var files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                if (file.EndsWith(".meta"))
                                {
                                    continue;
                                }
                                var assetPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file).Replace('\\', '/');
                                resPaths.Add(assetPath);
                            }
                        }
                        else if (System.IO.File.Exists(path))
                        {
                            resPaths.Add(path);
                        }
                    }
                }
                return resPaths.ToArray();
            }
            return emptyArray;
        }

        public string GetAssetBundleName(string assetPath)
        {
            if (group != null)
            {
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
                var info = group.infos.Find(x => x.guid == guid);
                if (info != null)
                    return info.bundlePath;
            }
            return "";
        }

        public string GetImplicitAssetBundleName(string assetPath)
        {
            var folder = assetPath;
            var projectFolder = System.IO.Path.GetFullPath(System.Environment.CurrentDirectory);
            while (!string.IsNullOrEmpty(folder) && System.IO.Path.GetFullPath(folder) != projectFolder)
            {
                var bundleName = GetAssetBundleName(folder);
                if (!string.IsNullOrEmpty(bundleName))
                    return bundleName;
                folder = System.IO.Path.GetDirectoryName(folder);
            }
            return "";
        }

        public string[] GetAllAssetBundleNames()
        {
            //return AssetDatabase.GetAllAssetBundleNames ();
            var assetBundleNames = new List<string>();
            if (group != null)
            {
                assetBundleNames.AddRange(group.infos.Select(x => x.bundlePath).ToArray());
                return assetBundleNames.ToArray();
            }
            return emptyArray;
        }

        public bool IsReadOnly()
        {
            return false;
        }

        public void SetAssetBundleNameAndVariant(string assetPath, string bundlePath, string variantName)
        {
            Debug.Log($"SetAssetBundleNameAndVariant assetPath:{assetPath} bundleName:{bundlePath} variantName:{variantName}");
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (group != null)
            {
                if (!string.IsNullOrEmpty(variantName))
                    bundlePath = $"{bundlePath}.{variantName}".Replace("\\", "/").Replace("/", "_");
                var addressInfo = group.infos.Find(x => x.guid == guid);
                if (addressInfo != null && string.IsNullOrEmpty((string)bundlePath))
                {
                    group.infos.Remove(addressInfo);
                }
                else if (addressInfo != null)
                {
                    addressInfo.bundlePath = bundlePath;
                }
                else if (addressInfo == null && !string.IsNullOrEmpty((string)bundlePath))
                {
                    addressInfo = new AssetBundleInfo();
                    addressInfo.guid = guid;
                    addressInfo.bundlePath = bundlePath;
                    group.infos.Add(addressInfo);
                }
                if (addressInfo != null)
                {
                    addressInfo.bundlePath = bundlePath;
                }
                if (string.IsNullOrEmpty(addressInfo.assetPath))
                    addressInfo.assetPath = AssetDatabase.GUIDToAssetPath(addressInfo.guid);
                AssetBundleSetting.Save();
            }
        }

        public void RemoveUnusedAssetBundleNames()
        {
            if (group != null)
            {
                for (int i = 0; i < group.infos.Count; i++)
                {
                    var item = group.infos[i];
                    var path = AssetDatabase.GUIDToAssetPath(item.guid);
                    if (string.IsNullOrEmpty(path) || (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path)))
                    {
                        group.infos.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public bool CanSpecifyBuildTarget
        {
            get { return true; }
        }
        public bool CanSpecifyBuildOutputDirectory
        {
            get { return true; }
        }

        public bool CanSpecifyBuildOptions
        {
            get { return true; }
        }

        public bool BuildAssetBundles(ABBuildInfo info)
        {
            if (info == null)
            {
                Debug.Log("Error in build");
                return false;
            }

            var buildManifest = BuildPipeline.BuildAssetBundles(info.outputDirectory, info.options, info.buildTarget);
            if (buildManifest == null)
            {
                Debug.Log("Error in build");
                return false;
            }

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                if (info.onBuild != null)
                {
                    info.onBuild(assetBundleName);
                }
            }
            return true;
        }

        public bool IsRefBundle(string bundlePath)
        {
            if (group != null)
            {
                var bundleInfo = group.infos.Find(x => x.bundlePath == bundlePath);
                if (bundleInfo == null)
                    return true;
                return bundleInfo.nameRule == NameRule.FileName;
            }
            return false;
        }

        public void MergeFolderBundles(string bundlePath)
        {
            var bundleInfo = group.infos.FindAll(x => x.bundlePath.StartsWith(bundlePath + "/"));
            string dirPath = null;
            foreach (var item in bundleInfo)
            {
                var guid = item.guid;
                var itemPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(itemPath))
                {
                    var currentDirPath = System.IO.Path.GetDirectoryName(itemPath);
                    if (string.IsNullOrEmpty(dirPath))
                    {
                        dirPath = currentDirPath;
                    }
                    else if (dirPath.StartsWith(currentDirPath))
                    {
                        dirPath = currentDirPath;
                    }
                    else if (!currentDirPath.StartsWith(dirPath))
                    {
                        dirPath = null;
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(dirPath))
            {
                var guid = AssetDatabase.GUIDToAssetPath(dirPath);
                group.infos.RemoveAll(x => bundleInfo.Contains(x));
                SetAssetBundleNameAndVariant(dirPath, bundlePath, "");
            }
        }

        public void SplitFolderBundles(string bundlePath)
        {
            Debug.Log("SplitFolderBundles:" + bundlePath);
            var bundleInfos = group.infos.FindAll(x => x.bundlePath.ToLower() == bundlePath.ToLower());
            foreach (var bundleInfo in bundleInfos)
            {
                if (bundleInfo == null)
                {
                    Debug.LogError("empty bundleinfo:" + bundlePath);
                    continue;
                }
                var folder = AssetDatabase.GUIDToAssetPath(bundleInfo.guid);
                if (System.IO.Directory.Exists(folder))
                {
                    var files = System.IO.Directory.GetFiles(folder, "*.*", System.IO.SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".meta"))
                        {
                            continue;
                        }
                        var rootPath = System.IO.Path.GetDirectoryName(folder);
                        var bundlePathIndex = folder.IndexOf(bundlePath);
                        if (bundlePathIndex != -1)
                            rootPath = folder.Substring(0, bundlePathIndex);
                        var assetPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file);
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file).ToLower();
                        var bundlePath2 = bundlePath + "/" + fileName;
                        SetAssetBundleNameAndVariant(assetPath, bundlePath2, "");
                    }
                    var guid = AssetDatabase.GUIDFromAssetPath(folder).ToString();
                    group.infos.RemoveAll(x => x.guid == guid);
                }
                else
                {
                    var assetPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, folder);
                    SetAssetBundleNameAndVariant(assetPath, bundleInfo.bundlePath, "");
                }

            }
            AssetBundleSetting.Save();
        }

        public void SetBundleNameRule(string bundlePath, NameRule rule)
        {
            Debug.Log("ToggleMutiBundles:" + bundlePath);
            var bundleInfos = group.infos.FindAll(x => x.bundlePath.ToLower() == bundlePath.ToLower());
            if (bundleInfos != null)
            {
                foreach (var item in bundleInfos)
                {
                    item.nameRule = rule;
                }
                AssetBundleSetting.Save();
            }
        }

        public void MarkBundleEnable(string bundlePath, bool enable)
        {
            if (group != null)
            {
                var infos = group.infos.FindAll(x => x.bundlePath == bundlePath);
                if (infos != null)
                {
                    foreach (var item in infos)
                    {
                        item.disable = !enable;
                    }
                }
            }
            AssetBundleSetting.Save();
        }

        public bool CheckBundleEnable(string bundlePath)
        {
            if (group != null)
            {
                var info = group.infos.Find(x => x.bundlePath == bundlePath);
                if (info != null)
                    return !info.disable;
            }
            return true;
        }
    }
}
