/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - md5相关工具。                                                                   *
*//************************************************************************************/

using System;
using System.Text;
using System.Security.Cryptography;

namespace UFrame
{

    public static class MD5Tool
    {
        /// <summary>
        /// string md5
        /// </summary>
        /// <returns>The d5 encrypt.</returns>
        /// <param name="strText">String text.</param>
        public static string StringToMD5(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
            return System.Text.Encoding.Default.GetString(result);
        }

        /// <summary>
        /// 将二进制数组转换为Md5
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BytesToMD5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            StringBuilder fileMD5 = new StringBuilder();
            foreach (byte b in result)
            {
                fileMD5.Append(Convert.ToString(b, 16));
            }

            return fileMD5.ToString();
        }

        /// <summary>
        /// 诸Hash值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetHashMD5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            int hashCode = 0;
            for (int i = 0; i < 4; i++)
            {
                hashCode += (Convert.ToInt32(result[i]) + Convert.ToInt32(result[i + 1]) + Convert.ToInt32(result[i + 2]) + Convert.ToInt32(result[i])) << i * 8;
            }
            return hashCode;
        }
		/// <summary>
		/// MD5字符串加密
		/// </summary>
		/// <param name="txt"></param>
		/// <returns>加密后字符串</returns>
		public static string GenerateMD5(string txt)
		{
			using (MD5 mi = MD5.Create())
			{
				byte[] buffer = Encoding.Default.GetBytes(txt);
				//开始加密
				byte[] newBuffer = mi.ComputeHash(buffer);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < newBuffer.Length; i++)
				{
					sb.Append(newBuffer[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}
		
		/// <summary>
		/// MD5流加密
		/// </summary>
		/// <param name="inputStream"></param>
		/// <returns></returns>
		public static string GenerateMD5(System.IO.Stream inputStream)
		{
			using (MD5 mi = MD5.Create())
			{
				//开始加密
				byte[] newBuffer = mi.ComputeHash(inputStream);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < newBuffer.Length; i++)
				{
					sb.Append(newBuffer[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}
    }
}