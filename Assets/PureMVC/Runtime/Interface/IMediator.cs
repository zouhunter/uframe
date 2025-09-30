/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 媒体界面接口                                                                    *
*//************************************************************************************/
using System.Collections.Generic;

namespace UFrame
{
    public interface IMediatorBase<Key>
    {
        ICollection<Key> Acceptors { get; }
        bool Strict { get; }
    }
    public interface IMediator<Key> : IMediatorBase<Key>
    {
        void OnNotify(Key key);
    }
    public interface IMediator<Key, T> : IMediatorBase<Key>
    {
        void OnNotify(Key key, T t);
    }
    public interface IMediator<Key, T1, T2> : IMediatorBase<Key>
    {
        void OnNotify(Key key, T1 t1, T2 t2);
    }
    public interface IMediator<Key, T1, T2, T3> : IMediatorBase<Key>
    {
        void OnNotify(Key key, T1 t1, T2 t2, T3 t3);
    }
    public interface IMediator<Key, T1, T2, T3, T4> : IMediatorBase<Key>
    {
        void OnNotify(Key key, T1 t1, T2 t2, T3 t3, T4 t4);
    }
    public interface IMediator<Key, T1, T2, T3, T4, T5> : IMediatorBase<Key>
    {
        void OnNotify(Key key, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    }
}
