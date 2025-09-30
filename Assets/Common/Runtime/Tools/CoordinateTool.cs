/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 坐标转换相关工具。                                                              *
*//************************************************************************************/

using UnityEngine;

namespace UFrame
{

    public static class CoordinateTool
    {
        #region 世界坐标转ui坐标
        public static Vector3 GetWorldPointToUIPos(Camera uiCamera, Camera worldCamera, Vector3 pos)
        {
            return uiCamera.ScreenToWorldPoint(worldCamera.WorldToScreenPoint(pos));
        }
        #endregion

        #region 归一化对象
        public static void NormalTransform(RectTransform ts)
        {
            ts.localPosition = Vector3.zero;
            ts.localScale = Vector3.one;
            ts.localRotation = Quaternion.Euler(Vector3.zero);
        }
        #endregion

        #region 获取周围的点,centerPos为正向坐标，和世界坐标无关
        /// <summary>
        /// 获取周围的点,centerPos为正向坐标，和世界坐标无关
        /// </summary>
        public static Vector3[] GetPointByCenter(float distance, Vector3 centerPos)
        {
            return null;//后续扩展
        }
        #endregion

        #region 获取地面有效点
        /// <summary>
        /// 获取有效的地面点
        /// </summary>
        /// <returns><c>true</c> if is valid point; otherwise, <c>false</c>.</returns>
        public static bool IsValidPoint(Vector3 pos)
        {
            return true;//后续拓展
        }
        #endregion

        #region 获取局部坐标
        /// <summary>
        /// 以world为世界中心，target在world的局部坐标
        /// </summary>
        /// <returns>The space position.</returns>
        /// <param name="target">Target.</param>
        /// <param name="world">World.</param>
        public static Vector3 GetSpacePos(Vector3 target, Vector3 world)
        {
            var v = target - world;
            return v;
        }
        #endregion

        #region 屏幕射线检测可用CObjectCharacter角色对象guid
        public static int ScreenPointToRay()
        {
            int guid = -1;
            Vector3 vc3;
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                vc3 = Input.touches[0].position;
            }
            else
            {
                vc3 = Input.mousePosition;
            }
            return guid;
        }
        #endregion

        #region 格子
        public static Vector3[] GetSlot(Vector3 center, float length, int width, int height)
        {
            Vector3[] vc = new Vector3[width * height];
            int index = 1;
            Vector3 vc3 = Vector3.zero;
            for (int i = 0; i < height; i++)
            {
                float x = height * length - length / 2f - (float)i * length;
                float y = length / 2;
                for (int j = 0; j < width;)
                {
                    vc3.x = x;
                    vc3.y = 0f;
                    vc3.z = y;
                    vc[index - 1] = GetSpacePos(vc3, center);
                    vc[index - 1].z += (-length * (float)width) / 2f;
                    vc[index - 1].x += (-length * (float)height) / 2f;
                    j++;
                    y = (float)j * length + length / 2f;
                    index++;
                }
            }
            return vc;
        }
        #endregion
    }
}