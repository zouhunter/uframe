//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-30
//* 描    述：
//             1.enum
//* ************************************************************************************

namespace UFrame.YieldFlow
{
    /// <summary>
    /// 节点运行状态常量定义。
    /// </summary>
    public class Status
    {
        /// <summary>
        /// 未激活/未开始
        /// </summary>
        public const byte Inactive = 0;
        /// <summary>
        /// 正在运行
        /// </summary>
        public const byte Running = 1;
        /// <summary>
        /// 执行失败
        /// </summary>
        public const byte Failure = 2;
        /// <summary>
        /// 执行成功
        /// </summary>
        public const byte Success = 3;
        /// <summary>
        /// 被中断
        /// </summary>
        public const byte Interrupt = 4;
    }
}

