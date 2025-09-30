/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源加载 默认句柄                                                               *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Source
{
    public class ResourceLoadHandle : SourceLoadHandle
    {
        public ResourceRequest resourceRequest;
    }

    public class ResourceLoadCtrl : SourceLoadCtrl<ResourceLoadHandle>
    {
        protected override T LoadPrefabInternal<T>(string path)
        {
            return Resources.Load<T>(path);
        }

        protected override bool ProcessHandle(ResourceLoadHandle handle,out Object asset)
        {
            if(handle.resourceRequest.isDone)
            {
                asset = handle.resourceRequest.asset;
                try
                {
                    if(handle.needRecover)
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
            asset = null;
            return false;
        }

        protected override ResourceLoadHandle LoadPrefabAsyncInternal<T>(string path, LoadSourceCallBack<T> callBack, System.Action emptyCallBack, object context)
        {
            var resourceRequest = Resources.LoadAsync<T>(path);
            var handle = sourceLoadHandlePool.GetObject();
            handle.path = path;
            handle.emptyCallBack = emptyCallBack;
            handle.callBack = callBack;
            handle.resourceRequest = resourceRequest;
            handle.context = context;
            return handle;
        }
    }
}
