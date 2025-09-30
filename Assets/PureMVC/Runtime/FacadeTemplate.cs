using System;

namespace UFrame
{
    public class FacadeTemplate<F, Key,ViewId,ProxyId> where F : FacadeTemplate<F, Key, ViewId, ProxyId>, new()
    {
        protected static object m_locker = new object();
        protected Facade<Key, ViewId, ProxyId> m_agent;
        protected static F m_context;
        public static Facade<Key, ViewId, ProxyId> Instance
        {
            get
            {
                lock (m_locker)
                {
                    if (m_context == null)
                        m_context = new F();
                    if (m_context.m_agent == null)
                        m_context.m_agent = m_context.CreateFacade();
                }
                return m_context.m_agent;
            }
        }
        protected virtual Facade<Key,ViewId,ProxyId> CreateFacade()
        {
            return new Facade<Key, ViewId, ProxyId>();
        }
        #region Model
        public static void RegisterProxy<P>(ProxyId proxyId,IProxy<P> proxy)
        {
            Instance.Model.RegisterProxy(proxyId,proxy);
        }
        public static void RegisterProxy<T1>(ProxyId proxyId, T1 data)
        {
            Instance.Model.RegisterProxy(proxyId, data);
        }
        public static void RegisterProxy<T1, T2>(ProxyId proxyId, T1 data, T2 data2)
        {
            Instance.Model.RegisterProxy(proxyId, data, data2);
        }
        public static void RegisterProxy<T1, T2, T3>(ProxyId proxyId, T1 data, T2 data2, T3 data3)
        {
            Instance.Model.RegisterProxy(proxyId, data, data2, data3);
        }
        public static void CansaleRetrieve(ProxyId proxyId)
        {
            Instance.Model.CansaleRetrieve(proxyId);
        }
        public static P RetrieveProxy<P, T>(ProxyId proxyId) where P : IProxy< T>
        {
            return Instance.Model.RetrieveProxy<P, T>(proxyId);
        }
        public static void RetrieveProxy<P, T>(ProxyId proxyId, Action<P> onRetieved) where P : IProxy< T>
        {
            Instance.Model.RetrieveProxy<P, T>(proxyId, onRetieved);
        }
        public static void RetrieveProxy<T>(ProxyId proxyId, Action<IProxy< T>> onRetieved)
        {
            Instance.Model.RetrieveProxy<T>(proxyId, onRetieved);
        }
        public static IProxy< T> RetrieveIProxy<T>(ProxyId proxyId)
        {
            return Instance.Model.RetrieveIProxy<T>(proxyId);
        }
        public static void RetrieveData<T>(ProxyId proxyId, Action<T> onRetieved)
        {
            Instance.Model.RetrieveData<T>(proxyId, onRetieved);
        }
        public static T RetrieveData<T>(ProxyId proxyId)
        {
            return Instance.Model.RetrieveData<T>(proxyId);
        }
        public static bool RetrieveData<T1, T2>(ProxyId proxyId, out T1 t1, out T2 t2)
        {
            return Instance.Model.RetrieveData(proxyId, out t1, out t2);
        }

        public static void RetrieveData<T1, T2>(ProxyId proxyId, Action<T1, T2> retrieved)
        {
            Instance.Model.RetrieveData(proxyId, retrieved);
        }

        public IProxy< T1, T2> RetrieveIProxy<T1, T2>(ProxyId proxyId)
        {
            return Instance.Model.RetrieveIProxy<T1, T2>(proxyId);
        }

        public static void RetrieveProxy<T1, T2>(ProxyId proxyId, Action<IProxy< T1, T2>> retrieved)
        {
            Instance.Model.RetrieveProxy<T1, T2>(proxyId, retrieved);
        }

        public P RetrieveProxy<P, T1, T2>(ProxyId proxyId) where P : IProxy< T1, T2>
        {
            return Instance.Model.RetrieveProxy<P, T1, T2>(proxyId);
        }

