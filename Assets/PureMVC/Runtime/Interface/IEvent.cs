/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 事件                                                                            *
*//************************************************************************************/
namespace UFrame
{
    public interface IEvent<Key>
    {
        void RegistEvent(Key observerKey, System.Action<Key> action,  bool strict = false);
        void RemoveEvent(Key observerKey, System.Action<Key> action);

        void RegistEvent<T>(Key observerKey, System.Action<Key, T> action,  bool strict = false);
        void RemoveEvent<T>(Key observerKey, System.Action<Key, T> action);

        void RegistEvent<T1, T2>(Key observerKey, System.Action<Key, T1, T2> action,  bool strict = false);
        void RemoveEvent<T1, T2>(Key observerKey, System.Action<Key, T1, T2> action);

        void RegistEvent<T1, T2, T3>(Key observerKey, System.Action<Key, T1, T2, T3> action,  bool strict = false);
        void RemoveEvent<T1, T2, T3>(Key observerKey, System.Action<Key, T1, T2, T3> action);

        void RegistEvent(Key observerKey, System.Action action,  bool strict = false);
        void RemoveEvent(Key observerKey, System.Action action);

        void RegistEvent<T1>(Key observerKey, System.Action<T1> action,  bool strict = false);
        void RemoveEvent<T1>(Key observerKey, System.Action<T1> action);

        void RegistEvent<T1, T2>(Key observerKey, System.Action<T1, T2> action,  bool strict = false);
        void RemoveEvent<T1, T2>(Key observerKey, System.Action<T1, T2> action);

        void RegistEvent<T1, T2, T3>(Key observerKey, System.Action<T1, T2, T3> action,  bool strict = false);
        void RemoveEvent<T1, T2, T3>(Key observerKey, System.Action<T1, T2, T3> action);
    }
}
