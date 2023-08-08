//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-22
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace UFrame.DressAssetBundle.Editors
{
    public class AddressABBuilder
    {
        public static Dictionary<string, string> DecodeTextList(string filePath)
        {
            Dictionary<string, string> pairDic = new Dictionary<string, string>();
            if (!File.Exists(filePath))
                return null;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        string[] lineArray = line.Split('\t', ',');
                        if (lineArray.Length != 2)
                        {
                            Debug.LogError("error line:" + line);
                            continue;
                        }
                        string address = lineArray[0];
                        if (pairDic.ContainsKey(address))
                        {
                            Debug.LogError("address already exists:" + address);
                            continue;
                        }
                        pairDic[address] = lineArray[1];
                    }
                }
            }
            return pairDic;
        }

        /// <summary>
        /// 收集文件 
        /// </summary>
        /// <param name="fileDic"></param>
        /// <param name="defineObj"></param>
        public static void ImportAssetList(Dictionary<string, string> assetDic, AddressDefineObject defineObj)
        {
            var addressList = defineObj.addressList;
            var refBundleList = defineObj.refBundleList;
            var changed = false;
            foreach (var pair in assetDic)
            {
                string address = pair.Key;
                string guid = AssetDatabase.AssetPathToGUID(pair.Value);
                if (!string.IsNullOrEmpty(guid))
                {
                    var refBundleItem = refBundleList.Find(x => x.guids.Contains(guid));
                    if (refBundleItem != null)
                    {
                        refBundleItem.guids.Remove(guid);
                        if (refBundleItem.guids.Count == 0)
                            refBundleList.Remove(refBundleItem);
                        changed = true;
                    }

                    var configItem = addressList.Find(x => x.guid == guid);
                    var configItemByAddress = addressList.Find(x => x.address == address);
                    if (configItem != null)
                    {
                        if (configItem.address != address)
                        {
                            Debug.Log($"address changed from:{configItem.address} to:{address}");
                            configItem.address = address;
                            changed = true;
                        }
                    }
                    else if (configItemByAddress != null)
                    {
                        if (configItemByAddress.guid != guid)
                        {
                            Debug.Log($"{address} guid changed from:{configItemByAddress.guid} to:{guid}");
                            configItemByAddress.guid = guid;
                            changed = true;
                        }
                    }
                    else
                    {
                        configItem = new UFrame.DressAssetBundle.AddressInfo();
                        configItem.guid = guid;
                        configItem.address = address;
                        configItem.active = true;
                        addressList.Add(configItem);
                        Debug.Log($"address add:{configItem.address} path:{pair.Value}");
                        changed = true;
                    }
                }
                else
                {
                    Debug.LogError("file not exists:" + pair.Value);
                }
            }
            if (changed)
            {
                EditorUtility.SetDirty(defineObj);
            }
        }

        /// <summary>
        /// 收集由文本配置的key和资源路径
        /// </summary>
        public static void ImportAssetList(string filePath, AddressDefineObject defineObj)
        {
            if (!File.Exists(filePath))
                return;
            var assetDic = DecodeTextList(filePath);
            if (assetDic != null)
            {
                ImportAssetList(assetDic, defineObj);
            }
        }

        /// <summary>
        /// 收集由文本配置的key和资源路径
        /// </summary>
        public static void RemoveAssetList(string filePath, AddressDefineObject defineObj)
        {
            if (!File.Exists(filePath))
                return;
            var assetDic = DecodeTextList(filePath);
            if (assetDic != null)
            {
                RemoveAssetList(assetDic, defineObj);
            }
        }

        /// <summary>
        /// 移除由文件配制的key和资源路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="defineObj"></param>
        public static void RemoveAssetList(Dictionary<string, string> assetDic, AddressDefineObject defineObj)
        {
            var curDir = System.Environment.CurrentDirectory;
            var addressList = defineObj.addressList;
            var refBundleList = defineObj.refBundleList;
            var changed = false;
            foreach (var pair in assetDic)
            {
                string address = pair.Key;
                string path = Path.Combine(curDir, pair.Value);
                string guid = AssetDatabase.AssetPathToGUID(pair.Value);
                if (File.Exists(path))
                {
                    var refBundleItem = refBundleList.Find(x => x.guids.Contains(guid));
                    if (refBundleItem != null)
                    {
                        refBundleItem.guids.Remove(guid);
                        if (refBundleItem.guids.Count == 0)
                            refBundleList.Remove(refBundleItem);
                        changed = true;
                    }

                    var configItem = addressList.Find(x => x.guid == guid);
                    var configItemByAddress = addressList.Find(x => x.address == address);
                    if (configItem != null)
                    {
                        addressList.Remove(configItem);
                        Debug.Log($"addressList remove by guid:{configItem.address}");
                        changed = true;
                    }
                    else if (configItemByAddress != null)
                    {
                        addressList.Remove(configItemByAddress);
                        Debug.Log($"addressList remove by address:{configItem.address}");
                        changed = true;
                    }
                }
                else
                {
                    Debug.LogError("file not exists:" + path);
                }
            }
            if (changed)
            {
                EditorUtility.SetDirty(defineObj);
            }
        }

        public static void AutoBuildAddressDefine(AddressDefineObject addressDefine, string outputPath = "AssetBundles/Output")
        {
            var bundleBuildPath = $"AssetBundles/{EditorUserBuildSettings.activeBuildTarget}";
            var collector = new BundleBuildCollector(addressDefine);
            collector.SplitGroupBundles();
            var manifest = BuildAssetBundles(collector, bundleBuildPath);
            var hashMap = AppendHashToBundles(manifest, bundleBuildPath, outputPath, addressDefine.hashLengthMax);
            WriteCatlog($"{outputPath}/catlog.txt", manifest, hashMap, collector);
            var fileHash = HashMd5File($"{outputPath}/catlog.txt",32);
            System.IO.File.WriteAllText($"{outputPath}/catlog.hash", fileHash);
        }

        public static AssetBundleManifest BuildAssetBundles(BundleBuildCollector collector, string outputPath)
        {
            AddressDefineObject defObj = collector.defObj;
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            var abBuild = collector.Collect();
            return BuildPipeline.BuildAssetBundles(outputPath, abBuild, defObj.options, EditorUserBuildSettings.activeBuildTarget);
        }

        public static Dictionary<string,string> AppendHashToBundles(AssetBundleManifest manifest, string buildPath, string outputPath, int hashLength)
        {
            if (System.IO.Directory.Exists(outputPath))
                System.IO.Directory.Delete(outputPath, true);
            System.IO.Directory.CreateDirectory(outputPath);
            var bundles = manifest.GetAllAssetBundles();
            var hashMap = new Dictionary<string, string>();
            foreach (var bundleName in bundles)
            {
                var filePath = System.IO.Path.Join(buildPath, bundleName);
                var fileName = System.IO.Path.GetRelativePath(buildPath, filePath);
                var fileHash = HashMd5File(filePath, hashLength);
                hashMap[bundleName] = fileHash;
                var newFilePath = $"{outputPath}/{fileName}_{fileHash}.bundle";
                System.IO.File.Copy(filePath, newFilePath);
            }
            return hashMap;
        }

        public static void WriteCatlog(string outPath, AssetBundleManifest manifest,Dictionary<string,string> hashMap, BundleBuildCollector collector)
        {
            AddressDefineObject defObj = collector.defObj;
            using (var fileStream = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    var assetBundleArray = manifest.GetAllAssetBundles();
                    System.Array.Sort(assetBundleArray);
                    binaryWriter.Write((ushort)assetBundleArray.Length);
                    for (int i = 0; i < assetBundleArray.Length; i++)
                    {
                        var assetBundleItem = assetBundleArray[i];
                        var hashStr = hashMap[assetBundleItem];
                        binaryWriter.Write($"{assetBundleItem}_{hashStr}");
                        var dependencies = manifest.GetDirectDependencies(assetBundleItem);
                        var dependenciesNum = 0;
                        if (dependencies != null && dependencies.Length > 0)
                            dependenciesNum = dependencies.Length;
                        binaryWriter.Write((byte)dependenciesNum);
                        for (int did = 0; did < dependenciesNum; did++)
                        {
                            binaryWriter.Write((ushort)System.Array.IndexOf(assetBundleArray, dependencies[did]));
                        }
                    }
                    var addressMap = new Dictionary<string, List<AddressInfo>>();
                    foreach (var addressInfo in defObj.addressList)
                    {
                        if (!addressInfo.active)
                            continue;
                        var path = AssetDatabase.GUIDToAssetPath(addressInfo.guid);
                        if (string.IsNullOrEmpty(path))
                            continue;
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (!asset)
                            continue;
                        if (!addressMap.TryGetValue(addressInfo.address, out var infos))
                            infos = addressMap[addressInfo.address] = new List<AddressInfo>();
                        infos.Add(addressInfo);
                    }
                    binaryWriter.Write((ushort)addressMap.Count);
                    foreach (var pair in addressMap)
                    {
                        var addresss = pair.Key;
                        //1.address
                        binaryWriter.Write(addresss);
                        //2.num
                        binaryWriter.Write((byte)pair.Value.Count);
                        foreach (var addressInfo in pair.Value)
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(addressInfo.guid);
                            if (string.IsNullOrEmpty(assetPath))
                                continue;
                            var bundleName = collector.GetCacheBundleName(addressInfo.address, addressInfo.flags, assetPath);

                            var flags = addressInfo.flags;
                            //3.flags
                            binaryWriter.Write(flags);
                            //4.abId
                            var id = System.Array.IndexOf(assetBundleArray, bundleName);
                            binaryWriter.Write((ushort)id);
                        }
                    }
                }
            }
        }

        public static string HashMd5File(string filePath,int length)
        {
            using (var md = MD5.Create())
            {
                using (var mem = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    byte[] hash = md.ComputeHash(mem);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < length / 2; i++)
                        sb.Append(hash[i].ToString("x2"));
                    return sb.ToString();
                }
            }
        }

    }
}

