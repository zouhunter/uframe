//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:18:46
//* 描    述：

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.LitUI
{
    [System.Serializable]
    public class UIBinding
    {
        public List<UIRefItem> refs = new List<UIRefItem>();
        private Dictionary<string, Object> m_refMap;
        public T GetRef<T>(string name) where T : Object
        {
            Object target = null;
            if (m_refMap == null || !m_refMap.TryGetValue(name, out target) || !target)
            {
                var refItem = refs.Find(x => x.name == name && x.obj is T);
                if (refItem != null)
                {
                    target = refItem.obj;
                    if (m_refMap == null)
                        m_refMap = new Dictionary<string, Object>();
                    m_refMap[name] = target;
                }
            }
            if (target is T)
            {
                return target as T;
            }
            return default(T);
        }
        public Object this[string name]
        {
            get
            {
                return GetRef<Object>(name);
            }
        }
    }

    [System.Serializable]
    public class UIRefItem
    {
        public string name;
        public Object obj;
    }
}
