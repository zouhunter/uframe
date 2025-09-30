/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 场景加载处理器                                                                  *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace UFrame.SceneLoad
{
    public abstract class CustomSceneLoadHandle : ISceneHandle
    {
        public string assetBundleName { get; set; }
        public string sceneName { get; set; }
        public LoadSceneMode loadSceneMode { get; set; }
        public bool Started => started;
        public Action<ISceneHandle> onComplete { get; set; }
        private event Action<float> onProgressChanged;
        private event Action onSuccess;
        private event Action onFailure;
        private bool started;
        protected bool loadingBundle;

        public CustomSceneLoadHandle()
        {
            started = false;
        }

        public void RegistOnFailure(Action onFailure)
        {
            this.onFailure += onFailure;
        }

        public void RegistOnProgress(Action<float> onProgressChanged)
        {
            this.onProgressChanged += onProgressChanged;
        }

        public void RegistOnSuccess(Action onSuccess)
        {
            this.onSuccess += onSuccess;
        }

        public void StartLoad()
        {
            if (!started)
            {
                started = true;
                loadingBundle = true;
                OnStartLoad();
            }
        }

        public virtual void UpdateState()
        {
            if (!IsDone)
            {
                OnProgress(Progress);
            }
        }

        protected abstract bool IsDone { get; }

        protected abstract float Progress { get; }

        protected abstract void OnStartLoad();

        protected void OnProgress(float progress)
        {
            if (onProgressChanged != null)
            {
                if (loadingBundle)
                {
                    progress *= 0.5f;
                }
                else
                {
                    progress = progress * 0.5f + 0.5f;
                }
                onProgressChanged.Invoke(progress);
            }
        }

        protected void OnSuccess()
        {
            if (onSuccess != null)
            {
                onSuccess.Invoke();
            }

            if (onComplete != null)
            {
                onComplete.Invoke(this);
            }
        }

        protected void OnFailure()
        {
            if (onFailure != null)
            {
                onFailure.Invoke();
            }

            if (onComplete != null)
            {
                onComplete.Invoke(this);
            }
        }

        public virtual void Cansale()
        {
            this.onFailure = null;
            this.onSuccess = null;
            this.onComplete = null;
        }

        public abstract void Release();
    }
}