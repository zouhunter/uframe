/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 通用事件-界面显示                                                               *
*//************************************************************************************/

using UnityEngine.Events;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }
    [System.Serializable]
    public class IntArrayEvent : UnityEvent<int[]> { }
    [System.Serializable]
    public class IntListEvent : UnityEvent<List<int>> { }

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    [System.Serializable]
    public class BoolArrayEvent : UnityEvent<bool[]> { }
    [System.Serializable]
    public class BoolListEvent : UnityEvent<List<bool>> { }


    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    [System.Serializable]
    public class FloatArrayEvent : UnityEvent<float[]> { }
    [System.Serializable]
    public class FloatListEvent : UnityEvent<List<float>> { }


    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
    [System.Serializable]
    public class StringArrayEvent : UnityEvent<string[]> { }
    [System.Serializable]
    public class StringListEvent : UnityEvent<List<string>> { }
}