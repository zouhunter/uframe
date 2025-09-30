/*************************************************************************************//*
*  ����: �޺���                                                                       *
*  ʱ��: 2021-05-02                                                                   *
*  �汾: master_aeee4                                                                 *
*  ����:                                                                              *
*   - �������ش�����                                                                  *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace UFrame.SceneLoad
{
    public abstract class CustomBundleSceneLoadHandle : CustomSceneLoadHandle
    {
        protected AsyncOperation operation;

        protected override bool IsDone => operation != null && operation.isDone;

        protected override float Progress => operation?.progress ?? 0;

        protected void OnBundleComplete(bool ok)
        {
            if (ok)
            {
                try
                {
                    this.operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                    operation.allowSceneActivation = false;
                    operation.completed += OnOperationSuccess;
                    loadingBundle = false;
                }
                catch (Exception e)
                {
                    OnFailure();
                    UnityEngine.Debug.LogException(e);
                }
            }
            else
            {
                OnFailure();
            }
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (Progress >= 0.9f)
            {
                SetActivation();
            }
        }

        protected void OnBundleComplete(AssetBundle bundle, string error)
        {
            if (bundle != null)
            {
                OnBundleComplete(true);
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogErrorFormat("assetBundleName bundle:{0} load failed:{1}", assetBundleName, error);
                }
                OnFailure();
            }
        }

        protected void OnAsyncProgress(AsyncOperation operation)
        {
            if (this.operation != operation)
            {
                this.operation = operation;
                this.operation.completed -= OnOperationSuccess;
                this.operation.completed += OnOperationSuccess;
            }

            loadingBundle = false;

            if (operation != null)
            {
                OnProgress(operation.progress);
            }
        }

        private void OnOperationSuccess(AsyncOperation obj)
        {
            base.OnSuccess();
        }

        protected void SetActivation()
        {
            if (operation != null)
                operation.allowSceneActivation = true;
        }
    }
}