/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源加载控制器接口                                                              *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Source
{
    public interface ISourceLoadCtrl : System.IDisposable
    {
        System.Action<UnityEngine.Object, bool> onSetObjectState { get; set; }
        T LoadSource<T>(string path, SourceLoadOption option) where T : Object;
        ICustomSourceHandle LoadSourceAsync<T>(string path, SourceLoadOption option, LoadSourceCallBack<T> callBack, object context) where T : Object;
        bool UpdateLoadStates();
        void LowerMemeory();//减少内存（没有引用的资源全部清空）
        void RegistRuntimePrefab(string sourcePath, Object prefab);
        void RemoveRuntimePrefab(string sourcePath);
        void CansaleLoadAllByContext(object context);
    }
}