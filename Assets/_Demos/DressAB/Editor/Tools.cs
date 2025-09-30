//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;

namespace UFrame.DressAB.Editors {
    
    public class Tools : Editor {
        [MenuItem("Tools/OpenCacheFolder")]
        static void OpenCacheFolder()
        {
            Application.OpenURL(new System.Uri(Application.persistentDataPath).AbsoluteUri);
        }
    }
}

