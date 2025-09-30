/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 数据模型接口                                                                    *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public interface IViewModel
    {
        bool ContainsKey(byte keyword);
        void OnAfterBinding(BridgeUI.IUIPanel panel);
        void OnBeforeUnBinding(BridgeUI.IUIPanel panel);
        bool HaveDefultProperty(byte keyword);
        BindableProperty<T> GetBindableProperty<T>(byte keyword);
        IBindableProperty GetBindableProperty(byte keyword, System.Type type);
        void SetBindableProperty(byte keyword, IBindableProperty value);
    }

}