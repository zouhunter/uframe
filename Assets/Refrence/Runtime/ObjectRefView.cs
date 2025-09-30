using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace UFrame.Refrence
{
    [System.Serializable]
    public class ObjectRef
    {
        public string name;
        public Object target;
        public string desc;

        public static ObjectRef copyInstance;
        public static List<ObjectRef> copytedRefs;
    }

    public class ObjectRefView : ScriptableObject
    {
        [FormerlySerializedAs("m_description")]
        public string description;
        [HideInInspector, FormerlySerializedAs("m_objRefs")]
        public List<ObjectRef> objRefs = new List<ObjectRef>();
    }

    public abstract class ObjectRefView<T> : ObjectRefView where T : Object
    {
        public Dictionary<string, T> BuildIndexDic()
        {
            var dic = new Dictionary<string, T>();
            for (int i = 0; i < objRefs.Count; i++)
            {
                var item = objRefs[i];
                if (item != null && item.target)
                {
                    if (item.target is T)
                    {
                        dic[item.name] = item.target as T;
                    }
                    else if (item.target is Component && typeof(T).Equals(typeof(GameObject)))
                    {
                        dic[item.name] = (item.target as Component).gameObject as T;
                    }
                    else if (item.target is GameObject && typeof(T).IsSubclassOf(typeof(Component)))
                    {
                        dic[item.name] = (item.target as GameObject).GetComponent<T>();
                    }
                    else if (item.target is Component && typeof(T).IsSubclassOf(typeof(Component)))
                    {
                        dic[item.name] = (item.target as Component).GetComponent<T>();
                    }
                    else
                    {
                        Debug.LogError("type error:" + item.target.GetType());
                    }
                }
                else
                {
                    Debug.LogError("ref empty!");
                }
            }
            return dic;
        }
    }

}