//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-08-15
//* 描    述：

//* ************************************************************************************
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

namespace UFrame.BridgeUI {
    
    public static class NodeInfoExtend {
        public static void SetPrefabGUID(this NodeInfo nodeInfo, GameObject prefab)
        {
            if (prefab != null)
            {
                var path = AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(path))
                {
                    nodeInfo.guid = AssetDatabase.AssetPathToGUID(path);
                }
            }
            else
            {
                nodeInfo.guid = null;
            }
        }
        public static GameObject GetPrefab(this NodeInfo nodeInfo)
        {
            if (!string.IsNullOrEmpty(nodeInfo.guid))
            {
                var path = AssetDatabase.GUIDToAssetPath(nodeInfo.guid);
                if (!string.IsNullOrEmpty(path))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    return prefab;
                }
            }
            return null;
        }

    }
}

