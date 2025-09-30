/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源加载 加载选项                                                               *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Source
{
    public interface ICustomSourceHandle : System.IDisposable { }

    public class SourceLoadedHandle : ICustomSourceHandle
    {
        public void Dispose() { }
    }

    public class SourceLoadHandle : ICustomSourceHandle
    {
        public System.MulticastDelegate callBack { get; set; }
        public bool needRecover { get; set; }
        public object context { get; set; }
        public string path { get; set; }
        public string assetName { get; set; }
        public System.Type type { get; set; }
        public System.Action<SourceLoadHandle> onRelease { get; set; }
        public System.Action emptyCallBack { get; set; }
        public virtual void Dispose()
        {
            needRecover = true;
            if (onRelease != null)
                onRelease.Invoke(this);
        }
    }
    public class SourceCacheInfo
    {
        public string path;
        public Object obj;
        public bool isActive;
    }

    public struct AsyncLoadInfo
    {
        public string path { get; set; }
        public SourceLoadOption option { get; set; }
        public System.MulticastDelegate callBack { get; set; }
        public System.Func<string, Object, SourceLoadOption, Object> onSourceLoad { get; set; }
        public System.Action<Object, bool> onSetObjectState { get; set; }
        public System.Action<AsyncLoadInfo> onRelease { get; set; }

        public void CallBack<T>(T prefab, object context) where T : Object
        {
            Object cacheObject = null;
            if (prefab != null)
            {
                cacheObject = onSourceLoad(path, prefab, option);
                onSetObjectState(cacheObject, true);
            }
           ((LoadSourceCallBack<T>)callBack).Invoke((T)cacheObject, context);
            onRelease.Invoke(this);
        }

        public void EmptyCallBack()
        {
            onRelease.Invoke(this);
        }
    }
}
