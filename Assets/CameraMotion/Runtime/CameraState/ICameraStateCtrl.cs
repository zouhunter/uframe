/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 相机控制器                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.CameraMotion
{
    public interface ICameraStateCtrl
    {
        void RegistCameraStates(int cameraId, System.Type type);
        Camera mainCamera { get; }
        void SetCameraState(int state);
        void RetreatCameraState();
        T GetCameraState<T>(int state) where T : CameraState;
    }
}