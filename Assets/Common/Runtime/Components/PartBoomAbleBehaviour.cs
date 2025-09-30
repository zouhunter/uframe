/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 局部刚体激活                                                                    *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/PartBoomAbleBehaviour")]
    public class PartBoomAbleBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected Vector3 m_force = Vector3.up;

        [SerializeField]
        protected List<Rigidbody> m_rigidBodys = new List<Rigidbody>();

        protected Matrix4x4[] m_fragmentsMatrixs;
        protected bool inited = false;

        [SerializeField]
        protected GameObject[] m_boomHides;

        protected virtual void CheckInit()
        {
            if (!inited)
            {
                m_fragmentsMatrixs = new Matrix4x4[m_rigidBodys.Count];
                for (int i = 0; i < m_rigidBodys.Count; i++)
                {
                    var childItem = m_rigidBodys[i];
                    var matrixs = Matrix4x4.TRS(childItem.transform.localPosition, childItem.transform.localRotation, childItem.transform.localScale);
                    m_fragmentsMatrixs[i] = matrixs;
                    var meshCollider = childItem.GetComponent<MeshCollider>();
                    if (meshCollider)
                    {
                        meshCollider.isTrigger = false;
                    }
                }
                inited = true;
            }
        }

        public void ToggleBoomView(bool boom)
        {
            CheckInit();

            if (m_boomHides != null && m_boomHides.Length > 0)
            {
                foreach (var subOringal in m_boomHides)
                {
                    subOringal.SetActive(!boom);
                }
            }

            if (!boom)
            {
                for (int i = 0; i < m_rigidBodys.Count; i++)
                {
                    //var child = m_rigidBodys[i];
                    ToggleBoomTarget(i, false);
                }
            }
            else
            {
                for (int i = 0; i < m_rigidBodys.Count; i++)
                {
                    //var child = m_rigidBodys[i];
                    ToggleBoomTarget(i, true);
                }
            }
        }

        protected void ToggleBoomTarget(int index, bool boom)
        {
            var child = m_rigidBodys[index];
            if (boom)
            {
                child.isKinematic = false;
                child.AddForce(m_force);
            }
            else
            {
                child.isKinematic = true;
                child.transform.localPosition = m_fragmentsMatrixs[index].GetPosition();
                child.transform.localRotation = m_fragmentsMatrixs[index].rotation;
            }
        }

        protected virtual void OnDisable()
        {
            for (int i = 0; i < m_rigidBodys.Count; i++)
            {
                var child = m_rigidBodys[i];
                child.isKinematic = true;
            }
        }

        public void BoomPartOf(float precent)
        {
            var count = m_rigidBodys.Count * precent;
            for (int i = 0; i < count; i++)
            {
                var index = Random.Range(0, m_rigidBodys.Count);
                ToggleBoomTarget(index, true);
            }
        }
    }
}