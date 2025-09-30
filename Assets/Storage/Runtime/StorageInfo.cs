//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-08-01
//* 描    述： 数据缓存信息

//* ************************************************************************************

namespace UFrame.Storage
{
    public class StorageInfo
    {
        public string key;
        public DataType type;
        public long anchor = -1;
        public ushort size;
        public string value;
    }

    public enum DataType : byte
    {
        STR = 0,
        INT = 1,
        FLOAT = 2,
        BYTE = 3,
        LONG = 4,
        DOUBLE = 5
    }
}

