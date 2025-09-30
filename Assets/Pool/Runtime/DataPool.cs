using System.Collections;
using System.Collections.Generic;

namespace UFrame.Pool
{
    public class DataPool
    {
        protected Hashtable m_hashTable = new Hashtable();
        public object this[object id]
        {
            get
            {
                return m_hashTable[id];
            }
            set
            {
                m_hashTable[id] = value;
            }
        }
        public Hashtable Map { get { return m_hashTable; } }
        public bool TryGetValue<K, V>(K key, out V value)
        {
            var objValue = m_hashTable[key];
            var exists = objValue != null && objValue is V;
            if (exists)
            {
                value = (V)objValue;
            }
            else
            {
                value = default(V);
            }
            return exists;
        }
        public void Clear()
        {
            m_hashTable.Clear();
        }
        public string ToJson()
        {
            return HashtableToJson(m_hashTable, 0);
        }
        protected static string HashtableToJson(Hashtable hr, int readcount = 0)
        {
            string json = "{";
            foreach (DictionaryEntry row in hr)
            {
                try
                {
                    string Jsonkey = "\"" + row.Key + "\":";
                    if (row.Value is Hashtable)
                    {
                        Hashtable t = (Hashtable)row.Value;
                        if (t.Count > 0)
                            json += row.Key + HashtableToJson(t, readcount++) + ",";
                        else
                            json += row.Key + "{},";
                    }
                    else if(row.Value is System.IConvertible)
                    {
                        string rowValue = System.Convert.ToString(row.Value);
                        if (row.Value is System.Nullable || row.Value is string)
                            rowValue = "\"" + rowValue + "\","; 
                        else
                            rowValue = rowValue + ",";
                        json += Jsonkey + rowValue;
                    }
                    else
                    {
                        string rowValue = row.Value.ToString();
                        json += Jsonkey + "\"" + rowValue + "\",";
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            var len = UnityEngine.Mathf.Max(1, json.Length - 1);
            json = json.Substring(0, len) + "}";
            return json;
        }
    }
}