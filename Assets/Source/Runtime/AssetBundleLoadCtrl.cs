/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 资源包加载处理器                                                                *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Source
{
    public class AssetBundleLoadHandle : SourceLoadHandle
    {
        public Object asset;
        public string error;
        public System.Action<string, bool> onReleaseBundle { get; set; }

        public void SetAsset(UnityEngine.Object obj)
        {
            asset = obj;
            error = obj ? null : "empty!";
        }
        public void SetBundle(AssetBundle assetBundle, string error)
        {
            this.error = error;
            if (assetBundle != null)
            {
                if (string.IsNullOrEmpty(assetName))
                {
                    this.asset = assetBundle;
                }
                else
                {
                    this.asset = assetBundle.LoadAsset(assetName, type);
                }
            }
            else
            {
                this.asset = null;
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            onReleaseBundle?.Invoke(path, false);
        }
    }

    public class AssetBundleLoadCtrl : SourceLoadCtrl<AssetBundleLoadHandle>
    {
        public UFrame.AssetBundles.IAssetBundleController AssetBundleCtrl { get; set; }

        protected override AssetBundleLoadHandle LoadPrefabAsyncInternal<T>(string path, LoadSourceCallBack<T> callBack, System.Action emptyCallBack, object context)
        {
            if (AssetBundleCtrl == null)
            {
                Debug.LogError("AssetBundleCtrl == null");
                return null;
            }

            var handle = new AssetBundleLoadHandle();
            handle.emptyCallBack = emptyCallBack;
            handle.callBack = callBack;
            handle.context = context;
            handle.type = typeof(T);
            string assetName;
            SplitPath<T>(ref path, out assetName);
            handle.path = path;
            handle.assetName = assetName;
            handle.onReleaseBundle = AssetBundleCtrl.UnloadAssetBundle;
            if (AssetBundleCtrl != null)
                AssetBundleCtrl.LoadAssetAsync<UnityEngine.Object>(path, assetName, handle.SetAsset, null, true);
            return handle;
        }

        protected virtual void SplitPath<T>(ref string path, out string name)
        {
            if (typeof(T).Equals(typeof(AssetBundle)))
            {
                name = null;
            }
            else
            {
                int index1 = path.LastIndexOf('/');
                int index2 = path.LastIndexOf('\\');
                if (index2 > 0)
                {
                    name = path.Substring(index2 + 1);
                    path = path.Substring(0, index2);
                }
                else if (index1 > 0)
                {
                    name = path.Substring(index1 + 1);
                }
                else
                {
                    name = path;
                }
                path = path.ToLower();
            }
        }

        protected override T LoadPrefabInternal<T>(string path)
        {
            if (AssetBundleCtrl == null)
            {
                Debug.LogError("AssetBundleCtrl == null");
                return default(T);
            }

            string assetName;
            SplitPath<T>(ref path, out assetName);
            return AssetBundleCtrl.LoadAssetSync<T>(path, assetName);
        }

        protected override bool ProcessHandle(AssetBundleLoadHandle handle, out UnityEngine.Object asset)
        {
            if (handle.asset != null)
            {
                asset = handle.asset;
                try
                {
                    if (handle.needRecover)
                    {
                        handle.callBack.DynamicInvoke(null, handle.context);
                    }
                    else
                    {
                        handle.callBack.DynamicInvoke(asset, handle.context);
                    }
                    return true;
                }
                catch (System.Exception e)
                {
                    throw new SourceException("回调执行失败:" + e);
                }
            }
            else if (handle.error != null)
            {
                asset = null;
                handle.callBack.DynamicInvoke(null, handle.context);
                Debug.Log("下载失败：" + handle.error);
                return true;
            }
            asset = null;
            return false;
        }
    }
}