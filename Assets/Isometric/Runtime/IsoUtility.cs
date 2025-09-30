/****************************************************************************//*
 *   等距视角 -（空间转换工具类）                                              *
 *    -  世界到屏幕坐标                                                        *
 *    -  本地坐标到世界坐标                                                    *
 *    -  世界坐标到本地坐标                                                    *
 *    -  线面交点                                                              *
 *                                                                             *
 *******************************************************************************/

using UnityEngine;

namespace UFrame.Isometric
{
    public static class IsoUtility
    {
        // 世界坐标->屏幕坐标
        public static Vector3 Vec3DToScreen(this Camera mock3dCam, Vector3 vec)
        {
            var screePos = mock3dCam.WorldToScreenPoint(vec);
            screePos.z = 1;
            return screePos;
        }

        // iso坐标->屏幕坐标
        public static Vector3 Iso3DToScreen(this Camera isoCam, Vector3 vec)
        {
            return isoCam.WorldToScreenPoint(vec);
        }

        // 3d坐标->iso坐标
        public static Vector3 Vec3DToIso(this Camera mock3DCamera, Camera isoCamera, Vector3 vec)
        {
            var screenPos = mock3DCamera.WorldToScreenPoint(vec);
            return isoCamera.ScreenToWorldPoint(screenPos) - isoCamera.transform.position;
        }

        // iso坐标->空间平面3d坐标
        public static Vector3 VecIsoToPlane(this Camera isoCamera, Camera mock3DCamera, Vector3 vec, float height)
        {
            var screenPos = isoCamera.WorldToScreenPoint(vec + isoCamera.transform.position);
            var ray = mock3DCamera.ScreenPointToRay(screenPos);
            return VecInsertPlane(ray.direction, ray.origin, Vector3.up, height);
        }

        // 世界坐标->3d平面投影坐标
        public static Vector3 Vec3DToPlane(this Transform mock3dCam, Vector3 vec, float height)
        {
            Vector3 camFoward = mock3dCam.forward;
            return VecInsertPlane(camFoward, vec, Vector3.up, height);
        }

        public static Vector3 VecIsoTo3D(this Camera IsoCamera,Camera mock3DCamera, Vector3 vec, float height)
        {
            var screenPos = IsoCamera.WorldToScreenPoint(vec + IsoCamera.transform.position);
            var ray = mock3DCamera.ScreenPointToRay(screenPos);
            return VecInsertPlane(ray.direction, ray.origin, Vector3.up, height);
        }

        // 平面投影距离
        public static float DistanceXZ(Vector3 o1, Vector3 o2)
        {
            o1.y = 0;
            o2.y = 0;
            return Vector3.Distance(o1, o2);
        }

        public static Quaternion Quat3DToIso(this Transform mock3DCamera, Quaternion quat)
        {
            return Quaternion.Inverse(mock3DCamera.rotation) * quat;
        }

        public static Quaternion QuatIsoTo3D(this Transform mock3DCamera, Quaternion quat)
        {
            return mock3DCamera.rotation * Quaternion.Inverse(quat);
        }

        // 矢量到平面的投影坐标
        public static Vector3 VecInsertPlane(Vector3 vecDir, Vector3 vecPoint, Vector3 planeNormal, float distance)
        {
            float dis = Vector3.Dot(planeNormal, vecPoint) - distance;

            Vector3 insertPoint;
            insertPoint.x = vecDir.x * 1000 + vecPoint.x;
            insertPoint.y = vecDir.y * 1000 + vecPoint.y;
            insertPoint.z = vecDir.z * 1000 + vecPoint.z;

            float td = Vector3.Dot(planeNormal, insertPoint) - distance;
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
