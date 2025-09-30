/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 点击事件-界面显示                                                               *
*//************************************************************************************/

using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class PointerSystemEvent : UnityEvent<PointerEventData> { }
    [System.Serializable]
    public class BaseSystemEvent : UnityEvent<BaseEventData> { }
}
