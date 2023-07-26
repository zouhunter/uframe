//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:10:47
//* 描    述： 

//* ************************************************************************************
using System;

namespace UFrame.LitUI
{
    [System.Serializable]
    public class UIInfo 
    {
        public string name;
        public string path;
        public byte layer;
        public int priority;
        public bool mutex;
        public bool Mutex => mutex || layer == 0;//0层默认互斥
        internal void CopyTo(UIInfo oldInfo)
        {
            oldInfo.name = name;
            oldInfo.path = path;
            oldInfo.layer = layer;
            oldInfo.priority = priority;
            oldInfo.mutex = mutex;
        }
    }
}
