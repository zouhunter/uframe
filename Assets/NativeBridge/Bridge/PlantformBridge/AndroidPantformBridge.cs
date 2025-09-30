#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace MateBridge
{
    class AndroidPantformBridge
    {
        private static AndroidJavaClass _bridgeClass;
        private static BridgeHandler _nativeHandler;
        public static MethodEvent nativeAction { get; set; }
        static AndroidPantformBridge()
        {
            _bridgeClass = new AndroidJavaClass("ai.mate.u3d.BridgeCore");
            _nativeHandler = new BridgeHandler(OnCallback);
            _bridgeClass.CallStatic("unity_regist_callback", _nativeHandler);
        }
        public static void UnityCall(string method, string param)
        {
            _bridgeClass.CallStatic("unity_call", method, param);
        }
        private static void OnCallback(string method, string param)
        {
            nativeAction?.Invoke(method, param);
        }

        private class BridgeHandler : AndroidJavaProxy
        {
            private SynchronizationContext _mainThreadContext;
            private MethodEvent _nativeAction;

            public BridgeHandler(MethodEvent nativeAction) : base("ai.mate.u3d.BridgeHandler")
            {
                _mainThreadContext = SynchronizationContext.Current;
                _nativeAction = nativeAction;
            }
            public void callMethod(string method, string param)
            {
                _mainThreadContext.Post(new SendOrPostCallback((state) =>
                {
                    _nativeAction?.Invoke(method, param);
                }), null);
            }
        }
    }
}

#endif
