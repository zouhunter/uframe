//*************************************************************************************
//* ��    �ߣ� zouhunter
//* ����ʱ�䣺 2021-10-17 12:14:39
//* ��    ���� 
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