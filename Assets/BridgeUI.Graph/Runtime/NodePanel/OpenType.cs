using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace UINodePanel
{
    public enum OpenType
    {
        ByName = 1,//在脚本中打开
        ByButton = 1 << 1,//按扭打开
        ByToggle = 1 << 2,//toggle开关
        Start = 1 << 3//一开始就打开
    }
}