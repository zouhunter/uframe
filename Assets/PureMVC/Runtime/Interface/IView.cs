/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 显示接口                                                                        *
*//************************************************************************************/

namespace UFrame
{
    public interface IView<ViewId,Key>
    {
        T RetrieveMediator<T>(ViewId viewId) where T: IMediatorBase<Key>;
        IMediatorBase<Key> RetrieveMediator(ViewId viewId);
        void RegisterMediator(ViewId viewId,IMediator<Key> mediator);
        void RegisterMediator<T>(ViewId viewId, IMediator<Key,T> mediator);
        void RegisterMediator<T1,T2>(ViewId viewId, IMediator<Key,T1,T2> mediator);
        void RegisterMediator<T1,T2,T3>(ViewId viewId, IMediator<Key,T1,T2,T3> mediator);
        void RemoveMediator(ViewId viewId);
    }
}
