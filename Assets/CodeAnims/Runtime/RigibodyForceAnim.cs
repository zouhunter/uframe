//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-09-16 04:20:17
//* 描    述： 刚体加力

//* ************************************************************************************
using UnityEngine;

namespace UFrame.CodeAnimation
{
    [AddComponentMenu("UFrame/CodeAnimation/RigibodyForceAnim")]
    public class RigibodyForceAnim : CodeAnimBase
    {
        [SerializeField]
        protected Vector3 m_force;
        [SerializeField]
        protected bool m_worldSpace;
        [SerializeField]
        protected PhysicMaterial m_physicMat;
        [SerializeField]
        protected bool m_lockYRotate;
        [SerializeField, DefaultComponent]
        protected Transform m_forceAnchor;

        protected Vector3 m_pos;
        protected Vector3 m_rot;
        protected Rigidbody m_rigidbody;
        public override bool IsPlaying => m_rigidbody?.velocity.magnitude > 0.01f;
        public override bool MoveAble => true;

        protected override void RecordState()
        {
            m_pos = transform.position;
            m_rot = transform.eulerAngles;
        }

        public override void Play()
        {
            MakeSurePhysic();
            var worldForce = m_force;
            if (!m_worldSpace && transform.parent)
            {
                worldForce = transform.parent.TransformDirection(m_force);
            }
            m_rigidbody.isKinematic = false;

            if (m_forceAnchor && m_forceAnchor != transform)
            {
                m_rigidbody.AddForceAtPosition(worldForce, m_forceAnchor.position, ForceMode.VelocityChange);
            }
            else
            {
                m_rigidbody.AddForce(worldForce, ForceMode.VelocityChange);
            }
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
        protected virtual void MakeSurePhysic()
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
                if (m_lockYRotate)
                {
                    var angleUp = Vector3.Angle(Vector3.up, transform.up);
                    var angleRight = Vector3.Angle(Vector3.up, transform.right);
                    var angleForward = Vector3.Angle(Vector3.up, transform.forward);
                    if (angleUp < 45 || angleUp > 135)
                    {
                        m_rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    }
                    else if (angleRight < 45 || angleRight > 135)
                    {
                        m_rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    }
                    else if (angleForward < 45 || angleForward > 135)
                    {
                        m_rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
                    }
                }
            }
        }
    }
}