/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - csv表读写                                                                       *
*//************************************************************************************/

using System.Collections.Generic;
using System.IO;
using System;
using System.Collections;
using System.Text;

namespace UFrame.TableCfg
{
    public class CsvHelper
    {
        private TableData m_tableDataCache;
        private static CsvHelper m_instance;
        public static CsvHelper Instance
        {
            get
            {

                if (m_instance == null)
                {
                    m_instance = new CsvHelper();
                }
                return m_instance;
            }

        }

        private Stack<StringBuilder> m_dataCachePool = new Stack<StringBuilder>();

        public TableData GetCacheData()
        {
            if (m_tableDataCache == null)
                m_tableDataCache = new TableData();
            m_tableDataCache.Dispose();
            return m_tableDataCache;
        }

        public static void Release()
        {
            if (m_instance != null)
            {
                m_instance.ClearCache();
            }
            m_instance = null;
        }

        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public void SaveCSV(TableData dt, string fullPath, System.Text.Encoding encoding)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            using (FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, encoding))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        WriteRowToString(sw, dt.Rows[i]);
                    }
                    sw.Close();
                }
                fs.Close();
            }
        }

        public void WriteRowToString(StreamWriter sw, IList<string> row)
        {
            if (row == null)
                return;
            for (int i = 0; i < row.Count; i++)
            {
                var str = row[i].Trim();
                if (str != null && (str.Contains("\"") || str.Contains("\n") || str.Contains(",")))
                {
                    str = str.Replace("\"", "\"\"");
                    sw.Write("\""); sw.Write(str); sw.Write("\"");
                }
                else
                {
                    sw.Write(str ?? "");
                }
                if (i < row.Count - 1)
                    sw.Write(",");
                else
                    sw.Write("\n");
            }
        }

        /// <summary>
        /// 读取CSV文件到DataTable中
        /// </summary>
        /// <param name="filePath">CSV的文件路径</param>
        /// <returns></returns>
        public void ReadCSVToTableData(string filePath, System.Text.Encoding encoding, TableData dt)
        {
            using (var fileStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ReadCSVToTableData(fileStream, encoding, dt);
                var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                dt.name = fileName;
                fileStream.Close();
            }
        }
        /// <summary>
        /// 解析csv文档
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public void ReadCSVToTableData(Stream fileStream, System.Text.Encoding encoding, TableData dt)
        {
            int lineNumber = 0;
            using (CsvFileReader reader = new CsvFileReader(fileStream, encoding, GetStringFromPool))
            {
                var row = new List<StringBuilder>();
                int count = 0;
                while ((count = reader.ReadRow(row)) > 0)
                {
                    var contentRows = new List<string>();
                    for (int i = 0; i < count; i++)
                    {
                        var item = row[i];
                        contentRows.Add(item.ToString().Trim());
                    }
                    if (0 == lineNumber)
                    {
                        if (row.Count > 0)
                            dt.name = row[0].ToString().Trim();
                    }
                    dt.Rows.Add(contentRows);
                    lineNumber++;
                }
                SaveStringsToPool(row);
            }
        }

        /// <summary>
        /// 按行解析
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="onReadline"></param>
        public void ReadCSV(Stream stream, System.Text.Encoding encoding, System.Action<int, List<string>,bool> onReadline)
        {
            int lineNumber = 0;
            using (CsvFileReader reader = new CsvFileReader(stream, encoding, GetStringFromPool))
            {
                var row = new List<StringBuilder>();
                int count = reader.ReadRow(row);
                var rowContent = new List<string>();
                while (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var arrayData = row[i];

                        if (arrayData.Length > 0)
                        {
                            rowContent.Add(arrayData.ToString().Trim());
                        }
                        else
                        {
                            rowContent.Add(null);
                        }
                    }
                    count = reader.ReadRow(row);
                    onReadline?.Invoke(lineNumber++, rowContent, count<=0);
                    rowContent.Clear();
                }
                SaveStringsToPool(row);
            }
        }

        public IEnumerator ReadCSVAsync(Stream stream, System.Text.Encoding encoding, System.Action<int, List<string>, bool> onReadline,int groupNum)
        {
            int lineMaxNum = 0;
            int lineNumber = 0;

            using (CsvFileReader reader = new CsvFileReader(stream, encoding, GetStringFromPool))
            {
                var row = new List<StringBuilder>();
                int count = reader.ReadRow(row);
                var rowContent = new List<string>();
                while (count > 0)
                {
                    if(lineMaxNum++> groupNum)
                    {
                        lineMaxNum = 0;
                        yield return null;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        var arrayData = row[i];
                        if(arrayData.Length > 0)
                        {
                            rowContent.Add(arrayData.ToString().Trim());
                        }
                        else
                        {
                            rowContent.Add(null);
                        }
                    }
                    count = reader.ReadRow(row);
                    onReadline?.Invoke(lineNumber++, rowContent, count <= 0);
                    rowContent.Clear();
                }
                SaveStringsToPool(row);
            }
        }

        public void ClearCache()
        {
            if (m_tableDataCache != null)
                m_tableDataCache.Dispose();
            m_tableDataCache = null;
            m_dataCachePool.Clear();
        }

        public StringBuilder GetStringFromPool()
        {
            if (m_dataCachePool.Count > 0)
            {
                return m_dataCachePool.Pop();
            }
            else
            {
                return new StringBuilder();
            }
        }

        public void SaveStringsToPool(List<StringBuilder> sbs)
        {
            foreach (var sb in sbs)
            {
                sb.Clear();
                m_dataCachePool.Push(sb);
            }
        }
    }
}