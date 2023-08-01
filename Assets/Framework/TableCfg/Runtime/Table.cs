/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   -多鍵表                                                                              *
*//************************************************************************************/
using UnityEngine;
using System.Collections.Generic;

namespace Jagat.TableCfg
{
    public class Table<Row> : ITable where Row : IRow, new()
    {
        private List<Row> m_rows;
        public List<Row> Rows
        {
            get
            {
                return m_rows;
            }
        }

        public Row this[int index]
        {
            get
            {
                return GetByLine(index);
            }
        }

        public Table()
        {
            m_rows = new List<Row>();
        }

        public int Count
        {
            get
            {
                return m_rows == null ? 0 : m_rows.Count;
            }
        }

        public string Name { get; set; }

        public void GetRows(List<IRow> rows)
        {
            foreach (var row in Rows)
            {
                rows.Add(row);
            }
        }

        public void SetRows(List<IRow> rows)
        {
            Rows.Clear();
            Rows.Capacity = rows.Count;
            foreach (var row in rows)
            {
                Rows.Add((Row)row);
            }
        }


        public List<Row>.Enumerator GetListEnumerator()
        {
            return m_rows.GetEnumerator();
        }

        public virtual void Clear()
        {
            if (m_rows != null)
                m_rows.Clear();
        }

        protected virtual void OnAddRow(Row row)
        {
            m_rows.Add(row);
        }

        public Row GetByLine(int line)
        {
            if (m_rows.Count > line && line >= 0)
            {
                return m_rows[line];
            }
            return default(Row);
        }

        public IRow CreateRow()
        {
            return new Row();
        }

        public void AddRow(IRow row)
        {
            if(row is Row)
            {
                OnAddRow((Row)row);
            }
            else
            {
                Debug.LogError("row type miss!");
            }
        }

        public virtual void SetCapacity(int rowNum)
        {
            m_rows.Capacity = Mathf.Max(m_rows.Capacity, rowNum);
        }
    }

    public class Table<Key, Row> : Table<Row> where Row : IRow<Key>, new()
    {
        private Dictionary<Key, Row> m_keyRowMap;

        public Dictionary<Key, Row> KeyRowMap { get => m_keyRowMap; private set => m_keyRowMap = value; }

        public Table()
        {
            m_keyRowMap = new Dictionary<Key, Row>();
        }


        public Row this[Key key]
        {
            get
            {
                return GetByKey(key);
            }
        }


        public Dictionary<Key, Row>.Enumerator GetKeyDictionaryEnumerator()
        {
            return m_keyRowMap.GetEnumerator();
        }

        public override void Clear()
        {
            base.Clear();
            m_keyRowMap.Clear();
        }

        protected override void OnAddRow(Row row)
        {
            if(row.K1 != null)
            {
                base.OnAddRow(row);
                if (m_keyRowMap.ContainsKey(row.K1))
                {
                    Debug.LogError($"{Name} key already exists:" + row.K1);
                    return;
                }
                m_keyRowMap[row.K1] = row;
            }
            else
            {
                Debug.LogWarning($"{Name} key empty in " + row);
            }
        }

        public Row GetByKey(Key key1)
        {
            if (key1 != null && m_keyRowMap.TryGetValue(key1, out var row))
                return row;
            return default(Row);
        }
        public override void SetCapacity(int rowNum)
        {
            base.SetCapacity(rowNum);
            m_keyRowMap.EnsureCapacity(rowNum);
        }
    }

    public class Table<Key1, Key2, Row> : Table<Row> where Row : IRow<Key1, Key2>, new()
    {
        private Dictionary<Key1, Dictionary<Key2, Row>> m_keyRowMap;
        public Dictionary<Key1, Dictionary<Key2, Row>> KeyRowMap
        {
            get
            {
                return m_keyRowMap;
            }
        }

        public Table()
        {
            m_keyRowMap = new Dictionary<Key1, Dictionary<Key2, Row>>();
        }

        public Row this[Key1 key1,Key2 key2]
        {
            get
            {
                return GetByKey(key1,key2);
            }
        }


