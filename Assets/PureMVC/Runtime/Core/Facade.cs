/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 控制器                                                                          *
*//************************************************************************************/

using System;

namespace UFrame
{
    public class Facade<Key,ViewId,ProxyId> : IFacade<Key>
    {
        protected ObserverMap<Key> m_observerMap;
        protected IModel<ProxyId> m_model;
        protected IView<ViewId,Key> m_view;
        protected IController<Key> m_controller;

        public IModel<ProxyId> Model => m_model;
        public IView<ViewId, Key> View => m_view;
        public IController<Key> Controller => m_controller;
        public ObserverMap<Key> Observer => m_observerMap;

        public Facade()
        {
            m_observerMap = new ObserverMap<Key>();
            m_view = CreateView();
            m_controller = CreateController();
            m_model = CreateModel();
        }

        protected virtual IView<ViewId, Key> CreateView()
        {
            return new View<ViewId, Key>(m_observerMap);
        }

        protected virtual IController<Key> CreateController()
        {
            return new Controller<Key>(m_observerMap);
        }

        protected virtual IModel<ProxyId> CreateModel()
        {
            return new Model<ProxyId>();
        }

        #region Notify
        public virtual bool HasObserver(Key observerKey)
        {
            return m_observerMap.HasObserver(observerKey);
        }
        public virtual void SendNotification(Key observerKey)
        {
            m_observerMap.NotifyObservers(observerKey);
        }
        public virtual void SendNotification<T>(Key observerKey, T body)
        {
            m_observerMap.NotifyObservers(observerKey, body);
        }
        public virtual void SendNotification<T1, T2>(Key observerKey, T1 body, T2 body2)
        {
            m_observerMap.NotifyObservers(observerKey, body, body2);
        }
        public virtual void SendNotification<T1, T2, T3>(Key observerKey, T1 body, T2 body2, T3 body3)
        {
            m_observerMap.NotifyObservers(observerKey, body, body2, body3);
        }
        #endregion Notify
    }
}
