using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.HiveBundle;
using UnityEditor;
using System.IO;
using System.Linq;

public class HiveBundleBuild
{
    [MenuItem("Tools/BuildSelect(HiveBundle)")]
    public static void BuildNowSelectAsset()
    {
        BuildAssetsBundles(Application.streamingAssetsPath,Selection.objects.Select(x => AssetDatabase.GetAssetPath(x)).ToArray());
    }

    private static string[] BuildAssetsBundles(string outputDir, string[] assetPaths)
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Directory.CreateDirectory(outputDir);
        Debug.LogError(outputDir);
        List<string> filePath = new List<string>();
        Dictionary<string, List<string>> buildMap = new Dictionary<string, List<string>>(); ;
        if (assetPaths.Length == 0)
        {
            Debug.LogError("没有选中文件！");
            return null;
        }
        else
        {
            foreach (string path in assetPaths)
            {
                filePath.Add(path);
                Debug.LogWarning("选中文件路径:" + path);
            }
            var groups = AssetBundleSetting.Instance.groups;
            foreach (var group in groups)
            {
                var bundleMaps = group.CreateBuildInfo(true);
                foreach (var path in filePath)
                {
                    var bundleName = bundleMaps.Where(x => x.Value.Contains(path.Replace('\\', '/'))).First().Key;
                    if (!string.IsNullOrEmpty(bundleName))
                    {
                        buildMap[bundleName] = bundleMaps[bundleName];
                        Debug.LogWarning("选中包" + bundleName);
                    }
                }
            }
        }
        List<AssetBundleBuild> abs = new List<AssetBundleBuild>();
        foreach (var pair in buildMap)
        {
            abs.Add(new AssetBundleBuild
            {
                assetBundleName = pair.Key,
                assetNames = pair.Value.ToArray()
            });
        }
        BuildPipeline.BuildAssetBundles(outputDir, abs.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
        return buildMap.Keys.ToArray();
    }

}
