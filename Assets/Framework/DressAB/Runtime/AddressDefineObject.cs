//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-17 10:33:11
//* 描    述： AddressDefineObject

//* ************************************************************************************
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UFrame.DressAssetBundle
{
    public class AddressDefineObject : ScriptableObject
    {
        public int abNameLengthMax = 10;
        public int hashLengthMax = 10;
        public bool autoSliceBundleOnBuild = true;
        public bool editable = true;
        [InspectorName("singleBundlefileSizeMin(kb)")]
        public int singleBundlefileSizeMin = 512;
        [InspectorName("singleBundlefileSizeMax(kb)")]
        public int singleBundlefileSizeMax = 1024;//2560k
        public int useCoundMin = 2;
        public List<string> objExt = new List<string>() { ".prefab", ".unity", ".asset" , ".fbx",".obj", ".exr", ".obj", ".json", ".csv", ".png", ".jpg", ".jpeg",".exr" ,".tga", ".txt" };
        public string[] flags = new string[16];
        public BuildAssetBundleOptions options;
        public List<AddressInfo> addressList = new List<AddressInfo>();
        public List<AssetRefBundle> refBundleList = new List<AssetRefBundle>();
    }
}
#endif