        public static void RetrieveProxy<P, T1, T2>(ProxyId proxyId, Action<P> retrieved) where P : IProxy< T1, T2>
        {
            Instance.Model.RetrieveProxy<P, T1, T2>(proxyId, retrieved);
        }

        public static bool RetrieveData<T1, T2, T3>(ProxyId proxyId, out T1 t1, out T2 t2, out T3 t3)
        {
            return Instance.Model.RetrieveData<T1, T2, T3>(proxyId, out t1, out t2, out t3);
        }

        public static void RetrieveData<T1, T2, T3>(ProxyId proxyId, Action<T1, T2, T3> retrieved)
        {
            Instance.Model.RetrieveData<T1, T2, T3>(proxyId, retrieved);
        }

        public static IProxy< T1, T2, T3> RetrieveIProxy<T1, T2, T3>(ProxyId proxyId)
        {
            return Instance.Model.RetrieveIProxy<T1, T2, T3>(proxyId);
        }

        public static void RetrieveProxy<T1, T2, T3>(ProxyId proxyId, Action<IProxy< T1, T2, T3>> retrieved)
        {
            Instance.Model.RetrieveProxy<T1, T2, T3>(proxyId, retrieved);
        }

        public static P RetrieveProxy<P, T1, T2, T3>(ProxyId proxyId) where P : IProxy< T1, T2, T3>
        {
            return Instance.Model.RetrieveProxy<P, T1, T2, T3>(proxyId);
        }

        public static void RetrieveProxy<P, T1, T2, T3>(ProxyId proxyId, Action<P> retrieved) where P : IProxy< T1, T2, T3>
        {
            Instance.Model.RetrieveProxy<P, T1, T2, T3>(proxyId, retrieved);
        }
        public static void RegisterProxy(ProxyId proxyId, IProxy<Key> proxy)
        {
            Instance.Model.RegisterProxy(proxyId,proxy);
        }
        public static void RemoveProxy(ProxyId proxyId)
        {
            Instance.Model.RemoveProxy(proxyId);
        }
        public static bool HasProxy(ProxyId proxyId)
        {
            return Instance.Model.HasProxy(proxyId);
        }
        #endregion Model

        #region View
        public static T RetrieveMediator<T>(ViewId viewId) where T : IMediatorBase<Key>
        {
            return Instance.View.RetrieveMediator<T>(viewId);
        }
        public static IMediatorBase<Key> RetrieveMediator(ViewId viewId)
        {
            return Instance.View.RetrieveMediator(viewId);
        }
        public static void RemoveMediator(ViewId viewId)
        {
            Instance.View.RemoveMediator(viewId);
        }

        public static void RegisterMediator(ViewId viewId, IMediator<Key> mediator)
        {
            Instance.View.RegisterMediator(viewId, mediator);
        }

        public static void RegisterMediator<T>(ViewId viewId, IMediator<Key, T> mediator)
        {
            Instance.View.RegisterMediator(viewId, mediator);
        }

        public static void RegisterMediator<T1, T2>(ViewId viewId, IMediator<Key, T1, T2> mediator)
        {
            Instance.View.RegisterMediator(viewId, mediator);
        }

        public static void RegisterMediator<T1, T2, T3>(ViewId viewId, IMediator<Key, T1, T2, T3> mediator)
        {
            Instance.View.RegisterMediator(viewId, mediator);
        }
        #endregion

        #region Command
        public static bool HaveCommand(Key observerKey)
        {
            return Instance.Controller.HaveCommand(observerKey);
        }

        public static void RemoveCommand(Key observerKey)
        {
            Instance.Controller.RemoveCommand(observerKey);
        }
        public static void RegistCommand(Key observerKey, ICommand<Key> command)
        {
            Instance.Controller.RegistCommand(observerKey, command);
        }

        public static void RegistCommand<T>(Key observerKey, ICommand<Key, T> command)
        {
            Instance.Controller.RegistCommand(observerKey, command);
        }

        public static void RegistCommand<T1, T2>(Key observerKey, ICommand<Key, T1, T2> command)
        {
            Instance.Controller.RegistCommand(observerKey, command);
        }

