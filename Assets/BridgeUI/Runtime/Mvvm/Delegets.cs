/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面事件                                                                        *
*//************************************************************************************/


namespace UFrame.BridgeUI
{
    public delegate void ValueChangedHandler1<T>(T newValue);
    public delegate void ValueChangedHandler2<T>(T oldValue,T newValue);

    public delegate void PanelAction(IUIPanel panel);
    public delegate void PanelAction<T>(IUIPanel panel,T arg0);
    public delegate void PanelAction<T,S>(IUIPanel panel,T arg0,S arg1);
    public delegate void PanelAction<T,S,R>(IUIPanel panel,T arg0,S arg1,R arg2);
    public delegate void PanelAction<T,S,R,Q>(IUIPanel panel,T arg0,S arg1,R arg2,Q arg3);
    public delegate void PanelAction<T,S,R,Q,P>(IUIPanel panel, T arg0,S arg1,R arg2,Q arg3,P arg4);
}