/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   压缩相关工具。                                                                    *
*//************************************************************************************/

namespace UFrame
{
    /// <summary>
    /// 压缩解压缩相关的实用函数。
    /// </summary>
    public static class ZipTool
    {
        private static IZipHelper s_ZipHelper = null;

        /// <summary>
        /// 设置压缩解压缩辅助器。
        /// </summary>
        /// <param name="zipHelper">要设置的压缩解压缩辅助器。</param>
        public static void SetZipHelper(IZipHelper zipHelper)
        {
            s_ZipHelper = zipHelper;
        }

        /// <summary>
        /// 压缩数据。
        /// </summary>
        /// <param name="bytes">要压缩的数据。</param>
        /// <returns>压缩后的数据。</returns>
        public static byte[] Compress(byte[] bytes)
        {
            if (s_ZipHelper == null)
            {
                return null;
            }

            try
            {
                return s_ZipHelper.Compress(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解压缩数据。
        /// </summary>
        /// <param name="bytes">要解压缩的数据。</param>
        /// <returns>解压缩后的数据。</returns>
        public static byte[] Decompress(byte[] bytes)
        {
            if (s_ZipHelper == null)
            {
                return null;
            }

            try
            {
                return s_ZipHelper.Decompress(bytes);
            }
            catch{
                return null;
            }
        }

        /// <summary>
        /// 压缩解压缩辅助器接口。
        /// </summary>
        public interface IZipHelper
        {
            /// <summary>
            /// 压缩数据。
            /// </summary>
            /// <param name="bytes">要压缩的数据。</param>
            /// <returns>压缩后的数据。</returns>
            byte[] Compress(byte[] bytes);

            /// <summary>
            /// 解压缩数据。
            /// </summary>
            /// <param name="bytes">要解压缩的数据。</param>
            /// <returns>解压缩后的数据。</returns>
            byte[] Decompress(byte[] bytes);
        }
    }
}