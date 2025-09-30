/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 程序界面接口                                                                    *
*//************************************************************************************/
namespace UFrame
{
    public interface IFacade<Key>
    {
        bool HasObserver(Key observerKey);
        void SendNotification(Key notificationKey);
        void SendNotification<T>(Key notificationKey, T body);
        void SendNotification<T1, T2>(Key notificationKey, T1 body, T2 body2);
        void SendNotification<T1, T2, T3>(Key notificationKey, T1 body, T2 body2, T3 body3);
    }
}
