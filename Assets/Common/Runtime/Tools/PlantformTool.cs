/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   平台相关工具。                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame
{
    public static class PlantformTool
    {
        #region URL
        private static string streamingAssetsURLPath = null;
        public static string GetStreamingAssetsURLPath()
        {
            if (string.IsNullOrEmpty(streamingAssetsURLPath))
            {
                streamingAssetsURLPath = "file://" + Application.dataPath + "/StreamingAssets";
                if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    streamingAssetsURLPath = "file://" + Application.dataPath + "/Raw";
                }
                else if(Application.platform == RuntimePlatform.Android)
                {
                    streamingAssetsURLPath = "jar:file://" + Application.dataPath + "!/assets";
                }
            }

            return streamingAssetsURLPath;
        }

        private static string temporaryCachePathURLPath = null;
        public static string GetTemporaryCachePathURLPath()
        {
            if (string.IsNullOrEmpty(temporaryCachePathURLPath))
            {
                temporaryCachePathURLPath = "file://" + Application.temporaryCachePath;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    temporaryCachePathURLPath = "file://" + Application.temporaryCachePath;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    temporaryCachePathURLPath = "jar:file://" + Application.temporaryCachePath;
                }
            }

            return temporaryCachePathURLPath;
        }

        private static string persistentDataPathURLPath = null;
        public static string GetPersistentDataPathURLPath()
        {
            if (string.IsNullOrEmpty(persistentDataPathURLPath))
            {
                persistentDataPathURLPath = "file://" + Application.persistentDataPath;
                if(Application.platform == RuntimePlatform.Android)
                {
                    persistentDataPathURLPath = "jar:file://" + Application.persistentDataPath;
                }
                else if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    persistentDataPathURLPath = "file://" + Application.persistentDataPath;
                }
            }

            return persistentDataPathURLPath;
        }
        #endregion
    }
}