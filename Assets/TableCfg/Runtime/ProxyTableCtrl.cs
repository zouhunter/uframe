//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-03-16 15:46:42
//* 描    述： 代理表管理器

//* ************************************************************************************
using UnityEngine;
using System.Collections.Generic;
using System;

namespace UFrame.TableCfg
{
    public delegate void TableLoadEvent(string fileName, int loadedNum, bool ok);
    public class ProxyTableCtrl
    {
        protected HashSet<IProxyTable> m_proxyTables = new HashSet<IProxyTable>();
        private int m_tableNum = 0;
        public int TableNum => m_tableNum;
        public int LoadedTableNum => m_loadedTableNum;
        private int m_loadedTableNum;
        private TableLoadEvent m_onLoadAction;
        private ITextLoader m_loader;
        private Dictionary<byte, ITableDecoder> m_decoders;
        public bool LoadedAll => m_loadedTableNum == m_tableNum;

        public bool GetAllTables(ref List<IProxyTable> tables)
        {
            if (m_proxyTables.Count > 0)
            {
                tables.AddRange(m_proxyTables);
                return true;
            }
            return false;
        }

        public void RegistTable(IProxyTable table)
        {
            RefreshTableInfo(table);
            if (table != null && m_proxyTables.Add(table))
            {
                m_tableNum += table.GetTableNum();
            }
        }

        public void RemoveTable(IProxyTable table)
        {
            if (table != null && m_proxyTables.Remove(table))
            {
                m_tableNum -= table.GetTableNum();
            }
        }

        public void ClearRegisted()
        {
            m_proxyTables.Clear();
        }

        public void StartLoad(TableLoadEvent onLoad = null)
        {
            m_loadedTableNum = 0;
            m_onLoadAction = onLoad;
            foreach (var proxyTable in m_proxyTables)
            {
                RefreshTableInfo(proxyTable);
                proxyTable.StartLoad(OnLoadTable);
            }
        }

        private void RefreshTableInfo(IProxyTable proxyTable)
        {
            proxyTable.SetLoader(m_loader);
            if (m_decoders != null)
            {
                foreach (var item in m_decoders)
                {
                    proxyTable.SetDecoder(item.Key, item.Value);
                }
            }
        }

        public void SetLoader(ITextLoader tableLoader)
        {
            m_loader = tableLoader;
            foreach (var table in m_proxyTables)
            {
                RefreshTableInfo(table);
            }
        }

        public void SetDecoder(byte style, ITableDecoder decoder)
        {
            if (m_decoders == null)
                m_decoders = new Dictionary<byte, ITableDecoder>();
            m_decoders[style] = decoder;
            foreach (var table in m_proxyTables)
            {
                RefreshTableInfo(table);
            }
        }

        private void OnLoadTable(string fileName, bool ok)
        {
            m_loadedTableNum++;
            m_onLoadAction?.Invoke(fileName, m_loadedTableNum, ok);
            if (!ok)
                Debug.LogWarning("failed load table from:" + fileName);
        }

    }
}
