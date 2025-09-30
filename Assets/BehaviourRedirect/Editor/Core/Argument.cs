using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace ScriptRefine
{
    [System.Serializable]
    public class Argument
    {
        public string name;
        public string type;
        public string defultValue;
        public string typeAssemble;
        public string subAssemble;
        public string subType;
        public List<AttributeInfo> attributes;
    }
}