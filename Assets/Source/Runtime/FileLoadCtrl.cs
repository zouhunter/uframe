/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 文件加载控制器                                                                  *
*//************************************************************************************/

using System;

namespace UFrame.Source
{
    public class FileLoadCtrl : SourceLoadCtrl<SourceLoadHandle>
    {
        protected string m_rootPath;

        public void SetRootPath(string rootPath)
        {
            m_rootPath = rootPath;
        }

        protected override SourceLoadHandle LoadPrefabAsyncInternal<T>(string path, LoadSourceCallBack<T> callBack, Action emptyCallBack, object context)
        {
            SourceLoadHandle loadHandle = new SourceLoadHandle();
            loadHandle.path = path;
            loadHandle.assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            loadHandle.callBack = callBack;
            loadHandle.emptyCallBack = emptyCallBack;
            loadHandle.context = context;
            loadHandle.type = typeof(T);
            return loadHandle;
        }

        // 从指定路径创建Object
        protected virtual UnityEngine.Object LoadObject(string path, System.Type type)
        {
            UnityEngine.Object obj = null;
            var fullPath = System.IO.Path.Combine(m_rootPath, path);
            try
            {
                if (System.IO.File.Exists(fullPath))
                {
                    var bytes = System.IO.File.ReadAllBytes(path);
                    if (type.Equals(typeof(UnityEngine.AssetBundle)))
                    {

                    }
                    else if (type.Equals(typeof(UnityEngine.TextAsset)))
                    {
                        UnityEngine.TextAsset asset = new UnityEngine.TextAsset(System.Text.Encoding.UTF8.GetString(bytes));
                        obj = asset;
                    }
                    else if (type.Equals(typeof(UnityEngine.AudioClip)))
                    {

                    }
                    else if (type.Equals(typeof(UnityEngine.Texture2D)))
                    {

                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return obj;
        }

        protected override T LoadPrefabInternal<T>(string path)
        {
            var obj = LoadObject(path, typeof(T));
            if (obj != null && obj is T)
            {
                return (T)obj;
            }
            return default(T);
        }


        protected override bool ProcessHandle(SourceLoadHandle handle, out UnityEngine.Object asset)
        {
            asset = LoadObject(handle.path, handle.type);
            return true;
        }
    }
}