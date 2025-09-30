/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 异常默认View                                                                    *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public interface IDiffuseView : IUIOpenClose
    {
        void Close();
        void CallBack(object arg0);
    }

}
