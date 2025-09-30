using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace ScriptRefine
{
    [CreateAssetMenu(menuName = "生成/脚本提炼")]
    public class RefineObj : ScriptableObject
    {
        public List<string> ignoreNameSpace = new List<string>();
        public List<string> ignoreType = new List<string>();
        public List<string> ignoreFolder = new List<string>();
        public List<RefineItem> refineList = new List<RefineItem>();
    }
}