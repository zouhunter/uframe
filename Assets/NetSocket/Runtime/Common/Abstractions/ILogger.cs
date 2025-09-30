using System;

namespace UFrame.NetSocket
{
    public interface ILogger
    {
        void LogDebug(string debug);
        void LogInformation(string info);
        void Log(UnityEngine.LogType logLevel, string info);
        void LogError(string error);
        void LogException(Exception ex);
        void LogWarning(string v);
    }
}