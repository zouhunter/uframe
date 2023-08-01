//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-08-01
//* 描    述： 本地数据控制器

//* ************************************************************************************
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace UFame.Storage
{
    public class StorageCtrl:System.IDisposable
    {
        private Dictionary<string, StorageInfo> infos;
        private string m_currentStoragePath;
        private HashSet<StorageInfo> m_changedInfos;
        private ushort m_minInfoSize = 32;//key+value
        private string m_storageDir = "Storage";
        public ushort minInfoSize
        {
            get
            {
                return m_minInfoSize;
            }
            set
            {
                if (value >= 16)
                    m_minInfoSize = value;
                else
                    Debug.LogError("min info size must bigger than 16");
            }
        }
        public string storageDirName
        {
            get
            {
                return m_storageDir;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_storageDir = value;
            }
        }
        public bool delySave { get; set; }

        public StorageCtrl()
        {
            infos = new Dictionary<string, StorageInfo>();
            m_changedInfos = new HashSet<StorageInfo>();
        }

        public void Init(string name)
        {
            try
            {
                DecodeFromFile(name);
            }
            catch (System.Exception e)
            {
                Debug.Log("failed decode user data:" + name);
                Debug.LogException(e);
            }
            m_changedInfos.Clear();
        }

        public void Dispose()
        {
            SaveToFile();
            m_changedInfos?.Clear();
            infos?.Clear();
        }

        private string GetStoragePath(string userid)
        {
            var dir = $"{Application.persistentDataPath}/{storageDirName}";
            System.IO.Directory.CreateDirectory(dir);
            return $"{dir}/{userid}.bytes";
        }

        private void DecodeFromFile(string userid)
        {
            m_currentStoragePath = GetStoragePath(userid);
            if (File.Exists(m_currentStoragePath))
            {
                infos.Clear();
                using (var fileStream = new System.IO.FileStream(m_currentStoragePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8))
                    {
                        var dataLength = reader.ReadUInt16();
                        for (int i = 0; i < dataLength; i++)
                        {
                            var info = new StorageInfo();
                            info.anchor = fileStream.Position;
                            ReadInfo(reader, info);
                            fileStream.Seek(info.anchor + info.size, SeekOrigin.Begin);
                            infos[info.key] = info;
                        }
                    }
                }
            }
        }
        private bool CheckNeedRebuild()
        {
            bool needRebuild = false;
            foreach (var info in m_changedInfos)
            {
                var needSize = GetInfoNeedSize(info);
                while (info.size < needSize)
                {
                    info.size += m_minInfoSize;
                    if (info.anchor >= 0)
                    {
                        needRebuild = true;
                    }
                }
            }
            return needRebuild;
        }
        private void SaveToFile()
        {
            if (m_changedInfos.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(m_currentStoragePath))
            {
                Debug.LogError("storage path empty!");
                return;
            }

            using (var fileStream = new System.IO.FileStream(m_currentStoragePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
            {
                var bytes = new byte[m_minInfoSize];
                using (var writer = new BinaryWriter(fileStream, System.Text.Encoding.UTF8))
                {
                    if (CheckNeedRebuild())
                    {
                        var dataLen = infos.Count;
                        writer.Write((ushort)dataLen);
                        foreach (var pair in infos)
                        {
                            var info = pair.Value;
                            info.anchor = fileStream.Position;
                            WriteInfo(writer, info);
                            long sizeLeft = info.size + info.anchor - fileStream.Position;
                            AppendSeat(writer, bytes, sizeLeft);
                        }
                    }
                    else
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        var dataLen = infos.Count;
                        writer.Write((ushort)dataLen);
                        foreach (var info in m_changedInfos)
                        {
                            if (info.anchor < 0)
                            {
                                fileStream.Seek(0, SeekOrigin.End);
                                info.anchor = fileStream.Position;
                                WriteInfo(writer, info);
                                long sizeLeft = info.size + info.anchor - fileStream.Position;
                                AppendSeat(writer, bytes, sizeLeft);
                            }
                            else
                            {
                                fileStream.Seek(info.anchor, SeekOrigin.Begin);
                                WriteInfo(writer, info);
                            }

                        }
                    }
                }
            }
            m_changedInfos.Clear();
        }
        private void AppendSeat(BinaryWriter writer, byte[] bytes, long sizeLeft)
        {
            while (sizeLeft > 0)
            {
                var sizeCharge = System.Math.Min(m_minInfoSize, sizeLeft);
                writer.Write(bytes, 0, (int)sizeCharge);
                sizeLeft -= sizeCharge;
            }
        }
        private int GetInfoNeedSize(StorageInfo info)
        {
            var infoSizeNeed = 2;//size
            infoSizeNeed += info.key == null ? 1 : System.Text.Encoding.UTF8.GetBytes(info.key).Length + 2;
            infoSizeNeed += 1;//type
            if (info.type == 0)//string
            {
                infoSizeNeed += info.value == null ? 1 : System.Text.Encoding.UTF8.GetBytes(info.value).Length + 2;
            }
            else if (info.type == 1)//int
            {
                infoSizeNeed += 4;
            }
            else if (info.type == 2)//float
            {
                infoSizeNeed += 4;
            }
            else if (info.type == 3)//byte
            {
                infoSizeNeed += 1;
            }
            else if (info.type == 4)//long
            {
                infoSizeNeed += 8;
            }
            else if (info.type == 5)//double
            {
                infoSizeNeed += 8;
            }
            return infoSizeNeed;
        }
        private void ReadInfo(BinaryReader reader, StorageInfo info)
        {
            info.size = reader.ReadUInt16();
            info.key = reader.ReadString();
            info.type = reader.ReadByte();
            if (info.type == 0)//str
            {
                info.value = reader.ReadString();
            }
            else if (info.type == 1)//int
            {
                info.value = reader.ReadInt32().ToString();
            }
            else if (info.type == 2)//float
            {
                info.value = reader.ReadSingle().ToString();
            }
            else if (info.type == 3)//byte
            {
                info.value = reader.ReadByte().ToString();
            }
            else if (info.type == 4)//long
            {
                info.value = reader.ReadInt64().ToString();
            }
            else if (info.type == 5)//double
            {
                info.value = reader.ReadDouble().ToString();
            }
        }
        private void WriteInfo(BinaryWriter writer, StorageInfo info)
        {
            writer.Write(info.size);
            writer.Write(info.key);
            writer.Write(info.type);
            if (info.type == 0)
            {
                writer.Write(info.value);
            }
            else if (info.type == 1)
            {
                writer.Write(GetInt(info.value));
            }
            else if (info.type == 2)
            {
                writer.Write(GetSingle(info.value));
            }
            else if (info.type == 3)
            {
                writer.Write(GetByte(info.value));
            }
            else if (info.type == 4)
            {
                writer.Write(GetLong(info.value));
            }
            else if (info.type == 5)
            {
                writer.Write(GetDouble(info.value));
            }
        }
        private int GetInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            int.TryParse(value, out var intValue);
            return intValue;
        }
        private float GetSingle(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            float.TryParse(value, out var floatValue);
            return floatValue;
        }
        private byte GetByte(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "0")
                return 0;
            return 1;
        }
        private double GetDouble(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            long.TryParse(value, out var doubleValue);
            return doubleValue;
        }
        private long GetLong(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            long.TryParse(value, out var longValue);
            return longValue;
        }
        public void SetValue(string key, string value)
        {
            SetValue(key, 0, value);
        }
        public void SetValue(string key, int value)
        {
            SetValue(key, 1, value);
        }
        public void SetValue(string key, float value)
        {
            SetValue(key, 2, value);
        }
        public void SetValue(string key, bool value)
        {
            SetValue(key, 3, value ? 1 : 0);
        }
        public void SetValue(string key, byte value)
        {
            SetValue(key, 3, value);
        }
        public void SetValue(string key, long value)
        {
            SetValue(key, 4, value);
        }
        public void SetValue(string key, double value)
        {
            SetValue(key, 5, value);
        }
        private void SetValue(string key, byte type, object value)
        {
            if (!infos.TryGetValue(key, out var info))
            {
                info = new StorageInfo();
                info.anchor = -1;
                info.key = key;
                info.type = type;
                info.value = value.ToString();
                info.size = (ushort)Mathf.Max(m_minInfoSize, GetInfoNeedSize(info));
                infos.Add(info.key, info);
                m_changedInfos.Add(info);
            }
            else
            {
                var nextValue = value.ToString();
                if (nextValue != info.value || info.type != type)
                {
                    info.value = value.ToString();
                    info.type = type;
                    m_changedInfos.Add(info);
                }
            }
            if(!delySave)
            {
                SaveToFile();
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = info.value;
                return true;
            }
            value = null;
            return false;
        }
        public bool TryGetValue(string key, out int value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = GetInt(info.value);
                return true;
            }
            value = 0;
            return false;
        }
        public bool TryGetValue(string key, out float value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = GetSingle(info.value);
                return true;
            }
            value = 0;
            return false;
        }
        public bool TryGetValue(string key, out byte value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = GetByte(info.value);
                return true;
            }
            value = 0;
            return false;
        }
        public bool TryGetValue(string key, out double value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = GetDouble(info.value);
                return true;
            }
            value = 0;
            return false;
        }
        public bool TryGetValue(string key, out long value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = GetLong(info.value);
                return true;
            }
            value = 0;
            return false;
        }
        public bool TryGetValue(string key, out bool value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                value = GetByte(info.value) != 0;
                return true;
            }
            value = false;
            return false;
        }
        public bool TryGetValue<V>(string key, out V value)
        {
            if (infos.TryGetValue(key, out var info))
            {
                var type = typeof(V);
                if (type.IsValueType && !string.IsNullOrEmpty(info.value))
                    value = (V)System.Convert.ChangeType(info.value, type);
                else
                    value = default(V);
                return true;
            }
            value = default(V);
            return false;
        }
    }
}

