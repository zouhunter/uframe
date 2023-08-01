//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEngine;

//namespace UFrame.TableCfg
//{
//    public class UrlTextLoader : ITextLoader
//    {
//        protected UrlDownLoadBehaviour m_downlander;
//        protected string m_urlRoot;
//        protected Encoding m_encoding;
//        protected int m_timeOut = 0;
//        public Encoding Encoding => m_encoding;
//        private string m_ext;

//        public UrlTextLoader(string urlRoot, string ext, string textFormat = "UTF-8",int timeout = 0)
//        {
//            m_urlRoot = urlRoot;
//            m_timeOut = timeout;
//            m_ext = ext;
//            m_encoding = System.Text.Encoding.GetEncoding(textFormat);
//            if (m_encoding == null)
//            {
//                m_encoding = System.Text.Encoding.UTF8;
//            }
//        }

//        public void LoadStream(string path, Action<byte, StreamContent> onLoad)
//        {
//            var url = $"{m_urlRoot}/{path}{m_ext}";
//            PapreDownlander();
//            m_downlander.Downlond(url, (bytes) => {
//                if (onLoad == null)
//                    return;

//                if(bytes == null)
//                {
//                    onLoad(0, null);
//                }
//                else
//                {
//                    using(var stream = new StreamContent(bytes))
//                    {
//                        onLoad(0, stream);
//                    }
//                }
//            });
//        }

//        protected void PapreDownlander()
//        {
//            if(!m_downlander)
//            {
//                var go = new GameObject("UrlTextLoader");
//                GameObject.DontDestroyOnLoad(go);
//                m_downlander = go.AddComponent<UrlDownLoadBehaviour>();
//                m_downlander.SetTimeOut(m_timeOut);
//            }
//        }
//    }
//}