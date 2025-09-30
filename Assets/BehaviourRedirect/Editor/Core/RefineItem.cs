using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
namespace ScriptRefine
{
    [System.Serializable]
    public class RefineItem : System.IComparable<RefineItem>
    {
        public string name;
        public string assemble;
        public string baseType;
        public string type;
        public string[] geneticDatas;
        public List<AttributeInfo> attributes;
        public List<Argument> arguments;
        public string metaFilePath;

        public RefineItem(MonoScript mono)
        {
            Update(mono);
        }
        public RefineItem(Type type)
        {
            Update(type);
        }

        public int CompareTo(RefineItem other)
        {
            return string.Compare(name, other.name);
        }

        internal void Update(MonoScript mono)
        {
            var assetpath = AssetDatabase.GetAssetPath(mono);
            if (assetpath.StartsWith("Assets"))
            {
                this.metaFilePath = assetpath + ".meta";
            }
            Update(mono.GetClass());
        }

        internal void Update(Type type)
        {
            this.name = type.Name;
            this.type = type.ToString();
            this.assemble = type.Assembly.ToString();
            if (type.BaseType != null)
            {
                this.baseType = type.BaseType.ToString();
            }
            arguments = new List<Argument>();
            RefineUtility.AnalysisArguments(type, arguments);
            attributes = new List<AttributeInfo>();
            var atts = type.GetCustomAttributes(false);
            RefineUtility.AnalysisAttributes(atts, attributes);
        }
    }
}
