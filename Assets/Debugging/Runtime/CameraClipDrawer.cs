/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 相机裁切可视化                                                                  *
*  记录: 
*  - 06-27 添加命名空间                                                               *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Debugging
{
    [AddComponentMenu("UFrame/Debug/CameraClipDrawer")]
    public class CameraClipDrawer : MonoBehaviour
    {
        [SerializeField]
        private Camera m_camera;
        [SerializeField]
        protected bool m_showNear;
        [SerializeField]
        protected bool m_showFar;
        [SerializeField]
        protected bool m_showBound;
        [SerializeField]
        protected float m_yClip;
        [SerializeField]
        protected float m_nearPlane = 100;

        // Update is called once per frame
        protected void OnDrawGizmos()
        {
            if (!m_camera)
                return;
            DrawCorners();
            DrawBoundarys();
        }

        protected void DrawBoundarys()
        {
            var corners = GetCameraWorldViewPositions(m_camera, m_yClip, true);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(corners[0], corners[1]); // UpperLeft -> UpperRight
            Gizmos.DrawLine(corners[1], corners[3]); // UpperRight -> LowerRight
            Gizmos.DrawLine(corners[3], corners[2]); // LowerRight -> LowerLeft
            Gizmos.DrawLine(corners[2], corners[0]); // LowerLeft -> UpperLeft

            Gizmos.DrawLine(corners[4], corners[0]);
            Gizmos.DrawLine(corners[5], corners[1]);
            Gizmos.DrawLine(corners[6], corners[2]);
            Gizmos.DrawLine(corners[7], corners[3]);
        }

        protected Vector3[] GetCameraWorldViewPositions(Camera camera, float height, bool getNearPos = false)
        {
            Vector3[] positions;
            if (getNearPos)
                positions = new Vector3[8];
            else
                positions = new Vector3[4];

            for (int i = 0; i < 4; i++)
            {
                var x = (i & 2) >> 1;//0,0,1,1
                var y = i & 1;//0,1,0,1
                var ray = m_camera.ViewportPointToRay(new Vector3(x, y, 1));
                positions[i] = VecInsertPanel(ray.direction, ray.origin, Vector3.up, height);
                if (getNearPos)
                    positions[i + 4] = ray.origin;
            }
            return positions;
        }

        protected void DrawCorners()
        {
            Vector3[] corners = GetCorners(m_nearPlane);

            // for debugging
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(corners[0], corners[1]); // UpperLeft -> UpperRight
            Gizmos.DrawLine(corners[1], corners[3]); // UpperRight -> LowerRight
            Gizmos.DrawLine(corners[3], corners[2]); // LowerRight -> LowerLeft
            Gizmos.DrawLine(corners[2], corners[0]); // LowerLeft -> UpperLeft
        }

        protected Vector3[] GetCorners(float distance)
        {
            var tx = m_camera.transform;
            Vector3[] corners = new Vector3[4];

            float halfFOV = (m_camera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
            float aspect = m_camera.aspect;

            float height = distance * Mathf.Tan(halfFOV);
            float width = height * aspect;

            // UpperLeft
            corners[0] = tx.position - (tx.right * width);
            corners[0] += tx.up * height;
            corners[0] += tx.forward * distance;

            // UpperRight
            corners[1] = tx.position + (tx.right * width);
            corners[1] += tx.up * height;
            corners[1] += tx.forward * distance;

            // LowerLeft
            corners[2] = tx.position - (tx.right * width);
            corners[2] -= tx.up * height;
            corners[2] += tx.forward * distance;

            // LowerRight
            corners[3] = tx.position + (tx.right * width);
            corners[3] -= tx.up * height;
            corners[3] += tx.forward * distance;
            return corners;
        }

        public static Vector3 VecInsertPanel(Vector3 vecDir, Vector3 vecPoint, Vector3 panelNormal, float panelD)
        {
            float dis = Vector3.Dot(panelNormal, vecPoint) - panelD;

            Vector3 insertPoint;
            insertPoint.x = vecDir.x * 1000 + vecPoint.x;
            insertPoint.y = vecDir.y * 1000 + vecPoint.y;
            insertPoint.z = vecDir.z * 1000 + vecPoint.z;

            float td = Vector3.Dot(panelNormal, insertPoint) - panelD;
            float k = dis / (dis - td);

            insertPoint.x -= vecPoint.x;
            insertPoint.y -= vecPoint.y;
            insertPoint.z -= vecPoint.z;

            insertPoint.x *= k;
            insertPoint.y *= k;
            insertPoint.z *= k;

            insertPoint.x += vecPoint.x;
            insertPoint.y += vecPoint.y;
            insertPoint.z += vecPoint.z;

            return insertPoint;
        }
    }
}