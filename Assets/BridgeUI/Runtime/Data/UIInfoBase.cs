/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面描述信息                                                                    *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public abstract class UIInfoBase
    {
        public int instanceID;
        public string discription;
        public string panelName;
        public UIType type;
        public abstract string IDName { get; }
    }
}