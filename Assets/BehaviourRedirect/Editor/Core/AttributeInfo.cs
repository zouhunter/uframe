using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace ScriptRefine
{
    [System.Serializable]
    public class AttributeInfo
    {
        public string attribute;
        public SupportAttributes attType = SupportAttributes.NoArgument;
        public string[] keys;
        public string[] values;

        public enum SupportAttributes
        {
            NoArgument,
            RequireComponent,
            CreateAssetMenu,
        }
    }
}