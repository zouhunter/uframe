/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 控制器接口                                                                      *
*//************************************************************************************/

namespace UFrame
{
    public interface IController<Key>
    {
        bool HaveCommand(Key observerKey);

        void RegistCommand(Key observerKey, ICommand<Key> command);
        void RegistCommand<T>(Key observerKey, ICommand<Key, T> command);
        void RegistCommand<T1, T2>(Key observerKey, ICommand<Key, T1, T2> command);
        void RegistCommand<T1, T2, T3>(Key observerKey, ICommand<Key, T1, T2, T3> command);

        void RegistCommand<C>(Key observerKey,bool oneTime) where C : ICommand<Key>, new();
        void RegistCommand<C, T>(Key observerKey, bool oneTime) where C : ICommand<Key, T>, new();
        void RegistCommand<C, T1, T2>(Key observerKey, bool oneTime) where C : ICommand<Key, T1, T2>, new();
        void RegistCommand<C, T1, T2, T3>(Key observerKey, bool oneTime) where C : ICommand<Key, T1, T2, T3>, new();

        void RemoveCommand(Key observerKey);
    }
}
