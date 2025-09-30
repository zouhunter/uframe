/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 节点名属性                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.BridgeUI.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class PortAttribute : PropertyAttribute
    {
        public string portInfo;
        public PortAttribute(string portInfo)
        {
            this.portInfo = portInfo;
        }
    }
}