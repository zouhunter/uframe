#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
namespace UFrame.HiveBundle
{
    [FilePath("ProjectSettings/AssetBundleSetting.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AssetBundleSetting : ScriptableSingleton<AssetBundleSetting>
    {
        public bool inspectorGui;
        public bool projectwindowGui;
        public List<AssetBundleGroup> groups = new List<AssetBundleGroup>();
    }

    [System.Serializable]
    public class AssetBundleGroup
    {
        public string name;
        public List<AssetBundleInfo> infos = new List<AssetBundleInfo>();

        public Dictionary<string, List<string>> CreateBuildInfo(bool fullPath, bool includeDisable = false, Dictionary<string, string> keypath = null)
        {
            var assetBundleMap = new Dictionary<string, List<string>>();

            var ToRelativeAssetPath = new Func<string, string>((string s) => System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, s).Replace('\\', '/'));
            foreach (var abInfo in infos)
            {
                if (abInfo.disable && !includeDisable)
                    continue;

                var bundleName = abInfo.GetBundleName(fullPath);
                if (!assetBundleMap.TryGetValue(bundleName, out var assets))
                {
                    assets = new List<string>();
                    assetBundleMap[bundleName] = assets;
                }

                var assetPath = abInfo.assetPath;
                if (!System.IO.File.Exists(assetPath) && !System.IO.Directory.Exists(assetPath))
                    assetPath = AssetDatabase.GUIDToAssetPath(abInfo.guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    if (System.IO.File.Exists(assetPath))
                    {
                        assets.Add(ToRelativeAssetPath(assetPath));
                        if (keypath != null)
                            keypath[bundleName] = abInfo.bundlePath;
                    }
                    else if (System.IO.Directory.Exists(assetPath))
                    {
                        if (abInfo.nameRule == NameRule.FileName)//通配子文件资源
                        {
                            var files = System.IO.Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                if (file.EndsWith(".meta") || file.EndsWith(".manifest"))
                                    continue;

                                var subBundleName = System.IO.Path.GetFileNameWithoutExtension(file);
                                if (!assetBundleMap.TryGetValue(subBundleName, out var subAssets))
                                {
                                    subAssets = new List<string>();
                                    assetBundleMap[subBundleName] = subAssets;
                                }
                                if (keypath != null)
                                    keypath[subBundleName] = abInfo.bundlePath + "/" + subBundleName;
                                subAssets.Add(ToRelativeAssetPath(file));
                            }
                        }
                        else
                        {
                            var files = System.IO.Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                if (file.EndsWith(".meta") || file.EndsWith(".manifest"))
                                    continue;
                                assets.Add(ToRelativeAssetPath(file));
                                if (keypath != null)
                                    keypath[bundleName] = abInfo.bundlePath;
                            }
                        }
                    }
                }
            }
            return assetBundleMap;

        }
    }
    [System.Serializable]
    public class AssetBundleInfo
    {
        public string guid;
        public string assetPath;
        public string bundlePath;
        public NameRule nameRule;
        public bool disable;
        public string GetBundleName(bool fullPath)
        {
            switch (nameRule)
            {
                case NameRule.Group:
                    if (bundlePath.Contains('/'))
                        return fullPath ? bundlePath : bundlePath.Substring(bundlePath.LastIndexOf('/') + 1);
                    return bundlePath;
                case NameRule.FileName:
                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    if (fullPath)
                    {
                        return bundlePath + "/" + fileName;
                    }
                    return fileName;
                default:
                    return bundlePath;
            }
        }
    }
}
#endif
