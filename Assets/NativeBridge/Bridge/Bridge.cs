using System;
using LitJson;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Diagnostics;

namespace MateBridge
{
    using
#if UNITY_ANDROID && !UNITY_EDITOR
            _activeBridge = AndroidPantformBridge;
#elif UNITY_IOS && !UNITY_EDITOR
            _activeBridge = IOSPantformBridge;
#else
            _activeBridge = EditorPlantformBridge;
#endif

    /// <summary>
    /// 回调
    /// </summary>
    /// <param name="method"></param>
    /// <param name="cid"></param>
    /// <param name="param"></param>
    public delegate void MethodEvent(string method, string param);


    /// <summary>
    /// 全局桥接具柄控制器提供静态方法调用
    /// </summary>
    public partial class Bridge
    {
        private static MethodEvent _handler;
        private static readonly Dictionary<string, MethodEvent> _handlers = new Dictionary<string, MethodEvent>();

        static Bridge()
        {
            _activeBridge.nativeAction = OnNativeMethod;
        }
        /// <summary>
        /// 注册局部方法处理器
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="methods"></param>
        public static void RegistCallback(MethodEvent handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// 注册局部方法处理器
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="methods"></param>
        public static void RegistCallback(MethodEvent handler, params string[] methods)
        {
            foreach (string method in methods)
            {
                if (_handlers.ContainsKey(method))
                {
                    UnityEngine.Debug.LogError("handler of method:" + method + "already existed");
                }
                _handlers[method] = handler;
            }
        }

        /// <summary>
        /// 调用原生方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="json"></param>
        /// <param name="callback"></param>
        public static void CallNative(string method, string json)
        {
            UnityEngine.Debug.LogFormat("CallNative:{0} json:{1}", method,json);
            Assert.IsNotNull(method);
            _activeBridge.UnityCall(method, json??"{}");
        }

        /// <summary>
        /// 调用原生方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void CallNative(string method)
        {
            CallNative(method, "{}");
        }

        /// <summary>
        /// 调用原生方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void CallNative(string method, JsonData param)
        {
            CallNative(method, param?.ToJson());
        }

        /// <summary>
        /// 原生回调事件
        /// </summary>
        /// <param name="method"></param>
        /// <param name="cid"></param>
        /// <param name="param"></param>
        private static void OnNativeMethod(string method,string param)
        {
            UnityEngine.Debug.LogFormat("OnNativeMethod:{0} param:{1}", method, param);
            if(_handlers.TryGetValue(method,out var methodAction))
            {
                methodAction?.Invoke(method, param);
            }
            else if(_handler != null)
            {
                _handler?.Invoke(method, param);
            }
            else
            {
                UnityEngine.Debug.LogError("OnNativeMethod, method not reg:" + method);
            }
        }
    }

}

