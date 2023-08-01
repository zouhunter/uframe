/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 从resource 加载表的模板类                                                       *
*//************************************************************************************/

using System.IO;
using System.Text;
using UnityEngine;

namespace UFrame.TableCfg
{
    public class ResourceTextLoader : ITextLoader
    {
        protected string m_rootPath;
        protected System.Text.Encoding m_encoding;
        public Encoding Encoding => m_encoding;

        public ResourceTextLoader(string rootPath = "", string textFormat = "UTF-8")
        {
            m_rootPath = rootPath;
            m_encoding = System.Text.Encoding.GetEncoding(textFormat);
            if (m_encoding == null)
            {
                m_encoding = Encoding.UTF8;
            }
        }

        public void LoadStream(string filepath, System.Action<byte, StreamContent> onLoad)
        {
            if (onLoad == null)
                return;

            var ext = System.IO.Path.GetExtension(filepath);
            if (!string.IsNullOrEmpty(ext))
            {
                filepath = filepath.Substring(0, filepath.Length - ext.Length);
            }
            var sourcePath = System.IO.Path.Combine(m_rootPath, filepath);
            var textAsset = Resources.Load<TextAsset>(sourcePath);
            if (textAsset != null)
            {
                using (var memStream = new StreamContent(new System.IO.MemoryStream(textAsset.bytes)))
                {
                    onLoad(0, memStream);
                }
            }
            else
            {
                Debug.LogError("表格路径不存在:" + sourcePath);
            }
        }

        protected virtual string DecodeText(TextAsset textAsset)
        {
            var formatedText = textAsset.text;
            if (m_encoding != System.Text.Encoding.UTF8)
            {
                formatedText = m_encoding.GetString(textAsset.bytes);
            }
            return formatedText;
        }
    }
}
