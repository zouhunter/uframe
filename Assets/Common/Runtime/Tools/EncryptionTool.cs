/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 加密相关工具。                                                                  *
*//************************************************************************************/

using System;
using System.IO;
using System.Security.Cryptography;

namespace UFrame
{

    /// <summary>
    /// 加密解密相关的实用函数。
    /// </summary>
    public static class EncryptionTool
    {
        private const int QuickEncryptLength = 220;

        /// <summary>
        /// 将 bytes 使用 code 做异或运算的快速版本。
        /// </summary>
        /// <param name="bytes">原始二进制流。</param>
        /// <param name="code">异或二进制流。</param>
        /// <returns>异或后的二进制流。</returns>
        public static byte[] GetQuickXorBytes(byte[] bytes, byte[] code)
        {
            return GetXorBytes(bytes, code, QuickEncryptLength);
        }

        /// <summary>
        /// 将 bytes 使用 code 做异或运算的快速版本。此方法将复用并改写传入的 bytes 作为返回值，而不额外分配内存空间。
        /// </summary>
        /// <param name="bytes">原始及异或后的二进制流。</param>
        /// <param name="code">异或二进制流。</param>
        /// <returns>异或后的二进制流。</returns>
        public static byte[] GetQuickSelfXorBytes(byte[] bytes, byte[] code)
        {
            return GetSelfXorBytes(bytes, code, QuickEncryptLength);
        }

        /// <summary>
        /// 将 bytes 使用 code 做异或运算。
        /// </summary>
        /// <param name="bytes">原始二进制流。</param>
        /// <param name="code">异或二进制流。</param>
        /// <returns>异或后的二进制流。</returns>
        public static byte[] GetXorBytes(byte[] bytes, byte[] code)
        {
            return GetXorBytes(bytes, code, -1);
        }

        /// <summary>
        /// 将 bytes 使用 code 做异或运算。此方法将复用并改写传入的 bytes 作为返回值，而不额外分配内存空间。
        /// </summary>
        /// <param name="bytes">原始及异或后的二进制流。</param>
        /// <param name="code">异或二进制流。</param>
        /// <returns>异或后的二进制流。</returns>
        public static byte[] GetSelfXorBytes(byte[] bytes, byte[] code)
        {
            return GetSelfXorBytes(bytes, code, -1);
        }

        /// <summary>
        /// 将 bytes 使用 code 做异或运算。
        /// </summary>
        /// <param name="bytes">原始二进制流。</param>
        /// <param name="code">异或二进制流。</param>
        /// <param name="length">异或计算长度，若小于 0，则计算整个二进制流。</param>
        /// <returns>异或后的二进制流。</returns>
        public static byte[] GetXorBytes(byte[] bytes, byte[] code, int length)
        {
            if (bytes == null)
            {
                return null;
            }

            int bytesLength = bytes.Length;
            if (length < 0 || length > bytesLength)
            {
                length = bytesLength;
            }

            byte[] results = new byte[bytesLength];
            Buffer.BlockCopy(bytes, 0, results, 0, bytesLength);
            return GetSelfXorBytes(results, code, length);
        }

        /// <summary>
        /// 将 bytes 使用 code 做异或运算。此方法将复用并改写传入的 bytes 作为返回值，而不额外分配内存空间。
        /// </summary>
        /// <param name="bytes">原始及异或后的二进制流。</param>
        /// <param name="code">异或二进制流。</param>
        /// <param name="length">异或计算长度，若小于 0，则计算整个二进制流。</param>
        /// <returns>异或后的二进制流。</returns>
        public static byte[] GetSelfXorBytes(byte[] bytes, byte[] code, int length)
        {
            if (bytes == null)
            {
                return null;
            }

            if (code == null)
            {
                return null;
            }

            int codeLength = code.Length;
            if (codeLength <= 0)
            {
                return null;
            }

            int codeIndex = 0;
            int bytesLength = bytes.Length;
            if (length < 0 || length > bytesLength)
            {
                length = bytesLength;
            }

            for (int i = 0; i < length; i++)
            {
                bytes[i] ^= code[codeIndex++];
                codeIndex = codeIndex % codeLength;
            }

            return bytes;
        }

        //DES 加密
        public static byte[] DesEncrypt(byte[] data, byte[] rgbKey, byte[] rgbValue)
        {
            var result = data;
            using (DESCryptoServiceProvider descsp = new DESCryptoServiceProvider())//加密、解密对象
            {
                using (MemoryStream memStream = new MemoryStream())//内存流对象
                {
                    ICryptoTransform crytpo = descsp.CreateEncryptor(rgbKey, rgbValue);
                    using (CryptoStream cryptoStream = new CryptoStream(memStream, crytpo, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);//向加密流中写入数据
                        cryptoStream.FlushFinalBlock();//将数据压入基础流
                        result = memStream.ToArray();//从内存流中获取字节序列
                    }
                    crytpo.Dispose();
                }
            }
            return result;
        }

        //DES 解密
        public static byte[] DesDecrypt(byte[] data, byte[] rgbKey, byte[] rgbValue)
        {
            var result = data;
            using (DESCryptoServiceProvider descsp = new DESCryptoServiceProvider())//加密、解密对象
            {
                using (MemoryStream memStream = new MemoryStream())//内存流对象
                {
                    ICryptoTransform crytpo = descsp.CreateDecryptor(rgbKey, rgbValue);
                    using (CryptoStream cryptoStream = new CryptoStream(memStream, crytpo, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);//向加密流中写入数据
                        cryptoStream.FlushFinalBlock();//将数据压入基础流
                        result = memStream.ToArray();//从内存流中获取字节序列
                    }
                    crytpo.Dispose();
                }
            }
            return result;
        }
    }
}