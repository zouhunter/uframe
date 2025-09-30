//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-12-17 11:13:15
//* 描    述：  

//* ************************************************************************************
using System.Collections.Generic;

namespace UFrame.DataPack
{
    [System.Serializable]
    public class DataInfoGroup
    {
        public int groupId;
        public List<DataInfo> infos;
    }
}