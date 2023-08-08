//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-19
//* 描    述： 

//* ************************************************************************************
#if UNITY_EDITOR
using System.Collections.Generic;

namespace UFrame.DressAssetBundle {

    [System.Serializable]
    public class AddressInfo
    {
        public string address;
        public string guid;
        public ushort flags;
        public bool active = true;
        public bool split;
    }

    [System.Serializable]
    public class AssetRefBundle
    {
        public string address;
        public List<string> guids = new List<string>();
    }
    public class RenameInfo
    {
        public string address;
        public string assetPath;
        public ushort flags;
        public string generateBundleName;
    }
}
#endif