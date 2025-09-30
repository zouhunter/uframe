/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 数据模型接口                                                                    *
*//************************************************************************************/

using System;

namespace UFrame
{
    public interface IModel<Key>
    {
        void RegisterProxy<T>(Key  proxyName, T data);
        T RetrieveData<T>(Key  proxyName);
        void RetrieveData<T>(Key  proxyName, Action<T> retrieved);
        IProxy<T> RetrieveIProxy<T>(Key  proxyName);
        void RetrieveProxy<T>(Key  proxyName, Action<IProxy<T>> retrieved);
        P RetrieveProxy<P, T>(Key  proxyName) where P : IProxy<T>;
        void RetrieveProxy<P, T>(Key  proxyName, Action<P> retrieved) where P : IProxy<T>;

        void RegisterProxy<T1, T2>(Key  proxyName, T1 data, T2 data2);
        bool RetrieveData<T1, T2>(Key  proxyName, out T1 t1, out T2 t2);
        void RetrieveData<T1, T2>(Key  proxyName, Action<T1, T2> retrieved);
        IProxy<T1, T2> RetrieveIProxy<T1, T2>(Key  proxyName);
        void RetrieveProxy<T1, T2>(Key  proxyName, Action<IProxy<T1, T2>> retrieved);
        P RetrieveProxy<P, T1, T2>(Key  proxyName) where P : IProxy<T1, T2>;
        void RetrieveProxy<P, T1, T2>(Key  proxyName, Action<P> retrieved) where P : IProxy<T1, T2>;

        void RegisterProxy<T1, T2, T3>(Key  proxyName, T1 data, T2 data2, T3 data3);
        bool RetrieveData<T1, T2, T3>(Key  proxyName, out T1 t1, out T2 t2, out T3 t3);
        void RetrieveData<T1, T2, T3>(Key  proxyName, Action<T1, T2, T3> retrieved);
        IProxy<T1, T2, T3> RetrieveIProxy<T1, T2, T3>(Key  proxyName);
        void RetrieveProxy<T1, T2, T3>(Key  proxyName, Action<IProxy<T1, T2, T3>> retrieved);
        P RetrieveProxy<P, T1, T2, T3>(Key  proxyName) where P : IProxy<T1, T2, T3>;
        void RetrieveProxy<P, T1, T2, T3>(Key  proxyName, Action<P> retrieved) where P : IProxy<T1, T2, T3>;

        void RegisterProxy(Key proxyName, IProxy proxy);
        void RemoveProxy(Key  proxyName);
        bool HasProxy(Key  proxyName);
        void CansaleRetrieve(Key  proxyName);
    }
}
