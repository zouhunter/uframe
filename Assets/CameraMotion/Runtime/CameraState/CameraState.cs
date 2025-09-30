/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 相机状态                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.CameraMotion
{
    public abstract class CameraState
    {
        public Camera camera { get; private set; }
        public void SetActive(bool active)
        {
            if (active)
            {
                if (camera == null)
                {
                    camera = GetStateCamera();
                }
                camera.gameObject.SetActive(true);
            }
            else
            {
                if (camera != null)
                {
                    camera.gameObject.SetActive(false);
                }
            }

        }
        public void Recover()
        {
            if (camera && camera.gameObject)
            {
                OnRecoverCamera();
            }
        }

        protected abstract void OnRecoverCamera();
        protected abstract Camera GetStateCamera();

    }
}
