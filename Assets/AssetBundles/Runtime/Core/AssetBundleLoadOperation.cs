/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源包加载句柄                                                                  *
*//************************************************************************************/

using System;
using UnityEngine;

namespace UFrame.AssetBundles
{
    public class AssetBundleLoadOperation
    {
        public string bundleName { get; private set; }
        public AssetBundle bundle { get; private set; }
        public Action<AssetBundle,string> onRequested { get; set; }
        public Action<float> onProgress { get; set; }
        protected string m_DownloadingError;
        public bool autoUnload { get; set; }
        public AssetBundleLoadOperation(string bundleName)
        {
            this.bundleName = bundleName;
        }

        public bool Update(UrlAssetBundleLoadCtrl bundleRequest)
        {
            if (this.bundle != null)
            {
                return false;
            }

            AssetBundleContent bundleContent = bundleRequest.GetLoadedAssetBundle(bundleName, out m_DownloadingError);
            
            if(bundleContent != null)
            {
                this.bundle = bundleContent.m_AssetBundle;
                if(onRequested != null)
                {
                    onRequested.Invoke(this.bundle, m_DownloadingError);
                }
                else
                {
                   throw new System.Exception("回调失败！");
                }
                return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(m_DownloadingError))
                {
                    Debug.LogError("downland error:" + m_DownloadingError);
                    return false;
                }
                else
                {
                    if (onProgress != null)
                    {
                        var progress = bundleRequest.GetBundleLoadProgress(bundleName);
                        onProgress.Invoke(progress);
                    }
                    return true;
                }
            }
        }
    }
}