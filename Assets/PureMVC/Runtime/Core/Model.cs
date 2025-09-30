/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 模型                                                                            *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame
{
    public class Model<Id> : IModel<Id>
    {
        protected IDictionary<Id, IProxy> m_proxyMap;
        protected Dictionary<Id, Action<IProxy>> m_waitRegisterEvents;

        public Model()
        {
            m_proxyMap = new Dictionary<Id, IProxy>();
            m_waitRegisterEvents = new Dictionary<Id, Action<IProxy>>();
        }
        public void RegisterProxy(Id  proxyId, IProxy proxy)
        {
            m_proxyMap[proxyId] = proxy;

            if (m_waitRegisterEvents.ContainsKey(proxyId))
            {
                m_waitRegisterEvents[proxyId].Invoke(proxy);
                m_waitRegisterEvents.Remove(proxyId);
            }
        }

        #region One
        public void RegisterProxy<T>(Id  proxyId, T data)
        {
            if (HasProxy(proxyId))
            {
                RetrieveIProxy<T>(proxyId).Data = data;
            }
            else
            {
                RegisterProxy(proxyId, new Proxy<T>(data));
            }
        }

        public void RetrieveData<T>(Id  proxyId, Action<T> retrieved)
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                retrieved(RetrieveData<T>(proxyId));
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is IProxy<T>)
                        {
                            retrieved((x as IProxy<T>).Data);
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is IProxy<T>)
                        {
                            retrieved((x as IProxy<T>).Data);
                        }
                    });
                }
            }
        }
        public void RetrieveProxy<T>(Id  proxyId, Action<IProxy<T>> retrieved)
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                var proxy = RetrieveIProxy<T>(proxyId);
                if (proxy is IProxy<T>)
                {
                    retrieved((IProxy<T>)proxy);
                }
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is IProxy<T>)
                        {
                            retrieved(((IProxy<T>)x));
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is IProxy<T>)
                        {
                            retrieved(((IProxy<T>)x));
                        }
                    });
                }
            }
        }
        public void RetrieveProxy<P, T>(Id  proxyId, Action<P> retrieved) where P : IProxy<T>
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                retrieved(RetrieveProxy<P, T>(proxyId));
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is P)
                        {
                            retrieved(((P)x));
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is P)
                        {
                            retrieved(((P)x));
                        }
                    });
                }
            }
        }
        public P RetrieveProxy<P, T>(Id  proxyId) where P : IProxy<T>
        {
            var proxy = RetrieveIProxy<T>(proxyId);
            if (proxy is P)
            {
                return (P)proxy;
            }
            return default(P);
        }
        public T RetrieveData<T>(Id  proxyId)
        {
            if (m_proxyMap.ContainsKey(proxyId) && m_proxyMap[proxyId] is IProxy<T>)
            {
                return ((m_proxyMap[proxyId] as IProxy<T>).Data);
            }
            else
            {
                return default(T);
            }
        }
        public IProxy<T> RetrieveIProxy<T>(Id  proxyId)
        {
            if (m_proxyMap.ContainsKey(proxyId) && m_proxyMap[proxyId] is IProxy<T>)
            {
                return ((m_proxyMap[proxyId] as IProxy<T>));
            }
            else
            {
                return default(IProxy<T>);
            }
        }
        #endregion One

        #region Two
        public void RegisterProxy<T1, T2>(Id  proxyId, T1 data1, T2 data2)
        {
            if (m_proxyMap.TryGetValue(proxyId, out var proxyObj) && proxyObj is IProxy<T1, T2>)
            {
                var proxy = proxyObj as IProxy<T1, T2>;
                proxy.Data1 = data1;
                proxy.Data2 = data2;
            }
            else
            {
                RegisterProxy(proxyId, new Proxy<T1, T2>(data1, data2));
            }
        }
        public void RegisterProxy<T1, T2>(Id  proxyId, IProxy<T1, T2> proxy)
        {
            m_proxyMap[proxyId] = proxy;

            if (m_waitRegisterEvents.ContainsKey(proxyId))
            {
                m_waitRegisterEvents[proxyId].Invoke(proxy);
                m_waitRegisterEvents.Remove(proxyId);
            }
        }
        public void RetrieveData<T1, T2>(Id  proxyId, Action<T1, T2> retrieved)
        {
            if (retrieved == null) return;

            if (m_proxyMap.TryGetValue(proxyId, out var acceptor))
            {
                if (acceptor is IProxy<T1, T2>)
                {
                    var proxy = acceptor as IProxy<T1, T2>;
                    retrieved(proxy.Data1, proxy.Data2);
                }
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is IProxy<T1, T2>)
                        {
                            var proxy = x as IProxy<T1, T2>;
                            retrieved(proxy.Data1, proxy.Data2);
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is IProxy<T1, T2>)
                        {
                            var proxy = x as IProxy<T1, T2>;
                            retrieved(proxy.Data1, proxy.Data2);
                        }
                    });
                }
            }
        }
        public void RetrieveProxy<T1, T2>(Id  proxyId, Action<IProxy<T1, T2>> retrieved)
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                var proxy = RetrieveIProxy<T1, T2>(proxyId);
                if (proxy is IProxy<T1, T2>)
                {
                    retrieved((IProxy<T1, T2>)proxy);
                }
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is IProxy<T1, T2>)
                        {
                            retrieved(((IProxy<T1, T2>)x));
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is IProxy<T1, T2>)
                        {
                            retrieved(((IProxy<T1, T2>)x));
                        }
                    });
                }
            }
        }
        public void RetrieveProxy<P, T1, T2>(Id  proxyId, Action<P> retrieved) where P : IProxy<T1, T2>
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                retrieved(RetrieveProxy<P, T1, T2>(proxyId));
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is P)
                        {
                            retrieved(((P)x));
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is P)
                        {
                            retrieved(((P)x));
                        }
                    });
                }
            }
        }
        public P RetrieveProxy<P, T1, T2>(Id  proxyId) where P : IProxy<T1, T2>
        {
            var proxy = RetrieveIProxy<T1, T2>(proxyId);
            if (proxy is P)
            {
                return (P)proxy;
            }
            return default(P);
        }
        public bool RetrieveData<T1, T2>(Id  proxyId, out T1 t1, out T2 t2)
        {
            if (m_proxyMap.ContainsKey(proxyId) && m_proxyMap[proxyId] is IProxy<T1, T2>)
            {
                var proxy = m_proxyMap[proxyId] as IProxy<T1, T2>;
                t1 = proxy.Data1;
                t2 = proxy.Data2;
                return true;
            }
            else
            {
                t1 = default(T1);
                t2 = default(T2);
                return false;
            }
        }
        public IProxy<T1, T2> RetrieveIProxy<T1, T2>(Id  proxyId)
        {
            if (m_proxyMap.ContainsKey(proxyId) && m_proxyMap[proxyId] is IProxy<T1, T2>)
            {
                return ((m_proxyMap[proxyId] as IProxy<T1, T2>));
            }
            else
            {
                return default(IProxy<T1, T2>);
            }
        }
        #endregion Two

        #region Three
        public void RegisterProxy<T1, T2, T3>(Id  proxyId, T1 data1, T2 data2, T3 data3)
        {
            if (m_proxyMap.TryGetValue(proxyId, out var proxyObj) && proxyObj is IProxy<T1, T2, T3>)
            {
                var proxy = proxyObj as IProxy<T1, T2, T3>;
                proxy.Data1 = data1;
                proxy.Data2 = data2;
                proxy.Data3 = data3;
            }
            else
            {
                RegisterProxy(proxyId, new Proxy<T1, T2, T3>(data1, data2, data3));
            }
        }
        public void RegisterProxy<T1, T2, T3>(Id  proxyId, IProxy<T1, T2, T3> proxy)
        {
            m_proxyMap[proxyId] = proxy;

            if (m_waitRegisterEvents.ContainsKey(proxyId))
            {
                m_waitRegisterEvents[proxyId].Invoke(proxy);
                m_waitRegisterEvents.Remove(proxyId);
            }
        }
        public void RetrieveData<T1, T2, T3>(Id  proxyId, Action<T1, T2, T3> retrieved)
        {
            if (retrieved == null) return;

            if (m_proxyMap.TryGetValue(proxyId, out var acceptor))
            {
                if (acceptor is IProxy<T1, T2, T3>)
                {
                    var proxy = acceptor as IProxy<T1, T2, T3>;
                    retrieved(proxy.Data1, proxy.Data2, proxy.Data3);
                }
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is IProxy<T1, T2, T3>)
                        {
                            var proxy = x as IProxy<T1, T2, T3>;
                            retrieved(proxy.Data1, proxy.Data2, proxy.Data3);
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is IProxy<T1, T2, T3>)
                        {
                            var proxy = x as IProxy<T1, T2, T3>;
                            retrieved(proxy.Data1, proxy.Data2, proxy.Data3);
                        }
                    });
                }
            }
        }
        public void RetrieveProxy<T1, T2, T3>(Id  proxyId, Action<IProxy<T1, T2, T3>> retrieved)
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                var proxy = RetrieveIProxy<T1, T2, T3>(proxyId);
                if (proxy is IProxy<T1, T2, T3>)
                {
                    retrieved((IProxy<T1, T2, T3>)proxy);
                }
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is IProxy<T1, T2, T3>)
                        {
                            retrieved(((IProxy<T1, T2, T3>)x));
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is IProxy<T1, T2, T3>)
                        {
                            retrieved(((IProxy<T1, T2, T3>)x));
                        }
                    });
                }
            }
        }
        public void RetrieveProxy<P, T1, T2, T3>(Id  proxyId, Action<P> retrieved) where P : IProxy<T1, T2, T3>
        {
            if (retrieved == null) return;

            if (HasProxy(proxyId))
            {
                retrieved(RetrieveProxy<P, T1, T2, T3>(proxyId));
            }
            else
            {
                if (m_waitRegisterEvents.ContainsKey(proxyId))
                {
                    m_waitRegisterEvents[proxyId] += (x) =>
                    {
                        if (x is P)
                        {
                            retrieved(((P)x));
                        }
                    };
                }
                else
                {
                    m_waitRegisterEvents.Add(proxyId, (x) =>
                    {
                        if (x is P)
                        {
                            retrieved(((P)x));
                        }
                    });
                }
            }
        }
        public P RetrieveProxy<P, T1, T2, T3>(Id  proxyId) where P : IProxy<T1, T2, T3>
        {
            var proxy = RetrieveIProxy<T1, T2, T3>(proxyId);
            if (proxy is P)
            {
                return (P)proxy;
            }
            return default(P);
        }
        public bool RetrieveData<T1, T2, T3>(Id  proxyId, out T1 t1, out T2 t2, out T3 t3)
        {
            if (m_proxyMap.ContainsKey(proxyId) && m_proxyMap[proxyId] is IProxy<T1, T2, T3>)
            {
                var proxy = m_proxyMap[proxyId] as IProxy<T1, T2, T3>;
                t1 = proxy.Data1;
                t2 = proxy.Data2;
                t3 = proxy.Data3;
                return true;
            }
            else
            {
                t1 = default(T1);
                t2 = default(T2);
                t3 = default(T3);
                return false;
            }
        }
        public IProxy<T1, T2, T3> RetrieveIProxy<T1, T2, T3>(Id  proxyId)
        {
            if (m_proxyMap.ContainsKey(proxyId) && m_proxyMap[proxyId] is IProxy<T1, T2, T3>)
            {
                return ((m_proxyMap[proxyId] as IProxy<T1, T2, T3>));
            }
            else
            {
                return default(IProxy<T1, T2, T3>);
            }
        }
        #endregion Three

        public bool HasProxy(Id  proxyId)
        {
            return m_proxyMap.ContainsKey(proxyId);
        }
        public void RemoveProxy(Id  proxyId)
        {
            if (m_proxyMap.ContainsKey(proxyId))
            {
                m_proxyMap.Remove(proxyId);
            }
        }
        public void CansaleRetrieve(Id  proxyId)
        {
            if (m_waitRegisterEvents.ContainsKey(proxyId))
            {
                m_waitRegisterEvents.Remove(proxyId);
            }
        }
    }
}
