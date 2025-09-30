/*-*-* Copyright (c) uframe@zht
 * Author: zouhangte
 * Creation Date: 2024-12-31
 * Version: 1.0.0
 * Description: 图片加载卸载
 *_*/

using UFrame;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

namespace UFrame
{
    public class TextureCtrl : IDisposable
    {
        protected Dictionary<string, Texture2D> _registedTextures = new Dictionary<string, Texture2D>();
        protected Dictionary<string, HashSet<string>> _flagTextures = new Dictionary<string, HashSet<string>>();
        protected Dictionary<string, Action<Texture2D>> _asyncOperations = new Dictionary<string, Action<Texture2D>>();
        protected ISourceLoader _sourceLoader;
        public TextureCtrl(ISourceLoader loader)
        {
            this._sourceLoader = loader;
        }

        public virtual void Dispose()
        {

        }

        /// <summary>
        /// 从资源加载图片
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="onLoad"></param>
        public void LoadTextureSource(string address, Action<Texture2D> onLoad, string group = null)
        {
            if (_registedTextures.TryGetValue(address, out var texture) && texture)
            {
                onLoad?.Invoke(texture);
                return;
            }

            if (_asyncOperations.TryGetValue(address, out var textureOp) && textureOp != null)
            {
                textureOp += onLoad;
                return;
            }

            _asyncOperations[address] = onLoad;
            _sourceLoader.LoadSourceAsync<Texture2D>(address, (texture) =>
            {
                if (texture)
                {
                    RegistTexture(address, texture, group);
                    if (!_asyncOperations.TryGetValue(address, out var textureOp))
                        return;
                    textureOp?.Invoke(texture);
                    _asyncOperations.Remove(address);
                }
            });
        }

        /// <summary>
        /// 从网络加载图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void LoadTextureFromUrl(string url, string group, Action<Texture2D> onLoad)
        {
            if (_registedTextures.TryGetValue(url, out var texture) && texture)
            {
                onLoad?.Invoke(texture);
                return;
            }

            if (_asyncOperations.TryGetValue(url, out var textureOp) && textureOp != null)
            {
                textureOp += onLoad;
                return;
            }
            _asyncOperations[url] = onLoad;
            var req = UnityWebRequest.Get(url);
            var textureHandler = new DownloadHandlerTexture();
            req.downloadHandler = textureHandler;
            var asyncOp = req.SendWebRequest();
            asyncOp.completed += (x) =>
            {
                try
                {
                    if (textureHandler != null && asyncOp.isDone && textureHandler.texture)
                    {
                        textureHandler.texture.name = System.IO.Path.GetFileNameWithoutExtension(url);
                        RegistTexture(url, textureHandler.texture, group);
                        texture = textureHandler.texture;
                    }
                    else
                    {
                        texture = null;
                    }
                    if (_asyncOperations.TryGetValue(url, out var textureOp))
                        textureOp?.Invoke(texture);
                    _asyncOperations.Remove(url);
                    req.Dispose();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }

            };
            _asyncOperations[url] = textureOp;
        }

        /// <summary>
        /// 从路径加载图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Texture2D LoadTextureFromPath(string path, string group)
        {
            Debug.Log("LoadTextureFromPath path:" + path);
            if (_registedTextures.TryGetValue(path, out var texture) && texture)
            {
                return texture;
            }
            if (System.IO.File.Exists(path))
            {
                var data = System.IO.File.ReadAllBytes(path);

                if (data == null && data.Length == 0)
                {
                    Debug.LogError(path + ",data.Length 0!");
                    return null;
                }

                try
                {
                    texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    texture.LoadImage(data, true);
                    texture.name = System.IO.Path.GetFileNameWithoutExtension(path);
                    _registedTextures[path] = texture;
                    RecordTextureFlag(path, group);
                    return texture;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                Debug.LogError("file not exists:" + path);
            }
            return null;
        }

        /// <summary>
        /// 注册图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="texture"></param>
        public void RegistTexture(string path, Texture2D texture, string group = "")
        {
            if (_registedTextures.TryGetValue(path, out var oldTexture) && oldTexture && oldTexture != texture)
            {
                UnityEngine.Object.Destroy(oldTexture);
            }
            _registedTextures[path] = texture;
            RecordTextureFlag(path, group);
        }

        /// <summary>
        /// 按tag清理图片
        /// </summary>
        /// <param name="group"></param>
        public void RecoverTextures(string group)
        {
            Debug.Log("RecoverTextures:" + group);
            if (_flagTextures.TryGetValue(group, out var texturePaths))
            {
                if (texturePaths == null)
                    return;

                foreach (var path in texturePaths)
                {
                    if (_registedTextures.TryGetValue(path, out var texture) && texture)
                    {
                        Debug.Log("destory texture:" + texture);
                        GameObject.Destroy(texture);
                        _registedTextures.Remove(path);
                    }
                    if (_asyncOperations.TryGetValue(path, out var op))
                    {
                        _asyncOperations.Remove(path);
                        _sourceLoader.UnloadSource(path);
                    }
                }
                _flagTextures.Remove(group);
            }
        }

        /// <summary>
        /// 记录group
        /// </summary>
        /// <param name="path"></param>
        /// <param name="group"></param>
        private void RecordTextureFlag(string path, string group)
        {
            if (string.IsNullOrEmpty(group))
                return;

            if (!_flagTextures.TryGetValue(group, out var paths))
                paths = _flagTextures[group] = new HashSet<string>();
            paths.Add(path);
        }
    }
}
