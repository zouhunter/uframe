/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 场景加载接口                                                                        *
*//************************************************************************************/


namespace UFrame.SceneLoad
{
    public interface ISceneHandle
    {
        bool Started { get; }
        void StartLoad();
        void UpdateState();
        void RegistOnSuccess(System.Action onSuccess);
        void RegistOnFailure(System.Action onFailure);
        void RegistOnProgress(System.Action<float> onProgressChanged);
        void Cansale();
        void Release();
    }
}
