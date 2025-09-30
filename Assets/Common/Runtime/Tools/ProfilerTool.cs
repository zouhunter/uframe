/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   性能分析相关工具。                                                                *
*//************************************************************************************/

using System.Diagnostics;

namespace UFrame
{
    /// <summary>
    /// 性能分析相关的实用函数。
    /// </summary>
    public static class ProfilerTool
    {
        private static IProfilerHelper s_ProfilerHelper = null;

        /// <summary>
        /// 设置性能分析辅助器。
        /// </summary>
        /// <param name="profilerHelper">要设置的性能分析辅助器。</param>
        public static void SetProfilerHelper(IProfilerHelper profilerHelper)
        {
            s_ProfilerHelper = profilerHelper;
        }

        /// <summary>
        /// 开始采样。
        /// </summary>
        /// <param name="name">采样名称。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void BeginSample(string name)
        {
            if (s_ProfilerHelper == null)
            {
                return;
            }

            s_ProfilerHelper.BeginSample(name);
        }

        /// <summary>
        /// 结束采样。
        /// </summary>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void EndSample()
        {
            if (s_ProfilerHelper == null)
            {
                return;
            }

            s_ProfilerHelper.EndSample();
        }
        /// <summary>
        /// 性能分析辅助器接口。
        /// </summary>
        public interface IProfilerHelper
        {
            /// <summary>
            /// 开始采样。
            /// </summary>
            /// <param name="name">采样名称。</param>
            void BeginSample(string name);

            /// <summary>
            /// 结束采样。
            /// </summary>
            void EndSample();
        }
    }
}