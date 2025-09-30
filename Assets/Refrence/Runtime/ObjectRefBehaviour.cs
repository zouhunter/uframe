//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-23 09:30:09
//* 描    述： 组件引导

//* ************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Refrence
{
    public class ObjectRefBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected string m_description;
        [SerializeField]
        protected List<ObjectRef> m_objRefs = new List<ObjectRef>();
        private Dictionary<string, Object> m_objectMap;

        public List<ObjectRef> objRefs => m_objRefs;

        protected virtual void Awake()
        {
            if (objRefs.Count > 0)
            {
                m_objectMap = new Dictionary<string, Object>();
                foreach (var pair in objRefs)
                {
                    m_objectMap[pair.name] = pair.target;
                }
            }
        }

        public Object FindObjectRef(string name)
        {
            if (m_objectMap != null && m_objectMap.TryGetValue(name, out var target))
            {
                return target;
            }
            else
            {
                var refItem = objRefs.Find(x => x.name == name);
                if (refItem != null)
                {
                    if (m_objectMap == null)
                        m_objectMap = new Dictionary<string, Object>();
                    m_objectMap[name] = refItem.target;
                    return refItem.target;
                }
            }
            return null;
        }

        public GameObject FindGameObjectRef(string name)
        {
            var target = FindObjectRef(name);
            if (target is GameObject)
            {
                return target as GameObject;
            }
            else if(target is Component)
            {
                return (target as Component).gameObject;
            }
            return null;
        }

        public T FindComponentRef<T>(string name) where T:Component
        {
            var target = FindObjectRef(name);
            if (target is GameObject)
            {
                return (target as GameObject).GetComponent<T>();
            }
            else if (target is T)
            {
                return target as T;
            }
            return null;
        }

        public void SetObjectRef(string name, Object target)
        {
            var refItem = objRefs.Find(x => x.name == name);
            if (refItem == null)
            {
                refItem = new ObjectRef();
                refItem.name = name;
                objRefs.Add(refItem);
            }
            refItem.target = target;

            if (m_objectMap == null)
                m_objectMap = new Dictionary<string, Object>();

            m_objectMap[name] = target;
        }
    }
}