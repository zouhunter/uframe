/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - http 请求句柄                                                                   *
*//************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequest;

namespace UFrame.Http
{
    public class HttpRequestInfo : IEnumerator
    {
        public string requestId;
        public string url;
        public string requestType = UnityWebRequest.kHttpVerbGET;
        public Dictionary<string, object> form = null;
        public byte[] bytes = null;
        public string json = null;
        public RequestEvent callBack;
        public Action<float> onProgress;
        public float timeOut = -1f;
        public int retryCounter = -1;
        public object context;
        public Func<DownloadHandler> handlerCreater;
        public Dictionary<string, string> headers;
        public string localPath;
        private bool m_isDone;

        public object Current => this;

        public string MakeRequestId()
        {
            if (!string.IsNullOrEmpty(requestId))
                return requestId;
            requestId = $"{url}{requestType}";
            var dataHash = new System.Text.StringBuilder();
            if (dataHash != null)
            {
                dataHash.Append(json);
            }
            if (bytes != null)
            {
                dataHash.Append(Hash128.Compute(System.Text.Encoding.UTF8.GetString(bytes)));
            }
            if (form != null)
            {
                foreach (var pair in form)
                {
                    dataHash.Append(Hash128.Compute(pair.Value.ToString()));
                }
            }
            requestId += Hash128.Compute(dataHash.ToString()).ToString();
            if (requestType != UnityWebRequest.kHttpVerbGET)
            {
                requestId += System.Guid.NewGuid();
            }
            return requestId;
        }

        public HttpRequestInfo() { }

        public HttpRequestInfo(string url, string requestType)
        {
            this.url = url;
            this.requestType = requestType;
        }
        public static HttpRequestInfo Get(string url)
        {
            var info = new HttpRequestInfo(url, "GET");
            return info;
        }
        public static HttpRequestInfo Post(string url)
        {
            var info = new HttpRequestInfo(url, "POST");
            return info;
        }

        public bool MoveNext()
        {
            return !m_isDone;
        }

        public void Reset()
        {
            m_isDone = false;
        }

        public void Finish(IHttpResponceInfo responce)
        {
            m_isDone = true;
            callBack?.Invoke(this, responce);
        }
    }

    public class HttpResponceInfo : IHttpResponceInfo
    {
        public string requestId;
        public float startTime = -1f;
        public float timeOut;
        public int retryCounter;
        public NativeObjectCreateEvent nativeObjectCreateEvent;
        public UnityWebRequest request;
        public AsyncOperation operation;
        public string error => request.error;
        public int responseCode => (int)request.responseCode;
        public bool success => request.result == Result.Success;
        public string text => request.downloadHandler.text;
        public byte[] bytes => request.downloadHandler.data;
        private Texture2D m_texture;
        public bool needProgress;

        public Texture2D texture
        {
            get
            {
                if (m_texture == null && request.downloadHandler is DownloadHandlerTexture)
                {
                    m_texture = DownloadHandlerTexture.GetContent(request);
                    if (m_texture)
                        nativeObjectCreateEvent?.Invoke(m_texture);
                }
                return m_texture;
            }
        }
    }

    public interface IHttpResponceInfo
    {
        bool success { get; }
        string text { get; }
        byte[] bytes { get; }
        int responseCode { get; }
        Texture2D texture { get; }
        string error { get; }
    }
}