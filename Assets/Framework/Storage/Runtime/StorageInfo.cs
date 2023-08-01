//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-08-01
//* 描    述：

//* ************************************************************************************
using UnityEngine;

namespace UFame.Storage
{
    internal class StorageInfo
    {
        internal string key;
        internal byte type;
        internal long anchor = -1;
        internal ushort size;
        internal string value;
    }
}

