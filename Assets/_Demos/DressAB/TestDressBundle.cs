//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-23
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using UFrame.DressAB;
using UFrame.Timer;

namespace UFrame
{

    public class TestDressBundle : BaseGameManage<TestDressBundle>
    {
        public string catlogPath;
        public string bundlePath;
        public string remotePath;
        private Http.UnityHttpCtrl httpCtrl;
        private AssetBundleLoadCtrl abLoadCtrl;

        private void Start()
        {
            //TestCatlogParser();
            TestRemoteLoad();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            System.GC.Collect(0);
        }

        private void TestBundleGroupPath()
        {
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            var objs = bundle.LoadAllAssets<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                Debug.LogError(objs[i]);
            }
            Debug.LogError(bundle.LoadAsset<GameObject>("GameObject"));
        }

        private void TestCatlogParser()
        {
            var parser = new CatlogParser();
            parser.LoadFromFile(catlogPath);
            var addressItem = parser.FindAddressItem("aa");
            if (addressItem != null)
            {
                Debug.LogError(addressItem.bundleItem);
                Debug.LogError(addressItem.next);
            }
            else
            {
                Debug.LogError("aa empty!");
            }
        }

        private void TestRemoteLoad()
        {
            httpCtrl = new Http.UnityHttpCtrl();
            httpCtrl.StartHttpLoop();
            abLoadCtrl = new AssetBundleLoadCtrl(OnDownloadFile, OnDownLoadText, Application.persistentDataPath + "/AssetBundleTest");
            abLoadCtrl.simulateInEditor = true;
            var operation = abLoadCtrl.LoadCatlogAsync(remotePath,true);
            operation.RegistComplete(OnCatlogInitFinish);
        }

        private void OnCatlogInitFinish(AsyncCatlogOperation obj)
        {
            //abLoadCtrl.LoadSceneAsync("ss");
            var operation = abLoadCtrl.LoadAssetAsync<GameObject>("aa");
            operation.RegistComplete((x) =>
            {
                GameObject.DontDestroyOnLoad(GameObject.Instantiate(x.asset));
            });
            var sceneOperation = abLoadCtrl.LoadSceneAsync("scene");
            System.GC.Collect();

            TimerAgent.Instance.DelyExecute(()=> {
                operation.Dispose();
                sceneOperation.RegistComplete(x=>x.Dispose());
            },1);
        }

        private void OnDownLoadText(string url, Action<string, object> onDownloadFinish, object content)
        {
            httpCtrl.Get(url, (x, y) => onDownloadFinish(y.text, content));
        }

        private void OnDownloadFile(string url, string localPath, Action<string, object> onDownloadFinish,System.Action<float> onProgress, object content)
        {
            httpCtrl.GetFile(url, localPath, (x, y) => onDownloadFinish(localPath, content));
        }
    }

    public class TimerAgent : Adapter<TimerAgent, Timer.SaftyTimerCtrl>
    {
        protected override SaftyTimerCtrl CreateAgent()
        {
            return new SaftyTimerCtrl();
        }
    }

}