        public Dictionary<Key1, Dictionary<Key2, Row>>.Enumerator GetKeyDictionaryEnumerator()
        {
            return m_keyRowMap.GetEnumerator();
        }

        public override void Clear()
        {
            base.Clear();
            m_keyRowMap.Clear();
        }

        protected override void OnAddRow(Row row)
        {
            base.OnAddRow(row);

            if (!m_keyRowMap.TryGetValue(row.K1, out var dic))
            {
                m_keyRowMap[row.K1] = new Dictionary<Key2, Row>();
            }
            if (m_keyRowMap[row.K1].ContainsKey(row.K2))
            {
                Debug.LogError("key already exists:" + row.K1 + "-" + row.K2);
                return;
            }
            m_keyRowMap[row.K1][row.K2] = row;
        }
        public Dictionary<Key2, Row> GetByKey(Key1 key1)
        {
            if (key1 != null && m_keyRowMap.TryGetValue(key1, out var dic))
            {
                return dic;
            }
            return null;
        }

        public Row GetByKey(Key1 key1, Key2 key2)
        {
            if (key1 != null && key2 != null && m_keyRowMap.TryGetValue(key1, out var dic))
            {
                if (dic.TryGetValue(key2, out var row))
                {
                    return row;
                }
            }
            return default(Row);
        }
    }

    public class Table<Key1, Key2, Key3, Row> : Table<Row> where Row : IRow<Key1, Key2, Key3>, new()
    {
        private Dictionary<Key1, Dictionary<Key2, Dictionary<Key3, Row>>> m_keyRowMap;
        public Dictionary<Key1, Dictionary<Key2, Dictionary<Key3, Row>>> KeyRowMap
        {
            get
            {
                return m_keyRowMap;
            }
        }

        public Table()
        {
            m_keyRowMap = new Dictionary<Key1, Dictionary<Key2, Dictionary<Key3, Row>>>();
        }

        public Row this[Key1 key1, Key2 key2,Key3 key3]
        {
            get
            {
                return GetByKey(key1, key2, key3);
            }
        }

        public Dictionary<Key1, Dictionary<Key2, Dictionary<Key3, Row>>>.Enumerator GetKeyDictionaryEnumerator()
        {
            return m_keyRowMap.GetEnumerator();
        }

        public override void Clear()
        {
            base.Clear();
            m_keyRowMap.Clear();
        }

        protected override void OnAddRow(Row row)
        {
            base.OnAddRow(row);
            if (!m_keyRowMap.TryGetValue(row.K1, out var dic))
            {
                dic = m_keyRowMap[row.K1] = new Dictionary<Key2, Dictionary<Key3, Row>>();
            }
            if (!dic.TryGetValue(row.K2, out var dic2))
            {
                dic2 = m_keyRowMap[row.K1][row.K2] = new Dictionary<Key3, Row>();
            }
            if (dic2.ContainsKey(row.K3))
            {
                Debug.LogError("key already exists:" + row.K1 + "-" + row.K2);
                return;
            }
            dic2[row.K3] = row;
        }

        public Dictionary<Key2, Dictionary<Key3,Row>> GetByKey(Key1 key1)
        {
            if (key1 != null && m_keyRowMap.TryGetValue(key1, out var dic))
            {
                return dic;
            }
            return null;
        }

        public Dictionary<Key3, Row> GetByKey(Key1 key1,Key2 key2)
        {
            if (key1 != null && key2 != null && m_keyRowMap.TryGetValue(key1, out var dic))
            {
                if (dic.TryGetValue(key2, out var dic2))
                {
                    return dic2;
                }
            }
            return null;
        }
        public Row GetByKey(Key1 key1, Key2 key2, Key3 key3)
        {
            if (key1 != null && key2 != null && key3 != null && m_keyRowMap.TryGetValue(key1, out var dic))
            {
                if (dic.TryGetValue(key2, out var dic2))
                {
                    if (dic2.TryGetValue(key3, out var row))
                        return row;
                }
            }
            return default(Row);
        }
    }
}