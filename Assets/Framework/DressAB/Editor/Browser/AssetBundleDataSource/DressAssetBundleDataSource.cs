using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace UFrame.DressAssetBundle.Browser.AssetBundleDataSource
{
    internal class DressAssetBundleDataSource : ABDataSource
    {
        private string[] emptyArray = new string[0];

        public AddressDefineObject addressDefine { get;private set; }

        public static List<ABDataSource> CreateDataSources()
        {
            var retList = new List<ABDataSource>();
            var guids = AssetDatabase.FindAssets("t:AddressDefineObject");
            foreach (var guid in guids)
            {
                var op = new DressAssetBundleDataSource();
                var path = AssetDatabase.GUIDToAssetPath(guid);
                op.addressDefine = AssetDatabase.LoadAssetAtPath<AddressDefineObject>(path);
                retList.Add(op);
            }
          
            return retList;
        }

        public string Name
        {
            get
            {
                if (!addressDefine)
                    return "Dress";
                return addressDefine.name;
            }
        }

        public string ProviderName
        {
            get
            {
                return "DressAB";
            }
        }

        public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
            if (addressDefine != null)
            {
                var guids = new List<string>();
                guids.AddRange(addressDefine.addressList.FindAll(x => x.address == assetBundleName && x.active).Select(x => x.guid));
                var refBundles = addressDefine.refBundleList.FindAll(x => x.address == assetBundleName);
                foreach (var refBundle in refBundles)
                    guids.AddRange(refBundle.guids);
                var resPaths = new List<string>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path))
                        resPaths.Add(path);
                }
                return resPaths.ToArray();
            }
            return emptyArray;
        }

        public string GetAssetBundleName(string assetPath)
        {
            if (addressDefine != null)
            {
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
                var info = addressDefine.addressList.Find(x => x.guid == guid && x.active);
                if (info != null)
                    return info.address;
                var refBundle = addressDefine.refBundleList.Find(x => x.guids.Contains(guid));
                if (refBundle != null)
                    return refBundle.address;
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
            //return AssetDatabase.GetImplicitAssetBundleName (assetPath);
            return "";
        }

        public string[] GetAllAssetBundleNames()
        {
            //return AssetDatabase.GetAllAssetBundleNames ();
            var assetBundleNames = new List<string>();
            if (addressDefine != null)
            {
                assetBundleNames.AddRange(addressDefine.addressList.Where(x => x.active).Select(x => x.address).ToArray());
                assetBundleNames.AddRange(addressDefine.refBundleList.Select(x => x.address).ToArray());
                return assetBundleNames.ToArray();
            }
            return emptyArray;
        }

        public bool IsReadOnly()
        {
            return false;
        }

        public void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variantName)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (addressDefine != null)
            {
                var address = bundleName;
                if (!string.IsNullOrEmpty(variantName))
                    address = $"{address}.{variantName}".Replace("\\", "/").Replace("/", "_");

                bool processed = false;
                var addressInfo = addressDefine.addressList.Find(x => x.guid == guid);
                if (addressInfo != null && string.IsNullOrEmpty(address))
                {
                    addressDefine.addressList.Remove(addressInfo);
                    EditorUtility.SetDirty(addressDefine);
                    processed = true;
                }
                else if (addressInfo != null)
                {
                    addressInfo.address = address;
                    EditorUtility.SetDirty(addressDefine);
                    processed = true;
                }

                AssetRefBundle refBundle = null;
                if (!processed && addressInfo == null)
                {
                    refBundle = addressDefine.refBundleList.Find(x => x.guids.Contains(guid));
                    if (refBundle != null && string.IsNullOrEmpty(address))
                    {
                        refBundle.guids.Remove(guid);
                        if (refBundle.guids.Count == 0)
                            addressDefine.refBundleList.Remove(refBundle);
                        EditorUtility.SetDirty(addressDefine);
                        processed = true;
                    }
                    else if(refBundle != null)
                    {
                        if(refBundle.address != address)
                        {
                            var targetBundle = addressDefine.refBundleList.Find(x => x.address == address);
                            if(targetBundle != null)
                            {
                                targetBundle.guids.Add(guid);
                                refBundle.guids.Remove(guid);
                                if(refBundle.guids.Count == 0)
                                {
                                    addressDefine.refBundleList.Remove(refBundle);
                                }
                            }
                            else
                            {
                                if (refBundle.guids.Count == 1)
                                    refBundle.address = address;
                                else
                                {
                                    refBundle.guids.Remove(guid);
                                    if (refBundle.guids.Count == 0)
                                        addressDefine.refBundleList.Remove(refBundle);

                                    var newbundle = new AssetRefBundle();
                                    newbundle.address = address;
                                    newbundle.guids.Add(guid);
                                    addressDefine.refBundleList.Add(newbundle);
                                }
                            }
                            EditorUtility.SetDirty(addressDefine);
                        }
                        processed = true;
                    }
                }

                if (!processed && refBundle == null)
                {
                    refBundle = new AssetRefBundle();
                    refBundle.address = address;
                    refBundle.guids.Add(guid);
                    addressDefine.refBundleList.Add(refBundle);
                    EditorUtility.SetDirty(addressDefine);
                    processed = true;
                }
            }
            if (addressDefine)
                EditorUtility.SetDirty(addressDefine);
            //AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, variantName);
        }

        public void RemoveUnusedAssetBundleNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
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

        public bool IsRefBundle(string assetbundleName)
        {
            if (addressDefine != null)
            {
                return addressDefine.refBundleList.FindIndex(x => x.address == assetbundleName) > -1;
            }
            return false;
        }
    }
}
