//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************

namespace UFrame.DressAssetBundle {

    public delegate void DownloadFileEvent(string url, string localPath, System.Action<string,object> onDownloadFinish,System.Action<float> onProgress,object content);
    public delegate void DownloadTextEvent(string url, System.Action<string, object> onDownloadFinish, object content);
    public delegate bool BundleReleaseEvent(BundleItem info,UnityEngine.AssetBundle ab);
}

