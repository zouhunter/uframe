/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-08-17                                                                   *
*  功能:                                                                              *
*   - 引导总配制                                                                      *
***************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Guide
{
    [System.Serializable]
    public class GuideCfg : TableCfg.IRow<int>
    {
        public int K1 { get { return id; } }
/*0*/   public int id { get; private set; } //GuideCfg
/*1*/   public string name { get; private set; } //引导名
/*2*/   public int[] steps { get; private set; } //引导步骤id
/*3*/   public int[] lunch_factors { get; private set; } //启动条件

        public object GetData(string key)
        {
            switch (key)
            {
                default:
                    break;
            }
            return null;
        }
        public void SetData(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
