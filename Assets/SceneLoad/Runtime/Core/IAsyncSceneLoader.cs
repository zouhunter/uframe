/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 场景加载管理器                                                                  *
*//************************************************************************************/

namespace UFrame.SceneLoad
{
    using UFrame;
    using UnityEngine.SceneManagement;

    public interface IAsyncSceneLoader
    {
        ISceneHandle LoadSceneAsync(string sceneName, LoadSceneMode mode);
        ISceneHandle LoadSceneAsync<Loader>(string bundleName, string sceneName, LoadSceneMode mode) where Loader : CustomSceneLoadHandle,new();
    }
}
