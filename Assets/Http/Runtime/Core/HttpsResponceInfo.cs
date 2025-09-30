using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace UFrame.Http
{
    public class HttpsResponceInfo:IHttpResponceInfo
    {
        public string requestId;
        private string m_text;
        private Texture2D m_texture;
        public NativeObjectCreateEvent nativeObjectCreateEvent;
        public string text=> ReadText();
        public bool success { get; protected set; }
        public HttpStatusCode code { get; protected set; }
        private byte[] m_bytes;
        public byte[] bytes => ReadBytes();
        public int responseCode => (int)code;
        public Texture2D texture =>ReadTexture();
        public bool isDone { get; private set; }

        public HttpWebResponse response;
        public HttpWebRequest request;
        public IAsyncResult operation;
        protected bool m_used;
        public float startTime;
        internal float timeOut;
        internal int retryCounter;
        internal float progress;
        private bool errorAsResult;

        public string error { get; private set; }

        public void GetResponce()
        {
            if (operation == null || isDone)
                return;

            if (operation != null && !operation.IsCompleted)
            {
                success = false;
                error = "GetResponce time out!";
                return;
            }

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(operation);
                success = response.StatusCode == HttpStatusCode.OK || (int)response.StatusCode == 555;
                errorAsResult = false;
            }
            catch (WebException e)
            {
                errorAsResult = true;
                response = (HttpWebResponse)e.Response;
                if (response != null && (int)response.StatusCode == 555)
                {
                    success = true;
                    Debug.LogError("server return 555!");
                }
                error = e.Message;
            }
            if(response!= null)
                code = response.StatusCode;
            progress = 1;
            isDone = true;
        }

        public void WriteFile(string filePath)
        {
            if (response == null)
                return;
            if(m_used)
                return;
            m_used = true;
            
            if(success)
            {
                try
                {
                    //获取response的流
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        using (var ms = new System.IO.FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
                        {
                            byte[] buffer = new byte[512];
                            while (true)
                            {
                                int sz = receiveStream.Read(buffer, 0, 512);
                                if (sz == 0) break;
                                ms.Write(buffer, 0, sz);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(requestId + "GetResponseError:" + e.Message);
                }
            }
        }

        public string ReadText()
        {
            if (response == null)
                return null;

            if (m_used)
                return m_text;

            m_used = true;

            try
            {
                //获取response的流
                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        m_text = readStream.ReadToEnd();
                        response.Close();
                        readStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(requestId + "GetResponseError:" + e.Message);
            }
            
            return m_text;
        }

        public byte[] ReadBytes()
        {
            if (response == null)
                return null;

            if (m_used)
                return m_bytes;

            m_used = true;

            try
            {
                //获取response的流
                using (Stream receiveStream = response.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[512];
                        while (true)
                        {
                            int sz = receiveStream.Read(buffer, 0, 512);
                            if (sz == 0) break;
                            ms.Write(buffer, 0, sz);
                        }
                        m_bytes = ms.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(requestId + " GetResponseError:" + e.Message);
            }
            return m_bytes;
        }

        public Texture2D ReadTexture()
        {
            if (m_texture != null)
                return m_texture;

            if (bytes == null)
                return null;

            m_texture = new Texture2D(1, 1);
            if (m_bytes != null && m_bytes.Length > 0)
            {
                m_texture.LoadImage(m_bytes, true);
            }
            nativeObjectCreateEvent?.Invoke(m_texture);
            return m_texture;
        }

        internal void Reset()
        {
            request?.Abort();
            request = null;
            m_text = null;
            m_used = false;
            response?.Close();
            response = null;
            startTime = 0;
            isDone = false;
            progress = 0;
        }
    }

}
