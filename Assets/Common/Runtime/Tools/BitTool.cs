using System;

namespace UFrame
{
    public static class BitTool
    {
        //ulong mask
        /// <summary>
        /// Sets the value at bitIndex of a 64 bit mask to true
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitTrue(ref ulong mask, int bitIndex)
        {
            mask |= (ulong)1 << bitIndex;
        }
        /// <summary>
        /// Sets the value at bitIndex of a 64 bit mask to false
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitFalse(ref ulong mask, int bitIndex)
        {
            mask &= ~((ulong)1 << bitIndex);
        }
        /// <summary>
        /// Get the value of the bit at bitIndex
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public static bool GetBit(ulong mask, int bitIndex)
        {
            return (mask & ((ulong)1 << bitIndex)) != 0;
        }


        //uint mask
        /// <summary>
        /// Sets the value at bitIndex of a 32 bit mask to true
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitTrue(ref uint mask, int bitIndex)
        {
            mask |= (uint)1 << bitIndex;
        }
        /// <summary>
        /// Sets the value at bitIndex of a 32 bit mask to false
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        public static void SetBitFalse(ref uint mask, int bitIndex)
        {
            mask &= ~((uint)1 << bitIndex);
        }
        /// <summary>
        /// Get the value of the bit at bitIndex
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="bitIndex"></param>
        /// <returns></returns>
        public static bool GetBit(uint mask, int bitIndex)
        {
            return (mask & ((uint)1 << bitIndex)) != 0;
        }
    }
}