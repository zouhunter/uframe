/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源缓存器                                                                      *
*//************************************************************************************/

using System.Collections.Generic;
using UFrame;
using UnityEngine;
using UFrame.Pool;

namespace UFrame.Source
{
    public class SourceCacheCtrl : System.IDisposable
    {
        //缓存打包对象
        private ObjectPool<SourceCacheInfo> cacheInfoPool;
        //按路径找对象
        private Dictionary<string, List<SourceCacheInfo>> sourceCatchDic;
        //按对象id查找引用源
        private Dictionary<int, SourceCacheInfo> instenceDic;

        public SourceCacheCtrl()
        {
            instenceDic = new Dictionary<int, SourceCacheInfo>();
            cacheInfoPool = new ObjectPool<SourceCacheInfo>(10, ()=>new SourceCacheInfo());
            sourceCatchDic = new Dictionary<string, List<SourceCacheInfo>>();
            Application.lowMemory += OnLowMemory;
        }

        /// <summary>
        /// 释放可以回收的对象
        /// </summary>
        public void OnLowMemory()
        {
            var infos = new List<SourceCacheInfo>(instenceDic.Values);
            for (int i = 0; i < infos.Count; i++)
            {
                var cacheInfo = infos[i];
                if (!cacheInfo.isActive || cacheInfo.obj == null)
                {
                    sourceCatchDic[cacheInfo.path].Remove(cacheInfo);
                    if (cacheInfo.obj != null)
                    {
                        instenceDic.Remove(cacheInfo.obj.GetInstanceID());
                        Object.DestroyImmediate(cacheInfo.obj);
                    }
                    cacheInfoPool.Store(cacheInfo);
                }
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void Dispose()
        {
            using (var enumerator = instenceDic.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.obj != null)
                    {
                        Object.DestroyImmediate(enumerator.Current.obj);
                    }
                }
            }

            instenceDic = null;
            cacheInfoPool.Clear();
            cacheInfoPool = null;
            sourceCatchDic = null;
        }
        /// <summary>
        /// 保存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        public void RecordSourceObjectToCatch<T>(string path, T obj) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new SourceException("地址为空！");
            }
            else if (obj == null)
            {
                throw new SourceException("对象为空！");
            }

            var cacheInfo = cacheInfoPool.GetObject();
            cacheInfo.path = path;
            cacheInfo.isActive = true;
            cacheInfo.obj = obj;

            if (!sourceCatchDic.ContainsKey(path))
            {
                sourceCatchDic[path] = new List<SourceCacheInfo>();
            }

            sourceCatchDic[path].Add(cacheInfo);
            instenceDic[obj.GetInstanceID()] = cacheInfo;
        }

        /// <summary>
        /// 申请使用缓存的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T AllocateSourceObjectFromCatch<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new SourceException("路径为空");
            }

            if (sourceCatchDic.ContainsKey(path))
            {
                var list = sourceCatchDic[path];
                for (int i = 0; i < list.Count; i++)
                {
                    var sourceCache = list[i];
                    if (!sourceCache.isActive && sourceCache.obj != null && sourceCache.obj is T)
                    {
                        sourceCache.isActive = true;
                        return sourceCache.obj as T;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 释放使用的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void ReleaseSourceObjectToCatch<T>(T obj) where T : Object
        {
            if (obj == null)
            {
                throw new SourceException("对象为空");
            }
            var instenceID = obj.GetInstanceID();
            if (instenceDic.ContainsKey(instenceID))
            {
                var sourceInfo = instenceDic[instenceID];
                if (sourceInfo.isActive)
                {
                    sourceInfo.isActive = false;
                }
            }
            else
            {
                throw new SourceException("未记录对象，无法回收：" + obj);
            }
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="sourcePath"></param>
        public void ClearCatchs(string sourcePath)
        {
            List<SourceCacheInfo> infos;
            if (sourceCatchDic.TryGetValue(sourcePath, out infos))
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    var cacheInfo = infos[i];
                    if (cacheInfo.obj != null)
                    {
                        instenceDic.Remove(cacheInfo.obj.GetInstanceID());
                        Object.DestroyImmediate(cacheInfo.obj);
                    }
                    cacheInfoPool.Store(cacheInfo);
                }
                sourceCatchDic.Remove(sourcePath);
            }
        }

    }
}