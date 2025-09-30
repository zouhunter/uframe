/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   空间转换相关工具。                                                                *
*//************************************************************************************/

using UnityEngine;

namespace UFrame
{
    public static class TransfromTool
    {
        /// <summary>
        /// UGUI根据父对象归一化
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="child">Child.</param>
        /// <param name="show">If set to <c>true</c> show.</param>
        public static void Set_Normalization_RectTransform_Parent(this RectTransform child, Transform parent, bool show = true)
        {
            child.SetParent(parent, false);
            child.localScale = Vector3.one;
            child.localRotation = Quaternion.Euler(Vector3.zero);
            child.anchorMin = Vector2.
                zero;
            child.anchorMax = Vector2.one;
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
            child.localPosition = Vector3.zero;
            child.gameObject.SetActive(show);
        }

        /// <summary>
        /// 普通对象父对象归一化
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="show"></param>
        public static void Set_Normalization_Transform_Parent(this Transform child, Transform parent, bool show = true)
        {
            if (parent == null || child == null)
                return;
            child.SetParent(parent);
            child.localScale = Vector3.one;
            child.localRotation = Quaternion.Euler(Vector3.zero);
            child.localPosition = Vector3.zero;
            child.gameObject.SetActive(show);
        }
        //忽略xz的lookat
        public static void LookAt(Transform ts, Transform target)
        {
            if (ts == null || target == null)
                return;
            ts.LookAt(target);
            var vc3 = ts.rotation.eulerAngles;
            vc3.x = 0;
            vc3.z = 0;
            ts.rotation = Quaternion.Euler(vc3);
        }

        //忽略xz的lookat pos
        public static void LookAtPos(Transform ts, Vector3 target)
        {
            if (ts == null)
            {
                return;
            }
            var to = target - ts.position;
            var vc2 = Vector2.zero;
            vc2.x = 0;
            vc2.y = 1;
            var v = vc2;
            vc2.x = to.x;
            vc2.y = to.z;
            var b = vc2;
            var angle = angle_360(v, b);
            var vc3 = Vector3.zero;
            vc3.x = 0;
            vc3.y = angle;
            vc3.z = 0;
            var v3 = vc3;
            ts.localRotation = Quaternion.Euler(v3);
        }
        //角度计算
        public static float angle_360(Vector2 from_, Vector2 to_)
        {
            if (Vector3.Normalize(to_).x < 0)
            {
                return -Vector2.Angle(from_, to_);
            }
            else
            {
                return Vector2.Angle(from_, to_);
            }
        }

    }
}