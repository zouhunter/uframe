#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using System.Collections;
using System.Threading;

namespace MateBridge
{
    class IOSPantformBridge
    {
        [DllImport("__Internal")]
        static extern void unity_call(string cmd, string param);

        [DllImport("__Internal")]
        static extern void unity_regist_callback(MethodEvent methodListenr);

        public static MethodEvent nativeAction { get; set; }
        private static SynchronizationContext _mainThreadContext;

        static IOSPantformBridge()
        {
            unity_regist_callback(OnNativeMethod);
            _mainThreadContext = SynchronizationContext.Current;
        }


        public static void UnityCall(string cmd, string param)
        {
            _mainThreadContext.Post(new SendOrPostCallback((state) =>
            {
                unity_call(cmd, param);
            }), null);
        }

        [MonoPInvokeCallback(typeof(MethodEvent))]
        public static void OnNativeMethod(string method, string param)
        {
            nativeAction?.Invoke(method,param);
        }
    }
}

#endif
