using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace UFrame.Http
{
    public class HttpsRequestCtrl
    {
        public event RequestErrorEvent onDownLandError;
        public event RequestSuccessEvent onRequestSuccess;

        public int conCurrentCount = 5;
        public int defaultRetryCount = 3;
        public int connectTimeout = 3;
        public int defaultTimeout = 3;

        protected List<HttpsResponceInfo> m_allRequestList = new List<HttpsResponceInfo>();
        protected Dictionary<string, List<HttpRequestInfo>> m_requestInfoMap = new Dictionary<string, List<HttpRequestInfo>>();
        protected int m_lastRequestCount;
        protected Queue<HttpsResponceInfo> m_finishedRequests = new Queue<HttpsResponceInfo>();
        protected HashSet<UnityEngine.Object> m_nativeObjects = new HashSet<UnityEngine.Object>();
        protected List<HttpsResponceInfo> m_delyRetryRequestInfos = new List<HttpsResponceInfo>();
        public int requestCount => m_delyRetryRequestInfos.Count + m_allRequestList.Count;
        private float m_delyRetryTimer;

        public virtual void RefreshRequests()
        {
            if (m_delyRetryRequestInfos.Count > 0 && m_delyRetryTimer < Time.time)
            {
                ProcessRetryRequests();
                m_delyRetryTimer = Time.time + 1;
            }

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

                if (m_delyRetryRequestInfos.Contains(handle))
                    continue;

                if (!m_requestInfoMap.TryGetValue(handle.requestId, out var requests) || requests.Count == 0)
                {
                    m_allRequestList.Remove(handle);
                    Debug.LogError("remove request:" + handle.requestId);
                    continue;
                }
                else
                {
                    var progress = handle.progress;
                    foreach (var request in requests)
                    {
                        request?.onProgress?.Invoke(progress);
                    }
                }

                if (handle.operation == null)
                {
                    StartRequest(handle);
                }
                else
                {
                    RefreshRequest(handle);
                }
            }

            if (m_lastRequestCount != m_allRequestList.Count)
            {
                m_lastRequestCount = m_allRequestList.Count;
                Debug.Log("http request count now:" + m_allRequestList.Count + " co:" + conCurrentCount);
            }

            while (m_finishedRequests.Count > 0)
            {
                FinishRequest(m_finishedRequests.Dequeue());
            }
        }

        private void ProcessRetryRequests()
        {
            for (int i = m_delyRetryRequestInfos.Count - 1; i >= 0; i--)
            {
                var info = m_delyRetryRequestInfos[i];
                m_delyRetryRequestInfos.Remove(info);
                if (m_requestInfoMap.TryGetValue(info.requestId, out var requests))
                {
                    Debug.Log("start do retry:" + info.requestId);
                    info.Reset();
                    foreach (var request in requests)
                    {
                        DoRequest(request);
                    }
                }
            }
        }

        protected virtual void StartRequest(HttpsResponceInfo handle)
        {
            if (handle.request == null)
            {
                Debug.LogError("request invalid:" + handle.requestId);
                m_allRequestList.Remove(handle);
                return;
            }
            Debug.LogFormat("start http request:{0}", handle.requestId);
            handle.operation = handle.request.BeginGetResponse(null, null);
            handle.startTime = Time.time;
        }

        protected virtual void RefreshRequest(HttpsResponceInfo handle)
        {
            if (handle.operation.IsCompleted)
            {
                OnRequestFinish(handle);
            }
            else if (handle.timeOut > 0 && Time.time - handle.startTime > handle.timeOut)
            {
                OnRequestTimeOut(handle);
            }
        }

        protected virtual void OnRequestFinish(HttpsResponceInfo handle)
        {
            handle.GetResponce();
            bool retry = false;
            var request = handle.request;
            try
            {
                if (!handle.success)
                {
                    if (handle.code < HttpStatusCode.BadRequest || handle.code >= HttpStatusCode.InternalServerError)
                    {
                        retry = true;
                    }
                    OnRequestError(request.RequestUri.AbsoluteUri,(int)handle.code, " " + handle.error, !retry || handle.retryCounter == 0);
                }
            }
            catch (Exception x)
            {
                Debug.LogError($"download {request.RequestUri} error event callback exception:");
                Debug.LogException(x);
            }

            if (retry && handle.retryCounter-- > 0)
            {
                if (request != null)
                    Debug.Log("retry request:" + request.RequestUri + " code:" + handle.code + " retryCounter:" + handle.retryCounter);
                MarkRetryRequest(handle);
            }
            else
            {
                m_finishedRequests.Enqueue(handle);
            }
        }

        protected virtual void OnRequestTimeOut(HttpsResponceInfo handle)
        {
            var request = handle.request;
            var url = request.RequestUri.AbsoluteUri;
            try
            {
                OnRequestError(url, (int)HttpStatusCode.RequestTimeout, " time out." + handle.timeOut, handle.retryCounter == 0);
            }
            catch (Exception x)
            {
                Debug.LogError($"download {url} error event callback exception:");
                Debug.LogException(x);
            }
            if (handle.retryCounter-- != 0)
            {
                Debug.Log($"timeout left:{handle.retryCounter},progress:{ handle.progress } restart request:{url} timeUse:{Time.time - handle.startTime}");
                handle.timeOut += 1;
                MarkRetryRequest(handle);
            }
            else
            {
                m_finishedRequests.Enqueue(handle);
            }
        }

        protected virtual void OnRequestError(string url,int code, string msg,bool finished)
        {
            var errorText = $"https request error:{url} ,{msg} code:{code}";
            if (onDownLandError == null)
            {
                Debug.LogError(errorText);
            }
            else
            {
                Debug.Log(errorText);
                onDownLandError?.Invoke(url, code, errorText, finished);
            }
        }

        protected void FinishRequest(HttpsResponceInfo handle)
        {
            var request = handle.request;
            if (m_requestInfoMap.TryGetValue(handle.requestId, out var requestInfos))
            {
                var requests = requestInfos.ToArray();
                foreach (var requestInfo in requests)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(requestInfo.localPath))
                            handle.WriteFile(requestInfo.localPath);
                        requestInfo.Finish(handle);
                        if(handle.success)
                            onRequestSuccess?.Invoke(requestInfo.url);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"download {requestInfo.url} success callback exception:");
                        Debug.LogException(e);
                        OnRequestError(request.RequestUri.AbsoluteUri, -1, "", true);
                    }
                }
            }
            ReleaseRequest(handle);
        }

        protected void MarkRetryRequest(HttpsResponceInfo handle)
        {
            if (!m_delyRetryRequestInfos.Contains(handle))
            {
                handle.Reset();
                m_delyRetryRequestInfos.Add(handle);
            }
        }

        public virtual void ClearRequests()
        {
            foreach (var handle in m_allRequestList)
                handle.Reset();
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

        private void ReleaseRequest(HttpsResponceInfo handle)
        {
            if (m_requestInfoMap.TryGetValue(handle.requestId, out var requests))
                requests.Clear();
            handle.Reset();
            m_allRequestList.Remove(handle);
        }

        public virtual void DoRequest(HttpRequestInfo info)
        {
            info.MakeRequestId();
            if (!m_requestInfoMap.TryGetValue(info.requestId, out var requests))
                requests = m_requestInfoMap[info.requestId] = new List<HttpRequestInfo>();
            if (!requests.Contains(info))
                requests.Add(info);
            var handle = m_allRequestList.Find(x => x.requestId == info.requestId);
            if (handle == null)
            {
                handle = new HttpsResponceInfo();
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
                try
                {
                    var req = handle.request = CreateWebRequest(info);
                    byte[] datas = info.bytes;
                    if (datas == null && !string.IsNullOrEmpty(info.json))
                    {
                        datas = Encoding.UTF8.GetBytes(info.json);
                    }
                    if (datas != null)
                    {
                        req.ContentLength = datas.Length;
                        using (Stream reqStream = req.GetRequestStream())
                        {
                            reqStream.Write(datas, 0, datas.Length);
                            reqStream.Close();
                        }
                    }
                    else if (info.form != null)
                    {
                        var flag = DateTime.Now.Ticks.ToString("x");
                        string boundary = string.Format("-----{0}", DateTime.Now.Ticks.ToString("x"));
                        req.ContentType = "multipart/form-data; boundary=" + boundary;
                        using (Stream reqStream = req.GetRequestStream())
                        {
                            WriteDataFromForm(info.form, reqStream, boundary);
                            reqStream.Close();
                        }
                    }
                }
                catch (WebException e)
                {
                    var errorInfo = "failed create webrequest:" + handle.requestId;
#if DEBUG
                    Debug.LogError(errorInfo);
                    Debug.LogException(e);
#endif
                    OnRequestError(info.url, 0, errorInfo, handle.retryCounter == 0);
                    if(handle.retryCounter > 0)
                    {
                        handle.retryCounter--;
                        MarkRetryRequest(handle);
                    }
                }
            }
            else
            {
                Debug.LogError("ignore create request:" + info.requestId);
            }
        }

        private void WriteDataFromForm(Dictionary<string, object> form, Stream rq, string boundary)
        {
            var sp = Encoding.UTF8.GetBytes($"--{boundary}\r\n");
            var end = Encoding.UTF8.GetBytes($"\r\n--{boundary}--");

            foreach (var pair in form)
            {
                if (!(pair.Value is byte[]))
                {
                    rq.Write(sp, 0, sp.Length);
                    var dataHeader = GetKeyValueHeader(pair.Key, pair.Value.ToString());
                    rq.Write(dataHeader, 0, dataHeader.Length);
                }
            }
            foreach (var pair in form)
            {
                if (pair.Value is byte[])
                {
                    rq.Write(sp, 0, sp.Length);
                    var dataHeader = GetFileHeader(pair.Key, pair.Key);
                    rq.Write(dataHeader, 0, dataHeader.Length);
                    var fileData = (byte[])pair.Value;
                    rq.Write(fileData, 0, fileData.Length);
                }

            }
            rq.Write(end, 0, end.Length);
            rq.Close();
        }

        protected virtual HttpWebRequest CreateWebRequest(HttpRequestInfo info)
        {
            HttpWebRequest req = WebRequest.CreateHttp(info.url);
            req.Method = info.requestType;
            info.timeOut = MathF.Max(info.timeOut, defaultTimeout);
            if (info.headers != null)
            {
                foreach (var pair in info.headers)
                {
                    if (pair.Key == "Content-Type")
                        req.ContentType = pair.Value;
                    else
                        req.Headers[pair.Key] = pair.Value;
                }
            }
            req.ContinueTimeout = connectTimeout;
            req.Timeout = (int)info.timeOut * 1000;
            req.AllowWriteStreamBuffering = true;
            return req;
        }

        private void OnCreateNativeObject(UnityEngine.Object target)
        {
            m_nativeObjects.Add(target);
        }

        protected byte[] GetKeyValueHeader(string name, string value)
        {
            string str = $"Content-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}\r\n";
            return Encoding.UTF8.GetBytes(str);
        }
        protected byte[] GetFileHeader(string name, string fileName)
        {
            string str = $"Content-Disposition: form-data; name=\"{name}\"; filename=\"{fileName}\"\r\n" +
                "Content-Type: application/octet-stream\r\n\r\n";
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
