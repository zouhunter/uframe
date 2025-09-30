//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-17 12:14:39
//* 描    述： 
//* ************************************************************************************

namespace UFrame
{
    using UnityEngine;

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class DisableInInspectorAttribute : PropertyAttribute
    {
        public DisableInInspectorAttribute()
        {
        }
    }
}