using System;

namespace MateBridge
{
    class EditorPlantformBridge
    {
        public static MethodEvent nativeAction { get; internal set; }

        public static void UnityCall(string cmd, string param)
        {
            UnityEngine.Debug.Log($"EditorBridge call {cmd} {param}");
        }
    }
}

