/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 显示规则                                                                        *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    /// <summary>
    /// 界面的显示状态
    /// </summary>
    [System.Serializable]
    public struct ShowMode
    {
        public bool auto;//当上级显示时显示
        public bool single;//隐藏所有打开的面板
        public MutexRule mutex;//排斥有相同类型面版
        public ParentShow baseShow;//父级的显示状态
    }
}