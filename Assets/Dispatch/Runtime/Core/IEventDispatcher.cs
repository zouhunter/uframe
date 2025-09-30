using System;
using System.Collections.Generic;

namespace UFrame.EventDispatch
{
    public interface IEventDispatcher<Key>
    {
        void RegistMessageNoHandle(Action<Key, string> messageNoHandle);
        void RegistMessageError(Action<Key, Exception> messageExceptionHandle);
        void RemoveMessageNoHandle(Action<Key, string> messageNoHandle);
        void RemoveMessageError(Action<Key, Exception> messageExceptionHandle);

        int RegistEvent(Key eventKey, Action callBack);
        int RegistEvent<T1>(Key eventKey, Action<T1> callBack);
        int RegistEvent<T1, T2>(Key eventKey, Action<T1, T2> callBack);
        int RegistEvent<T1, T2, T3>(Key eventKey, Action<T1, T2, T3> callBack);
        int RegistEvent<T1, T2, T3, T4>(Key eventKey, Action<T1, T2, T3, T4> callBack);
        int RegistEvent<T1, T2, T3, T4, T5>(Key eventKey, Action<T1, T2, T3, T4, T5> callBack);

        bool RemoveEvents(Key eventKey);
        bool RemoveEvent(Key eventKey, Action callBack);
        bool RemoveEvent<T1>(Key eventKey, Action<T1> callBack);
        bool RemoveEvent<T1, T2>(Key eventKey, Action<T1, T2> callBack);
        bool RemoveEvent<T1, T2, T3>(Key eventKey, Action<T1, T2, T3> callBack);
        bool RemoveEvent<T1, T2, T3, T4>(Key eventKey, Action<T1, T2, T3, T4> callBack);
        bool RemoveEvent<T1, T2, T3, T4, T5>(Key eventKey, Action<T1, T2, T3, T4, T5> callBack);
        void ExecuteAction(Key key);
        void ExecuteAction<T1>(Key key, T1 arg1);
        void ExecuteAction<T1, T2>(Key key, T1 arg1, T2 arg2);
        void ExecuteAction<T1, T2, T3>(Key key, T1 arg1, T2 arg2, T3 arg3);
        void ExecuteAction<T1, T2, T3, T4>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        void ExecuteAction<T1, T2, T3, T4, T5>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        int RegistEvent<R>(Key eventKey, Func<R> callBack);
        int RegistEvent<T1, R>(Key eventKey, Func<T1, R> callBack);
        int RegistEvent<T1, T2, R>(Key eventKey, Func<T1, T2, R> callBack);
        int RegistEvent<T1, T2, T3, R>(Key eventKey, Func<T1, T2, T3, R> callBack);
        int RegistEvent<T1, T2, T3, T4, R>(Key eventKey, Func<T1, T2, T3, T4, R> callBack);
        int RegistEvent<T1, T2, T3, T4, T5, R>(Key eventKey, Func<T1, T2, T3, T4, T5, R> callBack);

        bool RemoveEvent<R>(Key eventKey, Func<R> callBack);
        bool RemoveEvent<T1, R>(Key eventKey, Func<T1, R> callBack);
        bool RemoveEvent<T1, T2, R>(Key eventKey, Func<T1, T2, R> callBack);
        bool RemoveEvent<T1, T2, T3, R>(Key eventKey, Func<T1, T2, T3, R> callBack);
        bool RemoveEvent<T1, T2, T3, T4, R>(Key eventKey, Func<T1, T2, T3, T4, R> callBack);
        bool RemoveEvent<T1, T2, T3, T4, T5, R>(Key eventKey, Func<T1, T2, T3, T4, T5, R> callBack);
        R[] ExecuteFunc<R>(Key key);
        void ExecuteFunc<T1, R>(Key key, T1 arg1,List<R> results = null);
        void ExecuteFunc<T1, T2, R>(Key key, T1 arg1, T2 arg2,List<R> results = null);
        void ExecuteFunc<T1, T2, T3, R>(Key key, T1 arg1, T2 arg2, T3 arg3, List<R> results = null);
        void ExecuteFunc<T1, T2, T3, T4, R>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, List<R> results = null);
        void ExecuteFunc<T1, T2, T3, T4, T5, R>(Key key, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, List<R> results = null);

        bool RemoveEvent(Key eventKey, int eventId);
        int FindEvent(Key key, Action callback);
        int FindEvent(Key key, MulticastDelegate callback);
        bool ExistsEvent(Key key, MulticastDelegate action);
        bool ExistsEvent(Key key, Action action);
        void ExecuteEvents(Key eventKey, params object[] arguments);
        object[] ExecuteEventsReturn(Key key, params object[] arguments);
    }
}