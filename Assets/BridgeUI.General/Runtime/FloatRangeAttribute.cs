/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 范围标记                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.BridgeUI.Attributes
{
    [System.Serializable]
    public class FloatRangeAttribute: PropertyAttribute
    {
        public float min;
        public float max;
        public FloatRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}