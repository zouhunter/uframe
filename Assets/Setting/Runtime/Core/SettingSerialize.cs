//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-12-17 20:48:51
//* 描    述：  

//* ************************************************************************************

using System;
using System.Collections.Generic;

namespace UFrame.Setting
{
    [System.Serializable]
    public class SettingSerialize
    {
        [System.Serializable]
        public class SettingNode
        {
            public int k;
            public string v;
            public SettingNode() { }
            public SettingNode(int key, string value)
            {
                this.k = key;
                this.v = value;
            }
        }
        public List<SettingNode> i = new List<SettingNode>();
        public List<SettingNode> f = new List<SettingNode>();
        public List<SettingNode> s = new List<SettingNode>();
    }
}