//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************
using System.IO;
using System.Collections.Generic;

namespace UFrame.DressAB
{
    public class CatlogParser : System.IDisposable
    {
        private FileStream m_fileStream;
        private BinaryReader m_binaryReader;
        private long[] m_bundleAnchors;
        private BundleItem[] m_bundles;
        private Dictionary<string, long> m_addressAnchorMap;
        private Dictionary<long, AddressItem> m_addressItems;
        private Dictionary<long, ushort[]> m_addressFlagMap;

        public CatlogParser()
        {
            m_addressItems = new Dictionary<long, AddressItem>();
            m_addressFlagMap = new Dictionary<long, ushort[]>();
        }

        public void LoadFromFile(string filePath)
        {
            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
            }
            m_fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (m_fileStream != null)
            {
                m_binaryReader = new BinaryReader(m_fileStream, System.Text.Encoding.UTF8);
                var assetbundleNum = m_binaryReader.ReadUInt16();

                m_bundleAnchors = new long[assetbundleNum];
                m_bundles = new BundleItem[assetbundleNum];

                for (int i = 0; i < m_bundleAnchors.Length; i++)
                {
                    m_bundleAnchors[i] = m_fileStream.Position;
                    var strLength = m_binaryReader.ReadByte();//max 127
                    m_fileStream.Position += strLength;
                    var refNum = m_binaryReader.ReadByte();
                    m_fileStream.Position += refNum * 2;
                }

                m_addressAnchorMap = new Dictionary<string, long>();
                var addressNum = m_binaryReader.ReadUInt16();

                for (int addressline = 0; addressline < addressNum; addressline++)
                {
                    if (m_binaryReader.PeekChar() < 0)
                    {
                        UnityEngine.Debug.LogError("m_binaryReader.PeekChar() < 0!");
                        break;
                    }
                    var addressName = m_binaryReader.ReadString();
                    var anchor = m_fileStream.Position;
                    m_addressAnchorMap.Add(addressName, anchor);
                     var num = m_binaryReader.ReadByte();
                    var flagMap = m_addressFlagMap[anchor] = new ushort[num];
                    for (int i = 0; i < num; i++)
                    {
                        var flags = m_binaryReader.ReadUInt16();
                        flagMap[i] = flags;
                        m_fileStream.Position += 2;//abid
                    }
                }
            }
        }

        public List<AddressItem> GetAddressByFlags(ushort flags)
        {
            var addressList = new List<AddressItem>();
            var anchors = new List<long>();
            foreach (var addressPair in m_addressAnchorMap)
            {
                if (m_addressFlagMap.TryGetValue(addressPair.Value, out var flagMap))
                {
                    for (int i = 0; i < flagMap.Length; i++)
                    {
                        var currentFlags = flagMap[i];
                        if ((currentFlags & flags) != 0)
                        {
                            anchors.Add(i);
                        }
                    }
                }
                if (anchors.Count > 0)
                {
                    var addressItem = FindAddressItem(addressPair.Value);
                    for (int i = 0; i < anchors.Count; i++)
                    {
                        if (addressItem == null)
                            break;

                        var anchorIndex = anchors[i];
                        if (i == anchorIndex)
                        {
                            addressList.Add(addressItem);
                        }
                        addressItem = addressItem.next;
                    }
                }
                anchors.Clear();
            }
            return addressList;
        }

        public AddressItem BuildAddressItem(long anchor)
        {
            if (m_binaryReader != null)
            {
                m_binaryReader.BaseStream.Seek(anchor, SeekOrigin.Begin);
                var num = m_binaryReader.ReadByte();
                if (num <= 0)
                    return null;

                AddressItem rootItem = new AddressItem();
                AddressItem abItem = rootItem;
                for (int i = 0; i < num; i++)
                {
                    abItem.flags = m_binaryReader.ReadUInt16();
                    var abId = m_binaryReader.ReadUInt16();
                    abItem.bundleItem = GetBundleByIndex(abId);
                    if(i < num - 1)
                    {
                        abItem.next = new AddressItem();
                        abItem = abItem.next;
                    }
                }
                return rootItem;
            }
            return null;
        }

        public BundleItem GetBundleByIndex(ushort index)
        {
            if(m_bundleAnchors.Length <= index)
            {
                UnityEngine.Debug.LogError("GetBundleByIndex out range:" + index);
                return null;
            }

            var anchor = m_bundleAnchors[index];
            var bundleItem = m_bundles[index];
            if (bundleItem == null)
            {
                bundleItem = m_bundles[index] = new BundleItem();
                var lastPos = m_binaryReader.BaseStream.Position;
                m_binaryReader.BaseStream.Seek(anchor, SeekOrigin.Begin);
                bundleItem.bundleName = m_binaryReader.ReadString();
                var refNum = m_binaryReader.ReadByte();
                if (refNum > 0)
                {
                    bundleItem.refs = new BundleItem[refNum];
                    for (int i = 0; i < refNum; i++)
                    {
                        var subIndex = m_binaryReader.ReadUInt16();
                        bundleItem.refs[i] = GetBundleByIndex(subIndex);
                    }
                }
                m_binaryReader.BaseStream.Seek(lastPos, SeekOrigin.Begin);
            }
            UnityEngine.Debug.Assert(bundleItem != null,"bundleitem empty!");
            return bundleItem;
        }

        public bool ExistAddress(string address)
        {
            return m_addressAnchorMap.ContainsKey(address);
        }

        public AddressItem FindAddressItem(long anchor)
        {
            if (m_addressItems.TryGetValue(anchor, out var addressItem))
                return addressItem;
            addressItem = BuildAddressItem(anchor);
            if (addressItem != null)
            {
                m_addressItems[anchor] = addressItem;
            }
            return addressItem;
        }

        public AddressItem FindAddressItem(string address)
        {
            AddressItem addressItem = null;
            if (m_addressAnchorMap.TryGetValue(address, out var anchor))
            {
                return FindAddressItem(anchor);
            }
            return addressItem;
        }

        public AddressItem FindABItem(string address, ushort flags)
        {
            AddressItem rootItem = FindAddressItem(address);
            AddressItem resultItem = null;
            AddressItem abItem = rootItem;

            while (abItem != null)
            {
                if (flags == abItem.flags)
                {
                    resultItem = abItem;
                    break;
                }
                abItem = abItem.next;
            }

            if(resultItem == null)
            {
                abItem = rootItem;
                while (abItem != null)
                {
                    if (flags != 0 && (abItem.flags & flags) != 0)
                    {
                        resultItem = abItem;
                        break;
                    }
                    abItem = abItem.next;
                }
            }
            return resultItem ?? rootItem;
        }

        public void Dispose()
        {
            if (m_binaryReader != null)
                m_binaryReader.Dispose();
        }
    }
}

