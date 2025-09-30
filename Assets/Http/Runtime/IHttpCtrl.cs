using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Http
{
    public interface IHttpCtrl
    {
        int requestCount { get; }
        void DoRequest(HttpRequestInfo info);
        void CancelRequest(string requestId);
        void RefreshRequests();
        void ClearRequests();
        HttpRequestInfo Post(string url, RequestEvent callback, string json, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1);
        HttpRequestInfo Post(string url, RequestEvent callback, byte[] bytes, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1);
        HttpRequestInfo Post(string url, RequestEvent callback, Dictionary<string,object> form, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1);
        HttpRequestInfo Get(string url, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = 0,int retryCounter=-1);
        HttpRequestInfo GetTexture(string url, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = 0, int retryCounter = -1);
        HttpRequestInfo GetFile(string url,string localPath, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = 0, int retryCounter = -1);
    }
}