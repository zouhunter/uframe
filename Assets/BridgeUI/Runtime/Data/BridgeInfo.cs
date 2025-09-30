/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 记录面板之间的加载关联 
*   - 同时用于之间的数据交流,使用时实例化对象
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class BridgeInfo
    {
        #region 加载规则
        public string inNode;
        public string outNode;
        public short index;
        public ShowMode showModel;
        #endregion
        public BridgeInfo() { }
        public BridgeInfo(string inNode,string outNode,ShowMode showModel,short index)
        {
            this.inNode = inNode;
            this.outNode = outNode;
            this.showModel = showModel;
            this.index = index;
        }
    }
}