        public static void RegistCommand<T1, T2, T3>(Key observerKey, ICommand<Key, T1, T2, T3> command)
        {
            Instance.Controller.RegistCommand(observerKey, command);
        }

        public static void RegistCommand<T>(Key observerKey, bool oneTime) where T : ICommand<Key>, new()
        {
            Instance.Controller.RegistCommand<T>(observerKey, oneTime);
        }

        public static void RegistCommand<C, T>(Key observerKey, bool oneTime) where C : ICommand<Key, T>, new()
        {
            Instance.Controller.RegistCommand<C, T>(observerKey, oneTime);
        }

        public static void RegistCommand<C, T1, T2>(Key observerKey, bool oneTime) where C : ICommand<Key, T1, T2>, new()
        {
            Instance.Controller.RegistCommand<C, T1, T2>(observerKey, oneTime);
        }

        public static void RegistCommand<C, T1, T2, T3>(Key observerKey, bool oneTime) where C : ICommand<Key, T1, T2, T3>, new()
        {
            Instance.Controller.RegistCommand<C, T1, T2, T3>(observerKey, oneTime);
        }
        #endregion Command

        #region Action
        public static void RegistEvent(Key observerKey, Action<Key> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new ActionObserver<Key>(action, strict));
        }

        public static void RegistEvent<T>(Key observerKey, Action<Key, T> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new ActionObserver<Key, T>(action, strict));
        }

        public static void RegistEvent<T1, T2>(Key observerKey, Action<Key, T1, T2> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new ActionObserver<Key, T1, T2>(action, strict));
        }

        public static void RegistEvent<T1, T2, T3>(Key observerKey, Action<Key, T1, T2, T3> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new ActionObserver<Key, T1, T2, T3>(action, strict));
        }

        public static void RemoveEvent(Key observerKey, Action<Key> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }

        public static void RemoveEvent<T>(Key observerKey, Action<Key, T> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }

        public static void RemoveEvent<T1, T2>(Key observerKey, Action<Key, T1, T2> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }

        public static void RemoveEvent<T1, T2, T3>(Key observerKey, Action<Key, T1, T2, T3> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }
        #endregion Action

        #region Function
        public static void RegistEvent(Key observerKey, Action action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new EventObserver<Key>(action, strict));
        }

        public static void RemoveEvent(Key observerKey, Action action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }

        public static void RegistEvent<T1>(Key observerKey, Action<T1> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new EventObserver<Key, T1>(action, strict));
        }

        public static void RemoveEvent<T1>(Key observerKey, Action<T1> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }

        public static void RegistEvent<T1, T2>(Key observerKey, Action<T1, T2> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new EventObserver<Key, T1, T2>(action, strict));
        }

        public static void RemoveEvent<T1, T2>(Key observerKey, Action<T1, T2> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }
        public static void RegistEvent<T1, T2, T3>(Key observerKey, System.Action<T1, T2, T3> action,  bool strict = false)
        {
            Instance.Observer.RegisterObserver(observerKey, new EventObserver<Key, T1, T2, T3>(action, strict));
        }

        public static void RemoveEvent<T1, T2, T3>(Key observerKey, Action<T1, T2, T3> action)
        {
            Instance.Observer.RemoveObserver(observerKey, action);
        }
        #endregion Function

        #region Notify
        public static bool HasObserver(Key observerKey)
        {
            return Instance.Observer.HasObserver(observerKey);
        }
        public static void SendNotification(Key observerKey)
        {
            Instance.Observer.NotifyObservers(observerKey);
        }
        public static void SendNotification<T>(Key observerKey, T body)
        {
            Instance.Observer.NotifyObservers(observerKey, body);
        }
        public static void SendNotification<T1, T2>(Key observerKey, T1 body, T2 body2)
        {
            Instance.Observer.NotifyObservers(observerKey, body, body2);
        }
        public static void SendNotification<T1, T2, T3>(Key observerKey, T1 body, T2 body2, T3 body3)
        {
            Instance.Observer.NotifyObservers(observerKey, body, body2, body3);
        }
        #endregion Notify
    }
}
