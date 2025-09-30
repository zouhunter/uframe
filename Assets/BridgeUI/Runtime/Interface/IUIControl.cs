/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 控件接口                                                                        *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public interface IUIControl
    {
        bool Initialized { get; }
        void Initialize(IUIPanel context = null);//初始化
        void Release();//回收内存
    }
}
