/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - Unity3d控件相关工具。                                                           *
*//************************************************************************************/
using UnityEngine;

namespace UFrame
{
    public static class ComponentTool
    {
        /// <summary>
        /// 安全获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T MustComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return default(T);
            T component = go.GetComponent<T>();
            if (null != component)
            {
                return component;
            }
            else
            {
                return go.AddComponent<T>();
            }
        }
        /// <summary>
        /// 安全获得组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T MustComponent<T>(Component rect) where T : Component
        {
            if (rect == null) return default(T);
            T com = rect.GetComponent<T>();
            if (null != com)
            {
                return com;
            }
            else
            {
                return rect.gameObject.AddComponent<T>();
            }
        }

        /// <summary>
        /// 安全移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trans"></param>
        public static void RemoveComponentSafty<T>(GameObject go) where T : Component
        {
            T ret = go.GetComponent<T>();
            if (null != ret)
            {
                UnityEngine.Object.DestroyImmediate(ret,true);
            }
        }
        /// <summary>
        /// 安全移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trans"></param>
        public static void RemoveComponentSafty<T>(Component trans) where T : Component
        {
            T ret = trans.GetComponent<T>();
            if (null != ret)
            {
                UnityEngine.Object.DestroyImmediate(ret);
            }
        }

    }
}