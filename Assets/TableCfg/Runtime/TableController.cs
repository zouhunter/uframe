/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表控制器使用接口                                                                *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace UFrame.TableCfg
{
    public class TableController : ITableController
    {
        internal Dictionary<object, TableContent> m_tables = new Dictionary<object, TableContent>();
        internal ITextLoader m_textLoader;
        internal Dictionary<byte, ITableDecoder> m_decoders = new Dictionary<byte, ITableDecoder>();

        public virtual T GetConfig<T>(int tableID, int line) where T : IRow, new()
        {
            var table = GetTable<T>(tableID);
            if (table != null)
            {
                return table.GetByLine(line);
            }
            return default(T);
        }

        public virtual void SetLoader(ITextLoader tableLoader)
        {
            if (tableLoader != null)
                m_textLoader = tableLoader;
        }

        public virtual void SetDecoder(byte style, ITableDecoder decoder)
        {
            if (decoder != null)
                m_decoders[style] = decoder;
        }

        public virtual bool TryGetTable<T>(object tableID, out T table) where T : ITable, new()
        {
            table = default(T);
            if (!m_tables.TryGetValue(tableID, out TableContent tableContext) || tableContext.tableInstance == null)
            {
                return false;
            }
            if (!(tableContext.tableInstance is T))
            {
                return false;
            }
            table = (T)tableContext.tableInstance;
            return true;
        }

        public virtual Table<T> GetTable<T>(object tableID) where T : IRow, new()
        {
            if (TryGetTable(tableID, out Table<T> table))
            {
                return table;
            }
            return default(Table<T>);
        }

        public virtual Table<T> FindTable<T>() where T : IRow, new()
        {
            var table = default(Table<T>);
            foreach (var pair in m_tables)
            {
                if (pair.Value != null && pair.Value.tableInstance != null && pair.Value.tableInstance is Table<T>)
                {
                    table = pair.Value.tableInstance as Table<T>;
                }
            }
            return table;
        }

        public virtual T GetConfig<T>(object tableType, int line) where T : IRow, new()
        {
            var table = GetTable<T>(tableType);
            if (table != null)
                return table.GetByLine(line);
            return default(T);
        }

        public virtual void LoadTable(object tableId, string path, System.Type tableType, System.Action<object> onLoad = null)
        {
            if (m_textLoader == null)
                m_textLoader = new ResourceTextLoader();
            m_textLoader.LoadStream(path, (style, bytes) =>
             {
                 if (!m_decoders.TryGetValue(style, out var decoder) || decoder == null)
                 {
                     decoder = new CsvTableDecoder();
                     m_decoders[style] = decoder;
                 }
                 if (!m_tables.TryGetValue(tableId, out TableContent tableContent))
                     tableContent = m_tables[tableId] = new TableContent();
                 tableContent.tableId = tableId;
                 tableContent.tablePath = path;
                 if (tableContent.tableInstance == null || tableContent.tableInstance.GetType() != tableType)
                 {
                     tableContent.tableInstance = System.Activator.CreateInstance(tableType) as ITable;
                 }
                 tableContent.tableInstance.Name = tableId.ToString();
                 var operation = decoder.Decode(tableContent.tableInstance, bytes, m_textLoader.Encoding);
                 var loadFinishAction = new System.Action(() =>
                 {
                     try
                     {
                         OnLoadTable(tableContent);
                         if (onLoad != null)
                             onLoad.Invoke(tableContent.tableInstance);
                     }
                     catch (System.Exception e)
                     {
                         Debug.LogException(e);
                     }
                 });
                 if (operation.finished)
                 {
                     loadFinishAction?.Invoke();
                 }
                 else
                 {
                     operation.onFinish += loadFinishAction;
                 }

             });
        }

        public virtual void LoadTable<T>(object tableId, string path, System.Action<T> onLoad = null) where T : ITable, new()
        {
            if (m_textLoader == null)
                m_textLoader = new ResourceTextLoader();
            m_textLoader.LoadStream(path, (style, stream) =>
            {
                if (!m_decoders.TryGetValue(style, out var decoder) || decoder == null)
                {
                    decoder = new CsvTableDecoder();
                    m_decoders[style] = decoder;
                }
                if (!m_tables.TryGetValue(tableId, out TableContent tableContent))
                    tableContent = m_tables[tableId] = new TableContent();
                tableContent.tableId = tableId;
                tableContent.tablePath = path;
                if (tableContent.tableInstance == null || !(tableContent.tableInstance is T))
                    tableContent.tableInstance = new T();
                tableContent.tableInstance.Name = tableId.ToString();
                var loadFinishAction = new System.Action(() =>
                {
                    try
                    {
                        OnLoadTable(tableContent);
                        if (onLoad != null)
                            onLoad.Invoke((T)tableContent.tableInstance);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
                var operation = decoder.Decode(tableContent.tableInstance, stream, m_textLoader.Encoding);
                if (operation.finished)
                {
                    loadFinishAction?.Invoke();
                }
                else
                {
                    operation.onFinish += loadFinishAction;
                }
            });
        }

        public virtual void LoadTable<T>(object tableId, string path, System.Action<Table<T>> onLoad = null) where T : IRow, new()
        {
            LoadTable<Table<T>>(tableId, path, onLoad);
        }
        public virtual void LoadTable<Key, Row>(object tableId, string path, System.Action<Table<Key, Row>> onLoad = null) where Row : IRow<Key>, new()
        {
            LoadTable<Table<Key, Row>>(tableId, path, onLoad);
        }
        public virtual void LoadTable<Key1, Key2, Row>(object tableId, string path, System.Action<Table<Key1, Key2, Row>> onLoad = null) where Row : IRow<Key1, Key2>, new()
        {
            LoadTable<Table<Key1, Key2, Row>>(tableId, path, onLoad);
        }
        public virtual void LoadTable<Key1, Key2, Key3, Row>(object tableId, string path, System.Action<Table<Key1, Key2, Key3, Row>> onLoad = null) where Row : IRow<Key1, Key2, Key3>, new()
        {
            LoadTable<Table<Key1, Key2, Key3, Row>>(tableId, path, onLoad);
        }

        public virtual Table<Key, Row> GetTable<Key, Row>(object tableID) where Row : IRow<Key>, new()
        {
            if (TryGetTable(tableID, out Table<Key, Row> table))
            {
                return table;
            }
            return null;
        }
        public virtual Table<Key1, Key2, Row> GetTable<Key1, Key2, Row>(object tableID) where Row : IRow<Key1, Key2>, new()
        {
            if (TryGetTable(tableID, out Table<Key1, Key2, Row> table))
            {
                return table;
            }
            return null;
        }
        public virtual Table<Key1, Key2, Key3, Row> GetTable<Key1, Key2, Key3, Row>(object tableID) where Row : IRow<Key1, Key2, Key3>, new()
        {
            if (TryGetTable(tableID, out Table<Key1, Key2, Key3, Row> table))
            {
                return table;
            }
            return null;
        }

        public virtual Row GetConfig<Key, Row>(object tableType, Key key) where Row : IRow<Key>, new()
        {
            var table = GetTable<Key, Row>(tableType);
            if (table != null)
                return table.GetByKey(key);
            return default(Row);
        }
        public virtual Row GetConfig<Key1, Key2, Row>(object tableType, Key1 key1, Key2 key2) where Row : IRow<Key1, Key2>, new()
        {
            var table = GetTable<Key1, Key2, Row>(tableType);
            if (table != null)
                return table.GetByKey(key1, key2);
            return default(Row);
        }
        public virtual Row GetConfig<Key1, Key2, Key3, Row>(object tableType, Key1 key1, Key2 key2, Key3 key3) where Row : IRow<Key1, Key2, Key3>, new()
        {
            var table = GetTable<Key1, Key2, Key3, Row>(tableType);
            if (table != null)
                return table.GetByKey(key1, key2, key3);
            return default(Row);
        }

        protected virtual void OnLoadTable(TableContent tableContent) { }

        public virtual bool CheckAllTableValied()
        {
            foreach (var item in m_tables)
            {
                if (item.Value.tableInstance == null)
                    return false;
            }
            return true;
        }
        public virtual void ClearAll()
        {
            m_tables.Clear();
            CsvHelper.Instance.ClearCache();
        }
    }

}
