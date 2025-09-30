using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame
{
    public class HookFacade<Key> : Facade<Key,string,string>
    {
        public Action<Key> notifyNotHandle { get; set; }

        public event Action<Key, int, object, object, object, object, object> sendNotifyHook;
        public HashSet<Key> luaEventKeys;//lua已注册的事件 防止每帧调用
       
        public HookFacade()
        {
            luaEventKeys = new HashSet<Key>(); 
        }

        public void AddLuaEventKey(Key observerKey)
        {
            luaEventKeys.Add(observerKey);
        }

        public void RemoveLuaEventKey(Key observerKey)
        {
            luaEventKeys.Remove(observerKey);
        }

        bool CheckIsContainsLuaKey(Key observerKey)
        {
            return luaEventKeys.Contains(observerKey);
        }

        public override void SendNotification(Key observerKey)
        {
            base.SendNotification(observerKey);
            if (CheckIsContainsLuaKey(observerKey))
                sendNotifyHook?.Invoke(observerKey, 0, null, null, null, null, null);
        }

        public override void SendNotification<T>(Key observerKey, T body)
        {
            base.SendNotification(observerKey, body);
            if (CheckIsContainsLuaKey(observerKey))
                sendNotifyHook?.Invoke(observerKey, 1, body, null, null, null, null);
        }

        public override void SendNotification<T1, T2>(Key observerKey, T1 body, T2 body2)
        {
            base.SendNotification(observerKey, body, body2);
            if (CheckIsContainsLuaKey(observerKey))
                sendNotifyHook?.Invoke(observerKey, 2, body, body2, null, null, null);
        }

        public override void SendNotification<T1, T2, T3>(Key observerKey, T1 body, T2 body2, T3 body3)
        {
            base.SendNotification(observerKey, body, body2, body3);
            if (CheckIsContainsLuaKey(observerKey))
                sendNotifyHook?.Invoke(observerKey, 3, body, body2, body3, null, null);
        }
    }
}
