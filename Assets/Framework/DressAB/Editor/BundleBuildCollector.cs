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
using UnityEditorInternal;
using System.Reflection;

namespace UFrame.DressAB.Editors
{
    public class BundleBuildCollector
    {
        public AddressDefineObject defObj;
        public List<RenameInfo> renameInfos;

        public BundleBuildCollector(AddressDefineObject defObj)
        {
            this.defObj = defObj;
            renameInfos = new List<RenameInfo>();
        }

        public void SplitGroupBundles()
        {
            var activeAddressList = defObj.addressList.FindAll(x => x.active);
            activeAddressList.Sort((x, y) => string.Compare(x.address + x.flags, y.address + y.flags));

            foreach (var item in activeAddressList)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(item.guid);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                var objItem = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (objItem == null)
                    continue;

                if (item.split && System.IO.Directory.Exists(assetPath))
                {
                    var entries = System.IO.Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);

                    foreach (var entr in entries)
                    {
                        if (entr.EndsWith(".meta"))
                            continue;

                        var entrNoDress = entr;
                        if (System.IO.File.Exists(entr))
                            entrNoDress = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(entr), System.IO.Path.GetFileNameWithoutExtension(entr));

                        var refdress = System.IO.Path.GetRelativePath(assetPath, entrNoDress);
                        var refPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, entr);
                        if (!ExistSingleBundleAsset(refPath))
                        {
                            var addressInfo = new AddressInfo();
                            addressInfo.address = $"{item.address}/{refdress.Replace('\\','/')}";
                            addressInfo.flags = item.flags;
                            addressInfo.active = true;
                            addressInfo.split = false;
                            addressInfo.guid = AssetDatabase.GUIDFromAssetPath(refPath).ToString();
                            defObj.addressList.Add(addressInfo);
                            EditorUtility.SetDirty(defObj);
                        }
                    }
                }
            }
            defObj.addressList.RemoveAll(x => x.split);
            EditorUtility.SetDirty(defObj);
        }

        public AssetBundleBuild[] Collect()
        {
            if (defObj.autoSliceBundleOnBuild)
            {
                AutoSliceBundles(defObj);
            }

            var activeAddressList = defObj.addressList.FindAll(x => x.active);
            activeAddressList.Sort((x, y) => string.Compare(x.address + x.flags, y.address + y.flags));

            var abbuild = new List<AssetBundleBuild>();
            var assetDic = new Dictionary<string, List<string>>();
            var renameMap = new Dictionary<string, List<string>>();
            HashSet<string> collectedAssets = new HashSet<string>();

            foreach (var item in activeAddressList)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(item.guid);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                var objItem = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (objItem == null)
                    continue;

                var bundleName = GetBuildBundleName(item.address, item.flags, assetPath, defObj.abNameLengthMax, renameMap);
                if (!assetDic.TryGetValue(bundleName, out var assetList))
                {
                    assetList = new List<string>();
                    assetDic[bundleName] = assetList;
                }
                CollectAssets(assetList, assetPath, collectedAssets);
            }
            foreach (var item in defObj.refBundleList)
            {
                var bundleName = GetBuildBundleName(item.address, 0, null, defObj.abNameLengthMax, renameMap);
                if (!assetDic.TryGetValue(bundleName, out var assetList))
                {
                    assetList = new List<string>();
                    assetDic[bundleName] = assetList;
                }
                foreach (var guid in item.guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    CollectAssets(assetList, assetPath, collectedAssets);
                }
            }

            CollectSubAssetBundles(assetDic, defObj.abNameLengthMax, renameMap, collectedAssets);
            foreach (var pair in assetDic)
            {
                var build = new AssetBundleBuild();
                build.assetBundleName = pair.Key;
                build.assetNames = pair.Value.ToArray();
                abbuild.Add(build);
                //Debug.LogError(pair.Key + "->" + string.Join('|', build.assetNames));
            }
            return abbuild.ToArray();
        }

        private void SetCacheBundleName(string address,ushort flags,string assetPath,string generateBundleName)
        {
            var renameInfo = renameInfos.Find(x => x.address == address && x.flags == flags && x.assetPath == assetPath);
            if (renameInfo == null)
            {
                renameInfo = new RenameInfo();
                renameInfos.Add(renameInfo);

                renameInfo.address = address;
                renameInfo.flags = flags;
                renameInfo.assetPath = assetPath;
            }
            renameInfo.generateBundleName = generateBundleName;
        }

        public string GetCacheBundleName(string address,ushort flags,string assetPath)
        {
            var renameInfo = renameInfos.Find(x => x.address == address && x.flags == flags && x.assetPath == assetPath);
            if (renameInfo == null)
                return null;
            return renameInfo.generateBundleName;
        }

        private bool ExistSingleBundleAsset(string path)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            var inAddressList = defObj.addressList.Find(x => x.guid == guid && x.active) != null;
            var inRefBundleList = defObj.refBundleList.Find(x => x.guids.Contains(guid)) != null;
            return inAddressList || inRefBundleList;
        }

        public void CollectAssets(List<string> targetBundleAssetList, string assetPath, HashSet<string> collectedAssets)
        {
            if (System.IO.Directory.Exists(assetPath))
            {
                var files = System.IO.Directory.GetFiles(assetPath, "*");
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta"))
                        continue;

                    var refPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, file);
                    var subObj = AssetDatabase.LoadAssetAtPath<Object>(refPath);
                    if (!(subObj is DefaultAsset) && !targetBundleAssetList.Contains(file) && !collectedAssets.Contains(file) && !ExistSingleBundleAsset(refPath))
                    {
                        targetBundleAssetList.Add(file);
                        collectedAssets.Add(file);
                    }
                }
            }
            else
            {
                if (!targetBundleAssetList.Contains(assetPath) && !collectedAssets.Contains(assetPath))
                {
                    targetBundleAssetList.Add(assetPath);
                    collectedAssets.Add(assetPath);
                }
            }
        }

        public string GetBuildBundleName(string address, ushort flags, string assetPath, int fileNameMax, Dictionary<string, List<string>> stripNameCountMap)
        {
            string assetBundleName = GetCacheBundleName(address, flags, assetPath);
            if (!string.IsNullOrEmpty(assetBundleName))
                return assetBundleName;

            if (!string.IsNullOrEmpty(assetPath))
            {
                var importer = AssetImporter.GetAtPath(assetPath);
                if (!string.IsNullOrEmpty(importer.assetBundleName))
                {
                    var importerName = importer.assetBundleName;
                    if (!string.IsNullOrEmpty(importer.assetBundleVariant))
                    {
                        importerName = $"{importerName}.{importer.assetBundleVariant}";
                    }
                    assetBundleName = MakeShootName(importerName, fileNameMax, stripNameCountMap);
                }
            }
            if (string.IsNullOrEmpty(assetBundleName))
            {
                var variant = GetFlagStr(flags);
                if (!string.IsNullOrEmpty(variant))
                {
                    assetBundleName = MakeShootName($"{address}_{variant}", fileNameMax, stripNameCountMap);
                }
                else
                {
                    assetBundleName = MakeShootName(address, fileNameMax, stripNameCountMap);
                }
            }
            assetBundleName = assetBundleName.Replace("\\", "/").Replace("/", "_");
            SetCacheBundleName(address, flags, assetPath, assetBundleName);
            return assetBundleName;
        }

        public string GetFlagStr(ushort flags)
        {
            var bytes = System.BitConverter.GetBytes((ushort)flags);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString().TrimEnd('0');
        }

        public string MakeShootName(string name, int maxLength, Dictionary<string, List<string>> stripNameCountMap)
        {
            name = name.ToLower();
            if (maxLength >= name.Length)
                return name;
            name = SplitBestName(name);
            var halfLowLength = Mathf.CeilToInt(maxLength * 0.5f) - 1;
            var halfMaxLength = Mathf.FloorToInt(maxLength * 0.5f);
            var partA = name.Substring(0, halfLowLength);
            var partB = name.Substring(name.Length - halfMaxLength, halfMaxLength);
            var tempName = partA + partB;
            if (!stripNameCountMap.TryGetValue(tempName, out var originList))
            {
                originList = new List<string>();
                stripNameCountMap[tempName] = originList;
            }
            if (!originList.Contains(name))
            {
                originList.Add(name);
            }
            var index = originList.IndexOf(name);
            if (index == 0)
                return $"{partA}_{partB}";
            return $"{partA}_{index}_{partB}";
        }

        public void AutoSliceBundles(AddressDefineObject defObj)
        {
            int fileSizeMin = defObj.singleBundlefileSizeMin;
            var addressInfos = defObj.addressList;
            Debug.Log($"start AutoSliceBundles," + addressInfos.Count);
            HashSet<string> rootAssetPaths = new HashSet<string>();
            foreach (var addressInfo in addressInfos)
            {
                if (!addressInfo.active)
                    continue;
                var path = AssetDatabase.GUIDToAssetPath(addressInfo.guid);
                var rootObj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (!rootObj)
                    continue;

                if (Directory.Exists(path))
                {
                    var files = System.IO.Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach (var item in files)
                    {
                        rootAssetPaths.Add(item);
                    }
                }
                else
                {
                    rootAssetPaths.Add(path);
                }
            }

            Dictionary<string, HashSet<string>> refAssetUseMap = new Dictionary<string, HashSet<string>>();
            foreach (var assetPath in rootAssetPaths)
            {
                var refPaths = AssetDatabase.GetDependencies(assetPath, true);

                foreach (var refPath in refPaths)
                {
                    if (!refAssetUseMap.TryGetValue(refPath, out var parentList))
                    {
                        parentList = new HashSet<string>();
                        refAssetUseMap[refPath] = parentList;
                    }
                    if (assetPath != refPath)
                    {
                        parentList.Add(assetPath);
                    }
                }
            }
            Dictionary<string, HashSet<string>> refAssetDirectUseMap = new Dictionary<string, HashSet<string>>();
            BuildAssetRefMap(rootAssetPaths, refAssetDirectUseMap);

            Dictionary<string, string> assetGroupMap = new Dictionary<string, string>();
            foreach (var assetPath in refAssetDirectUseMap.Keys)
            {
                var bundleSize = SimulateCheckBundleSize(assetPath, true);
                if (bundleSize > defObj.singleBundlefileSizeMax)
                {
                    Debug.Log("big file ignore group:" + assetPath);
                    continue;
                }

                var group = GenerateBuildAssetGroup(assetPath, refAssetDirectUseMap);
                if (!string.IsNullOrEmpty(group))
                {
                    assetGroupMap[assetPath] = group;
                }
            }
            float viewProgress = 0;
            foreach (var refPair in refAssetUseMap)
            {
                var assetPath = refPair.Key;
                var parentList = refPair.Value;
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();

                if (parentList.Count >= defObj.useCoundMin && !rootAssetPaths.Contains(assetPath))
                {
                    var importer = AssetImporter.GetAtPath(assetPath);
                    if (!string.IsNullOrEmpty(importer.assetBundleName))
                        continue;

                    var ext = System.IO.Path.GetExtension(assetPath);
                    if (!defObj.objExt.Contains(ext.ToLower()))
                        continue;

                    var address = GetImplicitAddress(assetPath);
                    if (!string.IsNullOrEmpty(address))
                        continue;

                    var inAssetBundle = GetImplicitAssetBundleName(assetPath);
                    if (!string.IsNullOrEmpty(inAssetBundle))
                        continue;

                    var refItem = defObj.refBundleList.Find(x => x.guids.Contains(guid));
                    if (refItem == null)
                    {
                        if (assetGroupMap.TryGetValue(assetPath, out var group))
                        {
                            var addressGroup = group.Replace('.', '_');
                            refItem = defObj.refBundleList.Find(x => x.address == addressGroup);
                            if (refItem == null)
                            {
                                refItem = new AssetRefBundle();
                                refItem.address = addressGroup;
                                defObj.refBundleList.Add(refItem);
                            }
                            refItem.guids.Add(guid);
                        }
                        else
                        {
                            var bundleSize = SimulateCheckBundleSize(assetPath, false);
                            if (bundleSize < defObj.singleBundlefileSizeMin)
                                continue;

                            refItem = new AssetRefBundle();
                            refItem.guids.Add(guid);
                            refItem.address = AssetAddressGUI.GetPreviewAddress(assetPath,defObj);
                            defObj.refBundleList.Add(refItem);
                        }
                        EditorUtility.SetDirty(defObj);
                    }
                    Debug.Log($"auto slicing ,{assetPath} used by:" + string.Join("|", new List<string>(parentList).ToArray()));
                    EditorUtility.DisplayProgressBar("auto slicing...", assetPath, (viewProgress++ / refAssetUseMap.Count));
                }
                else
                {
                    Debug.Log("single use ignore:" + assetPath);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        private string GenerateBuildAssetGroup(string assetPath, Dictionary<string, HashSet<string>> directRefAssetUseMap)
        {
            if (!directRefAssetUseMap.TryGetValue(assetPath, out var parentList) || parentList.Count == 0)
            {
                return null;
            }

            List<string> usedList = null;
            foreach (var parent in parentList)
            {
                string groupName = null;
                if (parent.EndsWith(".prefab") || parent.EndsWith(".unity"))
                {
                    groupName = parent + "_sub";
                }
                else
                {
                    groupName = GenerateBuildAssetGroup(parent, directRefAssetUseMap);
                }

                if (!string.IsNullOrEmpty(groupName))
                {
                    if (usedList == null)
                        usedList = new List<string>();
                    usedList.Add(groupName);
                }
            }

            if (usedList != null && usedList.Count > 0)
            {
                return usedList[0];
            }
            else
            {
                return null;
            }
        }

        private void BuildAssetRefMap(IEnumerable<string> assetPaths, Dictionary<string, HashSet<string>> refAssetUseMap)
        {
            foreach (var assetPath in assetPaths)
            {
                var refPaths = AssetDatabase.GetDependencies(assetPath, false);
                List<string> subRefPaths = new List<string>();
                foreach (var refPath in refPaths)
                {
                    var ext = System.IO.Path.GetExtension(refPath).ToLower();
                    if (!defObj.objExt.Contains(ext))
                        continue;

                    if (!refAssetUseMap.TryGetValue(refPath, out var parentList))
                    {
                        parentList = new HashSet<string>();
                        refAssetUseMap[refPath] = parentList;
                    }
                    if (assetPath != refPath && !parentList.Contains(assetPath))
                    {
                        parentList.Add(assetPath);
                        subRefPaths.Add(refPath);
                    }
                }
                BuildAssetRefMap(subRefPaths, refAssetUseMap);
            }
        }

        private string SplitBestName(string assetPath)
        {
            foreach (var item in defObj.objExt)
            {
                if (assetPath.EndsWith("_" + item))
                {
                    assetPath = assetPath.Substring(0, assetPath.Length - item.Length - 1);
                    break;
                }
            }
            var maxNum = 32;
            var address = assetPath.Replace("\\", "/").Replace(".", "_");
            var array = address.Split("/");
            var nameArray = new List<string>();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                var partName = array[i];
                nameArray.Insert(0, partName);
                maxNum -= partName.Length;
                if (partName == "Assets" || partName == "Packages" || maxNum < 0)
                    break;
            }
            return string.Join('/', nameArray);
        }

        public string GetAssetAddress(string assetPath)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            if (guid == null)
                return null;
            var guidStr = guid.ToString();
            var info = defObj.addressList.Find(x => x.guid == guidStr);
            if (info != null)
            {
                return info.address;
            }
            var refInfo = defObj.refBundleList.Find(x => x.guids.Contains(guidStr));
            if (refInfo != null)
            {
                return refInfo.address;
            }
            return null;
        }

        public string GetImplicitAddress(string assetPath)
        {
            var address = GetAssetAddress(assetPath);
            if (!string.IsNullOrEmpty(address))
                return address;

            var folder = assetPath;
            var projectFolder = System.IO.Path.GetFullPath(System.Environment.CurrentDirectory);
            while (!string.IsNullOrEmpty(folder) && System.IO.Path.GetFullPath(folder) != projectFolder)
            {
                address = GetAssetAddress(folder);
                if (!string.IsNullOrEmpty(address))
                    return address;
                folder = System.IO.Path.GetDirectoryName(folder);
            }
            return "";
        }

        public string GetImplicitAssetBundleName(string assetPath)
        {
            var assetImporter = AssetImporter.GetAtPath(assetPath);
            if (assetImporter != null)
                return assetImporter.assetBundleName;
            var folder = assetPath;
            var projectFolder = System.IO.Path.GetFullPath(System.Environment.CurrentDirectory);
            while (!string.IsNullOrEmpty(folder) && System.IO.Path.GetFullPath(folder) != projectFolder)
            {
                var importer = AssetImporter.GetAtPath(folder);
                var bundleName = importer.assetBundleName;
                if (!string.IsNullOrEmpty(bundleName))
                    return bundleName;
                folder = System.IO.Path.GetDirectoryName(folder);
            }
            return "";
        }

        private static object InvokeInternalAPI(string type, string method, params object[] parameters)
        {
            var assembly = typeof(AssetDatabase).Assembly;
            var custom = assembly.GetType(type);
            var methodInfo = custom.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            return methodInfo != null ? methodInfo.Invoke(null, parameters) : 0;
        }

        private int GetFileBuildSize(string assetPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            int fileSize = 0;
            if (importer is TextureImporter)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                if (texture != null)
                    fileSize = (int)InvokeInternalAPI("UnityEditor.TextureUtil", "GetStorageMemorySize", texture);
                else
                    Debug.LogError("faild get texture:" + assetPath);
            }
            if (fileSize == 0 && System.IO.File.Exists(assetPath))
            {
                fileSize = System.IO.File.ReadAllBytes(assetPath).Length;
            }
            return (fileSize / 1024);
        }

        public int SimulateCheckBundleSize(string assetPath, bool topFileOnly)
        {
            var size = GetFileBuildSize(assetPath);
            if (topFileOnly)
                return size;
            var objRefs = AssetDatabase.GetDependencies(assetPath, true);
            foreach (var fileItem in objRefs)
            {
                size += GetFileBuildSize(assetPath);
            }
            return size;
        }

        private void CollectSubAssetBundles(Dictionary<string, List<string>> bundleBuilds, int fileNameMax, Dictionary<string, List<string>> stripNameCountMap, HashSet<string> collectedAssets)
        {
            var originList = new List<string>();
            foreach (var pair in bundleBuilds)
                originList.AddRange(pair.Value);
            foreach (var rootAssets in originList)
            {
                var subAssetNames = AssetDatabase.GetDependencies(rootAssets, true);
                foreach (var subAsset in subAssetNames)
                {
                    if (!originList.Contains(subAsset))
                    {
                        var importer = AssetImporter.GetAtPath(subAsset);
                        var assetBundleName = importer.assetBundleName;
                        if (string.IsNullOrEmpty(assetBundleName))
                            continue;
                        if (!string.IsNullOrEmpty(importer.assetBundleVariant))
                        {
                            assetBundleName = $"{assetBundleName}.{importer.assetBundleVariant}";
                        }
                        assetBundleName = MakeShootName(assetBundleName, fileNameMax, stripNameCountMap);
                        if (!bundleBuilds.TryGetValue(assetBundleName, out var assetlist))
                        {
                            assetlist = bundleBuilds[assetBundleName] = new List<string>();
                        }
                        CollectAssets(assetlist, subAsset, collectedAssets);
                    }
                }

            }

        }

    }
}