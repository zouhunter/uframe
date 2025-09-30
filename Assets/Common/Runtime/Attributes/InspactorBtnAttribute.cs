//*************************************************************************************
//* ��    �ߣ� zouhunter
//* ����ʱ�䣺 2021-10-17 12:14:39
//* ��    ���� 
//* ************************************************************************************

using UnityEngine;

namespace UFrame
{
    public class InspactorBtnAttribute : PropertyAttribute
    {
        public string btnName;
        public string funName;
        public InspactorBtnAttribute(string btnName,string funName)
        {
            this.btnName = btnName;
            this.funName = funName;
        }
    }
}
