using UnityEngine;

namespace UFrame
{
    public static class MatrixTool
    {
        public static void Matrix4x4(Transform transfrom, Matrix4x4 matrix4X4, Matrix4x4 rootMatrix)
        {
            Matrix4x4private(transfrom, matrix4X4, rootMatrix);
        }

        private static void Matrix4x4private(Transform transfrom, Matrix4x4 matrix4X4, Matrix4x4 rootMatrix)
        {
            //这里需要处理，原先为本地坐标
            matrix4X4 = rootMatrix * matrix4X4;
            //transfrom.localScale = matrix4X4.GetScale();
            transfrom.rotation = GetRotation(matrix4X4,transfrom.rotation);
            transfrom.position = GetPostion(matrix4X4);
        }

        public static Quaternion GetRotation(Matrix4x4 matrix4X4, Quaternion lastQ)
        {
            return GetRotationprivate(matrix4X4, lastQ);
        }

        private static Quaternion GetRotationprivate(Matrix4x4 matrix4X4, Quaternion lastQ)
        {
            var f = 1f + matrix4X4.m00 + matrix4X4.m11 + matrix4X4.m22;
            if (f > 0)
            {
                float qw = Mathf.Sqrt(f) / 2;
                float w = 4 * qw;
                float qx = (matrix4X4.m21 - matrix4X4.m12) / w;
                float qy = (matrix4X4.m02 - matrix4X4.m20) / w;
                float qz = (matrix4X4.m10 - matrix4X4.m01) / w;
                return new Quaternion(qx, qy, qz, qw);
            }
            return lastQ;
        }

        public static Vector3 GetPostion(Matrix4x4 matrix4X4)
        {
            return GetPostionprivate(matrix4X4);
        }

        private static Vector3 GetPostionprivate(Matrix4x4 matrix4X4)
        {
            var x = matrix4X4.m03;
            var y = matrix4X4.m13;
            var z = matrix4X4.m23;
            return new Vector3(x, y, z);
        }

        public static Vector3 GetScale(Matrix4x4 m)
        {
            return GetScalePrive(m);
        }

        private static Vector3 GetScalePrive(Matrix4x4 m)
        {
            var x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
            var y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
            var z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
            return new Vector3(x, y, z);
        }
    }
}
