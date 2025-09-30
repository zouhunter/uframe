using System;
using System.IO;
using UnityEngine;

namespace UFrame.Log
{
    public class LogController
    {
        private string m_outLogPath;
        private bool m_thread;
        private string m_fileFormat = "OutLog{0}.log";
        private string m_currentTagStr;

        public bool IsStarted { get; protected set; }
        public bool ExceptionStack { get; set; } = true;
        public bool ErrorStack { get; set; } = false;
        public bool WarningStack { get; set; } = false;
        public bool LogStack { get; set; } = false;
        public string LogFilePath => m_outLogPath;
        public bool LogThread => m_thread;

        private StreamWriter m_logWriter;
        public void StartThread(string dirPath, string logFileFormat = "OutLog{0}.log")
        {
            if (!IsStarted)
            {
                m_thread = true;
                IsStarted = true;
                SetLogFileFormat(logFileFormat);
                SetOutLogDirectory(dirPath);
                Application.logMessageReceivedThreaded += OnLogMessageReceived;
                Application.lowMemory += OnLowerMemory;
            }
        }

        public void StartMainThread(string dirPath, string logFileFormat = "OutLog{0}.log")
        {
            if (!IsStarted)
            {
                m_thread = false;
                IsStarted = true;
                SetLogFileFormat(logFileFormat);
                SetOutLogDirectory(dirPath);
                Application.logMessageReceived += OnLogMessageReceived;
                Application.lowMemory += OnLowerMemory;
            }
        }

        public void Release()
        {
            if (IsStarted)
            {
                if (m_thread)
                {
                    Application.logMessageReceivedThreaded -= OnLogMessageReceived;
                }
                else
                {
                    Application.logMessageReceived -= OnLogMessageReceived;
                }
                Application.lowMemory -= OnLowerMemory;
                IsStarted = false;

                if (m_logWriter != null)
                {
                    m_logWriter.Close();
                    m_logWriter.Dispose();
                    m_logWriter = null;
                }
            }
        }

        protected void SetLogFileFormat(string format)
        {
            m_fileFormat = format;
        }

        protected virtual void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            WriteLogToFile(type, condition, stackTrace);
        }

        private string GetStackTrees()
        {
            return new System.Diagnostics.StackTrace(false).ToString();
        }

        public void WriteLogToFile(LogType type, string condition, string stackTrace = null)
        {
            if (!IsStarted)
                return;

            switch (type)
            {
                case LogType.Error:
                    m_currentTagStr = "Err";
                    if (ErrorStack && string.IsNullOrEmpty(stackTrace))
                        stackTrace = GetStackTrees();
                    break;
                case LogType.Assert:
                    m_currentTagStr = "Assert";
                    if (ErrorStack && string.IsNullOrEmpty(stackTrace))
                        stackTrace = GetStackTrees();
                    break;
                case LogType.Warning:
                    m_currentTagStr = "Warn";
                    if (WarningStack && string.IsNullOrEmpty(stackTrace))
                        stackTrace = GetStackTrees();
                    break;
                case LogType.Log:
                    m_currentTagStr = "Log";
                    if (LogStack && string.IsNullOrEmpty(stackTrace))
                        stackTrace = GetStackTrees();
                    break;
                case LogType.Exception:
                    m_currentTagStr = "Exception";
                    if (ExceptionStack && string.IsNullOrEmpty(stackTrace))
                        stackTrace = GetStackTrees();
                    break;
            }

            if (m_logWriter == null)
            {
                m_logWriter = new StreamWriter(new FileStream(m_outLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 128, FileOptions.None));
            }
            var dateTime = DateTime.Now;
            m_logWriter.Write(dateTime.Hour);
            m_logWriter.Write(":");
            m_logWriter.Write(dateTime.Minute);
            m_logWriter.Write(":");
            m_logWriter.Write(dateTime.Second);
            m_logWriter.Write(",");
            m_logWriter.Write(dateTime.Millisecond);
            m_logWriter.Write("[");
            m_logWriter.Write(m_currentTagStr);
            m_logWriter.Write("] ");
            if(string.IsNullOrEmpty(stackTrace))
            {
                m_logWriter.WriteLine(condition);
            }
            else
            {
                m_logWriter.Write(condition);
                m_logWriter.WriteLine(stackTrace);
            }
            m_logWriter.Flush();
        }

        protected void SetOutLogDirectory(string dirPath)
        {
            if(m_fileFormat.Contains("{0}"))
            {
                var dateTime = DateTime.Now;
                m_outLogPath = dirPath + "/" + string.Format(m_fileFormat,  $"{dateTime.Year}_{dateTime.Month}_{dateTime.Day}_{dateTime.Hour}");
            }
            else
            {
                m_outLogPath = dirPath + "/" + m_fileFormat;
            }
            System.IO.Directory.CreateDirectory(dirPath);
        }

        protected virtual void OnLowerMemory()
        {
            OnLogMessageReceived("lower memory", "LogController", LogType.Log);
        }
    }
}