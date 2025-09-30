/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-01-06 13:58:38
 * Version: 1.0.0
 * Description: 
 *_*/

namespace UFrame.MobileInput
{
    /// <summary>
    /// 输入方式
    /// </summary>
    public enum InputType
    {
        None = 0,
        Hit = 1,
        Touch = 1 << 1,
        Drag = 1 << 2,
        Scale = 1 << 3,
        Rotate = 1 << 4,
        All = -1,
    }
}
