/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - http 请求控制器                                                                 *
*//************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace UFrame.Http
{
    public delegate void RequestSuccessEvent(string url);
    public delegate void RequestErrorEvent(string url, int code, string error, bool finished);
    public delegate void NativeObjectCreateEvent(UnityEngine.Object target);
    public delegate void RequestEvent(HttpRequestInfo requestInfo, IHttpResponceInfo requestHandle);

    public class HttpRequestCtrl
    {
        public event RequestErrorEvent onDownLandError;
        public event RequestSuccessEvent onRequestSuccess;

        public int conCurrentCount = 5;
        public int defaultRetryCount = 3;
        public int defaultTimeout = 3;

        protected List<HttpResponceInfo> m_allRequestList = new List<HttpResponceInfo>();
        protected Dictionary<string, List<HttpRequestInfo>> m_requestInfoMap = new Dictionary<string, List<HttpRequestInfo>>();
        protected int m_lastRequestCount;
        protected Queue<HttpResponceInfo> m_finishedRequests = new Queue<HttpResponceInfo>();
        public int requestCount => m_allRequestList.Count;
        protected HashSet<UnityEngine.Object> m_nativeObjects = new HashSet<UnityEngine.Object>();

        public virtual void RefreshRequests()
        {
            if (m_allRequestList.Count == 0)
                return;

            int startId = m_allRequestList.Count - 1;
            if (conCurrentCount > 0 && conCurrentCount < startId)
            {
                startId = conCurrentCount;
            }

            for (int i = startId; i >= 0 && i < m_allRequestList.Count; i--)
            {
                var handle = m_allRequestList[i];
                if (!m_requestInfoMap.TryGetValue(handle.requestId, out var requests) || requests.Count == 0)
                {
                    m_allRequestList.Remove(handle);
                    Debug.LogError("remove request:" + handle.requestId);
                    continue;
                }

                if (handle.operation == null)
                {
                    StartRequest(handle);
                }
                else
                {
                    RefreshRequest(handle);

                    if (handle.request != null && handle.needProgress)
                    {
                        float progress = 0;
                        if (handle.request.method == "GET")
                            progress = Mathf.Max(progress, handle.request.downloadProgress);
                        else
                            progress = Mathf.Max(progress, handle.request.uploadProgress);
                        foreach (var request in requests)
                        {
                            request?.onProgress?.Invoke(progress);
                        }
                    }
                }
            }

            if (m_lastRequestCount != m_allRequestList.Count)
            {
                m_lastRequestCount = m_allRequestList.Count;
#if DEBUG
                var sb = new System.Text.StringBuilder();
                sb.Append("http request count now:");
                sb.Append(m_allRequestList.Count);
                sb.Append(" co:");
                sb.Append(conCurrentCount);
                sb.Append("detail:");
                foreach (var item in m_allRequestList)
                {
                    sb.AppendLine(item.requestId);
                }
                Debug.Log(sb.ToString());
#endif
            }

            while (m_finishedRequests.Count > 0)
                FinishRequest(m_finishedRequests.Dequeue());
        }

        protected virtual void StartRequest(HttpResponceInfo handle)
        {
            handle.operation = handle.request.SendWebRequest();
            handle.startTime = Time.time;
            Debug.Log("start http request:" + handle.requestId);
        }

        protected virtual void RefreshRequest(HttpResponceInfo handle)
        {
            if (handle.operation.isDone)
            {
                OnRequestFinish(handle);
            }
            else
            {
                if (Time.time - handle.startTime > handle.timeOut)
                {
                    OnRequestTimeOut(handle);
                }
            }
        }

        protected virtual void OnRequestFinish(HttpResponceInfo handle)
        {
            bool finish = false;
            var request = handle.request;
            try
            {
                if (request != null)
                {
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        finish = true;
                    }
                    else if (request.responseCode.ToString().StartsWith("4"))
                    {
                        finish = true;
                    }
                }
                else
                {
                    onDownLandError?.Invoke(request.url, 0, " " + request.error, true);
                }
            }
            catch (Exception x)
            {
                Debug.LogError($"download {request.url} error event callback exception:");
                Debug.LogException(x);
            }

            if (!finish && handle.retryCounter-- != 0)
            {
                if (request != null)
                    Debug.Log("retry request:" + request.url + " code:" + request.responseCode);
                RetryRequest(handle);
            }
            else
            {
                m_finishedRequests.Enqueue(handle);
            }
        }

        protected virtual void OnRequestTimeOut(HttpResponceInfo handle)
        {
            var request = handle.request;
            var url = request.url;
            try
            {
                onDownLandError?.Invoke(url, (int)request.responseCode, " url is time out.", handle.retryCounter == 0);
            }
            catch (Exception x)
            {
                Debug.LogError($"download {request.url} error event callback exception:");
                Debug.LogException(x);
            }
            if (handle.retryCounter-- != 0)
            {
                Debug.Log($"timeout left:{handle.retryCounter},progress:{ handle.operation.progress } restart request:{url} timeout:{handle.timeOut}");
                handle.timeOut *= 2;
                RetryRequest(handle);
            }
            else
            {
                m_finishedRequests.Enqueue(handle);
            }
        }

        protected void FinishRequest(HttpResponceInfo handle)
        {
#if DEBUG
            Debug.Log("finish request:" + handle.requestId);
#endif
            var request = handle.request;
            if (m_requestInfoMap.TryGetValue(handle.requestId, out var requestInfos))
            {
                var requests = requestInfos.ToArray();
                foreach (var requestInfo in requests)
                {
                    try
                    {
                        requestInfo.Finish(handle);
                        if (handle.success)
                            onRequestSuccess?.Invoke(requestInfo.url);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"download {requestInfo.url} success callback exception:");
                        Debug.LogException(e);
                    }
                }
            }
            ReleaseRequest(handle);
        }

        protected void RetryRequest(HttpResponceInfo handle)
        {
            if (m_requestInfoMap.TryGetValue(handle.requestId, out var requestInfos))
            {
                var request = handle.request;
                request.Abort();
                request.Dispose();
                handle.request = null;
                var requests = requestInfos.ToArray();
                foreach (var requestInfo in requests)
                {
                    DoRequest(requestInfo);
                }
            }
        }

        public virtual void ClearRequests()
        {
            foreach (var handle in m_allRequestList)
            {
                var request = handle.request;
                if (request != null)
                {
                    request.Abort();
                    request.Dispose();
                }
            }
            m_allRequestList.Clear();
            m_requestInfoMap.Clear();
            foreach (var target in m_nativeObjects)
            {
                if (target)
                    GameObject.DestroyImmediate(target);
            }
            m_nativeObjects.Clear();
        }

        public virtual void CancelRequest(string requestId)
        {
            var request = m_allRequestList.Find(x => x.requestId == requestId);
            if (request != null)
            {
                ReleaseRequest(request);
            }
        }

        private void ReleaseRequest(HttpResponceInfo handle)
        {
            if (m_requestInfoMap.TryGetValue(handle.requestId, out var requests))
                requests.Clear();
            if (handle.request != null)
                handle.request.Dispose();
            m_allRequestList.Remove(handle);
            handle.request.Dispose();
        }

        public virtual void DoRequest(HttpRequestInfo info)
        {
            info.MakeRequestId();
            info.timeOut = Mathf.Max(defaultTimeout, info.timeOut);
            if (!m_requestInfoMap.TryGetValue(info.requestId, out var requests))
                requests = m_requestInfoMap[info.requestId] = new List<HttpRequestInfo>();
            if (!requests.Contains(info))
                requests.Add(info);
            var handle = m_allRequestList.Find(x => x.requestId == info.requestId);
            if (handle == null)
            {
                handle = new HttpResponceInfo();
                m_allRequestList.Add(handle);
                handle.requestId = info.requestId;
                handle.timeOut = Mathf.Max(handle.timeOut, info.timeOut);
                handle.retryCounter = defaultRetryCount;
                if (info.retryCounter > -1)
                    handle.retryCounter = info.retryCounter;
                handle.nativeObjectCreateEvent = OnCreateNativeObject;
            }
            if (handle.request == null)
            {
                handle.operation = null;
                switch (info.requestType)
                {
                    case UnityWebRequest.kHttpVerbPUT:
                        if (info.bytes != null)
                            handle.request = UnityWebRequest.Put(info.url, info.bytes);
                        else if (info.json != null)
                            handle.request = UnityWebRequest.Put(info.url, info.json);
                        break;
                    case UnityWebRequest.kHttpVerbPOST:
                        if (info.form != null)
                        {
                            var form = new WWWForm();
                            foreach (var pair in info.form)
                            {
                                if (pair.Value is byte[])
                                {
                                    form.AddBinaryData(pair.Key, (byte[])pair.Value);
                                }
                                else
                                {
                                    form.AddField(pair.Key, pair.Value.ToString());
                                }
                            }
                            handle.request = UnityWebRequest.Post(info.url, form);
                        }
                        else if (info.bytes != null)
                        {
                            handle.request = new UnityWebRequest(info.url, "POST");
                            handle.request.uploadHandler = new UploadHandlerRaw(info.bytes);
                        }
                        else if (info.json != null)
                        {
                            handle.request = UnityWebRequest.Post(info.url, info.json);
                        }
                        break;
                    default:
                        handle.request = UnityWebRequest.Get(info.url);
                        break;
                }

                handle.request.downloadHandler = info.handlerCreater?.Invoke() ?? new DownloadHandlerBuffer();

                if (info.headers != null)
                {
                    foreach (var pair in info.headers)
                    {
                        handle.request.SetRequestHeader(pair.Key, pair.Value);
                    }
                }
                handle.needProgress |= info.onProgress != null;
            }
        }

        private void OnCreateNativeObject(UnityEngine.Object target)
        {
            m_nativeObjects.Add(target);
        }
    }
}