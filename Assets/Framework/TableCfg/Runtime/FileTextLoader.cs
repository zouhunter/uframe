using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace Jagat.TableCfg
{
    public class FileTextLoader : ITextLoader
    {
        protected string m_rootPath;
        protected Encoding m_encoding;
        public Encoding Encoding => m_encoding;
        protected string m_fileExt;
        public FileTextLoader(string rootPath,string fileExt, Encoding encoding)
        {
            m_rootPath = rootPath;
            m_fileExt = fileExt;
            m_encoding = encoding;
        }


        public void LoadStream(string filepath, Action<byte, StreamContent> onLoad)
        {
            if (onLoad != null)
            {
                filepath = Path.Combine(m_rootPath, filepath) + m_fileExt;
                using (var stream = new StreamContent(File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    onLoad(0, stream);
                }
            }
        }
    }
}