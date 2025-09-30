//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-03-16 15:46:42
//* 描    述： 代理表管理器

//* ************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.TableCfg
{
    public class ProxyTableController
    {
        private HashSet<IProxyTable> m_proxyTables = new HashSet<IProxyTable>();
        private int m_tableNum = 0;
        public int TableNum => m_tableNum;
        public int LoadedTableNum => m_loadedTableNum;
        private int m_loadedTableNum;
        private System.Action<string, int, bool> m_onLoadAction;

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

        public void StartLoad(ITextLoader loader, System.Action<string, int, bool> onLoad)
        {
            m_loadedTableNum = 0;
            m_onLoadAction = onLoad;
            foreach (var proxyTable in m_proxyTables)
            {
                proxyTable.SetLoader(loader);
                proxyTable.StartLoad(OnLoadTable);
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