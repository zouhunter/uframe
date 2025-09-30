/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-12-15                                                                   *
*  功能:                                                                              *
*   - 模型预览相机                                                                    *
*//************************************************************************************/
using UnityEngine;

namespace UFrame
{
    public class CameraItemPerviewBehaviour : MonoBehaviour
    {
        public Transform rotYTs;//控制轴RY,旋转
        public Transform rotXTs;//控制轴RX，俯视和仰视
        public Transform posZTs;//控制轴PZ，远近
        public float smoothingSpeed = 20;
        public float minDistance = 1;
        public float maxDistance = 3;
        //俯视最大角度
        public float minVertAngle = 25;
        //仰视最小角度
        public float maxVertAngle = 50;
        //俯视最大角度
        public float minHoriAngle = -90;
        //仰视最小角度
        public float maxHoriAngle = 90;

        //水平目标值
        private float rotTargetY;
        //水平当前值
        private float currRotTargetY;
        //垂直目标值
        private float rotTargetX;
        //当前垂直目标值
        private float currRotTargetX;
        //目标距离
        private float targetDis;
        //相机1
        private Camera m_camera;

        public float TargetDistance => targetDis;
        public Camera Cam => m_camera;

        //场景更新信息
        public void Awake()
        {
            if (!rotYTs)
                rotYTs = transform.Find("RotY");
            if (!rotXTs)
                rotXTs = rotYTs.Find("RotX");
            if (!posZTs)
                posZTs = rotXTs.Find("PosZ");
            m_camera = posZTs.GetComponentInChildren<Camera>();
            currRotTargetY = rotYTs.localEulerAngles.y;
            currRotTargetX = rotXTs.localEulerAngles.x;
            if (m_camera.orthographic)
                targetDis = m_camera.orthographicSize;
            else
                targetDis = m_camera.fieldOfView;
        }

        public void SetHoriOffset(float xOffset)
        {
            rotTargetY += xOffset;
            rotTargetY = Mathf.Clamp(rotTargetY, minHoriAngle, maxHoriAngle);
        }

        public void SetVertiOffset(float yOffset)
        {
            rotTargetX -= yOffset;
            rotTargetX = Mathf.Clamp(rotTargetX, minVertAngle, maxVertAngle);
        }

        public void SetDistance(float distance)
        {
            targetDis = distance;
            targetDis = Mathf.Clamp(targetDis, minDistance, maxDistance);
        }

        public void SetDistanceOffset(float distanceOffset)
        {
            targetDis += distanceOffset;
            targetDis = Mathf.Clamp(targetDis, minDistance, maxDistance);
        }

        //重置旋转
        public void ResetCameraRot()
        {
            rotTargetY = currRotTargetY;
            currRotTargetY = currRotTargetY + (rotTargetY - currRotTargetY);
            rotYTs.localRotation = Quaternion.Euler(new Vector3(0, currRotTargetY, 0));
        }

        //timer
        public void Update()
        {
            if (rotYTs != null && Mathf.Abs(rotTargetY - currRotTargetY) > 1f)
            {
                currRotTargetY += (rotTargetY - currRotTargetY) * smoothingSpeed * Time.deltaTime;
                rotYTs.localRotation = Quaternion.Euler(new Vector3(0, currRotTargetY, 0));
            }

            if (rotXTs && Mathf.Abs(rotTargetX - currRotTargetX) > 1f)
            {
                currRotTargetX += (rotTargetX - currRotTargetX) * smoothingSpeed * Time.deltaTime;
                rotXTs.localRotation = Quaternion.Euler(new Vector3(currRotTargetX, 0, 0));
            }

            if (m_camera && m_camera.orthographic)
            {
                if (Mathf.Abs(targetDis - m_camera.orthographicSize) > 0.1f)
                {
                    m_camera.orthographicSize = m_camera.orthographicSize + (targetDis - m_camera.orthographicSize) * smoothingSpeed * Time.deltaTime;
                }
            }
            else if (m_camera && !m_camera.orthographic)
            {
                if (Mathf.Abs(targetDis - m_camera.fieldOfView) > 0.1f)
                {
                    m_camera.fieldOfView = m_camera.fieldOfView + (targetDis - m_camera.fieldOfView) * smoothingSpeed * Time.deltaTime;
                }
            }
        }

        //设置相机到固定位置
        public void SetCameraToTarget()
        {
            currRotTargetY = currRotTargetY + (rotTargetY - currRotTargetY);
            rotYTs.localRotation = Quaternion.Euler(new Vector3(0, currRotTargetY, 0));
            currRotTargetX = currRotTargetX + (rotTargetX - currRotTargetX);
            rotXTs.localRotation = Quaternion.Euler(new Vector3(currRotTargetX, 0, 0));
            m_camera.orthographicSize = m_camera.orthographicSize + (targetDis - m_camera.orthographicSize);
        }
    }
}