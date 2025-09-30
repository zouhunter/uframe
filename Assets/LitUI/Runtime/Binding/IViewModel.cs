/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 数据模型接口                                                                    *
*//************************************************************************************/

namespace UFrame.LitUI
{
    public interface IViewModel
    {
        bool ContainsKey(string keyword);
        void OnAfterBinding(BindingView panel);
        void OnBeforeUnBinding(BindingView panel);
        bool HaveDefultProperty(string keyword);
        BindableProperty<T> GetBindableProperty<T>(string keyword);
        IBindableProperty GetBindableProperty(string keyword, System.Type type);
        void SetBindableProperty(string keyword, IBindableProperty value);
        void SetActiveView(BindingView view);
    }

}
