//*************************************************************************************
//* 作    者： 
//* 创建时间： 2022-02-22 17:30:01
//* 描    述：

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;

namespace Jagat.TableCfg
{
    public abstract class ProxyTable<Proxy> : IProxyTable where Proxy : ProxyTable<Proxy>, new()
    {
        internal ITextLoader m_textLoader = null;
        internal Dictionary<byte, ITableDecoder> m_tableDecoders = null;
        private static Proxy m_instance;
        public static Proxy Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new Proxy();
                return m_instance;
            }
        }
        public bool Valid => Table != null;
        protected System.Action<string, bool> m_onLoad;
        protected AsyncTableOperation m_tableLoadOperation;
        public virtual ITable Table { get; protected set; }
        protected HashSet<string> m_loadedFiles;
        public virtual string FileName { get; protected set; }
        private List<string> m_fileNames;
        public virtual List<string> FileNames
        {
            get
            {
                if (m_fileNames == null)
                {
                    m_fileNames = new List<string>();
                    if (!string.IsNullOrEmpty(FileName))
                        m_fileNames.Add(FileName);
                }
                return m_fileNames;
            }
        }
        public bool Finished
        {
            get
            {
                if (Table == null || m_loadedFiles == null)
                    return false;
                if (FileNames != null)
                {
                    foreach (var fileName in FileNames)
                    {
                        if (!m_loadedFiles.Contains(fileName))
                            return false;
                    }
                }
                return true;
            }
        }

        protected virtual System.Text.Encoding TableEncoding { get { return System.Text.Encoding.UTF8; } }
        protected Dictionary<int, object> m_rowSerializeMap;

        protected virtual object CreateRowSerializer(int interfaceId)
        {
            if (typeof(IBinaryRowReader).GetHashCode() == interfaceId)
                return CreateBinaryRowReader();
            if (typeof(ILineRowReader).GetHashCode() == interfaceId)
                return CreateLineRowReader();
            if (typeof(ILineRowWriter).GetHashCode() == interfaceId)
                return CreateLineRowWriter();
            if (typeof(IBinaryRowWriter).GetHashCode() == interfaceId)
                return CreateBinaryRowWriter();
            return null;
        }

        protected virtual IBinaryRowReader CreateBinaryRowReader()
        {
            return new DefaultBinaryRowReader();
        }

        protected virtual ILineRowReader CreateLineRowReader()
        {
            return new DefaultLineRowReader();
        }

        protected virtual ILineRowWriter CreateLineRowWriter()
        {
            return new DefaultLineRowWriter();
        }

        protected virtual IBinaryRowWriter CreateBinaryRowWriter()
        {
            return new DefaultBinaryRowWriter();
        }

        protected virtual IRow CreateRow()
        {
            if (Table != null)
                return Table.CreateRow();
            return null;
        }

        public virtual void SetLoader(ITextLoader tableLoader)
        {
            if (tableLoader != null)
                m_textLoader = tableLoader;
        }

        public virtual void SetDecoder(byte style, ITableDecoder decoder)
        {
            if (m_tableDecoders == null)
                m_tableDecoders = new Dictionary<byte, ITableDecoder>();

            if (decoder != null)
                m_tableDecoders[style] = decoder;

            decoder.GetRowProcess = GetRowSerialize;
            decoder.RowCreater = CreateRow;
        }

        public object GetRowSerialize(int interfaceId)
        {
            if (m_rowSerializeMap == null)
                m_rowSerializeMap = new Dictionary<int, object>();

            if (!m_rowSerializeMap.TryGetValue(interfaceId, out var reader))
            {
                reader = CreateRowSerializer(interfaceId);
                m_rowSerializeMap[interfaceId] = reader;
            }
            return reader;
        }

        protected virtual void LoadTable<T>(string path, System.Action<string, bool> onLoad = null) where T : ITable, new()
        {
            if (Table == null)
                Table = new T();

            var table = Table;
            table.Name = path;

            if (m_textLoader == null)
                m_textLoader = new ResourceTextLoader();

            if (m_tableDecoders == null)
                m_tableDecoders = new Dictionary<byte, ITableDecoder>();

            m_textLoader.LoadStream(path, (style, streamContent) =>
            {
                if (streamContent == null || streamContent.target.Length == 0)
                {
                    Debug.LogError("failed load table:" + path);
                    OnTableLoad(path, false);
                    onLoad?.Invoke(path, false);
                    return;
                }
                if (!m_tableDecoders.TryGetValue(style, out var decoder))
                {
                    decoder = new CsvTableDecoder();
                    SetDecoder(style, decoder);
                }
                var operation = decoder.Decode(table, streamContent, m_textLoader.Encoding);
                if(operation.finished)
                {
                    FinishTableLoad(path);
                }
                else
                {
                    operation.onFinish += () => { FinishTableLoad(path); };
                }
            });
        }

        private void FinishTableLoad(string path)
        {
            try
            {
                if (!FileNames.Contains(path))
                    FileNames.Add(path);
                try
                {
                    OnTableLoad(path, true);
                }
                catch (TableException e)
                {
                    Debug.LogException(e);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public abstract AsyncTableOperation StartLoad(System.Action<string, bool> onLoad);

        protected void OnTableLoad(string path, bool success)
        {
            if (m_loadedFiles == null)
                m_loadedFiles = new HashSet<string>();
            m_loadedFiles.Add(path);
            m_tableLoadOperation?.SetFinish();
            m_onLoad?.Invoke(path, success);
        }

        protected virtual AsyncTableOperation StartLoadInternal<T>(System.Action<string, bool> onLoad) where T : ITable, new()
        {
            this.m_onLoad = onLoad;
            if (m_tableLoadOperation == null)
                m_tableLoadOperation = new AsyncTableOperation();
            m_tableLoadOperation.Reset();
            if (FileNames != null && FileNames.Count > 0)
            {
                for (int i = 0; i < FileNames.Count; i++)
                {
                    var fileName = FileNames[i];
                    LoadTable<T>(fileName, OnTableLoad);
                }
            }
            else
            {
                m_tableLoadOperation.SetFinish();
                Debug.LogError("StartLoad but empty load file!");
            }
            return m_tableLoadOperation;
        }

        protected virtual void OnRelease()
        {
            m_onLoad = null;
            m_loadedFiles?.Clear();
            Table?.Clear();
            Table = null;
            m_loadedFiles = null;
        }

        public static void Release()
        {
            m_instance?.OnRelease();
            m_instance = null;
        }

        public int GetTableNum()
        {
            if (FileNames == null || FileNames.Count == 0)
            {
                if (string.IsNullOrEmpty(FileName))
                    return 0;
                return 1;
            }
            else
            {
                if (string.IsNullOrEmpty(FileName) || FileNames.Contains(FileName))
                    return FileNames.Count;
                else
                    return FileNames.Count + 1;
            }
        }
        public void Dispose()
        {
            Release();
        }
    }

    public abstract class ProxyTable<Proxy, Row> : ProxyTable<Proxy> where Proxy : ProxyTable<Proxy, Row>, new() where Row : IRow, new()
    {
        public virtual Table<Row> GetTable()
        {
            return Table as Table<Row>;
        }
        public virtual Row GetRow(int line)
        {
            var table = GetTable();
            if (table != null)
                return table.GetByLine(line);
            return default(Row);
        }
        public override AsyncTableOperation StartLoad(System.Action<string, bool> onLoad)
        {
            return base.StartLoadInternal<Table<Row>>(onLoad);
        }
    }

    public abstract class ProxyTable<Proxy, K1, Row> : ProxyTable<Proxy, Row> where Proxy : ProxyTable<Proxy, K1, Row>, new() where Row : IRow<K1>, new()
    {
        public override AsyncTableOperation StartLoad(System.Action<string, bool> onLoad)
        {
            return StartLoadInternal<Table<K1, Row>>(onLoad);
        }
        public new Table<K1, Row> GetTable()
        {
            if (Table is Table<K1, Row>)
                return Table as Table<K1, Row>;
            return null;
        }
        public Row GetRowByKey(K1 key)
        {
            var table = GetTable();
            if (table != null)
                return table.GetByKey(key);
            return default(Row);
        }
    }

    public abstract class ProxyTable<Proxy, K1, K2, Row> : ProxyTable<Proxy, K1, Row> where Proxy : ProxyTable<Proxy, K1, K2, Row>, new() where Row : IRow<K1, K2>, new()
    {
        public override AsyncTableOperation StartLoad(System.Action<string, bool> onLoad)
        {
            return StartLoadInternal<Table<K1, K2, Row>>(onLoad);
        }
        public new Table<K1, K2, Row> GetTable()
        {
            if (Table is Table<K1, K2, Row>)
                return Table as Table<K1, K2, Row>;
            return null;
        }
        public Row GetRowByKey(K1 key, K2 key2)
        {
            var table = GetTable();
            if (table != null)
                return table.GetByKey(key, key2);
            return default(Row);
        }
    }
    public abstract class ProxyTable<Proxy, K1, K2, K3, Row> : ProxyTable<Proxy, K1, K2, Row> where Proxy : ProxyTable<Proxy, K1, K2, K3, Row>, new() where Row : IRow<K1, K2, K3>, new()
    {
        public override AsyncTableOperation StartLoad(System.Action<string, bool> onLoad)
        {
            return StartLoadInternal<Table<K1, K2, K3, Row>>(onLoad);
        }
        public new Table<K1, K2, K3, Row> GetTable()
        {
            if (Table is Table<K1, K2, K3, Row>)
                return Table as Table<K1, K2, K3, Row>;
            return null;
        }
        public Row GetRowByKey(K1 key, K2 key2, K3 key3)
        {
            var table = GetTable();
            if (table != null)
                return table.GetByKey(key, key2, key3);
            return default(Row);
        }
    }
}