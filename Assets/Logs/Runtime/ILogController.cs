using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Log
{
    public interface ILogController
    {
        void StartThread(string dirPath, string logFileFormat);
        void StartMainThread(string dirPath, string logFileFormat);
        void Release();
    }
}