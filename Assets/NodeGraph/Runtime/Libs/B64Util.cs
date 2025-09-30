/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 加密解密                                                                        *
*//************************************************************************************/

using System;

namespace UFrame.NodeGraph
{
    public static class B64Util
    {
        public static string DecodeString(string data)
        {
            if (data.StartsWith(NGSettings.BASE64_IDENTIFIER))
            {
                var bytes = Convert.FromBase64String(data.Substring(NGSettings.BASE64_IDENTIFIER.Length));
                data = System.Text.Encoding.UTF8.GetString(bytes);
            }
            return data;
        }

        public static string EncodeString(string data)
        {
            return NGSettings.BASE64_IDENTIFIER +
                   Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        }
    }
}
