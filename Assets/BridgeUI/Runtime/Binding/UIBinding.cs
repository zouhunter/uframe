//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-01-24 09:25:17
//* 描    述： 通用ui绑定

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BridgeUI
{
    public class UIBinding : BindingReference
    {
        public List<ReferenceItem> infos = new List<ReferenceItem>();
        private Dictionary<string, ReferenceItem> m_refMap;
        private bool m_mapInited;

        private void Awake()
        {
            IntRefItemMap();
        }

        private void IntRefItemMap()
        {
            if (m_mapInited)
                return;
            m_mapInited = true;
            m_refMap = new Dictionary<string, ReferenceItem>();
            foreach (var item in infos)
            {
                m_refMap[item.name] = item;
            }
        }

        public T GetRef<T>(string key) where T : Object
        {
            IntRefItemMap();

            if (m_refMap != null && m_refMap.TryGetValue(key, out var refItem))
            {
                if (refItem.referenceTarget is T)
                {
                    return refItem.referenceTarget as T;
                }
            }
            return default(T);
        }
        public T[] GetRefs<T>(string key) where T : Object
        {
            IntRefItemMap();

            if (m_refMap != null && m_refMap.TryGetValue(key, out var refItem))
            {
                if (typeof(T) == typeof(UnityEngine.Object))
                {
                    return (T[])refItem.referenceTargets.ToArray();
                }
                else if (refItem.referenceTargets != null)
                {
                    var array = new T[refItem.referenceTargets.Count];
                    for (int i = 0; i < refItem.referenceTargets.Count; i++)
                    {
                        array[i] = refItem.referenceTargets[i] as T;
                    }
                    return array;
                }
            }
            return null;
        }

        public T GetValue<T>(string key)
        {
            IntRefItemMap();

            if (m_refMap != null && m_refMap.TryGetValue(key, out var refItem))
            {
                if (string.IsNullOrEmpty(refItem.value))
                {
                    return default(T);
                }
                else if (typeof(T) == typeof(string))
                {
                    return (T)(object)refItem.value;
                }
                else if (typeof(T).IsValueType)
                {
                    return Utility.ChangeType<T>(refItem.value);
                }
            }
            return default(T);
        }

        public T[] GetValues<T>(string key)
        {
            IntRefItemMap();

            if (m_refMap != null && m_refMap.TryGetValue(key, out var refItem))
            {
                if (typeof(T) == typeof(string))
                {
                    return (T[])(object)refItem.values?.ToArray();
                }
                else if (typeof(T).IsValueType && refItem.values != null)
                {
                    var values = new T[refItem.values.Count];
                    for (int i = 0; i < refItem.values.Count; i++)
                    {
                        var value = refItem.values[i];
                        if (!string.IsNullOrEmpty(value))
                        {
                            values[i] = BridgeUI.Utility.ChangeType<T>(value);
                        }
                        else
                        {
                            values[i] = default(T);
                        }
                    }
                }
            }
            return default(T[]);
        }
    }
}