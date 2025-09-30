//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述： make sure catlog is latest

//* ************************************************************************************
namespace UFrame.DressAB
{
    public class CatlogCheckUpdater
    {
        public string hashFileName = "catlog.hash";
        public string catlogFileName = "catlog.txt";

        private DownloadFileEvent downloadFileFunc { get; set; }
        private DownloadTextEvent downloadTxtFunc { get; set; }
        private string cacheFolder { get; set; }

        private string url { get; set; }
        private AsyncCatlogOperation catlogOperaiton { get; set; }
        private string m_version;

        public CatlogCheckUpdater(DownloadFileEvent downloadFileFunc, DownloadTextEvent downloadTxtFunc, string cacheFolder)
        {
            this.downloadFileFunc = downloadFileFunc;
            this.downloadTxtFunc = downloadTxtFunc;
            this.cacheFolder = cacheFolder;
        }

        public void StartCheckUpdate(string url, bool update, AsyncCatlogOperation operation)
        {
            this.url = url;
            var versionUrl = $"{url}/{hashFileName}";
            this.catlogOperaiton = operation;

            if (update)
            {
                downloadTxtFunc?.Invoke(versionUrl, OnDowloadVersion, operation);
            }
            else
            {
                var localCatlogPath = $"{cacheFolder}/{catlogFileName}";
                var fileInfo = new System.IO.FileInfo(localCatlogPath);
                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    this.catlogOperaiton?.SetCatlogPath(localCatlogPath);
                }
                else
                {
                    downloadTxtFunc?.Invoke(versionUrl, OnDowloadVersion, operation);
                }
            }
        }

        private void OnDowloadVersion(string version,object content)
        {
            this.m_version = version;
            var localVersionPath = $"{cacheFolder}/{hashFileName}";
            var localCatlogPath = $"{cacheFolder}/{catlogFileName}";
            if (System.IO.File.Exists(localVersionPath))
            {
                var verionOld = System.IO.File.ReadAllText(localVersionPath);
                if(verionOld == version && !string.IsNullOrEmpty(version))
                {
                    OnCatlogPreparFinish(localCatlogPath,null);
                    return;
                }
            }
            var catlogUrl = $"{url}/{catlogFileName}";
            downloadFileFunc?.Invoke(catlogUrl, localCatlogPath, OnCatlogPreparFinish,null,null);
        }

        private void OnCatlogPreparFinish(string localCatlogPath,object content)
        {
            var localVersionPath = $"{cacheFolder}/{hashFileName}";
            System.IO.File.WriteAllText(localVersionPath,m_version);
            this.catlogOperaiton?.SetCatlogPath(localCatlogPath);
        }
    }
}

