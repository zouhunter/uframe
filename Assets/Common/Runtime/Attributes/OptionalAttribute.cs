//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 可选参数 （非default）
//* ************************************************************************************
using UnityEngine;

namespace UFrame
{
    public class OptionalAttribute : PropertyAttribute
    {
        public string refPropPath;
        public OptionalAttribute(string refPropPath)
        {
            this.refPropPath = refPropPath;
        }
    }
}