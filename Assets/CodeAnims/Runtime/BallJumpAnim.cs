//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-17 11:24:29
//* 描    述： 球式跳跃

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/BallJumpAnim")]
    public class BallJumpAnim : CodeAnimBase
    {
        [SerializeField]
        protected float m_force;
        [SerializeField]
        protected PhysicMaterial m_physicMat;

        protected Vector3 m_pos;
        protected Vector3 m_rot;
        protected Rigidbody m_rigidbody;
        protected bool m_inited;

        public override bool IsPlaying => m_rigidbody?.velocity.magnitude > 0.01f;

        protected override void RecordState()
        {
            m_pos = transform.position;
            m_rot = transform.eulerAngles;
        }

        public override void Play()
        {
            if (!m_inited)
            {
                InitPhysic();
                m_inited = true;
            }
            m_rigidbody.AddForce(Vector3.up * m_force, ForceMode.VelocityChange);
        }

        public override void ResetState()
        {
            if (m_rigidbody)
            {
                m_rigidbody.isKinematic = true;
            }
            if (m_recorded)
            {
                transform.position = m_pos;
                transform.eulerAngles = m_rot;
            }
        }
        protected virtual void InitPhysic()
        {
            if (m_physicMat)
            {
                var collider = GetComponentInChildren<Collider>();
                if (!collider)
                {
                    var meshRender = gameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRender)
                    {
                        collider = meshRender.gameObject.AddComponent<MeshCollider>();
                    }
                }
                if (collider)
                {
                    collider.material = m_physicMat;
                }
            }

            if (!m_rigidbody)
            {
                m_rigidbody = SaftyGetComponent<Rigidbody>();
                m_rigidbody.useGravity = true;
                m_rigidbody.isKinematic = false;
                m_rigidbody.freezeRotation = true;
                m_rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }
        }
    }
}