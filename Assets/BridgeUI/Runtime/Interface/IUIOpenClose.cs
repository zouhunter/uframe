/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面开关接口                                                                    *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public interface IUIOpenClose
    {
        void Open(string panelName, object data = null);
        void Open(int index, object data = null);
        void Hide(string panelName);
        void Hide(int index);
        void Close(string panelName);
        void Close(int index);
        bool IsOpen(int index);
        bool IsOpen(string panelName);
    }
}
