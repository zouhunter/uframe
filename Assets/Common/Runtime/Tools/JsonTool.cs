/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - Json相关工具。                                                                  *
*//************************************************************************************/

using System;

namespace UFrame
{
    /// <summary>
    /// JSON 相关的实用函数。
    /// </summary>
    public static class JsonTool
    {
        private static IJsonHelper s_JsonHelper = null;

        /// <summary>
        /// 设置 JSON 辅助器。
        /// </summary>
        /// <param name="jsonHelper">要设置的 JSON 辅助器。</param>
        public static void SetJsonHelper(IJsonHelper jsonHelper)
        {
            s_JsonHelper = jsonHelper;
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的 JSON 字符串。</returns>
        public static string ToJson(object obj)
        {
            if (s_JsonHelper == null)
            {
                return "";
            }

            return s_JsonHelper.ToJson(obj);
        }

        /// <summary>
        /// 将对象序列化为 JSON 流。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的 JSON 流。</returns>
        public static byte[] ToJsonData(object obj)
        {
            return ConverterTool.GetBytes(ToJson(obj));
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="json">要反序列化的 JSON 字符串。</param>
        /// <returns>反序列化后的对象。</returns>
        public static T ToObject<T>(string json)
        {
            if (s_JsonHelper == null)
            {
                return default(T);
            }

            try
            {
                return s_JsonHelper.ToObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="json">要反序列化的 JSON 字符串。</param>
        /// <returns>反序列化后的对象。</returns>
        public static object ToObject(Type objectType, string json)
        {
            if (s_JsonHelper == null)
            {
                return null;
            }

            if (objectType == null)
            {
                return null;
            }

            try
            {
                return s_JsonHelper.ToObject(objectType, json);
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// 将 JSON 流反序列化为对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="jsonData">要反序列化的 JSON 流。</param>
        /// <returns>反序列化后的对象。</returns>
        public static T ToObject<T>(byte[] jsonData)
        {
            return ToObject<T>(ConverterTool.GetString(jsonData));
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="jsonData">要反序列化的 JSON 流。</param>
        /// <returns>反序列化后的对象。</returns>
        public static object ToObject(Type objectType, byte[] jsonData)
        {
            return ToObject(objectType, ConverterTool.GetString(jsonData));
        }
        /// <summary>
        /// JSON 辅助器接口。
        /// </summary>
        public interface IJsonHelper
        {
            /// <summary>
            /// 将对象序列化为 JSON 字符串。
            /// </summary>
            /// <param name="obj">要序列化的对象。</param>
            /// <returns>序列化后的 JSON 字符串。</returns>
            string ToJson(object obj);

            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            T ToObject<T>(string json);

            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <param name="objectType">对象类型。</param>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            object ToObject(Type objectType, string json);
        }
    }
}