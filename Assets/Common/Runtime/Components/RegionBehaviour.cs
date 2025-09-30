/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 可视化区域                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame
{
    [ExecuteInEditMode]
    [AddComponentMenu("UFrame/Component/RegionBehaviour")]
    public class RegionBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected Transform m_begin;
        [SerializeField]
        protected Transform m_end;
        [SerializeField]
        protected bool m_drawGizmos;

        protected Vector3 m_center;
        protected Vector3 m_size;
        public Vector3 Center => (m_begin.position + m_end.position) * 0.5f;
        public Vector3 Size => m_end.position - m_begin.position;

        [SerializeField]
        protected Color m_gizmosColor = Color.green;
        protected void OnDrawGizmos()
        {
            if (m_drawGizmos)
            {
                Gizmos.color = m_gizmosColor;
                Gizmos.DrawCube(Center, Size);
            }
        }
        public bool Contains(Vector3 pos, bool ignoreY = true)
        {
            if (pos.x < m_begin.position.x || pos.x > m_end.position.x)
                return false;
            if (!ignoreY && (pos.y < m_begin.position.y || pos.y > m_end.position.y))
                return false;
            if (pos.z < m_begin.position.z || pos.z > m_end.position.z)
                return false;
            return true;
        }
    }
}