using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Guide
{
    [ExecuteInEditMode]
    public class GuideObj : ScriptableObject
    {
        [SerializeField]
        public List<GuideInfo> m_guides;

        [ContextMenu("Export Json")]
        public void ExportToJson()
        {
            var group = new GuideInfoGroup();
            group.guides.AddRange(m_guides);
            var jsonData = JsonUtility.ToJson(group);
            GUIUtility.systemCopyBuffer = jsonData;
            Debug.Log(jsonData);
        }
      
        [ExecuteInEditMode]
        private void OnEnable()
        {
           
        }
    }

    [SerializeField]
    public class GuideInfoGroup
    {
        [SerializeField]
        public List<GuideInfo> guides = new List<GuideInfo>();
    }
}
