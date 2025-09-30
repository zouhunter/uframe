//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-08-01
//* 描    述： 本地数据控制器

//* ************************************************************************************
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Storage
{
    public class StorageCtrl:System.IDisposable
    {
        private Dictionary<string, StorageInfo> infos;
        private string m_currentStoragePath;
        private HashSet<StorageInfo> m_changedInfos;
        private ushort m_minInfoSize = 32;//key+value
        private string m_storageDir = "Storage";
        private bool m_sourceError;
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

        public void InitByName(string name)
        {
            try
            {
                var dir = $"{Application.persistentDataPath}/{storageDirName}";
                System.IO.Directory.CreateDirectory(dir);
                DecodeFromFile($"{dir}/{name}.bytes");
            }
            catch (System.Exception e)
            {
                Debug.Log("failed decode user data:" + name);
                Debug.LogException(e);
            }
            m_changedInfos.Clear();
        }
        public void InitByPath(string filePath)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(filePath);
                System.IO.Directory.CreateDirectory(dir);
                DecodeFromFile(filePath);
            }
            catch (System.Exception e)
            {
                Debug.Log("failed decode user data:" + filePath);
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


        private void DecodeFromFile(string path)
        {
            m_currentStoragePath = path;
            if (File.Exists(m_currentStoragePath))
            {
                infos.Clear();
                m_sourceError = false;
                using (var fileStream = new System.IO.FileStream(m_currentStoragePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    long maxLen = fileStream.Length;
                    fileStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8))
                    {
                        var dataLength = reader.ReadUInt16();
                        for (int i = 0; i < dataLength; i++)
                        {
                            if (fileStream.Position >= maxLen)
                            {
                                Debug.LogError("storage data error!!!");
                                m_sourceError = true;
                                break;
                            }
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
            bool needRebuild = m_sourceError;
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

            bool needRebuild = CheckNeedRebuild();
            if (needRebuild)
            {
                System.IO.File.Delete(m_currentStoragePath);
            }
            using (var fileStream = new System.IO.FileStream(m_currentStoragePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
            {
                var bytes = new byte[m_minInfoSize];
                using (var writer = new BinaryWriter(fileStream, System.Text.Encoding.UTF8))
                {
                    if (needRebuild)
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
            switch (info.type)
            {
                case DataType.STR:
                    infoSizeNeed += info.value == null ? 8 : System.Text.Encoding.UTF8.GetBytes(info.value).Length + 2;
                    break;
                case DataType.INT:
                    infoSizeNeed += 4;
                    break;
                case DataType.FLOAT:
                    infoSizeNeed += 4;
                    break;
                case DataType.BYTE:
                    infoSizeNeed += 1;
                    break;
                case DataType.LONG:
                    infoSizeNeed += 8;
                    break;
                case DataType.DOUBLE:
                    infoSizeNeed += 8;
                    break;
                default:
                    break;
            }
            return infoSizeNeed;
        }

        private void ReadInfo(BinaryReader reader, StorageInfo info)
        {
            info.size = reader.ReadUInt16();
            info.key = reader.ReadString();
            info.type = (DataType)reader.ReadByte();
            switch (info.type)
            {
                case DataType.STR:
                    info.value = reader.ReadString();
                    break;
                case DataType.INT:
                    info.value = reader.ReadInt32().ToString();
                    break;
                case DataType.FLOAT:
                    info.value = reader.ReadSingle().ToString();
                    break;
                case DataType.BYTE:
                    info.value = reader.ReadByte().ToString();
                    break;
                case DataType.LONG:
                    info.value = reader.ReadInt64().ToString();
                    break;
                case DataType.DOUBLE:
                    info.value = reader.ReadDouble().ToString();
                    break;
                default:
                    break;
            }
            Debug.LogFormat("read:{0} value:{1}", info.key, info.value);
        }

        private void WriteInfo(BinaryWriter writer, StorageInfo info)
        {
            writer.Write(info.size);
            writer.Write(info.key);
            writer.Write((byte)info.type);
            switch (info.type)
            {
                case DataType.STR:
                    writer.Write(info.value);
                    break;
                case DataType.INT:
                    writer.Write(GetInt(info.value));
                    break;
                case DataType.FLOAT:
                    writer.Write(GetSingle(info.value));
                    break;
                case DataType.BYTE:
                    writer.Write(GetByte(info.value));
                    break;
                case DataType.LONG:
                    writer.Write(GetLong(info.value));
                    break;
                case DataType.DOUBLE:
                    writer.Write(GetDouble(info.value));
                    break;
                default:
                    break;
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
            SetValue(key, DataType.INT, value);
        }
        public void SetValue(string key, float value)
        {
            SetValue(key, DataType.FLOAT, value);
        }
        public void SetValue(string key, bool value)
        {
            SetValue(key, DataType.BYTE, value ? 1 : 0);
        }
        public void SetValue(string key, byte value)
        {
            SetValue(key, DataType.BYTE, value);
        }
        public void SetValue(string key, long value)
        {
            SetValue(key, DataType.LONG, value);
        }
        public void SetValue(string key, double value)
        {
            SetValue(key, DataType.DOUBLE, value);
        }
        private void SetValue(string key, DataType type, object value)
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
            SaveToFile();
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

