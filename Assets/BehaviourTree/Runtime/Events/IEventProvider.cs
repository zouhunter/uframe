using System;

namespace UFrame.BehaviourTree
{
    /// <summary>
    /// 事件中心接口，定义事件注册、移除、发送、持久化等功能
    /// </summary>
    public interface IEventProvider
    {
        /// <summary>
        /// 注册事件监听
        /// </summary>
        /// <param name="eventKey">事件名</param>
        /// <param name="callback">回调方法</param>
        void RegistEvent(string eventKey, Action<object> callback);

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="eventKey">事件名</param>
        /// <param name="callback">回调方法</param>
        void RemoveEvent(string eventKey, Action<object> callback);

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventKey">事件名</param>
        /// <param name="arg">事件参数</param>
        void SendEvent(string eventKey, object arg = null);

        /// <summary>
        /// 设置事件为持久事件（不会被Clear清除）
        /// </summary>
        /// <param name="eventName">事件名</param>
        void SetPersistentEvent(string eventName);
    }
}