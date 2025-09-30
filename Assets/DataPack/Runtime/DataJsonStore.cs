//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-12-17 12:20:05
//* 描    述： json存储结构

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.DataPack
{
    [System.Serializable]
    public class DataJsonStore
    {
        public List<DataInfoGroup> groupList;
    }
}