/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 命令接口                                                                        *
*//************************************************************************************/

namespace UFrame
{
    public interface ICommandBase {
        bool Strict { get; }
    }

    public interface ICommand<Key>: ICommandBase
    {
        void Execute(Key observerKey);
    }
    public interface ICommand<Key,T1> : ICommandBase
    {
        void Execute(Key observerKey,T1 data);
    }
    public interface ICommand<Key,T1, T2> : ICommandBase
    {
        void Execute(Key observerKey,T1 t1,T2 t2);
    }
    public interface ICommand<Key, T1, T2, T3>: ICommandBase
    {
        void Execute(Key observerKey, T1 t1, T2 t2,T3 t3);
    }
    public interface ICommand<Key, T1, T2, T3, T4>: ICommandBase
    {
        void Execute(Key observerKey, T1 t1, T2 t2, T3 t3,T4 t4);
    }
    public interface ICommand<Key, T1, T2, T3, T4, T5>: ICommandBase
    {
        void Execute(Key observerKey, T1 t1, T2 t2, T3 t3, T4 t4,T5 t5);
    }
}
