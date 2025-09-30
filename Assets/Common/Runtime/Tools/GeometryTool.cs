/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 几何相关工具。                                                                  *
*//************************************************************************************/

using System;

using UnityEngine;

namespace UFrame
{
    public static class GeometryTool
    {
        private static Vector2 vc2 = Vector2.zero;
        private static Vector3 vc3 = Vector3.zero;

        #region 根据角度和长度获得绕一个点的一个点
        //根据角度和长度获得绕（0,0,0）的一个点
        public static Vector3 RotateAround(Vector3 worldPos, float worldAngle, float targetDisance, float targetAngle)
        {
            Vector3 vc3 = new Vector3();
            vc3.z = (float)Math.Cos(AngleToRadian(worldAngle + targetAngle)) * targetDisance;
            vc3.x = (float)Math.Sin(AngleToRadian(worldAngle + targetAngle)) * targetDisance;
            return vc3 + worldPos;
        }
        #endregion

        #region 搜敌

        //判断一个点是否在一个四边形区域之内
        public static bool CheckPointInQua(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector3 target)
        {
            Vector2 m = new Vector2(target.x, target.z);
            var ab = b - a;
            var am = m - a;
            //var ad = d - a;
            var bc = c - b;
            var bm = m - b;
            //var ba = a - b;
            var cd = d - c;
            var cm = m - c;
            //var cb = b - c;
            var da = a - d;
            var dm = m - d;
            //var dc = c - d;
            if (Vector2.Dot(ab, am) >= 0 && Vector2.Dot(bc, bm) >= 0 && Vector2.Dot(cd, cm) >= 0 && Vector2.Dot(da, dm) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //判断一个点是否在扇形区域内
        public static bool CheckPointIsInSector(Vector3 center, Vector3 rotate, float angle, float disance, Vector3 target)
        {
            Vector2 m = new Vector2(target.x, target.z);
            Vector2 a = new Vector2();
            a.y = (float)Math.Cos(AngleToRadian(rotate.y + angle / 2)) * disance;
            a.x = (float)Math.Sin(AngleToRadian(rotate.y + angle / 2)) * disance;
            a = a + m;
            Vector2 b = new Vector2();
            b.y = (float)Math.Cos(AngleToRadian(rotate.y - angle / 2)) * disance;
            b.x = (float)Math.Sin(AngleToRadian(rotate.y - angle / 2)) * disance;
            b = b + m;
            Vector2 c = new Vector2(center.x, center.z);
            var ca = a - c;
            var cb = b - c;
            var cm = m - c;
            if (Vector2.Dot(ca, cb) >= Vector2.Dot(ca, cm))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 角度 弧度 转换
        //角度转弧度
        public static float AngleToRadian(float angle)
        {
            return Mathf.PI / 180 * angle;
        }

        //弧度转角度
        public static float RadianToAngle(float Radian)
        {
            return 180 / Mathf.PI * Radian;
        }
        #endregion

        #region  获取两点之间的一个点,向量运算
        /// <summary>
        /// 获取两点之间的一个点,在方法中进行了向量的减法，以及乘法运算
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">结束点</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static Vector3 GetBetweenPoint(Vector3 start, Vector3 end, float distance)
        {
            Vector3 normal = (end - start).normalized;
            return normal * distance + start;
        }

        //获取指向结束点直线上的一个点
        public static Vector3 GetLinePoint(Vector3 start, Vector3 end, float distance)
        {
            Vector3 normal = (end - start).normalized;
            return normal * distance + end;
        }
        #endregion

        #region 点相对于transform朝向的正角度0-360
        public static float ComputAngle(Vector3 pos, RectTransform ts)
        {
            var localPos = pos - ts.position;
            localPos.y = 0;
            var angle = Vector3.Angle(localPos.normalized, new Vector3(0, 0, 1));
            if (localPos.x < 0)
            {
                angle = 360 - angle;
            }
            angle = angle - ts.eulerAngles.y;
            if (angle < 0)
            {
                angle = 360 + angle;
            }
            return angle;
        }
        #endregion

        #region 像素转分辨率
        public static Vector2 PixelsToResolution(Vector2 target)
        {
            float screenW = (float)Screen.width;
            float screenH = (float)Screen.height;
            var w = target.x / screenW;
            var y = target.y / screenH;
            var h = screenW * 1920f / screenW;
            var m = screenW * 1080f / screenW;
            vc2.x = w * h;
            vc2.y = y * m;
            return vc2;
        }
        #endregion

        #region 位偏移运算
        public static long SeralizationVc3ToLong(Vector3 value)
        {
            long yCode = 0;
            if (value.y > 0)
            {
                yCode = 1;
            }
            var currX = (long)(value.x * 100);
            var currY = (long)(Math.Abs((value.y * 100)));
            var currZ = (long)(value.z * 100);
            return (currX << 40) + (currY << 20) + (currZ << 1) + yCode;
        }

        public static Vector3 SeralizationLongToVc3(long value)
        {
            var longX = (value >> 40) << 40;
            var longY = ((value - longX) >> 20) << 20;
            var longZ = ((value - longX - longY) >> 1) << 1;
            var longYCode = value - longX - longY - longZ;
            vc3.x = ((float)(longX >> 40)) / 100f;
            vc3.y = (((float)(longY >> 20)) / 100f);
            vc3.z = ((float)(longZ >> 1)) / 100f;
            if (longYCode == 0)
            {
                vc3.y = -vc3.y;
            }
            return vc3;
        }
        #endregion

        #region 求垂直于A,B长度为distance的pos，xz平面上
        /// <summary>
        /// 求垂直于A,B平面的坐标
        /// </summary>
        /// <returns>The vertical position.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="distance">Distance.</param>
        public static Vector3 ComputeVerticalPos(Vector3 a, Vector3 b, float distance)
        {
            var aa = a;
            a.y = 0;
            a = a.normalized;
            b.y = 0;
            b = b.normalized;
            var vc3 = Vector3.Cross((b - a).normalized, new Vector3(0, 1, 0)).normalized;
            vc3 = (UnityEngine.Random.Range(0, 10) > 5 ? -1 : 1) * vc3 * distance;
            return vc3 + aa;
        }
        #endregion

        #region 圆环坐标
        public static Vector3[] GetCiclePoints(float range, int pointCount)
        {
            var points = new Vector3[pointCount];
            if (pointCount <= 0)
                return points;
            var angle = 360f / pointCount;
            var originDir = Vector3.forward * range;
            for (int i = 0; i < pointCount; i++)
            {
                points[i] = Quaternion.AngleAxis(angle * i, Vector3.up) * originDir;
            }
            return points;
        }
        #endregion

        #region 物体组边界
        public static Bounds CreateBoundFromMesh(GameObject target)
        {
            var meshRenders = target.GetComponentsInChildren<MeshFilter>();
            Vector3 center = Vector3.zero;
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            for (int i = 0; i < meshRenders.Length; i++)
            {
                var childItem = meshRenders[i].transform;
                var vertices = meshRenders[i].sharedMesh.vertices;
                foreach (var vertice in vertices)
                {
                    var worldVerticePos = childItem.TransformPoint(vertice);
                    var localVerticePos = target.transform.InverseTransformPoint(worldVerticePos);
                    max = Vector3.Max(max, localVerticePos);
                    min = Vector3.Min(min, localVerticePos);
                }
            }
            return new Bounds((max + min) * 0.5f, max - min);
        }
        #endregion

    }
}