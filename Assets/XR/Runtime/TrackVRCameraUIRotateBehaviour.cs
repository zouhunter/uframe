//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-28 12:33:29
//* 描    述： 绕视野ui

//* ************************************************************************************
using System;
using UnityEngine;

namespace UFrame.XR
{
    [AddComponentMenu("UFrame/Component/TrackVRCameraUIRotateBehaviour")]
    public class TrackVRCameraUIRotateBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected float m_triggerAngle = 5;
        [SerializeField]
        protected float m_moveSpeed = 5;
        [SerializeField]
        protected float m_checkTime = 1;
        [SerializeField]
        protected float m_redios = 2;
        [SerializeField]
        protected float m_height = 2;
        [SerializeField]
        protected Transform m_center;
        [SerializeField]
        protected Camera m_camera;
        [SerializeField]
        protected bool m_clampAngle;
        [SerializeField]
        protected bool m_delyFix;

        protected float m_lastCheckTime;
        protected bool m_moving;
        protected float m_targetAngle;
        protected float m_startMoveAngle;
        protected float m_moveTime;
        protected float m_startMoveTime;
        protected bool m_lookCamera;
        protected bool m_quickFollow;
        private void OnEnable()
        {
            ReCalcutePosition();
        }

        public void SetHeight(float height)
        {
            m_height = height;
            ReCalcutePosition();
            m_lookCamera = height < 1.5f;
        }

        public void ReCalcutePosition()
        {
            m_startMoveAngle = m_targetAngle = GetCameraAngle();
            SetCurrentAngle(m_startMoveAngle);
        }

        private void Update()
        {
            if (!m_quickFollow && Time.time - m_lastCheckTime > m_checkTime)
            {
                m_lastCheckTime = Time.time;
                CheckNeedMove();
            }

            if (m_quickFollow)
            {
                m_targetAngle = GetCameraAngle();
                SetCurrentAngle(m_targetAngle);
            }

            if (m_moving)
            {
                if (Time.time - m_startMoveTime > m_moveTime)
                {
                    SetCurrentAngle(m_targetAngle);
                    m_moving = false;
                }
                else
                {
                    var retios = (Time.time - m_startMoveTime) / m_moveTime;
                    var angle = Mathf.Lerp(m_startMoveAngle, m_targetAngle, retios);
                    SetCurrentAngle(angle);
                }
            }
        }

        protected float GetCameraAngle()
        {
            var cameraForward = m_camera.transform.forward;
            cameraForward.y = 0;
            var angle = Vector3.SignedAngle(Vector3.forward, cameraForward, Vector3.up);
            if (!m_quickFollow && m_clampAngle && Mathf.Abs(angle) < m_triggerAngle)
            {
                angle = 0;
            }
            return angle;
        }

        protected float GetCurrentAngle()
        {
            var forward = transform.position - m_center.transform.position;
            forward.y = 0;
            return Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);
        }

        protected void CheckNeedMove()
        {
            var cameraAngle = GetCameraAngle();
            var currentAngle = GetCurrentAngle();
            var angleDistance = Mathf.Abs(cameraAngle - currentAngle);
            if (angleDistance > m_triggerAngle)
            {
                m_moving = true;
                m_startMoveAngle = currentAngle;
                m_targetAngle = cameraAngle;
                m_moveTime = angleDistance / m_moveSpeed;
                m_startMoveTime = Time.time;
            }
        }

        protected void SetCurrentAngle(float angle)
        {
            transform.position = Quaternion.Euler(new Vector3(0, angle, 0)) * (Vector3.forward * m_redios) + Vector3.up * m_height + m_center.transform.position;
            if (m_lookCamera)
            {
                transform.forward = transform.position - m_camera.transform.position;
            }
            else
            {
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
            }
        }

        public void SetFollowState(bool quick)
        {
            m_quickFollow = quick;
            if (!quick)
            {
                m_startMoveAngle = m_targetAngle = 0;
                SetCurrentAngle(m_startMoveAngle);
            }
        }
    }
}