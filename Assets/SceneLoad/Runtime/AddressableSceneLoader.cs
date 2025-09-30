//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2022-04-02 14:15:10
//*  描    述：

//* ************************************************************************************
#if ADDRESSABLE
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UFrame.SceneLoad;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressableSceneLoader : CustomSceneLoadHandle
{
    protected override bool IsDone => opeation.IsDone;
    protected override float Progress => GetProgress();

    protected AsyncOperationHandle<SceneInstance> opeation;

    protected override void OnStartLoad()
    {
        opeation = Addressables.LoadSceneAsync(assetBundleName, loadSceneMode);
        opeation.Completed += OnComplete;
    }

    private void OnComplete(AsyncOperationHandle<SceneInstance> obj)
    {
        base.OnSuccess();
    }

    private float GetProgress()
    {
        var status = opeation.GetDownloadStatus();
        return status.Percent;
    }

    public override void Release()
    {
        try
        {
            Addressables.Release(opeation);
        }
        catch (Exception e)
        {
            Debug.LogError("failed release scene," + e.Message);
        }
    }
}
#endif