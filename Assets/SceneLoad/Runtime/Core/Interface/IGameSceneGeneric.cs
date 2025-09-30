/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 场景接口                                                                        *
*//************************************************************************************/

namespace UFrame.SceneLoad
{
    public interface IGameSceneGeneric<SID>
    {
        void OnEnter(SID sceneId, bool alone);
        void OnExit(SID sceneId, bool alone);
    }

    public interface IReloadAbleGeneric<SID>
    {
        void OnReload(SID sceneId);
    }

    public interface IEntryableGeneric<SID>
    {
        void OnEntry(SID sceneId);
    }

    public interface IUpdateable
    {
        void OnUpdate();
    }

}
