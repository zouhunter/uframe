/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 场景加载句柄                                                                    *
*//************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UFrame.SceneLoad
{
    public class SceneLoadHandle : ISceneHandle
    {
        public string sceneName { get; private set; }
        public LoadSceneMode loadSceneMode { get; set; }
        protected AsyncOperation operation;
        protected event Action<float> onProgressChanged;
        protected event Action onSuccess;
        protected event Action onFailure;

        public Action<ISceneHandle> onComplete { get; set; }

        private bool started;
        private bool cancel;
        public bool Started => started;
        public float TimeOutActive = 5;
        private float timeOutTimer;
        public SceneLoadHandle(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public void RegistOnFailure(Action onFailure)
        {
            this.onFailure += onFailure;
        }

        public void Failure()
        {
            if (onFailure != null)
            {
                onFailure.Invoke();
            }
        }

        public void Release()
        {
            Resources.UnloadUnusedAssets();
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
            if(!started)
            {
                cancel = false;
                started = true;
                timeOutTimer = Time.time;
                this.operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if(this.operation != null)
                {
                    if(this.operation.isDone)
                    {
                        OnCompleted(this.operation);
                    }
                    else
                    {
                        operation.allowSceneActivation = false;
                        operation.completed += OnCompleted;
                    }
                }
                else
                {
                    Debug.LogError("scene not exists:" + sceneName);
                }
            }
        }

        protected void OnCompleted(AsyncOperation operation)
        {
            if (onSuccess != null){
                onSuccess.Invoke();
            }
            onComplete?.Invoke(this);
            if (cancel)
            {
                SceneManager.UnloadSceneAsync(sceneName);
            }
        }

        public void UpdateState()
        {
            if (operation == null)
                return;

            if(!operation.isDone)
            {
                if (onProgressChanged != null)
                    onProgressChanged.Invoke(operation.progress);

                if(operation.progress >= 0.9f || timeOutTimer - Time.time > TimeOutActive)
                {
                    operation.allowSceneActivation = true;
                }
            }
            else
            {
                operation.allowSceneActivation = true;
            }
        }

        public void Cansale()
        {
            this.onFailure = null;
            this.onSuccess = null;
            this.onComplete = null;
            cancel = true;
            if (SceneManager.GetSceneByName(sceneName) != null)
            {
                SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }
}