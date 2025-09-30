//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************
using UnityEngine;
using System;

namespace UFrame
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class EnumLabelAttribute : PropertyAttribute
    {
        public string label;
        public int[] orders = new int[0];
        public EnumLabelAttribute(string label)
        {
            this.label = label;
        }

        public EnumLabelAttribute(string label, params int[] order)
        {
            this.label = label;
            this.orders = order;
        }
    }
}