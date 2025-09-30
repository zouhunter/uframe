/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 控制器                                                                          *
*//************************************************************************************/
using System.Collections.Generic;

namespace UFrame
{
    public class Controller<Key> : IController<Key>
    {
        public ObserverMap<Key> m_observerMap;
        public IDictionary<Key, DynimicCmd<Key>> m_dynimicCommandMap;
        public IDictionary<Key, ICommandBase> m_commandInstanceMap;
        public DynimicCommandObserver<Key> m_commandObserver;

        /// <summary>
        /// 构造函数，初始化控制器
        /// </summary>
        /// <param name="observerMap">观察者映射</param>
        public Controller(ObserverMap<Key> observerMap)
        {
            m_observerMap = observerMap;
            m_commandObserver = new DynimicCommandObserver<Key>(FindOrObserver, this);
            m_dynimicCommandMap = new Dictionary<Key, DynimicCmd<Key>>();
            m_commandInstanceMap = new Dictionary<Key, ICommandBase>();
        }

        /// <summary>
        /// 记录命令冲突错误
        /// </summary>
        /// <param name="observerKey">观察者键</param>
        protected void LogConfict(Key observerKey)
        {
            UnityEngine.Debug.LogError("已经注册" + observerKey + "的命令" + "->不能重复注册");
        }

        /// <summary>
        /// 移除动态命令
        /// </summary>
        /// <param name="observerKey">观察者键</param>
        protected void RemoveDynimcCommand(Key observerKey)
        {
            m_dynimicCommandMap.Remove(observerKey);
            m_observerMap.RemoveObserver(observerKey, m_commandObserver);
        }

