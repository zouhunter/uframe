/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 显示层                                                                          *
*//************************************************************************************/
using System.Collections.Generic;

namespace UFrame
{
    public class View<Id, Key> : IView<Id, Key>
    {
        public IDictionary<Id, IMediatorBase<Key>> m_mediatorMap;
        private ObserverMap<Key> m_observerMap;

        /// <summary>
        /// 构造函数，初始化视图
        /// </summary>
        /// <param name="observerMap">观察者映射</param>
        public View(ObserverMap<Key> observerMap)
        {
            m_mediatorMap = new Dictionary<Id, IMediatorBase<Key>>();
            m_observerMap = observerMap;
        }

        /// <summary>
        /// 记录中介者
        /// </summary>
        /// <param name="mediator">中介者</param>
        /// <returns>是否成功记录</returns>
        protected bool RecordMediator(Id viewId,IMediatorBase<Key> mediator)
        {
            if (mediator == null || m_mediatorMap.ContainsKey(viewId)) return false;
            m_mediatorMap[viewId] = mediator;
            return true;
        }

        /// <summary>
        /// 注册观察者
        /// </summary>
        /// <param name="mediator">中介者</param>
        /// <param name="observer">观察者</param>
        protected void RegistObserver(IMediatorBase<Key> mediator, BaseObserver<Key> observer)
        {
            if (mediator.Acceptors != null)
            {
                foreach (var acceptor in mediator.Acceptors)
                {
                    m_observerMap.RegisterObserver(acceptor, observer);
                }
            }
        }
        /// <summary>
        /// 查找mediator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IMediatorBase<Key> RetrieveMediator(Id viewId)
        {
            if (m_mediatorMap.TryGetValue(viewId, out var mediator))
                return mediator;
            return default;
        }
        /// <summary>
        /// 查找mediator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T RetrieveMediator<T>(Id viewId) where T : IMediatorBase<Key>
        {
            if (m_mediatorMap.TryGetValue(viewId, out var mediator) && mediator is T media)
                return media;
            return default;
        }

        /// <summary>
        /// 注册中介者
        /// </summary>
        /// <param name="mediator">中介者</param>
        public void RegisterMediator(Id viewId,IMediator<Key> mediator)
        {
            if (RecordMediator(viewId,mediator))
                RegistObserver(mediator, new ActionObserver<Key>(mediator.OnNotify, mediator.Strict));
        }

        /// <summary>
        /// 注册中介者
        /// </summary>
        /// <typeparam name="T">中介者类型</typeparam>
        /// <param name="mediator">中介者</param>
        public void RegisterMediator<T>(Id viewId, IMediator<Key, T> mediator)
        {
            if (RecordMediator(viewId, mediator))
                RegistObserver(mediator, new ActionObserver<Key, T>(mediator.OnNotify, mediator.Strict));
        }

        /// <summary>
        /// 注册中介者
        /// </summary>
        /// <typeparam name="T1">中介者类型1</typeparam>
        /// <typeparam name="T2">中介者类型2</typeparam>
        /// <param name="mediator">中介者</param>
        public void RegisterMediator<T1, T2>(Id viewId, IMediator<Key, T1, T2> mediator)
        {
            if (RecordMediator(viewId, mediator))
                RegistObserver(mediator, new ActionObserver<Key, T1, T2>(mediator.OnNotify, mediator.Strict));
        }

        /// <summary>
        /// 注册中介者
        /// </summary>
        /// <typeparam name="T1">中介者类型1</typeparam>
        /// <typeparam name="T2">中介者类型2</typeparam>
        /// <typeparam name="T3">中介者类型3</typeparam>
        /// <param name="mediator">中介者</param>
        public void RegisterMediator<T1, T2, T3>(Id viewId, IMediator<Key, T1, T2, T3> mediator)
        {
            if (RecordMediator(viewId, mediator))
                RegistObserver(mediator, new ActionObserver<Key, T1, T2, T3>(mediator.OnNotify, mediator.Strict));
        }

        /// <summary>
        /// 移除中介者
        /// </summary>
        /// <param name="mediator">中介者</param>
        public void RemoveMediator(Id viewId)
        {
            if (viewId == null || !m_mediatorMap.TryGetValue(viewId,out var mediator) || mediator == null) return;

            if (mediator.Acceptors != null)
            {
                foreach (var acceptor in mediator.Acceptors)
                {
                    m_observerMap.RemoveObserver(acceptor, mediator);
                }
            }
            m_mediatorMap.Remove(viewId);
        }
    }
}
