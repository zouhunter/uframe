//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

namespace UFrame
{
    public class EnumMaskAttribute : UnityEngine.PropertyAttribute
    {
        public System.Type classType;
        public EnumMaskAttribute(System.Type classType)
        {
            this.classType = classType;
        }
    }
}