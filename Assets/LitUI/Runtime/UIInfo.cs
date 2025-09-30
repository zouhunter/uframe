//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:10:47
//* 描    述： ui信息描述

//* ************************************************************************************
using System;

namespace UFrame.LitUI
{
    [System.Serializable]
    public class UIInfo 
    {
        public string name;
        public string guid;
        public byte layer;
        public int priority;
        public int modify;
        public bool mutex;
        public void CopyTo(UIInfo oldInfo)
        {
            oldInfo.name = name;
            oldInfo.layer = layer;
            oldInfo.guid = guid;
            oldInfo.priority = priority;
            oldInfo.modify = modify;
            oldInfo.mutex = mutex;
        }
    }
}
