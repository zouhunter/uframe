/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-08-17                                                                   *
*  功能:                                                                              *
*   - 引导步骤配制                                                                    *
***************************************************************************************/
using System;

namespace UFrame.Guide
{
    [System.Serializable]
    public class GuideStepCfg : TableCfg.IRow<int>
    {
        public int K1 { get { return id; } }
/*0*/   public int id { get; private set; } //GuideStepCfg
/*1*/   public int step_index { get; private set; } //引导步骤index
/*2*/   public string step_name { get; private set; } //步骤名
/*3*/   public int action_factor { get; private set; } //执行
/*4*/   public int[] start_factors { get; private set; } //启动条件
/*5*/   public int[] end_factors { get; private set; } //结束条件

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
