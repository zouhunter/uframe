/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 资源加载控制器                                                                  *
*//************************************************************************************/

using UnityEngine;
using UFrame;

namespace UFrame.Source
{
    public interface ISyncSourceCtrl: ISourceController
    {
        /// 同步加载GameObject
        GameObject LoadGameObjectInstence(string sourcePath);
        //同步加载资源
        T LoadSource<T>(string sourcePath, SourceLoadOption option = SourceLoadOption.None) where T : Object;
    }
    public interface IAsyncSyncSourceCtrl: ISourceController
    {
        /// 异步加载GameObject
        ICustomSourceHandle LoadGameObjectInstenceAsync(string sourcePath, LoadSourceCallBack<GameObject> callBack, object context = null);
        //异步加载资源
        ICustomSourceHandle LoadSourceAsync<T>(string sourcePath, LoadSourceCallBack<T> callBack, SourceLoadOption option = SourceLoadOption.None, object context = null) where T : Object;

    }
    public interface ISourceController
    {
        //绑定资源路径
        void RegistSourcePath(int id, string path);
        
        //资源获取方式
        SourceLoadType sourceLoadType { get; }
        
        //得到资源路径
        string FindSourcePath(int sourceID);
      
        /// 回收资源
        void RecoverSource<T>(T obj) where T : Object;

        /// 回收资源
        void ReleaseUnUsedSources();
    }
}