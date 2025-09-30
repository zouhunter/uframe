/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 预制体信息                                                                      *
*//************************************************************************************/

using UnityEngine;
namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class PrefabUIInfo : UIInfoBase
    {
        public GameObject prefab;
        public override string IDName { get { return panelName; } }
    }
}