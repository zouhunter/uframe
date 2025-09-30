/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-08-17                                                                   *
*  功能:                                                                              *
*   - 引导条件判断配制                                                                *
***************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Guide
{
    [System.Serializable]
    public class GuideFactorCfg : TableCfg.IRow<int>
    {
        public int K1 { get { return id; } }
/*0*/   public int id { get; private set; } //GuideFactorCfg
/*1*/   public int type_id { get; private set; } //功能
/*2*/   public string[] args { get; private set; } //参数

        public object GetData(string key)
        {
            throw new NotImplementedException();
        }

        public void SetData(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
