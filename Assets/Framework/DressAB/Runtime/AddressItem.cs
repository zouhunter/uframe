//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述： address 地址描述

//* ************************************************************************************
namespace UFrame.DressAssetBundle
{
    public class AddressItem
    {
        public ushort flags;
        public BundleItem bundleItem;
        public AddressItem next;
    }
    public class BundleItem
    {
        public string bundleName;
        public BundleItem[] refs;
    }
}

