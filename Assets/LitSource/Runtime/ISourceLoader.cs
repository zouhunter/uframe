/*-*-* Copyright (c) uframe@zht
 * Author: zouhangte
 * Creation Date: 2024-12-31
 * Version: 1.0.0
 * Description: 资源控制基础类
 *_*/
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UFrame
{
    public interface ISourceLoader
    {
        void LoadSourceAsync<T>(string address, Action<T> onLoad) where T : UnityEngine.Object;
        void UnloadSource(string address);
    }

    public class AddressableSourceLoader : ISourceLoader, IDisposable
    {
        private Dictionary<string, List<AsyncOperationHandle>> _sourceLoadHandles;//管理handle释放
        private Dictionary<string, UnityEngine.Object> _sourceCache;

        public AddressableSourceLoader()
        {
            _sourceCache = new Dictionary<string, UnityEngine.Object>();
            _sourceLoadHandles = new Dictionary<string, List<AsyncOperationHandle>>();
        }

        public void Dispose()
        {
            var keys = _sourceLoadHandles.Keys.ToArray();
            foreach (var key in keys)
            {
                UnloadSource(key);
            }
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="onLoad"></param>
        public void LoadSourceAsync<T>(string address, Action<T> onLoad) where T : UnityEngine.Object
        {
            if (_sourceCache.TryGetValue(address, out var source) && source is T obj)
            {
                onLoad?.Invoke(obj);
                return;
            }
            var sourceHandle = Addressables.LoadAssetAsync<T>(address);
            if (!_sourceLoadHandles.TryGetValue(address, out var handles))
            {
                handles = _sourceLoadHandles[address] = new List<AsyncOperationHandle>();
            }
            handles.Add(sourceHandle);
            sourceHandle.Completed += (op) =>
            {
                if (op.Status != AsyncOperationStatus.Succeeded)
                {
                    UnityEngine.Debug.LogError("address load failed:" + address);
                    return;
                }
                _sourceCache[address] = sourceHandle.Result;
                onLoad?.Invoke(sourceHandle.Result);
            };
        }
        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="address"></param>
        public void UnloadSource(string address)
        {
            if (_sourceCache.TryGetValue(address, out var obj) && obj)
            {
                UnityEngine.Object.Destroy(obj);
                _sourceCache.Remove(address);
            }
            if (_sourceLoadHandles.TryGetValue(address, out var handles))
            {
                foreach (var item in handles)
                {
                    if (item.IsValid())
                        Addressables.Release(item);
                }
                _sourceLoadHandles.Remove(address);
            }
        }

    }

}
