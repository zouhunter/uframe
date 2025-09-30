//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-17 12:14:39
//* 描    述： 获取当前GameObject或子物体的控制为默认控件
//* ************************************************************************************

namespace UFrame
{
    public class DefaultComponentAttribute : UnityEngine.PropertyAttribute
    {
        public bool childOnly;
        public DefaultComponentAttribute(bool childOnly = false)
        {
            this.childOnly = childOnly;
        }
    }
}