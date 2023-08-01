//*************************************************************************************
//* 作    者： zht
//* 创建时间： 2023-05-09 09:52:58
//* 描    述： 带双向认证的http控制器

//* ************************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UFrame.Http
{
    public class TLSHttpCtrl : HttpsRequestCtrl, IHttpCtrl
    {
        public static TimeSpan CONNECT_TIMEOUT = new TimeSpan(10);
        private X509Certificate certificate;
        private string publicKey;

        public void Init(string rowData, string password)
        {
            var bytes = Convert.FromBase64String(rowData);
            certificate = new X509Certificate2(bytes, password, X509KeyStorageFlags.Exportable);
            publicKey = certificate.GetPublicKeyString();
        }


        protected override HttpWebRequest CreateWebRequest(HttpRequestInfo info)
        {
            var request = base.CreateWebRequest(info);
            request.ClientCertificates.Add(certificate);
            return request;
        }

        public HttpRequestInfo GetTexture(string url, RequestEvent callback, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1)
        {
            return this.Get(url, callback, headers, context, timeOut,retryCounter);
        }

        public HttpRequestInfo Get(string url, RequestEvent callback, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1)
        {
            return this.GetFile(url,null,callback,headers,context,timeOut,retryCounter);
        }

        public HttpRequestInfo GetFile(string url, string localPath, RequestEvent callback, Dictionary<string, string> headers = null, object context = null, int timeOut = 0, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, "GET");
            info.callBack = callback;
            info.headers = headers;
            info.context = context;
            info.timeOut = timeOut;
            info.localPath = localPath;
            info.retryCounter = retryCounter;
            DoRequest(info);
            return info;
        }

        public HttpRequestInfo Post(string url, RequestEvent callback, string json, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1)
        {
            headers["Content-Type"] = "application/json";
            var info = new HttpRequestInfo(url, "POST");
            info.callBack = callback;
            info.json = json;
            info.headers = headers;
            info.context = context;
            info.timeOut = timeOut;
            info.retryCounter = retryCounter;
            DoRequest(info);
            return info;
        }

        public HttpRequestInfo Post(string url, RequestEvent callback, byte[] datas, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1)
        {
            headers["Content-Type"] = "application/json";
            var info = new HttpRequestInfo(url, "POST");
            info.callBack = callback;
            info.bytes = datas;
            info.headers = headers;
            info.context = context;
            info.timeOut = timeOut;
            info.retryCounter = retryCounter;
            DoRequest(info);
            return info;
        }

        public async void PostTesting(string url, RequestEvent callback, byte[] datas, Dictionary<string, string> headers, object context = null, int timeOut = 0)
        {
            Debug.LogError("Post:" + url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            foreach (var pair in headers)
            {
                if (pair.Key == "Content-Type")
                    continue;
                req.Headers[pair.Key] = pair.Value;
            }
            //InitRequest(req, headers, timeOut);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = datas.Length;
            req.ClientCertificates.Add(certificate);
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(datas, 0, datas.Length);
                reqStream.Close();
            }
            try
            {
                var response = await req.GetResponseAsync();
                //获取response的流
                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        var text = readStream.ReadToEnd();
                        Debug.LogError(text);
                        response.Close();
                        readStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void PostFormMultipartMain(string requestUri, string sign, string file, Dictionary<string, string> headers)
        {
            HttpWebRequest request = WebRequest.CreateHttp(requestUri);
            foreach (var pair in headers)
            {
                if (pair.Key == "Content-Type")
                    continue;
                request.Headers[pair.Key] = pair.Value;
            }
            request.Method = WebRequestMethods.Http.Post;

            var sp = Encoding.UTF8.GetBytes("-----------------------------7e33352f1074\r\n");
            var end = Encoding.UTF8.GetBytes("\r\n-----------------------------7e33352f1074--");

            request.ContentType = "multipart/form-data; boundary=---------------------------7e33352f1074";

            var rq = request.GetRequestStream();

            rq.Write(sp, 0, sp.Length);

            var dataHeader = GetKeyValueHeader("type", "img");
            rq.Write(dataHeader, 0, dataHeader.Length);
            rq.Write(sp, 0, sp.Length);

            dataHeader = GetKeyValueHeader("sign", sign);
            rq.Write(dataHeader, 0, dataHeader.Length);
            rq.Write(sp, 0, sp.Length);

            var filePath = file;
            dataHeader = GetFileHeader("file", filePath);
            rq.Write(dataHeader, 0, dataHeader.Length);
            var fileData = File.ReadAllBytes(filePath);
            rq.Write(fileData, 0, fileData.Length);

            rq.Write(end, 0, end.Length);
            rq.Close();

            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            Debug.LogError(reader.ReadToEnd());
        }


        internal bool CheckServerCertificate(string certificatePublicKey)
        {
            return certificatePublicKey == this.publicKey;
        }

        public HttpRequestInfo Post(string url, RequestEvent callback, Dictionary<string,object> form, Dictionary<string, string> headers, object context = null, int timeOut = 0, int retryCounter = -1)
        {
            var info = new HttpRequestInfo(url, "POST");
            info.callBack = callback;
            info.form = form;
            info.headers = headers;
            info.context = context;
            info.timeOut = timeOut;
            info.retryCounter = retryCounter;
            DoRequest(info);
            return info;
        }

    }
}