        /// <summary>
        /// 注册观察者
        /// </summary>
        /// <param name="observerKey">观察者键</param>
        /// <param name="observer">观察者</param>
        /// <param name="command">命令</param>
        protected void RegisterObserver(Key observerKey, BaseObserver<Key> observer, ICommandBase command)
        {
            m_observerMap.RegisterObserver(observerKey, observer);
            m_commandInstanceMap[observerKey] = command;
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <param name="observerKey">观察者键</param>
        /// <param name="command">命令</param>
        public void RegistCommand(Key observerKey, ICommand<Key> command)
        {
            if (!m_commandInstanceMap.ContainsKey(observerKey))
            {
                var observer = new ActionObserver<Key>(command.Execute, false);
                RegisterObserver(observerKey, observer, command);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <typeparam name="T1">命令参数类型</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="command">命令</param>
        public void RegistCommand<T1>(Key observerKey, ICommand<Key, T1> command)
        {
            if (!m_commandInstanceMap.ContainsKey(observerKey))
            {
                var observer = new ActionObserver<Key, T1>(command.Execute, false);
                RegisterObserver(observerKey, observer, command);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <typeparam name="T1">命令参数类型1</typeparam>
        /// <typeparam name="T2">命令参数类型2</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="command">命令</param>
        public void RegistCommand<T1, T2>(Key observerKey, ICommand<Key, T1, T2> command)
        {
            if (!m_commandInstanceMap.ContainsKey(observerKey))
            {
                var observer = new ActionObserver<Key, T1, T2>(command.Execute, false);
                RegisterObserver(observerKey, observer, command);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        /// <typeparam name="T1">命令参数类型1</typeparam>
        /// <typeparam name="T2">命令参数类型2</typeparam>
        /// <typeparam name="T3">命令参数类型3</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="command">命令</param>
        public void RegistCommand<T1, T2, T3>(Key observerKey, ICommand<Key, T1, T2, T3> command)
        {
            if (!m_commandInstanceMap.ContainsKey(observerKey))
            {
                var observer = new ActionObserver<Key, T1, T2, T3>(command.Execute, false);
                RegisterObserver(observerKey, observer, command);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册动态命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="oneTime">是否为一次性命令</param>
        public void RegistCommand<T>(Key observerKey, bool oneTime) where T : ICommand<Key>, new()
        {
            if (!m_dynimicCommandMap.ContainsKey(observerKey))
            {
                m_dynimicCommandMap[observerKey] = new DynimicCmd<Key, T>() { oneTime = oneTime, onRegist = RegisterObserver, onRemoveDynimic = RemoveDynimcCommand };
                m_observerMap.RegisterObserver(observerKey, m_commandObserver);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册动态命令
        /// </summary>
        /// <typeparam name="C">命令类型</typeparam>
        /// <typeparam name="T1">命令参数类型1</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="oneTime">是否为一次性命令</param>
        public void RegistCommand<C, T1>(Key observerKey, bool oneTime = false) where C : ICommand<Key, T1>, new()
        {
            if (!m_dynimicCommandMap.ContainsKey(observerKey))
            {
                m_dynimicCommandMap[observerKey] = new DynimicCmd<Key, C, T1>() { oneTime = oneTime, onRegist = RegisterObserver, onRemoveDynimic = RemoveDynimcCommand };
                m_observerMap.RegisterObserver(observerKey, m_commandObserver);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册动态命令
        /// </summary>
        /// <typeparam name="C">命令类型</typeparam>
        /// <typeparam name="T1">命令参数类型1</typeparam>
        /// <typeparam name="T2">命令参数类型2</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="oneTime">是否为一次性命令</param>
        public void RegistCommand<C, T1, T2>(Key observerKey, bool oneTime = false) where C : ICommand<Key, T1, T2>, new()
        {
            if (!m_dynimicCommandMap.ContainsKey(observerKey))
            {
                m_dynimicCommandMap[observerKey] = new DynimicCmd<Key, C, T1, T2>() { oneTime = oneTime, onRegist = RegisterObserver, onRemoveDynimic = RemoveDynimcCommand };
                m_observerMap.RegisterObserver(observerKey, m_commandObserver);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 注册动态命令
        /// </summary>
        /// <typeparam name="C">命令类型</typeparam>
        /// <typeparam name="T1">命令参数类型1</typeparam>
        /// <typeparam name="T2">命令参数类型2</typeparam>
        /// <typeparam name="T3">命令参数类型3</typeparam>
        /// <param name="observerKey">观察者键</param>
        /// <param name="oneTime">是否为一次性命令</param>
        public void RegistCommand<C, T1, T2, T3>(Key observerKey, bool oneTime = false) where C : ICommand<Key, T1, T2, T3>, new()
        {
            if (!m_dynimicCommandMap.ContainsKey(observerKey))
            {
                m_dynimicCommandMap[observerKey] = new DynimicCmd<Key, C, T1, T2, T3>() { oneTime = oneTime, onRegist = RegisterObserver, onRemoveDynimic = RemoveDynimcCommand };
                m_observerMap.RegisterObserver(observerKey, m_commandObserver);
            }
            else
            {
                LogConfict(observerKey);
            }
        }

        /// <summary>
        /// 检查是否存在命令
        /// </summary>
        /// <param name="notificationName">通知名称</param>
        /// <returns>是否存在命令</returns>
        public bool HaveCommand(Key notificationName)
        {
            return m_commandInstanceMap.ContainsKey(notificationName) || m_dynimicCommandMap.ContainsKey(notificationName);
        }

        /// <summary>
        /// 移除命令
        /// </summary>
        /// <param name="notificationName">通知名称</param>
        public void RemoveCommand(Key notificationName)
        {
            bool removeObserver = false;
            if (m_commandInstanceMap.ContainsKey(notificationName))
            {
                m_commandInstanceMap.Remove(notificationName);
                removeObserver = true;
            }
            if (m_dynimicCommandMap.ContainsKey(notificationName))
            {
                m_dynimicCommandMap.Remove(notificationName);
                removeObserver = true;
            }
            if (removeObserver)
            {
                m_observerMap.RemoveObserver(notificationName, this);
            }
        }

        /// <summary>
        /// 查找或创建观察者
        /// </summary>
        /// <param name="observerKey">观察者键</param>
        /// <returns>基础观察者</returns>
        public BaseObserver<Key> FindOrObserver(Key observerKey)
        {
            if (m_dynimicCommandMap.TryGetValue(observerKey, out var acceptor))
            {
                var observer = acceptor.Create(this, observerKey);
                return observer;
            }
            return null;
        }
    }
}
