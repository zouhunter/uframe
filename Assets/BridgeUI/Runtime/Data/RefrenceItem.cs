using UnityEngine;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class SerializeableReferenceItem
    {
        public string name;
        public string assembleName;
        public string typeName;
        public int refereneceInstenceID;
        public string value;
        public bool isArray;
        public List<int> referenceInstenceIDs;
        public List<string> values;
    }

    [System.Serializable]
    public class ReferenceItem
    {
        public string name;
        public Object referenceTarget;
        public string value;
        public System.Type type;
        public List<Object> referenceTargets;
        public List<string> values;
        public bool isArray;
        public string typeFullName;
        public string desc;
    }

    public enum ReferenceItemType
    {
        Reference,
        ConventAble,
        Struct,
        ReferenceArray,
        ConventAbleArray,
        StructArray,
    }

    public class ScriptCreateCatchBehaiver : MonoBehaviour
    {
        public string assembleCatch;
        public string typeCatch;
    }
}
