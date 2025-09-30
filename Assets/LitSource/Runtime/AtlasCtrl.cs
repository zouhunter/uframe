/*-*-* Copyright (c) uframe@zht
 * Author: zouhangte
 * Creation Date: 2024-12-31
 * Version: 1.0.0
 * Description: 图集加载卸载
 *  1.注意：确保图集名即地址
 *_*/

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.U2D;

namespace UFrame
{
    public class AtlasCtrl : IDisposable
    {
        private Dictionary<string, SpriteAtlas> _atlasCache = new Dictionary<string, SpriteAtlas>();
        private Dictionary<string, Action<SpriteAtlas>> _atlasCallbacks = new Dictionary<string, Action<SpriteAtlas>>();
        private Dictionary<string, List<SpriteLoadInfo>> _spriteLoadBackDic = new Dictionary<string, List<SpriteLoadInfo>>();
        private HashSet<string> _dontdestroyOnLoadAtlas = new HashSet<string>();
        private ISourceLoader _sourceLoader;

        private class SpriteLoadInfo
        {
            /// <summary>
            /// Sprite对应图片名
            /// </summary>
            public string IconName { get; private set; }
            /// <summary>
            /// Sprite加载回调
            /// </summary>
            public Action<Sprite> SpriteBack { get; private set; }

            public SpriteLoadInfo(string iconName, Action<Sprite> callBack)
            {
                IconName = iconName;
                SpriteBack = callBack;
            }

            public void Dispose()
            {
                IconName = null;
                SpriteBack = null;
            }
        }

        public AtlasCtrl(ISourceLoader loader)
        {
            _sourceLoader = loader;
            SpriteAtlasManager.atlasRegistered += OnAtlasRegisted;
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        public virtual void Dispose()
        {
            ReleaseLoadedAtlas(false);
            SpriteAtlasManager.atlasRegistered -= OnAtlasRegisted;
            SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
        }

        /// <summary>
        /// 加载图集
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <param name="callback"></param>
        /// <param name="dontdestroyOnLoad"></param>
        public void LoadAtlasAsync(string atlasPath, System.Action<SpriteAtlas> callback, bool dontdestroyOnLoad = false)
        {
            if (dontdestroyOnLoad)
                _dontdestroyOnLoadAtlas.Add(atlasPath);

            if (_atlasCache.TryGetValue(atlasPath, out var atlas) && atlas)
            {
                callback?.Invoke(atlas);
                return;
            }
            if (_atlasCallbacks.TryGetValue(atlasPath, out var handle) && handle != null)
            {
                handle += callback;
            }
            else
            {
                _atlasCallbacks[atlasPath] = callback;
                DoLoadAtlas(atlasPath);
            }
        }

        /// <summary>
        /// 加载图集资源
        /// </summary>
        /// <param name="atlasPath"></param>
        private void DoLoadAtlas(string atlasPath)
        {
            _sourceLoader.LoadSourceAsync<SpriteAtlas>(atlasPath, (resAtlas) =>
            {
                if (resAtlas == null)
                {
                    Debug.LogErrorFormat(string.Format("{0}路径加载的图集失败", atlasPath));
                    _atlasCallbacks.Remove(atlasPath);
                    return;
                }
                _atlasCache[atlasPath] = resAtlas;
                _atlasCallbacks.TryGetValue(atlasPath, out var callback);
                try
                {
                    callback?.Invoke(resAtlas);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                _atlasCallbacks.Remove(atlasPath);
            });
        }

        /// <summary>
        /// 通过图集路径和图片名 从图集里获取对应Sprite
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <param name="icon"></param>
        /// <param name="spriteBack"></param>
        public void LoadSpriteAsync(string atlasPath, string icon, Action<Sprite> spriteBack, bool dontdestroyOnLoad = false)
        {
            if (dontdestroyOnLoad)
                _dontdestroyOnLoadAtlas.Add(atlasPath);

            if (_atlasCache.TryGetValue(atlasPath, out var atlas) && atlas != null)
            {
                Sprite sprite = atlas.GetSprite(icon);
                spriteBack?.Invoke(sprite);
            }
            else
            {
                //缓存加载sprite回调
                if (!_spriteLoadBackDic.TryGetValue(atlasPath, out List<SpriteLoadInfo> spriteActionList))
                {
                    spriteActionList = new List<SpriteLoadInfo>();
                    _spriteLoadBackDic[atlasPath] = spriteActionList;
                }
                SpriteLoadInfo spriteLoadInfo = new SpriteLoadInfo(icon, spriteBack);
                spriteActionList.Add(spriteLoadInfo);
                //加载图集
                LoadAtlasAsync(atlasPath, (atlas) =>
                {
                    if (atlas && _spriteLoadBackDic.TryGetValue(atlasPath, out List<SpriteLoadInfo> list))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            SpriteLoadInfo info = list[i];
                            Sprite sprite = atlas.GetSprite(info.IconName);
                            info.SpriteBack?.Invoke(sprite);
                            info.Dispose();
                        }
                        list.Clear();
                    }
                }, dontdestroyOnLoad);
            }
        }

        /// <summary>
        /// 释放加载的图集
        /// </summary>
        /// <param name="ignoreDontDestroyOnload"></param>
        public void ReleaseLoadedAtlas(bool ignoreDontDestroyOnload)
        {
            foreach (var list in _spriteLoadBackDic.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Dispose();
                }
            }
            _spriteLoadBackDic.Clear();
            var keys = new List<string>(_atlasCallbacks.Keys);
            foreach (var key in keys)
            {
                if (ignoreDontDestroyOnload && _dontdestroyOnLoadAtlas.Contains(key))
                    continue;

                if (_atlasCallbacks.TryGetValue(key, out var handle))
                    _atlasCallbacks.Remove(key);
            }
        }

        /// <summary>
        /// 引擎请求图集
        /// </summary>
        /// <param name="atlasName"></param>
        /// <param name="action"></param>
        private void OnAtlasRequested(string atlasName, Action<SpriteAtlas> action)
        {
            if (_atlasCache.TryGetValue(atlasName, out var atlas))
            {
                action?.Invoke(atlas);
            }
            else
            {
                if (_atlasCallbacks.TryGetValue(atlasName, out var actions))
                {
                    actions += action;
                }
                else
                {
                    _atlasCallbacks[atlasName] = action;
                    DoLoadAtlas(atlasName);
                }
            }
        }

        /// <summary>
        /// 图集注册成功
        /// </summary>
        /// <param name="atlas"></param>
        private void OnAtlasRegisted(SpriteAtlas atlas)
        {
            _atlasCache[atlas.name] = atlas;
        }
    }
}
