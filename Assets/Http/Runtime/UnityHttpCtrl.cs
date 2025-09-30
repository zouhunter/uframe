/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - http控制器                                                                      *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Events;
using System;

namespace UFrame.Http
{
    public class UnityHttpCtrl : HttpRequestCtrl, IHttpCtrl
    {
        public bool Runing => Regsited && m_allRequestList.Count > 0;
        public virtual bool Regsited { get; private set; }
        public virtual void StartHttpLoop()
        {
            if (!Regsited)
            {
                Regsited = true;
                BaseGameManage.Single?.StartCoroutine(HttpProcess());
            }
        }

        public virtual void StopHttpLoop()
        {
            if (Regsited)
            {
                ClearRequests();
                Regsited = false;
                BaseGameManage.Single?.StopCoroutine(HttpProcess());
            }
        }

        protected IEnumerator HttpProcess()
        {
            while (Regsited)
            {
                yield return null;
                RefreshRequests();
            }
        }

        public virtual void RegistOnRequestError(RequestErrorEvent callback)
        {
            onDownLandError += callback;
        }

        public virtual void RemoveOnRequestError(RequestErrorEvent callback)
        {
            onDownLandError -= callback;
        }

        public virtual void OnLateUpdate()
        {
            if (Regsited)
            {
                RefreshRequests();
            }
        }

        public HttpRequestInfo GetFile(string url, string path, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = -1, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, UnityWebRequest.kHttpVerbGET);
            if (info != null)
            {
                info.context = context;
                info.handlerCreater = () => new DownloadHandlerFile(path);
                info.headers = headers;
                info.timeOut = timeOut;
                info.callBack = callback;
                info.retryCounter = retryCounter;
                DoRequest(info);
            }
            return info;
        }

        public HttpRequestInfo GetTexture(string url, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = -1, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, UnityWebRequest.kHttpVerbGET);
            if (info != null)
            {
                info.context = context;
                info.headers = headers;
                info.callBack = callback;
                info.timeOut = timeOut;
                info.retryCounter = retryCounter;
                info.handlerCreater = () => new DownloadHandlerTexture();
                DoRequest(info);
            }
            return info;
        }

        public HttpRequestInfo Get(string url, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = -1, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, UnityWebRequest.kHttpVerbGET);
            if (info != null)
            {
                info.context = context;
                info.headers = headers;
                info.callBack = callback;
                info.timeOut = timeOut;
                info.retryCounter = retryCounter;
                DoRequest(info);
            }
            return info;
        }

        public HttpRequestInfo Post(string url, RequestEvent callback, byte[] bytes, Dictionary<string, string> headers = null, object context = null, int timeOut = -1, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, UnityWebRequest.kHttpVerbPOST);
            if (info != null)
            {
                info.context = context;
                info.bytes = bytes;
                info.headers = headers;
                info.callBack = callback;
                info.timeOut = timeOut;
                info.retryCounter = retryCounter;
                DoRequest(info);
            }
            return info;
        }


        public HttpRequestInfo Post(string url, RequestEvent callback, Dictionary<string,object> form, Dictionary<string, string> headers = null, object context = null, int timeOut = -1, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, UnityWebRequest.kHttpVerbPOST);
            if (info != null)
            {
                info.context = context;
                info.form = form;
                info.headers = headers;
                info.callBack = callback;
                info.timeOut = timeOut;
                info.retryCounter = retryCounter;
                DoRequest(info);
            }
            return info;
        }

        public HttpRequestInfo Post(string url, RequestEvent callback, string json, Dictionary<string, string> headers = null, object context = null, int timeOut = -1, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, UnityWebRequest.kHttpVerbPOST);
            if (info != null)
            {
                info.context = context;
                info.json = json;
                info.headers = headers;
                info.callBack = callback;
                info.timeOut = timeOut;
                info.retryCounter = retryCounter;
                DoRequest(info);
            }
            return info;
        }
    }
}